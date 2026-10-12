// Case 07 — review the design of this function.

interface StatementLine {
  date: string; // ISO yyyy-mm-dd
  description: string;
  amountMinor: number; // signed, in minor units
}

interface ParsedStatement {
  accountNumber: string;
  currency: string;
  openingBalanceMinor: number;
  closingBalanceMinor: number;
  lines: StatementLine[];
}

/**
 * Parse a bank statement in the fixed-width MT940-lite format the bank
 * exports. Returns the parsed statement, or throws with the offending line
 * number. The format: a header record, N transaction records, one trailer.
 */
export function parseStatement(text: string): ParsedStatement {
  const rows = text.trim().split(/\r?\n/);
  if (rows.length < 2) {
    throw new Error("statement must have a header and a trailer");
  }

  const header = rows[0];
  if (!header.startsWith("H|")) {
    throw new Error("line 1: expected header record");
  }
  const accountNumber = header.slice(2, 12).trim();
  const currency = header.slice(13, 16);
  if (!/^[A-Z]{3}$/.test(currency)) {
    throw new Error(`line 1: bad currency code '${currency}'`);
  }
  const openingBalanceMinor = parseSignedAmount(header.slice(17, 32), 1);

  const lines: StatementLine[] = [];
  let runningMinor = openingBalanceMinor;
  for (let i = 1; i < rows.length - 1; i++) {
    const row = rows[i];
    const lineNumber = i + 1;
    if (!row.startsWith("T|")) {
      throw new Error(`line ${lineNumber}: expected transaction record`);
    }
    const rawDate = row.slice(2, 10);
    const date = `${rawDate.slice(0, 4)}-${rawDate.slice(4, 6)}-${rawDate.slice(6, 8)}`;
    if (Number.isNaN(Date.parse(date))) {
      throw new Error(`line ${lineNumber}: bad date '${rawDate}'`);
    }
    const amountMinor = parseSignedAmount(row.slice(11, 26), lineNumber);
    const description = row.slice(27).trim();
    runningMinor += amountMinor;
    lines.push({ date, description, amountMinor });
  }

  const trailer = rows[rows.length - 1];
  if (!trailer.startsWith("F|")) {
    throw new Error(`line ${rows.length}: expected trailer record`);
  }
  const closingBalanceMinor = parseSignedAmount(trailer.slice(2, 17), rows.length);
  if (closingBalanceMinor !== runningMinor) {
    throw new Error(
      `trailer balance ${closingBalanceMinor} does not match computed ${runningMinor}`,
    );
  }

  return { accountNumber, currency, openingBalanceMinor, closingBalanceMinor, lines };
}

function parseSignedAmount(field: string, lineNumber: number): number {
  const trimmed = field.trim();
  if (!/^[+-]\d+$/.test(trimmed)) {
    throw new Error(`line ${lineNumber}: bad amount '${trimmed}'`);
  }
  return Number(trimmed);
}
