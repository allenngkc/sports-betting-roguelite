#!/usr/bin/env python3
"""room_gate_check.py -- Room acceptance-gate checker (no Unity editor required).

Re-runs every gate of the room-art acceptance checklist that can be verified
from files already on disk (the Room.unity scene text plus the three capture
PNGs), so a scarce shared Unity editor lease only has to be spent on
build/bake/capture, never on manual measurement.

Usage:
    python tools/room_gate_check.py --scene <path to Room.unity> --captures <dir> \
        [--reference <dir>] [--write-reference]

    --scene            Path to Assets/Scenes/Room.unity
    --captures         Directory containing standing-overview.png,
                       seated-tv-couch.png, focused-laptop-desk.png
    --reference        Directory of "before" captures, for the R9-B
                       before/after region-luminance comparison. Optional --
                       R9-B reports SKIP without it.
    --write-reference  Write tools/room_gate_reference.json from --scene's
                       current BoxCollider dimensions instead of comparing
                       against it (Gate 4 reports SKIP for this run).

Exit code 0 if every non-SKIP gate passes, 1 otherwise.
"""

import argparse
import json
import re
import sys
from pathlib import Path

try:
    from PIL import Image
except ImportError:
    print("ERROR: Pillow is required (pip install pillow). numpy is not used.", file=sys.stderr)
    sys.exit(1)

# ---------------------------------------------------------------------------
# Expected values (the ratified acceptance spec for this room).
# ---------------------------------------------------------------------------

EXPECTED_SINGLETONS = ["RoomArtRoot", "RoomArtGenerated", "RoomPostFx", "AdaptiveProbeVolume"]
EXPECTED_LIGHT_COUNT = 8
EXPECTED_DRESSING_COUNT = 6
EXPECTED_COLLIDER_COUNT = 27

CAPTURE_NAMES = ["standing-overview.png", "seated-tv-couch.png", "focused-laptop-desk.png"]
CAPTURE_SIZE = (2560, 1440)

R9A_IMAGE = "standing-overview.png"
R9A_BOX = (1480, 670, 1790, 740)          # bunk 2 mattress
R9A_EXPECTED_MEAN = 43.9
R9A_TOLERANCE = 1.0

R9B_IMAGE = "standing-overview.png"

# EVERY REGION MUST BE A SINGLE SURFACE. A box straddling two surfaces makes the
# 10% test meaningless in both directions: a bright outlier (a fixture, the window)
# dominates the mean, so a real change in the surface under test gets diluted below
# the threshold, while a trivial shift in the bright element swings the mean past it.
#
# The first two boxes here originally did exactly that, carried over from ad-hoc
# hand measurement. Measured sd/mean on the R6 reference frame:
#
#   ceiling            (1120,0,2400,480)  sd/mean 0.374  <- contained the fixture
#   far wall by window ( 780,580,950,770) sd/mean 0.406  <- contained the window
#
# Replaced with surface-pure boxes at 0.080 and 0.109. Keep that ratio under ~0.15
# if these ever move; anything above it is measuring composition, not brightness.
R9B_REGIONS = {
    # Ceiling plaster away from the tube's pool, so it is indirect-dominated - which
    # is where an ambient change shows up first and largest.
    "ceiling plaster":    (700,    60, 1150,  300),
    "right wall plaster": (2150,  200, 2450,  560),
    # Far wall below the window and clear of the radiator.
    "far wall plaster":   (640,   850,  800, 1000),
    "floor aisle":        (1090, 1300, 1470, 1420),
    "couch (dark)":       (150,   800,  760, 1140),
    "whole frame":        (0,       0, 2560, 1440),
}
R9B_TOLERANCE_PCT = 10.0

# Gate 4's reference is a fixed file next to this script, not a CLI arg --
# the CLI's --reference flag is for the R9-B capture-image comparison.
REFERENCE_JSON_PATH = Path(__file__).resolve().parent / "room_gate_reference.json"

