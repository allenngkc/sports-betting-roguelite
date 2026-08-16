import json, glob, os, time
from collections import Counter, defaultdict

BASE = os.path.expanduser(r"~\.claude\projects")
LANES = ["markets-pregame", "sgp", "research", "tv-sweat", "room-refinement", "surething-ui"]
CUTOFF = time.time() - 4 * 86400  # last 4 days

SPAWN = {"Task", "Agent"}
HANDS_ON = {"Edit", "Write", "MultiEdit", "NotebookEdit"}

report = {}
for lane in LANES:
    d = os.path.join(BASE, f"C--Users-Allen-orca-workspaces-sports-betting-roguelite-{lane}")
    files = [f for f in glob.glob(os.path.join(d, "*.jsonl")) if os.path.getmtime(f) > CUTOFF]
    tools = Counter()
    spawn_prompts = []
    turns = 0
    for f in files:
        try:
            with open(f, encoding="utf-8", errors="replace") as fh:
                for line in fh:
                    try:
                        o = json.loads(line)
                    except Exception:
                        continue
                    if o.get("type") != "assistant":
                        continue
                    c = (o.get("message") or {}).get("content")
                    if not isinstance(c, list):
                        continue
                    turns += 1
                    for b in c:
                        if isinstance(b, dict) and b.get("type") == "tool_use":
                            name = b.get("name", "?")
                            tools[name] += 1
                            if name in SPAWN:
                                p = (b.get("input") or {}).get("description") or (b.get("input") or {}).get("prompt", "")
                                spawn_prompts.append(str(p)[:80])
        except Exception:
            pass
    total = sum(tools.values())
    spawns = sum(tools[s] for s in SPAWN)
    hands = sum(tools[h] for h in HANDS_ON)
    report[lane] = {
        "files": len(files), "tool_calls": total, "spawns": spawns,
        "hands_on_edits": hands, "bash": tools.get("Bash", 0) + tools.get("PowerShell", 0),
        "top": tools.most_common(6), "spawn_samples": spawn_prompts[:5],
    }

for lane, r in report.items():
    print(f"\n== {lane} ==  sessions(4d): {r['files']}  tool_calls: {r['tool_calls']}")
    print(f"   SUB-AGENT SPAWNS: {r['spawns']}   hands-on edits: {r['hands_on_edits']}   shell: {r['bash']}")
    print(f"   top tools: {r['top']}")
    for s in r["spawn_samples"]:
        print(f"   spawn: {s}")
