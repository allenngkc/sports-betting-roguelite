#!/usr/bin/env python3
"""C29 — a test run reports its case count, and zero cases is a failure.

    Every test invocation reports its executed case count, and a run with zero
    executed cases exits non-zero. No verdict, gate, grant or Design-verified
    claim rests on a run that did not state how many tests it ran.
                                                    -- C29, DD 2026-08-05

WHY THIS EXISTS. A mistyped `-testFilter` matches nothing and Unity exits **0** with
`testcasecount="0"`. The suite looks green. It ran nothing. That is the fifth vacuous green of the
fortnight and the worst of them: the other four were single gates measuring the wrong thing, while
this is the *runner*, and it can green any suite from any seat with one typo.

Concretely, the invocation that cost a window:

    -testFilter "Capture_SeatedSweat_NamedMoments(48151623)"   -> 0 cases, exit 0, "Passed"
    -testFilter ".*48151623.*"                                 -> 1 case,  runs

Both look identical in a terminal. Only the case count tells them apart.

USAGE
    python tools/assert_test_run.py <results.xml> [--min N] [--expect-passed N] [--quiet]

    --min N            require at least N executed cases (default 1)
    --expect-passed N  additionally require exactly N passed
    --quiet            print only on failure

EXIT CODES
    0  the run executed >= --min cases and any --expect-passed held
    1  zero cases, too few cases, or expectations unmet   <- C29's failure
    2  the results file is missing or unparseable         <- also a failure: a run that
                                                             produced no report is a run
                                                             that reported nothing
"""
import argparse
import os
import sys
import xml.etree.ElementTree as ET


def main() -> int:
    ap = argparse.ArgumentParser(description="C29 zero-case guard for NUnit/Unity test results.")
    ap.add_argument("results", help="path to the NUnit XML written by -testResults")
    ap.add_argument("--min", type=int, default=1, help="minimum executed cases (default 1)")
    ap.add_argument("--expect-passed", type=int, default=None, help="require exactly N passed")
    ap.add_argument("--quiet", action="store_true", help="print only on failure")
    a = ap.parse_args()

    # A missing report is not "no news". A run whose report never appeared did not
    # demonstrate anything, and treating that as neutral is the same mistake one level up.
    if not os.path.exists(a.results):
        print(f"C29 FAIL: results file not found: {a.results}", file=sys.stderr)
        print("         a run that produced no report reported nothing.", file=sys.stderr)
        return 2
    try:
        root = ET.parse(a.results).getroot()
    except ET.ParseError as e:
        print(f"C29 FAIL: results file is not parseable XML: {a.results}\n         {e}", file=sys.stderr)
        return 2

    def num(attr: str) -> int:
        v = root.get(attr)
        try:
            return int(v)
        except (TypeError, ValueError):
            return 0

    # `total` is what NUnit reports as executed. `testcasecount` is what it DISCOVERED, which can be
    # non-zero while nothing ran — precisely the case this guard exists for, so the count that
    # decides is `total`, and both are printed so the difference is never invisible.
    total = num("total")
    discovered = num("testcasecount")
    passed, failed = num("passed"), num("failed")
    skipped, inconclusive = num("skipped"), num("inconclusive")

    if not a.quiet:
        print(f"C29: executed {total} (discovered {discovered}) | "
              f"passed {passed} failed {failed} skipped {skipped} inconclusive {inconclusive}")

    if total < a.min:
        print(f"C29 FAIL: {total} executed cases, required >= {a.min}.", file=sys.stderr)
        if discovered > total:
            print(f"         {discovered} discovered but {total} executed — the filter matched "
                  f"nothing that ran.", file=sys.stderr)
        print("         A run that executed nothing is NOT a pass. Check the -testFilter: a "
              "parameterised\n         name needs its quotes, or use the regex form "
              '(".*<seed>.*").', file=sys.stderr)
        return 1

    if a.expect_passed is not None and passed != a.expect_passed:
        print(f"C29 FAIL: expected exactly {a.expect_passed} passed, got {passed}.", file=sys.stderr)
        return 1

    return 0


if __name__ == "__main__":
    sys.exit(main())