# Tolerance for comparing collider float dimensions read back out of text.
# Same scene file re-parsed twice will match exactly; this just guards
# against harmless text formatting differences (e.g. trailing zeros).
DIMENSION_TOLERANCE = 1e-4


# ---------------------------------------------------------------------------
# Scene text parsing.
#
# Room.unity is Unity's YAML-ish scene format: custom tags like
# `--- !u!114 &12345` and duplicate anchors make it reject standard YAML
# parsers (PyYAML included) without a bespoke Loader. We only need a handful
# of scalar fields, so plain text scanning is simpler and more robust than
# fighting a YAML library into accepting Unity's dialect.
# ---------------------------------------------------------------------------

DOC_HEADER_RE = re.compile(r'^--- !u!(?P<type>\d+) &(?P<anchor>-?\d+)\s*$', re.M)
CLASS_NAME_RE = re.compile(r'\A\s*([A-Za-z_][A-Za-z0-9_]*):')
NAME_FIELD_RE = re.compile(r'^\s*m_Name:\s*(.*?)\s*$', re.M)
PREFAB_NAME_OVERRIDE_RE = re.compile(r'propertyPath:\s*m_Name\s*\n\s*value:\s*(.*?)\s*\n')
SIZE_RE = re.compile(r'm_Size:\s*\{x:\s*(-?[\d.eE+-]+),\s*y:\s*(-?[\d.eE+-]+),\s*z:\s*(-?[\d.eE+-]+)\}')
CENTER_RE = re.compile(r'm_Center:\s*\{x:\s*(-?[\d.eE+-]+),\s*y:\s*(-?[\d.eE+-]+),\s*z:\s*(-?[\d.eE+-]+)\}')
DANGLING_MESH_RE = re.compile(r'm_Mesh:\s*\{fileID:\s*0\}')


def split_documents(text):
    """Split a Unity scene file into (class_id, class_name, body) tuples.

    Each document starts with `--- !u!<classID> &<anchor>` and the following
    line names the class, e.g. `GameObject:`. The body runs until the next
    `---` marker (or EOF).
    """
    headers = list(DOC_HEADER_RE.finditer(text))
    docs = []
    for i, m in enumerate(headers):
        start = m.end()
        end = headers[i + 1].start() if i + 1 < len(headers) else len(text)
        body = text[start:end]
        class_match = CLASS_NAME_RE.match(body)
        class_name = class_match.group(1) if class_match else ""
        docs.append((m.group("type"), class_name, body))
    return docs


def load_scene_text(scene_path):
    if not scene_path.is_file():
        raise FileNotFoundError(f"scene file not found: {scene_path}")
    return scene_path.read_text(encoding="utf-8", errors="replace")


def gate2_object_counts(docs):
    """Gate 2: singleton names, Light count, Dressing_* count.

    GameObject names normally live as `m_Name: <name>` inside a `GameObject:`
    document. But a GameObject that is the (renamed) root of a
    PrefabInstance -- RoomArtRoot, in this scene -- has NO `GameObject:`
    document of its own at all; its name lives only in the PrefabInstance's
    override list as a `propertyPath: m_Name` / `value: <name>` pair. Both
    sources must be counted or RoomArtRoot silently reads as absent.
    """
    name_counts = {}

    for _class_id, class_name, body in docs:
        if class_name == "GameObject":
            m = NAME_FIELD_RE.search(body)
            if m:
                name = m.group(1)
                name_counts[name] = name_counts.get(name, 0) + 1
        elif class_name == "PrefabInstance":
            for pm in PREFAB_NAME_OVERRIDE_RE.finditer(body):
                name = pm.group(1)
                if name:
                    name_counts[name] = name_counts.get(name, 0) + 1

    light_count = sum(1 for _cid, cname, _b in docs if cname == "Light")
    dressing_count = sum(count for name, count in name_counts.items() if name.startswith("Dressing_"))

    return name_counts, light_count, dressing_count


