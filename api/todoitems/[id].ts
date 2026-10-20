import type { VercelRequest, VercelResponse } from "@vercel/node";
import { ensureSchema, getSql, handleDbError, toDto, TodoItemRow } from "../_db";

function setCors(res: VercelResponse) {
  res.setHeader("Access-Control-Allow-Origin", process.env.ALLOWED_ORIGIN ?? "*");
  res.setHeader("Access-Control-Allow-Methods", "GET,PUT,DELETE,OPTIONS");
  res.setHeader("Access-Control-Allow-Headers", "Content-Type");
}

export default async function handler(req: VercelRequest, res: VercelResponse) {
  setCors(res);
  if (req.method === "OPTIONS") {
    res.status(204).end();
    return;
  }

  const id = req.query.id as string;

  try {
    await ensureSchema();
    const sql = getSql();

    if (req.method === "GET") {
      const rows = (await sql`SELECT * FROM todo_items WHERE id = ${id}`) as TodoItemRow[];
      if (rows.length === 0) {
        res.status(404).json({ error: "Not found." });
        return;
      }
      res.status(200).json(toDto(rows[0]));
      return;
    }

    if (req.method === "PUT") {
      const body = req.body ?? {};
      const rows = (await sql`
        UPDATE todo_items
        SET text = ${body.text ?? ""},
            priority = ${body.priority ?? "Medium"},
            done = ${!!body.done},
            points_earned = ${body.pointsEarned ?? 0},
            completed_at = ${body.completedAt ?? null}
        WHERE id = ${id}
        RETURNING *
      `) as TodoItemRow[];

      if (rows.length === 0) {
        res.status(404).json({ error: "Not found." });
        return;
      }
      res.status(204).end();
      return;
    }

    if (req.method === "DELETE") {
      await sql`DELETE FROM todo_items WHERE id = ${id}`;
      res.status(204).end();
      return;
    }

    res.setHeader("Allow", "GET,PUT,DELETE,OPTIONS");
    res.status(405).json({ error: "Method not allowed." });
  } catch (err) {
    handleDbError(res, err);
  }
}
