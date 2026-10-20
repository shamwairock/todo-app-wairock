import { neon, NeonQueryFunction } from "@neondatabase/serverless";
import type { VercelResponse } from "@vercel/node";

export interface TodoItemRow {
  id: string;
  text: string;
  priority: string;
  done: boolean;
  points_earned: number;
  created_at: string;
  completed_at: string | null;
}

export interface TodoItemDto {
  id: string;
  text: string;
  priority: string;
  done: boolean;
  pointsEarned: number;
  createdAt: string;
  completedAt: string | null;
}

let sql: NeonQueryFunction<false, false> | null = null;
let schemaReady: Promise<void> | null = null;

// @neondatabase/serverless talks to Neon over HTTP (no TCP connection pool to
// manage), which is exactly what a Vercel serverless function needs: each
// invocation is short-lived and stateless, same as the DB driver itself.
export function getSql(): NeonQueryFunction<false, false> {
  if (!sql) {
    const connectionString = process.env.DATABASE_URL;
    if (!connectionString) {
      throw new DbNotConfiguredError();
    }
    sql = neon(connectionString);
  }
  return sql;
}

export class DbNotConfiguredError extends Error {
  constructor() {
    super("DATABASE_URL is not set yet.");
  }
}

export async function ensureSchema(): Promise<void> {
  if (!schemaReady) {
    const db = getSql();
    schemaReady = db`
      CREATE TABLE IF NOT EXISTS todo_items (
        id uuid PRIMARY KEY,
        text text NOT NULL,
        priority text NOT NULL,
        done boolean NOT NULL DEFAULT false,
        points_earned integer NOT NULL DEFAULT 0,
        created_at timestamptz NOT NULL,
        completed_at timestamptz
      )
    `.then(() => undefined);
  }
  await schemaReady;
}

export function toDto(row: TodoItemRow): TodoItemDto {
  return {
    id: row.id,
    text: row.text,
    priority: row.priority,
    done: row.done,
    pointsEarned: row.points_earned,
    createdAt: row.created_at,
    completedAt: row.completed_at,
  };
}

export function handleDbError(res: VercelResponse, err: unknown): void {
  if (err instanceof DbNotConfiguredError) {
    res.status(503).json({ error: "Database not configured yet. Set the DATABASE_URL env var in Vercel." });
    return;
  }
  console.error(err);
  res.status(500).json({ error: "Unexpected database error." });
}
