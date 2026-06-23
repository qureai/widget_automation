import * as fs from "fs";
import * as path from "path";

const REPORT_PATH = path.resolve("reports", "heal-report.json");

export interface HealEvent {
  timestamp: string;
  name: string;
  primaryStrategy: string;
  usedStrategy: string;
  healedSelector?: string;
}

export async function logHeal(event: Omit<HealEvent, "timestamp">): Promise<void> {
  const record: HealEvent = { timestamp: new Date().toISOString(), ...event };

  let existing: HealEvent[] = [];
  if (fs.existsSync(REPORT_PATH)) {
    try {
      existing = JSON.parse(fs.readFileSync(REPORT_PATH, "utf-8"));
    } catch {
      existing = [];
    }
  }

  existing.push(record);
  fs.mkdirSync(path.dirname(REPORT_PATH), { recursive: true });
  fs.writeFileSync(REPORT_PATH, JSON.stringify(existing, null, 2), "utf-8");

  console.warn(
    `[healReport] "${record.name}" — primary: ${record.primaryStrategy}, used: ${record.usedStrategy}` +
      (record.healedSelector ? `, healed to: ${record.healedSelector}` : "")
  );
}
