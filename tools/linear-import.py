"""Mechanical, idempotent Linear import via the GraphQL API (needs LINEAR_API_KEY).

  python tools/linear-import.py --surface tv          # one surface batch
  python tools/linear-import.py --all                 # every batch file
  python tools/linear-import.py --surface tv --dry    # print plan, no writes

Creates the surface project (description = context-packs/<surface>.md), ensures
labels, creates one issue per record using the ticket template (spec verbatim;
Expected behavior = TO DERIVE until the DD/orchestrator fills it at dispatch),
moves dry-run issues into their surface project, and checkpoints every created
id in docs/design/linear-import/checkpoint.json so re-runs never duplicate.
"""
import argparse, io, json, os, sys, time, urllib.request

API = "https://api.linear.app/graphql"
TEAM_KEY = "SBR"
DIR = "docs/design/linear-import"
CKPT = f"{DIR}/checkpoint.json"


def gql(query, variables=None):
    key = os.environ.get("LINEAR_API_KEY")
    if not key:  # fallback: a key file outside the repo, never pasted into chat
        kp = os.path.join(os.path.expanduser("~"), ".linear_api_key")
        if os.path.exists(kp):
            key = io.open(kp, encoding="utf-8").read().strip()
    if not key:
        sys.exit("LINEAR_API_KEY not set and ~/.linear_api_key missing")
    body = json.dumps({"query": query, "variables": variables or {}}).encode()
    req = urllib.request.Request(API, body, {"Content-Type": "application/json", "Authorization": key})
    for attempt in range(5):
        try:
            with urllib.request.urlopen(req, timeout=60) as r:
                out = json.load(r)
            if "errors" in out:
                raise RuntimeError(out["errors"])
            return out["data"]
        except Exception:
            if attempt == 4:
                raise
            time.sleep(2 ** attempt)


def ticket_description(rec):
    doc = rec["owning_doc"] or "no owning document (console, per K15): vision + constitution apply"
    raw = rec["state_raw"][:200]
    return (
        "## Context\n"
        f"Migrated from the design register (legacy ID **{rec['old_id']}**, section \"{rec['section']}\", "
        f"batch {rec['batch'] or 'n/a'}). Lifecycle at migration: **{rec['lifecycle']}** (raw: {raw}). "
        "Read the project context pack first; this ticket assumes it.\n\n"
        "## References\n"
        f"- Owning document: `{doc}`\n"
        "- Project context pack: this project's description\n"
        "- Related issues: TO LINK (legacy IDs named in the spec below)\n\n"
        "## Scope\n"
        "- In: TO CONFIRM from the spec at dispatch time\n"
        "- Out: anything not named in the spec\n"
        "- Files: TO CONFIRM at dispatch (see project ownership)\n"
        "- Size: one dispatch; split before Todo if larger\n\n"
        "## Spec / ruling (verbatim from the register)\n"
        f"{rec['body']}\n\n"
        "## Verification recipe\n"
        "TO CONFIRM at dispatch from the project's verification section.\n\n"
        "## Expected behavior\n"
        "TO DERIVE: the DD or orchestrator fills this with checkable statements before dispatch.\n\n"
        "---\n"
        f"Legacy ID: {rec['old_id']} | Lifecycle: {rec['lifecycle']} | Batch: {rec['batch'] or 'n/a'} | Surface: {rec['surface_label']}\n"
    )


