"""Split register-export.json into per-surface import batches (untruncated bodies).

Excludes laws (they stay in the constitution) and marks the dry-run issues that
must be MOVED into their surface project instead of re-created.
"""
import json, io, os, collections

SRC = "docs/design/register-export.json"
OUT = "docs/design/linear-import"
PROJECT_BY_SURFACE = {
    "laptop": "Laptop - SureThing", "tv": "TV - match theater", "room": "Room",
    "phone": "Phone", "console": "Console", "cross-surface": "Cross-surface",
}
OWNING_DOC = {
    "laptop": "docs/design/surething-design.md", "tv": "docs/design/tv-design.md",
    "room": "docs/design/room-design.md", "phone": "docs/design/phone-design.md",
    "console": None, "cross-surface": "docs/design/constitution.md",
}
STATE_MAP = {"Exploration": "Backlog", "Candidate": "Backlog", "Parked": "Backlog", "Approved": "Todo",
             "In Build": "In Progress", "Implemented": "In Review", "Design-verified": "Done",
             "Closed": "Done", "Struck": "Canceled", "Unclassified": "Backlog"}

dry = {r["old_id"] for r in json.load(io.open("docs/design/linear-dryrun-input.json", encoding="utf-8"))}
d = json.load(io.open(SRC, encoding="utf-8"))
laws, batches = [], collections.defaultdict(list)
for r in d["records"]:
    if r["state"] == "Law":
        laws.append({"old_id": r["old_id"], "title": r["title"], "body": r["body"]})
        continue
    surf = r["surface"] if r["surface"] in OWNING_DOC else "cross-surface"
    batches[surf].append({
        "old_id": r["old_id"],
        "title": (r["title"] or r["body"][:90]).strip()[:120],
        "surface": surf, "surface_label": r["surface"], "section": r["section"],
        "lifecycle": r["state"], "linear_state": STATE_MAP[r["state"]],
        "state_raw": r["state_raw"], "batch": r["batch"],
        "body": r["body"],  # untruncated, verbatim
        "owning_doc": OWNING_DOC[surf], "project": PROJECT_BY_SURFACE[surf],
        "already_in_dry_run": r["old_id"] in dry,
    })

os.makedirs(OUT, exist_ok=True)
for surf, items in batches.items():
    io.open(f"{OUT}/{surf}.json", "w", encoding="utf-8").write(json.dumps(items, ensure_ascii=False, indent=1))
io.open(f"{OUT}/laws-excluded.json", "w", encoding="utf-8").write(json.dumps(laws, ensure_ascii=False, indent=1))
print("batches:", {k: len(v) for k, v in batches.items()},
      "| laws excluded:", len(laws),
      "| dry-run moves:", sum(i["already_in_dry_run"] for v in batches.values() for i in v))
