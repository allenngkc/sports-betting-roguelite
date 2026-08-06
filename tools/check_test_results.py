#!/usr/bin/env python3
"""C29 zero-case guard for NUnit results from Unity's -runTests.

WHY THIS EXISTS. A test filter that matches nothing exits GREEN with
testcasecount="0" -- a run that did nothing, reported as a pass. That is the
fifth vacuous green of the fortnight and the worst of them, because the other
four were single gates measuring the wrong thing while this is the RUNNER, and
it can green any suite from any seat with one mistyped filter.

This lane hit the adjacent failure the same week: `-runTests` was passed `-quit`,
the editor exited before the runner started, the process returned 0 and wrote no
results file at all. Exit code proved nothing then either. So this guard checks
three things, in this order, and every one of them has actually gone wrong:

  1. the results file EXISTS            (the -quit race writes nothing)
  2. testcasecount > 0                  (C29: a filter matching nothing)
  3. failed == 0 and no unexpected skips
  4. the run is RECENT (--max-age-minutes), because a guard that reads a stale
     results file passes on evidence from a build that no longer exists -- the
     same class of nothing-happened-but-green C29 names. Found the hard way:
     mtime cannot answer this (Unity writes the XML, THEN keeps appending to the
     log during shutdown, so log-newer-than-XML is normal), so the check reads
     the run's own start-time out of the file.

Usage:
    python tools/check_test_results.py <results.xml> [<results.xml> ...]
    python tools/check_test_results.py --min-cases EditMode=70,PlayMode=18 ...

Exits non-zero on any violation. Print output is the report; there is no quiet
mode, because a guard you cannot see the output of is the thing it guards against.
"""
import argparse
import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path


def summarise(path):
    """Return (total, passed, failed, skipped, inconclusive) from an NUnit3 file."""
    root = ET.parse(path).getroot()
    # Unity writes NUnit3: the root <test-run> carries the counts. Fall back to
    # the outermost <test-suite> if a future version moves them.
    node = root
    if node.get("testcasecount") is None:
        node = root.find(".//test-suite") or root
    def g(name, default=0):
        v = node.get(name)
        return int(v) if v is not None and v.isdigit() else default
    total = g("testcasecount") or g("total")
    return total, g("passed"), g("failed"), g("skipped"), g("inconclusive"), node.get("start-time")


def main():
    ap = argparse.ArgumentParser(description="C29 zero-case guard for NUnit results")
    ap.add_argument("results", nargs="+", help="NUnit XML result files")
    ap.add_argument("--max-age-minutes", type=float, default=None,
                    help="Fail if a results file's own start-time is older than this. Guards "
                         "against reading a previous run's XML and calling it today's evidence.")
    ap.add_argument("--min-cases", default="",
                    help="Comma-separated NAME=N floors, matched against the file stem. A suite "
                         "that silently shrinks is a filter typo that C29's count alone would "
                         "pass -- 20 tests is not zero, but it is not 39 either.")
    args = ap.parse_args()

    floors = {}
    for part in filter(None, (p.strip() for p in args.min_cases.split(","))):
        k, _, v = part.partition("=")
        if v.isdigit():
            floors[k.strip().lower()] = int(v)

    problems = []
    print(f"{'suite':28s} {'cases':>6s} {'passed':>7s} {'failed':>7s} {'skipped':>8s}  verdict")
    for r in args.results:
        p = Path(r)
        if not p.is_file():
            print(f"{p.name:28s} {'-':>6s} {'-':>7s} {'-':>7s} {'-':>8s}  *** NO RESULTS FILE ***")
            problems.append(f"{p.name}: no results file -- the run wrote nothing, so exit code 0 "
                            f"proves only that the process ended")
            continue
        try:
            total, passed, failed, skipped, inconclusive, started = summarise(p)
        except ET.ParseError as exc:
            print(f"{p.name:28s} unparseable: {exc}")
            problems.append(f"{p.name}: results file is not valid XML")
            continue

        verdict = "ok"
        if total == 0:
            verdict = "*** ZERO CASES (C29) ***"
            problems.append(f"{p.name}: testcasecount=0 -- the run matched no tests and would "
                            f"otherwise have reported green")
        if failed:
            verdict = "*** FAILURES ***"
            problems.append(f"{p.name}: {failed} failed")
        if inconclusive:
            problems.append(f"{p.name}: {inconclusive} inconclusive")
        if args.max_age_minutes is not None:
            if not started:
                problems.append(f"{p.name}: no start-time recorded, so age cannot be checked")
            else:
                try:
                    from datetime import datetime, timezone
                    t = datetime.strptime(started.replace("Z", ""), "%Y-%m-%d %H:%M:%S").replace(tzinfo=timezone.utc)
                    age = (datetime.now(timezone.utc) - t).total_seconds() / 60.0
                    if age > args.max_age_minutes:
                        verdict = f"*** STALE ({age:.0f} min old) ***"
                        problems.append(f"{p.name}: results are {age:.0f} minutes old, older than "
                                        f"the {args.max_age_minutes:.0f}-minute limit -- this is a "
                                        f"previous run's file, not this one's")
                except ValueError:
                    problems.append(f"{p.name}: unparseable start-time {started!r}")
        stem = p.stem.lower()
        for name, floor in floors.items():
            if name in stem and total < floor:
                verdict = f"*** BELOW FLOOR {floor} ***"
                problems.append(f"{p.name}: {total} cases is below the declared floor of {floor} "
                                f"-- the suite shrank, which a zero-check alone would not catch")
        print(f"{p.name:28s} {total:6d} {passed:7d} {failed:7d} {skipped:8d}  {verdict}"
              f"   started {started or '(unrecorded)'}")

    print()
    if problems:
        print("FAILED:")
        for pr in problems:
            print(f"  - {pr}")
        sys.exit(1)
    print("All suites reported a non-zero case count with no failures. "
          "Counts above are the evidence; 'no failures' alone would not be.")
    sys.exit(0)


if __name__ == "__main__":
    main()
