import type { VercelRequest, VercelResponse } from "@vercel/node";
import { getSql } from "../_db";

export default async function handler(_req: VercelRequest, res: VercelResponse) {
  if (!process.env.DATABASE_URL) {
    res.status(200).json({ connected: false, reason: "DATABASE_URL is not set yet." });
    return;
  }

  try {
    const sql = getSql();
    await sql`SELECT 1`;
    res.status(200).json({ connected: true });
  } catch (err) {
    res.status(200).json({ connected: false, reason: err instanceof Error ? err.message : String(err) });
  }
}