def load_dry_map():
    try:
        txt = io.open("docs/design/linear-dryrun-result.txt", encoding="utf-8").read()
        j = json.loads(txt[txt.index("{"): txt.rindex("}") + 1])
        return {i["old_id"]: i["identifier"] for i in j["issues"]}
    except Exception:
        return {}


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--surface")
    ap.add_argument("--all", action="store_true")
    ap.add_argument("--dry", action="store_true")
    a = ap.parse_args()
    if a.all:
        surfaces = [f[:-5] for f in os.listdir(DIR)
                    if f.endswith(".json") and f not in ("checkpoint.json", "laws-excluded.json")]
    else:
        surfaces = [a.surface]

    ckpt = json.load(io.open(CKPT, encoding="utf-8")) if os.path.exists(CKPT) else {"projects": {}, "issues": {}}

    def save():
        json.dump(ckpt, io.open(CKPT, "w", encoding="utf-8"), indent=1)

    if a.dry:
        for surf in surfaces:
            recs = json.load(io.open(f"{DIR}/{surf}.json", encoding="utf-8"))
            todo = [r for r in recs if r["old_id"] not in ckpt["issues"]]
            print(f"[dry] {surf}: project '{recs[0]['project']}', {len(todo)} to create/move, "
                  f"{sum(r['already_in_dry_run'] for r in todo)} moves")
        return

    team = gql(
        "query($k:String!){ teams(filter:{key:{eq:$k}}){ nodes{ id name "
        "states{ nodes{ id name } } labels{ nodes{ id name } } } } }", {"k": TEAM_KEY}
    )["teams"]["nodes"][0]
    states = {s["name"]: s["id"] for s in team["states"]["nodes"]}
    labels = {l["name"]: l["id"] for l in team["labels"]["nodes"]}

    def label_id(name):
        if name not in labels:
            labels[name] = gql(
                "mutation($i:IssueLabelCreateInput!){ issueLabelCreate(input:$i){ issueLabel{ id } } }",
                {"i": {"name": name, "teamId": team["id"]}},
            )["issueLabelCreate"]["issueLabel"]["id"]
        return labels[name]

    dry = load_dry_map()
    for surf in surfaces:
        recs = json.load(io.open(f"{DIR}/{surf}.json", encoding="utf-8"))
        pname = recs[0]["project"]
        pack_path = f"{DIR}/context-packs/{surf}.md"
        pack = io.open(pack_path, encoding="utf-8").read() if os.path.exists(pack_path) else f"Context pack pending for {pname}."
        if pname not in ckpt["projects"]:
            pid = gql(
                "mutation($i:ProjectCreateInput!){ projectCreate(input:$i){ project{ id url } } }",
                {"i": {"name": pname, "teamIds": [team["id"]], "description": pack[:100000]}},
            )["projectCreate"]["project"]["id"]
            ckpt["projects"][pname] = pid
            save()
        pid = ckpt["projects"][pname]
        made = 0
        for rec in recs:
            oid = rec["old_id"]
            if oid in ckpt["issues"]:
                continue
            if oid in dry:  # move the dry-run issue rather than re-create it
                found = gql(
                    "query($q:String!){ issues(filter:{ title:{ startsWith:$q } }){ nodes{ id identifier } } }",
                    {"q": f"[{oid}]"},
                )["issues"]["nodes"]
                if found:
                    gql("mutation($id:String!,$i:IssueUpdateInput!){ issueUpdate(id:$id,input:$i){ success } }",
                        {"id": found[0]["id"], "i": {"projectId": pid}})
                    ckpt["issues"][oid] = found[0]["identifier"]
                continue
            inp = {
                "teamId": team["id"], "projectId": pid,
                "title": f"[{oid}] {rec['title']}",
                "description": ticket_description(rec),
                "stateId": states[rec["linear_state"]],
                "labelIds": [label_id(rec["surface_label"]), label_id(rec["lifecycle"].lower())],
            }
            res = gql("mutation($i:IssueCreateInput!){ issueCreate(input:$i){ issue{ identifier } } }", {"i": inp})
            ckpt["issues"][oid] = res["issueCreate"]["issue"]["identifier"]
            made += 1
            if made % 10 == 0:
                save()
                print(f"  {surf}: {made} created...", flush=True)
            time.sleep(0.25)  # stay well under the API rate limit
        save()
        print(f"{surf}: {made} created, total tracked {len(ckpt['issues'])}")


if __name__ == "__main__":
    main()
