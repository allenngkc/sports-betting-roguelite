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

Exit code 0 unless some gate FAILs (SKIP, VOID and INFO gates never fail the
run), 1 otherwise.
"""

import argparse
import json
import math
import re
import sys
from collections import Counter
from datetime import datetime
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
EXPECTED_LIGHT_COUNT = 8   # 6 Mixed + 2 Realtime (the R10 baked bounce was measured and removed)
EXPECTED_DRESSING_COUNT = 6

# Unity class IDs for the collider types Gate 3 inventories, plus the
# CharacterController, which is detected and reported separately because it
# is the player, not room collision, and must never be folded into the room
# total (see gate3_collider_inventory).
COLLIDER_CLASS_IDS = {
    "64": "MeshCollider",
    "65": "BoxCollider",
    "135": "SphereCollider",
    "136": "CapsuleCollider",
}
CHARACTER_CONTROLLER_CLASS_ID = "143"
CHARACTER_CONTROLLER_CLASS_NAME = "CharacterController"

# R16 / C18: the room's true collider inventory, declared as DATA so Gate 3
# diffs against named, explicit expected members instead of a bare count --
# "an inventory names its members, a gate names the build it certifies."
# LaptopScreen and PhoneScreen keep MeshColliders because interaction
# raycasting is not re-plumbed to satisfy a number. TVScreen and WindowPane's
# MeshColliders are being removed in a parallel change and are deliberately
# NOT in this expected set -- until that lands, Gate 3 must FAIL and name
# them, which is the proof the gate can see them at all.
EXPECTED_COLLIDER_INVENTORY = {
    "BoxCollider": {"total": 27, "solid": 24, "trigger": 3},
    "MeshCollider": {"total": 2, "owners": {"LaptopScreen", "PhoneScreen"}},
    "SphereCollider": {"total": 0},
    "CapsuleCollider": {"total": 0},
}
EXPECTED_TOTAL_ROOM_COLLIDERS = 29  # 27 BoxCollider + 2 MeshCollider

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

# ---------------------------------------------------------------------------
# R23 -- the screens-dark conformance instrument for law §1.1.
#
# §1.1 says a blue-tinted room is the explicit failure mode. It spent months
# reporting ITSELF as failing because it was being judged on gameplay frames --
# frames containing three emissive screens and a green TV light. Those cannot
# separate "the room is cool" from "the screens are cool", so the law was
# unfalsifiable on its own evidence.
#
# R23 fixed that by ruling a purpose-built set: screens dark, the room's own rig,
# its own grade, and wall/floor/bunk reported as MEASURED mean chroma and hue
# angle rather than eyeballed. This is that measurement.
#
# Luminance cannot answer this question at all -- a warm room and a cool room can
# share a mean. Chroma and hue are the only instruments that can, which is why
# R23 named them specifically.
R23_IMAGE = "conformance-room-screens-dark.png"
# R26: the grade-bypassed pass is a ruled half of the set, not a diagnostic extra, and is
# named as its twin so the pair reads as one isolation rather than two pictures.
R23_UNGRADED_IMAGE = "conformance-room-screens-dark-UNGRADED.png"

# The ruling names wall, floor and bunk. Boxes are the surface-pure ones already
# validated for R9-B, so the two measurements are directly comparable.
R23_REGIONS = {
    "wall (right plaster)":  (2150,  200, 2450,  560),
    "wall (far plaster)":    (640,   850,  800, 1000),
    "floor (aisle)":         (1090, 1300, 1470, 1420),
    "bunk (1 / couch side)": (150,   800,  760, 1140),
    "bunk (2 mattress)":     (1480,  670, 1790,  740),
    "ceiling plaster":       (700,    60, 1150,  300),
}

# CIELAB hue angle bands. Warm runs red through yellow; the failure mode §1.1
# names is the blue quadrant. Anything between is reported as neutral rather
# than forced into a verdict it does not support.
R23_WARM_HUE = (20.0, 110.0)
R23_COOL_HUE = (200.0, 300.0)

# Below this chroma a hue angle is not meaningful -- it is the direction of a
# vector too short to trust, and calling a near-grey surface "cool" on the
# strength of a 0.4 chroma reading would be measuring noise.
R23_CHROMA_FLOOR = 1.5

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
GAMEOBJECT_REF_RE = re.compile(r'm_GameObject:\s*\{fileID:\s*(-?\d+)\}')
IS_TRIGGER_RE = re.compile(r'm_IsTrigger:\s*(\d+)')


def split_documents(text):
    """Split a Unity scene file into (class_id, anchor, class_name, body) tuples.

    Each document starts with `--- !u!<classID> &<anchor>` and the following
    line names the class, e.g. `GameObject:`. The body runs until the next
    `---` marker (or EOF). The anchor is kept (not just class id/name)
    because Gate 3 must resolve each collider to its owning GameObject by
    following `m_GameObject: {fileID: N}` to the document anchored at N.
    """
    headers = list(DOC_HEADER_RE.finditer(text))
    docs = []
    for i, m in enumerate(headers):
        start = m.end()
        end = headers[i + 1].start() if i + 1 < len(headers) else len(text)
        body = text[start:end]
        class_match = CLASS_NAME_RE.match(body)
        class_name = class_match.group(1) if class_match else ""
        docs.append((m.group("type"), m.group("anchor"), class_name, body))
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

    for _class_id, _anchor, class_name, body in docs:
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

    light_count = sum(1 for _cid, _anchor, cname, _b in docs if cname == "Light")
    dressing_count = sum(count for name, count in name_counts.items() if name.startswith("Dressing_"))

    return name_counts, light_count, dressing_count


def collect_collider_docs(docs):
    """Group every collider document by Unity class id (Box/Mesh/Sphere/
    Capsule), plus the CharacterController (143) kept in its own bucket
    since it is the player, not room collision, and must never be folded
    into the room total.

    Cross-checks each class-id against its literal class-name tag (e.g. 65
    vs `BoxCollider:`) the same way the old single-type gate did -- if these
    ever disagree for any type, the class-id table or a tag name changed and
    nothing downstream should be trusted.
    """
    by_class_id = {cid: [] for cid in COLLIDER_CLASS_IDS}
    controller_docs = []
    for cid, anchor, _cname, body in docs:
        if cid in by_class_id:
            by_class_id[cid].append((anchor, body))
        elif cid == CHARACTER_CONTROLLER_CLASS_ID:
            controller_docs.append((anchor, body))

    for cid, expected_name in COLLIDER_CLASS_IDS.items():
        by_name_count = sum(1 for _c, _a, cname, _b in docs if cname == expected_name)
        if by_name_count != len(by_class_id[cid]):
            raise AssertionError(
                f"{expected_name} detection disagreement: {len(by_class_id[cid])} by class-id {cid} "
                f"vs {by_name_count} by '{expected_name}:' tag -- scene format may have changed"
            )

    cc_by_name_count = sum(1 for _c, _a, cname, _b in docs if cname == CHARACTER_CONTROLLER_CLASS_NAME)
    if cc_by_name_count != len(controller_docs):
        raise AssertionError(
            f"CharacterController detection disagreement: {len(controller_docs)} by class-id "
            f"{CHARACTER_CONTROLLER_CLASS_ID} vs {cc_by_name_count} by 'CharacterController:' tag"
        )

    return by_class_id, controller_docs


def resolve_owner_names(docs, anchored_bodies):
    """Resolve each (anchor, body) collider document to its owning
    GameObject's m_Name, by following `m_GameObject: {fileID: N}` to the
    GameObject document anchored at N.

    Returns a list of (collider_anchor, owner_name, owner_fileID, body).
    owner_name is None when no GameObject document with that anchor exists
    in the scene (e.g. a stripped prefab member) -- callers must surface
    that as a named mismatch, never drop it silently.
    """
    go_by_anchor = {
        anchor: body for _cid, anchor, cname, body in docs if cname == "GameObject"
    }

    out = []
    for anchor, body in anchored_bodies:
        gm = GAMEOBJECT_REF_RE.search(body)
        owner_id = gm.group(1) if gm else None
        owner_name = None
        if owner_id is not None and owner_id in go_by_anchor:
            nm = NAME_FIELD_RE.search(go_by_anchor[owner_id])
            owner_name = nm.group(1) if nm else None
        out.append((anchor, owner_name, owner_id, body))
    return out


def _owner_label(name, fileid):
    return name if name is not None else f"<unresolved fileID {fileid}>"


def gate3_collider_inventory(docs):
    """Gate 3 (C18): a NAMED inventory of every collider in the scene, not a
    bare count. Parses every collider class present, resolves each to its
    owning GameObject, reads solid vs trigger, and diffs the result against
    EXPECTED_COLLIDER_INVENTORY -- naming exactly what is unexpected and
    exactly what expected member is missing, never just "N vs M".

    Returns (GateResult, box_collider_bodies) -- the BoxCollider body list is
    handed back so Gate 4 (dimension comparison) can reuse it without a
    second parse of the scene.
    """
    by_class_id, controller_docs = collect_collider_docs(docs)

    box = resolve_owner_names(docs, by_class_id["65"])
    mesh = resolve_owner_names(docs, by_class_id["64"])
    sphere = resolve_owner_names(docs, by_class_id["135"])
    capsule = resolve_owner_names(docs, by_class_id["136"])
    controllers = resolve_owner_names(docs, controller_docs)

    box_solid, box_trigger = [], []
    for anchor, name, fileid, body in box:
        tm = IS_TRIGGER_RE.search(body)
        is_trigger = tm is not None and tm.group(1) != "0"
        (box_trigger if is_trigger else box_solid).append((anchor, _owner_label(name, fileid)))

    mesh_owners = [(anchor, _owner_label(name, fileid)) for anchor, name, fileid, _b in mesh]
    mesh_owner_names = {n for _a, n in mesh_owners}

    exp = EXPECTED_COLLIDER_INVENTORY
    expected_mesh_owners = exp["MeshCollider"]["owners"]

    problems = []

    box_total = len(box)
    if box_total != exp["BoxCollider"]["total"]:
        problems.append(f"BoxCollider total: expected {exp['BoxCollider']['total']}, observed {box_total}")
    if len(box_solid) != exp["BoxCollider"]["solid"]:
        problems.append(f"BoxCollider solid: expected {exp['BoxCollider']['solid']}, observed {len(box_solid)}")
    if len(box_trigger) != exp["BoxCollider"]["trigger"]:
        problems.append(f"BoxCollider trigger: expected {exp['BoxCollider']['trigger']}, observed {len(box_trigger)}")

    if len(mesh_owners) != exp["MeshCollider"]["total"]:
        problems.append(f"MeshCollider total: expected {exp['MeshCollider']['total']}, observed {len(mesh_owners)}")
    for anchor, name in sorted(mesh_owners, key=lambda t: t[1]):
        if name not in expected_mesh_owners:
            problems.append(f"unexpected MeshCollider on '{name}' (anchor {anchor})")
    for missing_name in sorted(expected_mesh_owners - mesh_owner_names):
        problems.append(f"missing expected MeshCollider on '{missing_name}'")

    for label, found, expected_count in (
        ("SphereCollider", sphere, exp["SphereCollider"]["total"]),
        ("CapsuleCollider", capsule, exp["CapsuleCollider"]["total"]),
    ):
        if len(found) != expected_count:
            problems.append(f"{label} total: expected {expected_count}, observed {len(found)}")
        for anchor, name, fileid, _b in found:
            problems.append(f"unexpected {label} on '{_owner_label(name, fileid)}' (anchor {anchor})")

    total_room_colliders = box_total + len(mesh_owners) + len(sphere) + len(capsule)
    if total_room_colliders != EXPECTED_TOTAL_ROOM_COLLIDERS:
        problems.append(
            f"total room colliders: expected {EXPECTED_TOTAL_ROOM_COLLIDERS}, observed {total_room_colliders}"
        )

    detail = list(problems)
    detail.append(
        f"inventory: BoxCollider {box_total} ({len(box_solid)} solid, {len(box_trigger)} trigger); "
        f"MeshCollider {len(mesh_owners)} on {sorted(mesh_owner_names) if mesh_owner_names else '-'}; "
        f"SphereCollider {len(sphere)}; CapsuleCollider {len(capsule)}"
    )
    if controllers:
        cc_names = ", ".join(_owner_label(name, fileid) for _a, name, fileid, _b in controllers)
        detail.append(
            f"CharacterController (player -- excluded from room inventory): "
            f"{len(controllers)} on {cc_names}"
        )

    expected_summary = (
        f"{EXPECTED_TOTAL_ROOM_COLLIDERS} room colliders = Box {exp['BoxCollider']['total']} "
        f"[{exp['BoxCollider']['solid']} solid/{exp['BoxCollider']['trigger']} trig] "
        f"+ Mesh {exp['MeshCollider']['total']} on {sorted(expected_mesh_owners)} "
        f"+ Sphere {exp['SphereCollider']['total']} + Capsule {exp['CapsuleCollider']['total']}"
    )
    observed_summary = (
        f"{total_room_colliders} room colliders = Box {box_total} "
        f"[{len(box_solid)} solid/{len(box_trigger)} trig] "
        f"+ Mesh {len(mesh_owners)} on {sorted(mesh_owner_names) if mesh_owner_names else []} "
        f"+ Sphere {len(sphere)} + Capsule {len(capsule)}"
    )

    result = GateResult(
        3, "collider inventory (named, C18)",
        "PASS" if not problems else "FAIL",
        expected_summary, observed_summary, detail,
    )
    box_bodies = [body for _a, _n, _f, body in box]
    return result, box_bodies


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


def region_relief_pct(img, box):
    """Relief% for a region: 100 * mean(|dL| at a 4px stride) / mean(L).

    THESE NUMBERS ARE NOT COMPARABLE TO THE RELIEF FIGURES IN THE PHASE DOCS.
    PHASE_A_FINDINGS.md and PHASE_B_INDIRECT_LIGHT.md quote 8.7% for the right
    wall and 2.3% for the couch; this function reports about 10.0% and 2.6% for
    the same pixels. Nothing regressed and nothing improved - those figures were
    measured at stride 3 over EVERY pixel, while this strides 4 over the 2-stride
    grid, and a coarser stride crosses more surface detail per sample. Both are
    self-consistent; only one of them is reproducible on demand, which is why the
    harness uses this one.

    Only ever read this metric as a RATIO between two captures measured by this
    same function. The absolute value is an artefact of the sampling, so do not
    compare it against a published figure and conclude anything changed.

    Mean luminance (region_mean_luminance above) cannot see fine surface
    detail -- a flat grey patch and a richly normal-mapped patch can share
    the same mean. Relief is the thing actually being judged, so measure it
    directly: take the absolute luminance difference between pixel pairs 4
    apart, both horizontally and vertically, average over every sampled
    pair, and divide by the region's mean luminance.

    SAMPLE GRID: region_mean_luminance strides both axes by 2 -- its
    docstring explains why that stride is part of the gate, not an
    implementation detail. This function's stride-4 pairs MUST land on that
    same 2-stride grid, or every lookup silently misses and the region
    reports 0.00 -- that is exactly the failure a previous attempt at this
    metric hit. 4 is a multiple of 2, so pairing grid point (x, y) with
    (x+4, y) and (x, y+4) always lands on a sample that grid already took;
    no separate/independent sample set is introduced here.

    Raises ValueError if a region yields zero valid stride-4 pairs, instead
    of silently returning 0.0 -- a 0.00 reading must mean "measured and
    genuinely flat", never "the sampling missed".
    """
    crop = img.crop(box)
    w, h = crop.size
    px = crop.load()

    # Luminance at every point on the same 2x2 grid region_mean_luminance
    # samples, keyed by local (x, y), so the mean and the stride-4 diffs
    # below are computed from identical samples.
    lum = {}
    for y in range(0, h, 2):
        for x in range(0, w, 2):
            r, g, b = px[x, y]
            lum[(x, y)] = 0.2126 * r + 0.7152 * g + 0.0722 * b

    if not lum:
        raise ValueError(f"region {box} is empty after cropping")
    mean_l = sum(lum.values()) / len(lum)

    diff_total = 0.0
    diff_count = 0
    for (x, y), l in lum.items():
        right = lum.get((x + 4, y))
        if right is not None:
            diff_total += abs(l - right)
            diff_count += 1
        down = lum.get((x, y + 4))
        if down is not None:
            diff_total += abs(l - down)
            diff_count += 1

    if diff_count == 0:
        raise ValueError(
            f"region {box} yielded zero stride-4 sample pairs -- grid/stride "
            "misalignment (see region_relief_pct docstring); refusing to "
            "silently report 0.0"
        )
    if mean_l == 0:
        raise ValueError(f"region {box} has zero mean luminance; relief% is undefined")

    mean_abs_dl = diff_total / diff_count
    return 100.0 * mean_abs_dl / mean_l


# ---------------------------------------------------------------------------
# Result plumbing + table printing.
# ---------------------------------------------------------------------------

class GateResult:
    def __init__(self, gate, check, status, expected, observed, detail=None):
        self.gate = gate
        self.check = check
        self.status = status  # "PASS" | "FAIL" | "SKIP" | "VOID" | "INFO"
        self.expected = expected
        self.observed = observed
        self.detail = detail or []


def region_cast(img, box, step=2):
    """Mean cast of a region as CIELAB (L*, chroma, hue angle in degrees).

    Averages in LINEAR light and converts the mean once, rather than converting
    per pixel and averaging afterwards. That ordering matters: hue is an angle,
    and averaging angles across a region is meaningless -- two surfaces at 10 deg
    and 350 deg average to 180, the opposite of both. Averaging the linear
    tristimulus first and taking the hue of the result is the only form of this
    measurement that means what it appears to mean.
    """
    crop = img.crop(box)
    w, h = crop.size
    px = crop.load()
    r = g = b = 0.0
    n = 0
    for y in range(0, h, step):
        for x in range(0, w, step):
            p = px[x, y]
            r += srgb_to_linear(p[0])
            g += srgb_to_linear(p[1])
            b += srgb_to_linear(p[2])
            n += 1
    if n == 0:
        raise ValueError(f"region {box} sampled no pixels")
    r, g, b = r / n, g / n, b / n

    # linear sRGB -> CIEXYZ (D65) -> CIELAB
    x = 0.4124564 * r + 0.3575761 * g + 0.1804375 * b
    y = 0.2126729 * r + 0.7151522 * g + 0.0721750 * b
    z = 0.0193339 * r + 0.1191920 * g + 0.9503041 * b
    xn, yn, zn = 0.95047, 1.00000, 1.08883

    def f(t):
        return t ** (1.0 / 3.0) if t > 216.0 / 24389.0 else (841.0 / 108.0) * t + 4.0 / 29.0

    fx, fy, fz = f(x / xn), f(y / yn), f(z / zn)
    lstar = 116.0 * fy - 16.0
    a_star = 500.0 * (fx - fy)
    b_star = 200.0 * (fy - fz)
    chroma = math.hypot(a_star, b_star)
    hue = math.degrees(math.atan2(b_star, a_star)) % 360.0
    return lstar, chroma, hue


def cast_verdict(chroma, hue):
    """WARM / COOL / neutral, refusing to call a hue it cannot support."""
    if chroma < R23_CHROMA_FLOOR:
        return "neutral"
    if R23_WARM_HUE[0] <= hue <= R23_WARM_HUE[1]:
        return "WARM"
    if R23_COOL_HUE[0] <= hue <= R23_COOL_HUE[1]:
        return "COOL"
    return "neutral"


def srgb_to_linear(c):
    """8-bit sRGB channel to linear. The room authors in linear and the design
    doc quotes sRGB hex, so every colour comparison in this project has to cross
    this boundary explicitly rather than by eye."""
    c = c / 255.0
    return c / 12.92 if c <= 0.04045 else ((c + 0.055) / 1.055) ** 2.4


def skip(gate, check, reason):
    return GateResult(gate, check, "SKIP", "-", reason)


def void(gate, check, reason):
    """A gate that has been explicitly ruled out by a design decision (R22),
    as distinct from SKIP (un-runnable by this tool). VOID must not count as
    a pass and must not silently disappear into SKIP's bucket -- it needs a
    human to re-verify it before it can be trusted again."""
    return GateResult(gate, check, "VOID", "-", reason)


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


def format_mtime(path):
    dt = datetime.fromtimestamp(path.stat().st_mtime).astimezone()
    return dt.strftime("%Y-%m-%d %H:%M:%S %Z")


def print_header(scene_path, captures_dir):
    """C18: the report must say what build it certifies -- a PASS must never
    be misreadable as covering geometry other than what this run actually
    parsed and captured. Prints the scene and capture paths plus their
    last-modified timestamps, and warns if the captures predate the scene
    (meaning the frames do not show the geometry just parsed, and every
    capture-derived result below -- R9-A, R9-B, R10 -- is stale)."""
    scene_mtime = scene_path.stat().st_mtime
    capture_path = captures_dir / CAPTURE_NAMES[0]

    print(f"Scene:    {scene_path}  (modified {format_mtime(scene_path)})")
    if capture_path.is_file():
        print(f"Captures: {captures_dir}  ({CAPTURE_NAMES[0]} modified {format_mtime(capture_path)})")
        if capture_path.stat().st_mtime < scene_mtime:
            print(
                f"WARNING: {CAPTURE_NAMES[0]} is OLDER than the scene -- these captures do not "
                "show the geometry this run just parsed. Every capture-derived result below "
                "(R9-A, R9-B, R10) is stale until the room is recaptured."
            )
    else:
        print(f"Captures: {captures_dir}  ({CAPTURE_NAMES[0]} not found)")
    print()


def print_summary(results, exit_code):
    counts = Counter(r.status for r in results)
    order = ["PASS", "FAIL", "SKIP", "VOID", "INFO"]
    parts = ", ".join(f"{counts[s]} {s}" for s in order if counts.get(s))
    print()
    print(f"Summary: {parts} -- exit code {exit_code}")


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
        "--conformance",
        help="Directory holding the R23 screens-dark set. Supplying it runs the §1.1 "
             "conformance measurement, which CAN fail the run -- that is the point of it.",
    )
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

        # --- Header (C18): state the build this run certifies --------------
        print_header(scene_path, captures_dir)

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

        # --- Gate 3: named collider inventory (C18 / R16) --------------------
        gate3_result, collider_bodies = gate3_collider_inventory(docs)
        results.append(gate3_result)

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

        # --- Gates 6, 7, 8: VOIDed by R22, pending human re-verification -----
        # Not SKIP: SKIP means "this tool cannot run it". VOID means a design
        # ruling took the prior result off the board entirely -- it must not
        # read as a pass, and must not silently blend into SKIP's bucket.
        results.append(void(6, "UI/HUD readability", "voided by R22 pending human re-verification"))
        results.append(void(7, "UI/HUD contrast", "voided by R22 pending human re-verification"))
        results.append(void(8, "structural-only check", "voided by R22 pending human re-verification"))

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

        # --- R10: relief% -- fine surface detail, INFORMATIONAL ONLY ---------
        # Mean luminance (R9-B above) cannot see relief at all; this is the metric
        # the room is actually judged on. Always reported, never fails the run and
        # never affects the exit code -- there is no ratified tolerance for it yet,
        # only the hand-measured sanity values this was built to reproduce.
        if reference_captures_dir is None:
            results.append(skip("R10", "relief% (informational)", "no --reference given"))
        else:
            ref_img = load_capture(reference_captures_dir, R9B_IMAGE)
            cur_img = load_capture(captures_dir, R9B_IMAGE)
            detail = []
            for name, box in R9B_REGIONS.items():
                ref_relief = region_relief_pct(ref_img, box)
                cur_relief = region_relief_pct(cur_img, box)
                ratio = cur_relief / ref_relief if ref_relief != 0 else float("inf")
                detail.append(
                    f"{name:20s} reference={ref_relief:6.2f}% current={cur_relief:6.2f}% "
                    f"ratio={ratio:5.2f}x"
                )
            results.append(GateResult(
                "R10", "relief% (informational)",
                "INFO",
                "n/a -- informational, never fails",
                "see detail below",
                detail,
            ))

        # --- R23: §1.1 conformance, screens dark --------------------------------
        if not args.conformance:
            results.append(skip("R23", "law 1.1 cast (screens dark)", "no --conformance given"))
        else:
            cdir = Path(args.conformance)
            graded = load_capture(cdir, R23_IMAGE)
            ungraded_path = cdir / R23_UNGRADED_IMAGE
            ungraded = load_capture(cdir, R23_UNGRADED_IMAGE) if ungraded_path.is_file() else None

            detail = []
            cool_surfaces = []
            for name, box in R23_REGIONS.items():
                lstar, chroma, hue = region_cast(graded, box)
                verdict = cast_verdict(chroma, hue)
                if verdict == "COOL":
                    cool_surfaces.append(name)
                line = (f"{name:22s} L*={lstar:5.2f} chroma={chroma:5.2f} "
                        f"hue={hue:6.1f}deg  {verdict}")
                if ungraded is not None:
                    _, uc, uh = region_cast(ungraded, box)
                    line += f"   | ungraded chroma={uc:5.2f} hue={uh:6.1f}deg {cast_verdict(uc, uh)}"
                detail.append(line)

            if ungraded is not None:
                detail.append("")
                detail.append("the ungraded column is the diagnostic R18 requires: it separates "
                              "the room's LIGHT from its GRADE, and answering that question "
                              "later would otherwise cost a whole editor lease.")

            passed = not cool_surfaces
            results.append(GateResult(
                "R23", "law 1.1 cast (screens dark)",
                "PASS" if passed else "FAIL",
                "no surface reads COOL",
                "all surfaces warm/neutral" if passed
                else f"{len(cool_surfaces)} COOL: {', '.join(cool_surfaces)}",
                detail,
            ))

    except (FileNotFoundError, ValueError, AssertionError, KeyError, json.JSONDecodeError) as exc:
        print(f"ERROR: {exc}", file=sys.stderr)
        sys.exit(1)

    print_table(results)

    # "INFO" (R10) is deliberately excluded from ever failing the run -- it has no
    # ratified tolerance, only sanity values it should reproduce approximately.
    # "VOID" (Gates 6/7/8, R22) is likewise excluded -- a ruling took them off
    # the board pending human re-verification; that is not the same as a pass,
    # but it must not hard-fail the exit code either. Both are reported
    # distinctly in the summary line below, never silently merged into PASS.
    exit_code = 0 if all(r.status in ("PASS", "SKIP", "INFO", "VOID") for r in results) else 1
    print_summary(results, exit_code)
    sys.exit(exit_code)


if __name__ == "__main__":
    main()
