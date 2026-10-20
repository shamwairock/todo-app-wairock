import type { VercelRequest, VercelResponse } from "@vercel/node";
import { ensureSchema, getSql, handleDbError, toDto, TodoItemRow } from "../_db";

function setCors(res: VercelResponse) {
  res.setHeader("Access-Control-Allow-Origin", process.env.ALLOWED_ORIGIN ?? "*");
  res.setHeader("Access-Control-Allow-Methods", "GET,POST,PUT,DELETE,OPTIONS");
  res.setHeader("Access-Control-Allow-Headers", "Content-Type");
}

export default async function handler(req: VercelRequest, res: VercelResponse) {
  setCors(res);
  if (req.method === "OPTIONS") {
    res.status(204).end();
    return;
  }

  try {
    await ensureSchema();
    const sql = getSql();

    if (req.method === "GET") {
      const rows = (await sql`SELECT * FROM todo_items ORDER BY created_at ASC`) as TodoItemRow[];
      res.status(200).json(rows.map(toDto));
      return;
    }

    if (req.method === "POST") {
      const body = req.body ?? {};
      const id: string = body.id || crypto.randomUUID();
      const text: string = body.text ?? "";
      const priority: string = body.priority ?? "Medium";
      const createdAt: string = body.createdAt ?? new Date().toISOString();

      const rows = (await sql`
        INSERT INTO todo_items (id, text, priority, done, points_earned, created_at, completed_at)
        VALUES (${id}, ${text}, ${priority}, false, 0, ${createdAt}, null)
        RETURNING *
      `) as TodoItemRow[];

      res.status(201).json(toDto(rows[0]));
      return;
    }

    res.setHeader("Allow", "GET,POST,OPTIONS");
    res.status(405).json({ error: "Method not allowed." });
  } catch (err) {
    handleDbError(res, err);
  }
}
