"""Export docs/design/REGISTER.md into structured JSON for the Linear migration.

Usage: python tools/register-export.py [REGISTER.md] [out.json]
Prints a summary; writes one record per table row with a normalized state.
Never modifies the register.
"""
import json, re, sys, collections, io

SRC = sys.argv[1] if len(sys.argv) > 1 else "docs/design/REGISTER.md"
OUT = sys.argv[2] if len(sys.argv) > 2 else "docs/design/register-export.json"

SURFACE_BY_PREFIX = {"S": "laptop", "T": "tv", "R": "room", "C": "cross-surface",
                     "K": "console", "P": "phone", "G": "gates", "M": "markets", "V": "misc", "TV": "tv"}

# Normalize the register's free-text state vocabulary onto the Linear workflow.
STATE_RULES = [
    # order matters: terminal/negative words first, then laws, then positive rulings
    (r"design-?verified", "Design-verified"),
    (r"struck|strike|revoked|dumped|deprecated|superseded|withdrawn|cancell?ed|void", "Struck"),
    (r"parked|deferred|held|quarantined?", "Parked"),
    (r"^\s*≡|re-?pointed|alias", "Closed"),          # alias rows point at the base row
    (r"closed|retired", "Closed"),
    (r"implemented|landed|shipped|merged", "Implemented"),
    (r"in build|building|dispatched|flagged|owed|violation", "In Build"),
    (r"law|constitution|standing", "Law"),         # not work: becomes a rule, not a ticket
    (r"granted|approved|ruled|signed|authored|confirmed|amended|new|spec issued|reasoning corrected", "Approved"),
    (r"debt|candidate|proposed", "Candidate"),
    (r"exploration|open|pending", "Exploration"),
]

def normalize(state_cell):
    s = state_cell.lower()
    for pat, name in STATE_RULES:
        if re.search(pat, s):
            return name
    return "Unclassified"

def split_row(line):
    # markdown table row -> cells; tolerate escaped pipes
    body = line.strip()
    if body.startswith("|"): body = body[1:]
    if body.endswith("|"): body = body[:-1]
    parts, cur, i = [], "", 0
    while i < len(body):
        ch = body[i]
        if ch == "\\" and i + 1 < len(body) and body[i+1] == "|":
            cur += "|"; i += 2; continue
        if ch == "|":
            parts.append(cur.strip()); cur = ""; i += 1; continue
        cur += ch; i += 1
    parts.append(cur.strip())
    return parts

lines = io.open(SRC, encoding="utf-8", errors="replace").read().splitlines()
section = None
records, problems = [], []
for ln, line in enumerate(lines, 1):
    if line.startswith("#"):
        section = line.lstrip("#").strip()
        continue
    if not line.startswith("|") or set(line.strip()) <= set("|-: "):
        continue
    cells = split_row(line)
    if not cells or cells[0].lower() in ("id", "item", "#"):
        continue
    raw_id = re.sub(r"[*`]", "", cells[0]).strip()
    m = re.match(r"^([A-Z]{1,2})-?(\d+[A-Za-z0-9.\-()]*)", raw_id)
    if not m:
        problems.append({"line": ln, "reason": "no id", "cell": raw_id[:60]}); continue
    prefix = m.group(1)
    rec = {
        "old_id": raw_id.split()[0],
        "prefix": prefix,
        "surface": SURFACE_BY_PREFIX.get(prefix, "unknown"),
        "section": section,
        "title": cells[1][:200] if len(cells) > 1 else "",
        "state_raw": cells[2][:200] if len(cells) > 2 else "",
        "state": normalize(cells[2] if len(cells) > 2 else ""),
        "batch": cells[3][:80] if len(cells) > 3 else "",
        "body": " | ".join(cells[1:]),
        "line": ln,
        "cells": len(cells),
    }
    if rec["cells"] != 4:
        problems.append({"line": ln, "reason": f"{rec['cells']} cells (expected 4)", "id": rec["old_id"]})
    records.append(rec)

json.dump({"source": SRC, "count": len(records), "problems": problems, "records": records},
          io.open(OUT, "w", encoding="utf-8"), ensure_ascii=False, indent=1)

print(f"rows exported: {len(records)}  problems: {len(problems)}  -> {OUT}")
print("by surface:", dict(collections.Counter(r["surface"] for r in records)))
print("by state:  ", dict(collections.Counter(r["state"] for r in records)))
dups = [k for k, v in collections.Counter(r["old_id"] for r in records).items() if v > 1]
print("duplicate ids:", len(dups), dups[:10])
for p in problems[:8]: print("  problem:", p)