def gate3_collider_docs(docs):
    """Gate 3: BoxCollider documents. Cross-check class-id 65 against the
    literal `BoxCollider:` class line -- if these ever disagree, the class-id
    table or the tag name changed and the count should not be trusted."""
    by_id = [b for cid, cname, b in docs if cid == "65"]
    by_name = [b for cid, cname, b in docs if cname == "BoxCollider"]
    if len(by_id) != len(by_name):
        raise AssertionError(
            f"BoxCollider detection disagreement: {len(by_id)} by class-id 65 "
            f"vs {len(by_name)} by 'BoxCollider:' tag -- scene format may have changed"
        )
    return by_name


def gate4_collider_dimensions(collider_bodies):
    """Extract each BoxCollider's (size, center) as a canonical, sortable tuple."""
    dims = []
    for body in collider_bodies:
        size_m = SIZE_RE.search(body)
        center_m = CENTER_RE.search(body)
        if not size_m or not center_m:
            raise ValueError("a BoxCollider document is missing m_Size or m_Center")
        size = tuple(round(float(v), 6) for v in size_m.groups())
        center = tuple(round(float(v), 6) for v in center_m.groups())
        dims.append(size + center)
    return sorted(dims)


def gate5_dangling_mesh_refs(text):
    return len(DANGLING_MESH_RE.findall(text))


# ---------------------------------------------------------------------------
# Capture (PNG) checks.
# ---------------------------------------------------------------------------

def load_capture(captures_dir, filename):
    path = captures_dir / filename
    if not path.is_file():
        raise FileNotFoundError(f"capture image not found: {path}")
    img = Image.open(path).convert("RGB")
    if img.size != CAPTURE_SIZE:
        raise ValueError(f"{path} is {img.size}, expected {CAPTURE_SIZE}")
    return img


def region_mean_luminance(img, box):
    """Mean Rec.709 luminance over a box, sampling every 2nd pixel on both
    axes (a 2x2 grid stride, i.e. 1/4 of the pixels) for speed. No numpy:
    plain Python over PIL's pixel access is fast enough for a handful of
    small regions plus one whole-frame region.

    THE STRIDE IS PART OF THE GATE, NOT AN IMPLEMENTATION DETAIL. Sampling a
    flattened pixel list with stride 2 instead of striding both axes gives
    43.58 on the bunk-2 region where this gives 43.97 - a 0.39 spread against
    a +/-1.0 acceptance band, so a third of the tolerance would be decided by
    how the loop happens to be written. Do not "optimise" this to a flat
    stride, and re-baseline every reference if it ever changes.
    """
    crop = img.crop(box)
    w, h = crop.size
    px = crop.load()
    total = 0.0
    count = 0
    for y in range(0, h, 2):
        for x in range(0, w, 2):
            r, g, b = px[x, y]
            total += 0.2126 * r + 0.7152 * g + 0.0722 * b
            count += 1
    if count == 0:
        raise ValueError(f"region {box} is empty after cropping")
    return total / count


# ---------------------------------------------------------------------------
# Result plumbing + table printing.
# ---------------------------------------------------------------------------

class GateResult:
    def __init__(self, gate, check, status, expected, observed, detail=None):
        self.gate = gate
        self.check = check
        self.status = status  # "PASS" | "FAIL" | "SKIP"
        self.expected = expected
        self.observed = observed
        self.detail = detail or []


def skip(gate, check, reason):
    return GateResult(gate, check, "SKIP", "-", reason)


def print_table(results):
    headers = ["GATE", "CHECK", "STATUS", "EXPECTED", "OBSERVED"]
    rows = [[r.gate, r.check, r.status, r.expected, r.observed] for r in results]
    widths = [max(len(str(h)), *(len(str(row[i])) for row in rows)) for i, h in enumerate(headers)]

    def fmt_row(cols):
        return "  ".join(str(c).ljust(widths[i]) for i, c in enumerate(cols))

    print(fmt_row(headers))
    print("  ".join("-" * w for w in widths))
    for r, row in zip(results, rows):
        print(fmt_row(row))
        for line in r.detail:
            print("       " + line)


# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main():
    parser = argparse.ArgumentParser(
        description="Room acceptance-gate checker -- runs every gate that doesn't need the Unity editor."
    )
    parser.add_argument("--scene", required=True, help="Path to Room.unity")
    parser.add_argument("--captures", required=True, help="Directory containing the three capture PNGs")
    parser.add_argument("--reference", help="Directory of 'before' captures, for the R9-B before/after check")
    parser.add_argument(
        "--write-reference",
        action="store_true",
        help="Write tools/room_gate_reference.json from --scene's current collider dimensions "
             "instead of checking Gate 4 against it",
    )
    args = parser.parse_args()

    scene_path = Path(args.scene)
    captures_dir = Path(args.captures)
    reference_captures_dir = Path(args.reference) if args.reference else None

    results = []

    try:
        scene_text = load_scene_text(scene_path)
        docs = split_documents(scene_text)

        # --- Gate 1: pre/post capture diff -- needs two Unity runs ---------
        results.append(skip(1, "pre/post capture diff", "requires two separate Unity editor captures (before and after the change); nothing to compare from a single scene/capture set"))

        # --- Gate 2: object counts -----------------------------------------
        name_counts, light_count, dressing_count = gate2_object_counts(docs)
        detail = []
        gate2_ok = True
        for name in EXPECTED_SINGLETONS:
            n = name_counts.get(name, 0)
            ok = n == 1
            gate2_ok &= ok
            detail.append(f"{name}: expected 1, observed {n} [{'ok' if ok else 'MISMATCH'}]")
        light_ok = light_count == EXPECTED_LIGHT_COUNT
        gate2_ok &= light_ok
        detail.append(f"Light components: expected {EXPECTED_LIGHT_COUNT}, observed {light_count} [{'ok' if light_ok else 'MISMATCH'}]")
        dressing_ok = dressing_count == EXPECTED_DRESSING_COUNT
        gate2_ok &= dressing_ok
        detail.append(f"Dressing_* objects: expected {EXPECTED_DRESSING_COUNT}, observed {dressing_count} [{'ok' if dressing_ok else 'MISMATCH'}]")
        results.append(GateResult(
            2, "object counts", "PASS" if gate2_ok else "FAIL",
            "1 each singleton, 8 Light, 6 Dressing_*",
            f"{sum(1 for n in EXPECTED_SINGLETONS if name_counts.get(n,0)==1)}/4 singletons, {light_count} Light, {dressing_count} Dressing_*",
            detail,
        ))

        # --- Gate 3: collider count -----------------------------------------
        collider_bodies = gate3_collider_docs(docs)
        collider_count = len(collider_bodies)
        results.append(GateResult(
            3, "collider count",
            "PASS" if collider_count == EXPECTED_COLLIDER_COUNT else "FAIL",
            str(EXPECTED_COLLIDER_COUNT), str(collider_count),
        ))

        # --- Gate 4: collision dimensions unchanged -------------------------
        current_dims = gate4_collider_dimensions(collider_bodies)
        if args.write_reference:
            REFERENCE_JSON_PATH.parent.mkdir(parents=True, exist_ok=True)
            payload = {
                "generated_from": str(scene_path),
                "collider_count": len(current_dims),
                "colliders": [
                    {"size": list(d[0:3]), "center": list(d[3:6])} for d in current_dims
                ],
            }
            REFERENCE_JSON_PATH.write_text(json.dumps(payload, indent=2) + "\n", encoding="utf-8")
            results.append(skip(4, "collision dims unchanged", f"--write-reference given; wrote {REFERENCE_JSON_PATH}"))
        else:
            if not REFERENCE_JSON_PATH.is_file():
                raise FileNotFoundError(
                    f"reference file not found: {REFERENCE_JSON_PATH} "
                    "(run once with --write-reference to create it)"
                )
            ref_payload = json.loads(REFERENCE_JSON_PATH.read_text(encoding="utf-8"))
            ref_dims = sorted(
                tuple(round(v, 6) for v in (c["size"] + c["center"]))
                for c in ref_payload["colliders"]
            )
            mismatches = []
            if len(ref_dims) != len(current_dims):
                mismatches.append(f"count differs: reference {len(ref_dims)} vs current {len(current_dims)}")
            else:
                for i, (ref, cur) in enumerate(zip(ref_dims, current_dims)):
                    if any(abs(a - b) > DIMENSION_TOLERANCE for a, b in zip(ref, cur)):
                        mismatches.append(f"#{i}: reference size/center {ref} vs current {cur}")
            gate4_ok = not mismatches
            results.append(GateResult(
                4, "collision dims unchanged",
                "PASS" if gate4_ok else "FAIL",
                f"matches {REFERENCE_JSON_PATH.name} ({len(ref_dims)} colliders)",
                f"{len(current_dims)} colliders, {len(mismatches)} mismatch(es)",
                mismatches[:10],
            ))

        # --- Gate 5: no dangling mesh references ----------------------------
        dangling = gate5_dangling_mesh_refs(scene_text)
        results.append(GateResult(
            5, "dangling mesh refs",
            "PASS" if dangling == 0 else "FAIL",
            "0", str(dangling),
        ))

        # --- Gates 6, 7, 8: not automatable / out of scope by agreement -----
        results.append(skip(6, "UI/HUD readability", "requires a human reading rendered UI text; not checkable from files on disk"))
        results.append(skip(7, "UI/HUD contrast", "requires a human reading rendered UI text; not checkable from files on disk"))
        results.append(skip(8, "structural-only check", "out of scope for this tool by prior agreement"))

        # --- R9-A: bunk 2 mattress luminance ---------------------------------
        current_img = load_capture(captures_dir, R9A_IMAGE)
        r9a_mean = region_mean_luminance(current_img, R9A_BOX)
        r9a_ok = abs(r9a_mean - R9A_EXPECTED_MEAN) <= R9A_TOLERANCE
        results.append(GateResult(
            "R9-A", "bunk 2 mattress luminance",
            "PASS" if r9a_ok else "FAIL",
            f"{R9A_EXPECTED_MEAN} +/- {R9A_TOLERANCE}",
            f"{r9a_mean:.2f}",
        ))

        # --- R9-B: region means within 10% of reference ----------------------
        if reference_captures_dir is None:
            results.append(skip("R9-B", "region means vs reference", "no --reference given"))
        else:
            ref_img = load_capture(reference_captures_dir, R9B_IMAGE)
            cur_img = load_capture(captures_dir, R9B_IMAGE)
            detail = []
            all_ok = True
            for name, box in R9B_REGIONS.items():
                ref_mean = region_mean_luminance(ref_img, box)
                cur_mean = region_mean_luminance(cur_img, box)
                pct_change = (cur_mean - ref_mean) / ref_mean * 100.0 if ref_mean != 0 else float("inf")
                ok = abs(pct_change) <= R9B_TOLERANCE_PCT
                all_ok &= ok
                detail.append(
                    f"{name:20s} before={ref_mean:7.2f} after={cur_mean:7.2f} "
                    f"change={pct_change:+6.2f}% [{'ok' if ok else 'FAIL'}]"
                )
            results.append(GateResult(
                "R9-B", "region means vs reference",
                "PASS" if all_ok else "FAIL",
                f"within +/-{R9B_TOLERANCE_PCT:.0f}% of reference",
                "see detail below",
                detail,
            ))

    except (FileNotFoundError, ValueError, AssertionError, KeyError, json.JSONDecodeError) as exc:
        print(f"ERROR: {exc}", file=sys.stderr)
        sys.exit(1)

    print_table(results)

    exit_code = 0 if all(r.status in ("PASS", "SKIP") for r in results) else 1
    sys.exit(exit_code)


if __name__ == "__main__":
    main()
