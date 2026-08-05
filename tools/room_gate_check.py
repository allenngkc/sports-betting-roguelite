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
import hashlib
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
# MeshColliders were removed at a1fd6fb (T57 rebuilt those quads at true world
# size; Quad/ArtQuad now add a MeshCollider only under keepCollider), so they
# are deliberately NOT in this expected set. If either reappears, Gate 3 FAILs
# and names it -- which is the proof the gate can see them at all.
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

# ---------------------------------------------------------------------------
# R19 -- the institution's metal, measured on surface-pure boxes.
#
# INFORMATIONAL, NEVER JUDGED, and deliberately NOT folded into R23. Law §1.1 is
# about the room's own plaster, floor and bunk; a small dark metal fixture reading
# cool is not "a blue-tinted room". Putting metal into R23's pass/fail would make
# the institutional palette landing correctly read as a law violation, which is
# the instrument convicting the design it exists to protect.
#
# These replace the first-pass boxes whose numbers the handoff flagged as
# unratified. That caveat paid: both first-pass boxes were bleeding the warm
# plaster wall behind the metal, and reported the WALL's hue (~100-112deg) as the
# metal's. Purity here is sd/mean of luminance, same criterion as R9B_REGIONS:
#   housing face  0.020   conduit drop  0.053   conduit ceiling run  0.046
#
# The conduit drop is a narrow strip ON THE PIPE BODY between two fittings. A
# cylinder genuinely has a lit and a shaded face, so any pure strip picks one --
# the full-width reading is carried alongside so the strip is never mistaken for
# the whole pipe (C25: a measurement is reported with its scope attached).
R19_REGIONS = {
    "housing face (steel)":   (1900, 1100, 2020, 1190),
    "conduit drop (body)":    (1896,  480, 1906,  680),
    "conduit drop (full w.)": (1888,  480, 1908,  680),
    "conduit ceiling run":    (1580,  322, 1720,  332),
    # R19(a), 2026-08-03: the occupant's two machines. Until now R19(a)'s
    # separation had only ever been albedo arithmetic -- no region sampled any
    # body, so the ruling was verified against source and never against a frame
    # (constitution §2.5). These are the laptop's keyboard deck and the phone's
    # face, both projected from world geometry and then confirmed by eye.
    "laptop body (his)":      (1524, 1020, 1584, 1040),
    "phone body (his)":       (1564, 1085, 1592, 1097),
    # R19(c), 2026-08-04: the drab green #3A4230 was placed at §2's placements and
    # never once measured on a frame. Bunk frames should carry it; the couch must
    # NOT (the ruling names the couch as excluded). All four boxes confirmed by eye
    # to sit on frame members / couch fabric, not on the plaster behind them.
    "bunk1 post (frame)":     ( 929, 1026,  953, 1186),
    "bunk2 slab (frame)":     (1612,  757, 1732,  793),
    "couch fabric (not grn)": ( 216,  912,  416,  992),
    # NO BEZEL REGION, and the reason became a ruling. TVBody wore BezelBlack
    # #3C3C38 -- the material R19(a)'s premise names as the shared one -- but it
    # is a slab BEHIND the screen whose only exposed part is a ~6cm border, and
    # the riveted housing covers that border on the right and bottom. Every
    # candidate box straddled rivets, so no surface-pure region was obtainable.
    # NOT the same as invisible: the border IS exposed on the left, and retiring
    # the material moved 170k pixels in the seated frame. BezelBlack was retired
    # (Allen, 2026-08-03) and TVBody now wears ArtHousingSteel, so the housing
    # face above is R19(a)'s comparand and there is nothing else to sample.
}

# ---------------------------------------------------------------------------
# R20 -- does a wear surface actually READ?
#
# INFORMATIONAL. R12's standing complaint is that surface detail gets asserted
# rather than measured, and R7 died of exactly that: 1.92% of pixels changed
# against a 1.69% baseline, i.e. very nearly invisible, discovered only after the
# work was built. This makes the question numeric.
#
# The metric is p95-p5 luminance SPREAD inside one surface, not sd/mean: wear is
# sparse and hard-edged by design (chips are 8-14% coverage), so sd is dominated
# by the 86-92% that is intact paint. Spread asks "how far apart are the light
# and dark parts of this surface", which is nearer to what "reads" means.
#
# BENCHMARK, and it is the honest part: the CEILING STAIN. Design doc §1.7 names
# it as the surface that demonstrably reads at review distance -- weakest normal
# map in the room, most visible surface, because the fluorescent rakes it at
# theta ~87deg. So the ceiling's own spread is the bar. Below it is not reading.
R20_REGIONS = {
    "ceiling stain (BENCHMARK)":  ( 700,   60, 1150,  300),
    "housing paint, flat":        (1900, 1100, 2020, 1190),
    "housing paint, most varied": (1780, 1180, 1840, 1240),
    "desk, mid":                  (1560, 1130, 1690, 1210),
    "desk, far/dark end":         (1690, 1150, 1820, 1230),
}

# Below this chroma a hue angle is not meaningful -- it is the direction of a
# vector too short to trust, and calling a near-grey surface "cool" on the
# strength of a 0.4 chroma reading would be measuring noise.
R23_CHROMA_FLOOR = 1.5

# Gate 4's reference is a fixed file next to this script, not a CLI arg --
# the CLI's --reference flag is for the R9-B capture-image comparison.
REFERENCE_JSON_PATH = Path(__file__).resolve().parent / "room_gate_reference.json"

# Gate 4 keys its reference by BoxCollider OWNER NAME, not shape alone. A bare
# (size, center) multiset cannot tell a real dimension change from two
# colliders swapping owners: sort both snapshots and the swap is byte-identical
# before and after, so the gate PASSes while blind to a live defect -- the same
# disease Gate 3 had when it only counted colliders instead of naming them.
# NOTE THE REMAINING GAP: this only catches a swap between colliders whose
# local size/center differ. If two colliders already share identical local
# size AND center (this scene has several such groups -- WallLeft/WallRight,
# Floor/Ceiling, the four DeskLeg* posts), a swap between just THOSE two is
# still invisible, because Gate 4 never reads the owning GameObject's
# Transform (world position), only the collider's own local fields. Closing
# that fully would mean comparing world-space bounds, not just local ones --
# out of scope for this fix; see gate4's blind_spot text in main() for the
# declaration a reader actually sees.
#
# A reference file written before the owner field existed has no owner data
# and must never be silently compared as if it did; bump this whenever the
# reference's shape changes again, so an old file is always detected rather
# than misread.
REFERENCE_SCHEMA_VERSION = 3
# Certification date is passed in, never read from the clock: a gate report must
# be reproducible, and a wall-clock read makes two runs of the same scene differ.
TODAY = "2026-08-04"

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
LOCAL_POSITION_RE = re.compile(r'm_LocalPosition:\s*\{x:\s*(-?[\d.eE+-]+),\s*y:\s*(-?[\d.eE+-]+),\s*z:\s*(-?[\d.eE+-]+)\}')
# R29: a GameObject's own enabled flag. Gate 2 counted names only, so a DISABLED
# duplicate satisfied a count -- the gate could not tell the two states apart at
# all, which R29 rules is a bigger finding than the gate's result.
IS_ACTIVE_RE = re.compile(r'^\s*m_IsActive:\s*(\d+)\s*$', re.M)
# A PrefabInstance-rooted object carries its active state only if the instance
# overrides it; otherwise the value lives in the prefab ASSET and is genuinely
# not readable from the scene file. That case is reported "unknown", never
# assumed active -- assuming is how a bare count became a vacuous gate.
PREFAB_ACTIVE_OVERRIDE_RE = re.compile(r'propertyPath:\s*m_IsActive\s*\n\s*value:\s*(\d+)')
SOURCE_PREFAB_GUID_RE = re.compile(r'm_SourcePrefab:\s*\{fileID:\s*\d+,\s*guid:\s*([0-9a-f]+)')
META_GUID_RE = re.compile(r'^guid:\s*([0-9a-f]+)\s*$', re.M)
M_FATHER_RE = re.compile(r'm_Father:\s*\{fileID:\s*(-?\d+)\}')


def prefab_root_active_state(assets_root, guid, _cache={}):
    """Active state of a prefab ASSET's root GameObject, for R29.

    When a PrefabInstance does not override m_IsActive, the value lives in the
    prefab asset, so the scene alone genuinely cannot answer -- but the asset
    can, and it is a file like any other. Resolving it turns a permanent
    UNCOVERED into a real verdict without an editor lease.

    Returns "active" / "inactive" / "unknown". Every failure to resolve returns
    "unknown" rather than a guess: an unresolvable prefab is exactly the case
    R29 says must not be assumed active.
    """
    key = (str(assets_root), guid)
    if key in _cache:
        return _cache[key]

    state = "unknown"
    if assets_root is not None and guid:
        try:
            for meta in Path(assets_root).rglob("*.prefab.meta"):
                m = META_GUID_RE.search(meta.read_text(encoding="utf-8", errors="replace"))
                if not m or m.group(1) != guid:
                    continue
                prefab = meta.with_suffix("")           # strip ".meta"
                docs = split_documents(prefab.read_text(encoding="utf-8", errors="replace"))
                # The root is the Transform with no parent; follow it back to its
                # GameObject rather than trusting document order.
                root_anchor = None
                for _cid, _anchor, cname, body in docs:
                    if cname not in ("Transform", "RectTransform"):
                        continue
                    fm = M_FATHER_RE.search(body)
                    if fm and fm.group(1) == "0":
                        gm = GAMEOBJECT_REF_RE.search(body)
                        if gm:
                            root_anchor = gm.group(1)
                        break
                for _cid, anchor, cname, body in docs:
                    if cname == "GameObject" and anchor == root_anchor:
                        am = IS_ACTIVE_RE.search(body)
                        if am:
                            state = "active" if am.group(1) == "1" else "inactive"
                        break
                break
        except OSError:
            state = "unknown"

    _cache[key] = state
    return state


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


def gate2_object_counts(docs, assets_root=None):
    """Gate 2: singleton names, Light count, Dressing_* count.

    GameObject names normally live as `m_Name: <name>` inside a `GameObject:`
    document. But a GameObject that is the (renamed) root of a
    PrefabInstance -- RoomArtRoot, in this scene -- has NO `GameObject:`
    document of its own at all; its name lives only in the PrefabInstance's
    override list as a `propertyPath: m_Name` / `value: <name>` pair. Both
    sources must be counted or RoomArtRoot silently reads as absent.

    R29 (DD 2026-08-03): every count is now split by ACTIVE STATE. A gate
    certifies the configuration it ran against, so a name is not a member of
    this inventory merely by existing -- it must be enabled. Three buckets:
    active / inactive / unknown, and "unknown" is a real answer, not a
    rounding error toward "active".
    """
    empty = lambda: {"active": 0, "inactive": 0, "unknown": 0}

    def state_of(body, regex):
        m = regex.search(body)
        if not m:
            return "unknown"
        return "active" if m.group(1) == "1" else "inactive"

    name_states = {}
    go_by_anchor = {}

    for _class_id, anchor, class_name, body in docs:
        if class_name == "GameObject":
            state = state_of(body, IS_ACTIVE_RE)
            m = NAME_FIELD_RE.search(body)
            name = m.group(1) if m else None
            go_by_anchor[anchor] = state
            if name:
                name_states.setdefault(name, empty())[state] += 1
        elif class_name == "PrefabInstance":
            # One instance carries at most one m_IsActive override, applying to
            # the root it renames. With no override the value lives in the
            # prefab asset -- resolve it there rather than reporting unknown.
            state = state_of(body, PREFAB_ACTIVE_OVERRIDE_RE)
            if state == "unknown":
                gm = SOURCE_PREFAB_GUID_RE.search(body)
                state = prefab_root_active_state(assets_root, gm.group(1) if gm else None)
            for pm in PREFAB_NAME_OVERRIDE_RE.finditer(body):
                name = pm.group(1)
                if name:
                    name_states.setdefault(name, empty())[state] += 1

    # A Light's own enable flag is not the question here -- Gate 2 counts light
    # OBJECTS, so the owning GameObject's active state is what decides whether
    # the light is in the scene at all. A Light living inside a prefab has no
    # GameObject document here, so its owner resolves to unknown.
    light_states = empty()
    for _cid, _anchor, cname, body in docs:
        if cname != "Light":
            continue
        gm = GAMEOBJECT_REF_RE.search(body)
        light_states[go_by_anchor.get(gm.group(1), "unknown") if gm else "unknown"] += 1

    dressing_states = empty()
    for name, states in name_states.items():
        if name.startswith("Dressing_"):
            for bucket, n in states.items():
                dressing_states[bucket] += n

    return name_states, light_states, dressing_states


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

    Returns (GateResult, box_collider_records) -- the full (anchor, owner_name,
    owner_fileID, body) records for every BoxCollider, handed back so Gate 4
    (dimension comparison) can key its comparison on owner identity, not just
    reuse the text. Handing back bare bodies here once cost Gate 4 its owner
    identity entirely -- do not go back to that.
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
        blind_spot="inventories collider type, owning GameObject, and the solid/trigger split "
                   "only -- does not verify collider position, size, layer, m_Enabled state, "
                   "physics material, or any trigger behaviour beyond that split.",
    )
    return result, box


def gate4_collider_dimensions(box_collider_records, owner_positions=None):
    """Extract each BoxCollider's (size, center), keyed by its OWNER NAME.

    This is keyed on owner, not just shape, on purpose: a bare (size, center)
    multiset cannot tell a real dimension change from two colliders swapping
    owners -- sort both snapshots and the swap is byte-identical before and
    after, so a shape-only Gate 4 PASSes while blind to a live defect. Keying
    on the owner name Gate 3 already resolved closes that hole for any swap
    between colliders whose local size/center differ, and lets a FAIL name
    the collider, not just an index.

    NOT closed by this: two colliders that already share identical local
    size AND center (several exist in this room -- e.g. WallLeft/WallRight,
    Floor/Ceiling, the four DeskLeg* posts). A swap between just those two is
    still invisible, because this only ever sees the collider's own local
    fields, never the owning GameObject's world-space Transform.

    Raises ValueError if two BoxColliders resolve to the same owner name --
    silently keeping only one via a dict overwrite would just relocate the
    same blind spot one level down, which defeats the point of this fix.
    """
    owner_positions = owner_positions or {}
    by_owner = {}
    for anchor, name, fileid, body in box_collider_records:
        owner_label = _owner_label(name, fileid)
        size_m = SIZE_RE.search(body)
        center_m = CENTER_RE.search(body)
        if not size_m or not center_m:
            raise ValueError(f"BoxCollider on '{owner_label}' (anchor {anchor}) is missing m_Size or m_Center")
        size = tuple(round(float(v), 6) for v in size_m.groups())
        center = tuple(round(float(v), 6) for v in center_m.groups())
        position = owner_positions.get(fileid)
        if owner_label in by_owner:
            raise ValueError(
                f"more than one BoxCollider resolves to owner '{owner_label}' -- Gate 4 keys "
                "on owner name and cannot disambiguate them without reintroducing an anonymous "
                "fallback; give the GameObjects distinct names or extend Gate 4 to also key on anchor"
            )
        by_owner[owner_label] = {"size": size, "center": center, "position": position}
    return by_owner


BARE_FILEID_RE = re.compile(r'\{fileID: (-?\d+)\}')


def canonical_scene_records(docs):
    """Content fingerprint of a scene, immune to anchor reassignment (§1.5, §9.2).

    Measured 2026-08-03: the builder is BYTE-unstable and CONTENT-stable. Two
    consecutive rebuilds produced three different md5s (committed b16bbd38, run1
    73d29510, run2 c4d033ee) while differing in nothing but fileIDs. So §9.2's
    "run the builder twice and compare" could never be a byte comparison -- a byte
    diff is always red and therefore always ignored, which is why the law went
    years unmeasured.

    The cheap fix is to erase every fileID and compare line multisets. That is
    ALSO a vacuous gate: erasing an anchor erases parent/child identity with it,
    so a re-parenting that shuffles no lines reads as identical. Instead each
    fileID is RESOLVED to a stable label -- "Transform@Bunk2PostFront",
    "GameObject:Couch" -- so a reference that changes what it points AT changes
    the record, while a reference that merely gets renumbered does not.

    Asset references ({fileID: N, guid: G, type: T}) are left untouched: the guid
    is the real identity there and is already stable across rebuilds.
    """
    go_name = {}
    for _cid, anchor, cname, body in docs:
        if cname == "GameObject":
            m = NAME_FIELD_RE.search(body)
            go_name[anchor] = m.group(1) if m else "<unnamed>"

    label = {}
    for _cid, anchor, cname, body in docs:
        if cname == "GameObject":
            label[anchor] = f"GameObject:{go_name.get(anchor, '<unnamed>')}"
        else:
            gm = GAMEOBJECT_REF_RE.search(body)
            owner = go_name.get(gm.group(1), "<external>") if gm else "<none>"
            label[anchor] = f"{cname}@{owner}"

    def resolve(m):
        fid = m.group(1)
        if fid == "0":
            return "{null}"
        return "{->" + label.get(fid, "external:" + fid) + "}"

    records = Counter()
    for _cid, anchor, cname, body in docs:
        canon = BARE_FILEID_RE.sub(resolve, body)
        lines = sorted(l.rstrip() for l in canon.splitlines() if l.strip())
        records[label.get(anchor, cname) + "||" + "|".join(lines)] += 1
    return records


def scene_content_fingerprint(docs):
    """Stable hash of a scene's CONTENT (anchor-renumbering-immune, see
    canonical_scene_records). Used to expire a human walkthrough certification
    the moment the geometry it certified changes."""
    recs = canonical_scene_records(docs)
    blob = chr(10).join(f"{k} :: {v}" for k, v in sorted(recs.items()))
    return hashlib.sha256(blob.encode("utf-8")).hexdigest()


def collider_owner_positions(docs):
    """Map GameObject anchor -> its Transform's m_LocalPosition (R22, 2026-08-03).

    Gate 4 read only the collider's own local m_Size/m_Center, so MOVING a
    collider's GameObject changed nothing it looked at. Proven, not theorised:
    Bunk2PostFront was relocated 0.13m to clear the couch sightline, the scene
    rebuilt, and Gate 4 reported "27 colliders, 0 mismatch(es)" -- a gate named
    "collision dims unchanged" passing a collision change. Fourth-vacuous-gate
    class (C18 §4.2); this closes it.
    """
    positions = {}
    for _cid, _anchor, cname, body in docs:
        if cname not in ("Transform", "RectTransform"):
            continue
        gm = GAMEOBJECT_REF_RE.search(body)
        pm = LOCAL_POSITION_RE.search(body)
        if gm and pm:
            positions[gm.group(1)] = tuple(round(float(v), 6) for v in pm.groups())
    return positions


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
    def __init__(self, gate, check, status, expected, observed, detail=None, blind_spot=""):
        self.gate = gate
        self.check = check
        self.status = status  # "PASS" | "FAIL" | "SKIP" | "VOID" | "INFO"
        self.expected = expected
        self.observed = observed
        self.detail = detail or []
        # What this gate does NOT verify, stated even (especially) when it PASSes --
        # "every gate states what it cannot see" (Design Director standing instruction,
        # issued after three green gates were found to be measuring nothing). Must be
        # true of the code as written, not reassuring prose -- see per-callsite comments.
        self.blind_spot = blind_spot


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


def skip(gate, check, reason, blind_spot=""):
    return GateResult(gate, check, "SKIP", "-", reason, blind_spot=blind_spot)


def void(gate, check, reason, blind_spot=""):
    """A gate that has been explicitly ruled out by a design decision (R22),
    as distinct from SKIP (un-runnable by this tool). VOID must not count as
    a pass and must not silently disappear into SKIP's bucket -- it needs a
    human to re-verify it before it can be trusted again."""
    return GateResult(gate, check, "VOID", "-", reason, blind_spot=blind_spot)


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
        # Printed for every gate, PASS included: a verdict alone does not say what
        # was checked to reach it. Falls back to a loud marker rather than silently
        # omitting the line if a gate was added without writing one.
        print("       blind spot: " + (r.blind_spot or "(none declared -- this is a bug, file one)"))
        for line in r.detail:
            print("       " + line)


def format_mtime(path):
    dt = datetime.fromtimestamp(path.stat().st_mtime).astimezone()
    return dt.strftime("%Y-%m-%d %H:%M:%S %Z")


def print_header(scene_path, captures_dir, results):
    """C18: the report must say what build it certifies -- a PASS must never
    be misreadable as covering geometry other than what this run actually
    parsed and captured. Prints the scene and capture paths plus their
    last-modified timestamps, and warns if the captures predate the scene
    (meaning the frames do not show the geometry just parsed, and every
    capture-derived result below -- R9-A, R9-B, R10 -- is stale).

    Also prints the run's total blind area up front (Task C): how many of
    this run's checks are VOID or SKIP, i.e. produced no verdict at all --
    computed from the actual results this run collected, not a guess, so it
    can never drift from what print_table/print_summary go on to show."""
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

    total = len(results)
    void_count = sum(1 for r in results if r.status == "VOID")
    skip_count = sum(1 for r in results if r.status == "SKIP")
    blind = void_count + skip_count
    print(
        f"Blind area: {blind}/{total} checks this run produce NO verdict at all "
        f"({skip_count} SKIP -- not run by this tool/invocation; {void_count} VOID -- "
        "ruled off the board pending human re-verification). See 'blind spot' under "
        "every gate below, PASS included, for what even a verdict does not cover."
    )
    print()


def print_summary(results, exit_code):
    """Task B / Ruling T54: a past report claimed '8/8' when three gates were VOID.
    This must be structurally impossible to misread that way again -- so no bare
    "N/8"-style ratio is printed anywhere here, and every check that did not
    produce a PASS/FAIL verdict this run is named individually, with why, right
    next to the totals. If a reader could still walk away thinking every check
    passed, this function is wrong.
    """
    counts = Counter(r.status for r in results)
    order = ["PASS", "FAIL", "SKIP", "VOID", "INFO"]
    parts = ", ".join(f"{counts[s]} {s}" for s in order if counts.get(s))
    total = len(results)
    verdicts = counts.get("PASS", 0) + counts.get("FAIL", 0)
    unassessed = [r for r in results if r.status not in ("PASS", "FAIL")]

    print()
    print(f"Summary: {parts} ({total} checks total) -- exit code {exit_code}")
    print(
        f"Verdict coverage: {verdicts}/{total} checks produced a PASS or FAIL verdict this run. "
        f"The other {total - verdicts} did not: SKIP means this tool/invocation did not run that "
        "check at all; VOID means a ruling took a prior result off the board pending human "
        "re-verification; INFO means it was measured but is never judged. None of those three "
        "are passes, and none should be read as one -- 'no FAIL' is not 'all passed'."
    )
    if unassessed:
        print(f"Unassessed this run ({len(unassessed)}/{total}) -- gate, status, why:")
        for r in unassessed:
            if r.status == "INFO":
                why = "no ratified tolerance yet; informational only by design, never produces a verdict"
            else:
                why = r.observed
            print(f"  Gate {r.gate} [{r.status}] {r.check}: {why}")
    else:
        print(f"All {total} checks produced a PASS or FAIL verdict.")


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
    parser.add_argument(
        "--certify-human-gates",
        metavar="COMMIT",
        help="Record that a human walked THIS scene and passed gates 6-8, stamping the commit "
             "and the scene's content fingerprint into the reference. The certification expires "
             "automatically the moment the scene's content changes (C18).",
    )
    parser.add_argument(
        "--certify-basis",
        metavar="TEXT",
        help="Provenance of the human verdict being recorded. A re-certification on a STANDING "
             "verdict is not a fresh walk, and the record must say which it was -- otherwise the "
             "next reader sees a green gate and assumes someone walked this build.",
    )
    parser.add_argument(
        "--compare-scene",
        help="Path to a second Room.unity to compare CONTENT against, ignoring fileID "
             "renumbering. This is what makes §9.2 ('run the builder twice') a real check: "
             "the builder is byte-unstable and content-stable, so a byte diff is always red "
             "and therefore always ignored.",
    )
    parser.add_argument(
        "--report",
        help="Also write this run's full report to PATH. Every number this harness has ever "
             "produced reached the register by being hand-copied out of a terminal, which makes "
             "the claim the artifact and the measurement unreproducible -- C11 wants the evidence, "
             "C17 wants it retained, C25 wants its scope attached. The file carries all three.",
    )
    args = parser.parse_args()

    scene_path = Path(args.scene)
    captures_dir = Path(args.captures)
    reference_captures_dir = Path(args.reference) if args.reference else None

    # Tee rather than redirect: a run that is being recorded must still be a run
    # you can watch, or nobody will pass --report when it matters.
    report_file = None
    if args.report:
        report_path = Path(args.report)
        report_path.parent.mkdir(parents=True, exist_ok=True)
        report_file = report_path.open("w", encoding="utf-8")

        class _Tee:
            def __init__(self, *streams): self._streams = streams
            def write(self, data):
                for s in self._streams:
                    s.write(data)
                return len(data)
            def flush(self):
                for s in self._streams:
                    s.flush()

        sys.stdout = _Tee(sys.__stdout__, report_file)

    results = []

    if args.certify_human_gates:
        _docs = split_documents(load_scene_text(scene_path))
        _payload = json.loads(REFERENCE_JSON_PATH.read_text(encoding="utf-8"))
        _payload["human_gates"] = {
            "gates": [6, 7, 8],
            "certified_commit": args.certify_human_gates,
            "certified_at": TODAY,
            "content_fingerprint": scene_content_fingerprint(_docs),
            "basis": args.certify_basis or "fresh walkthrough of this build",
            "note": "R22: gates 6-8 have no automated instrument; a human walks the build. "
                    "Expires automatically when content_fingerprint stops matching.",
        }
        REFERENCE_JSON_PATH.write_text(json.dumps(_payload, indent=2) + chr(10), encoding="utf-8")
        print(f"human gates 6-8 certified at {args.certify_human_gates}; "
              f"fingerprint {_payload['human_gates']['content_fingerprint'][:16]}...")
        sys.exit(0)

    try:
        scene_text = load_scene_text(scene_path)
        docs = split_documents(scene_text)

        # --- Gate 1: pre/post capture diff -- needs two Unity runs ---------
        results.append(skip(
            1, "pre/post capture diff",
            "requires two separate Unity editor captures (before and after the change); nothing to compare from a single scene/capture set",
            blind_spot="did not run: no before/after Unity capture pair exists for this invocation, "
                        "so nothing about a pre/post visual diff is checked anywhere in this run.",
        ))

        # --- Gate 2: object counts, by active state (R29) --------------------
        # R29: "a gate that ran against one state certifies that state only."
        # The count is of ACTIVE objects. An inactive same-named object is named
        # rather than silently folded in (that was the original defect: a
        # disabled duplicate satisfied the count). Where the scene file cannot
        # reveal a state at all -- a PrefabInstance with no m_IsActive override,
        # whose value lives in the prefab asset -- the gate reports UNCOVERED and
        # leaves the pass total, per R29's "re-run against the active state, or
        # record it as uncovered", never a pass that covers one of two states.
        # Assets root lets Gate 2 resolve a PrefabInstance's active state from the
        # prefab asset when the scene carries no override (R29).
        assets_root = next((p for p in scene_path.resolve().parents if p.name == "Assets"), None)
        name_states, light_states, dressing_states = gate2_object_counts(docs, assets_root)
        detail = []
        gate2_ok = True
        unknowns = []

        def note(label, states, expected):
            nonlocal gate2_ok
            ok = states["active"] == expected and states["inactive"] == 0 and states["unknown"] == 0
            gate2_ok &= ok
            if states["unknown"]:
                unknowns.append(label)
            extra = ""
            if states["inactive"]:
                extra += f", {states['inactive']} INACTIVE"
            if states["unknown"]:
                extra += f", {states['unknown']} state-unreadable"
            detail.append(
                f"{label}: expected {expected} active, observed {states['active']} active"
                f"{extra} [{'ok' if ok else 'MISMATCH'}]")

        for name in EXPECTED_SINGLETONS:
            note(name, name_states.get(name, {"active": 0, "inactive": 0, "unknown": 0}), 1)
        note("Light components", light_states, EXPECTED_LIGHT_COUNT)
        note("Dressing_* objects", dressing_states, EXPECTED_DRESSING_COUNT)

        if unknowns:
            detail.append(
                "UNCOVERED (R29): active state unreadable from the scene file for "
                + ", ".join(unknowns)
                + " -- a PrefabInstance carries m_IsActive only when it overrides it; "
                  "otherwise the value lives in the prefab asset. Not assumed active.")

        gate2_status = "SKIP" if unknowns else ("PASS" if gate2_ok else "FAIL")
        results.append(GateResult(
            2, "object counts (active state)", gate2_status,
            f"1 active each singleton, {EXPECTED_LIGHT_COUNT} active Light, "
            f"{EXPECTED_DRESSING_COUNT} active Dressing_*",
            (f"{sum(1 for n in EXPECTED_SINGLETONS if name_states.get(n, {}).get('active', 0) == 1)}/4 "
             f"singletons, {light_states['active']} Light, {dressing_states['active']} Dressing_* "
             f"(active)" + (f" -- {len(unknowns)} UNCOVERED" if unknowns else "")),
            detail,
            blind_spot="counts objects by name and enabled flag -- it CAN now distinguish active "
                       "from inactive and names any disabled same-named object, which it could not "
                       "do before R29. It still does not verify parenting, transform values, or "
                       "that the correctly-named active object is the one intended rather than a "
                       "same-named duplicate elsewhere in the hierarchy. It reads m_IsActive only: "
                       "an object whose ANCESTOR is disabled is inactive in the running scene yet "
                       "reads active here, so this gate certifies the object's own flag, not its "
                       "effective state in Play Mode. Where a PrefabInstance does not override "
                       "m_IsActive the flag is read from the SOURCE PREFAB asset, so the verdict "
                       "covers the asset's default rather than anything the scene states; if that "
                       "asset cannot be resolved the state is reported UNCOVERED, never assumed.",
        ))

        # --- Gate 3: named collider inventory (C18 / R16) --------------------
        gate3_result, box_collider_records = gate3_collider_inventory(docs)
        results.append(gate3_result)

        # --- Gate 4: collision dimensions unchanged, keyed by owner ---------
        # Keyed on owner name (reusing Gate 3's resolution), not shape alone -- a
        # bare (size, center) multiset let two identically-shaped colliders swap
        # owners invisibly (sorted, the swap is byte-identical before/after). See
        # gate4_collider_dimensions' docstring for the full story.
        current_dims = gate4_collider_dimensions(box_collider_records, collider_owner_positions(docs))
        if args.write_reference:
            REFERENCE_JSON_PATH.parent.mkdir(parents=True, exist_ok=True)
            payload = {
                "schema_version": REFERENCE_SCHEMA_VERSION,
                "generated_from": str(scene_path),
                "collider_count": len(current_dims),
                "colliders": [
                    {"owner": owner, "size": list(d["size"]), "center": list(d["center"]),
                     "position": list(d["position"]) if d["position"] else None}
                    for owner, d in sorted(current_dims.items())
                ],
            }
            REFERENCE_JSON_PATH.write_text(json.dumps(payload, indent=2) + "\n", encoding="utf-8")
            results.append(skip(
                4, "collision dims unchanged", f"--write-reference given; wrote {REFERENCE_JSON_PATH}",
                blind_spot="did not run: this invocation OVERWROTE the reference file from the "
                            "current scene instead of checking against it, so no comparison "
                            "happened, and whatever the previous reference recorded is now gone.",
            ))
        else:
            if not REFERENCE_JSON_PATH.is_file():
                raise FileNotFoundError(
                    f"reference file not found: {REFERENCE_JSON_PATH} "
                    "(run once with --write-reference to create it)"
                )
            ref_payload = json.loads(REFERENCE_JSON_PATH.read_text(encoding="utf-8"))

            if ref_payload.get("schema_version") != REFERENCE_SCHEMA_VERSION:
                # Old-format reference: no owner field, so a real comparison would be
                # exactly the shape-only multiset that let owner swaps go undetected.
                # Refuse to compare against it -- SKIP and say so, never a silent PASS.
                results.append(skip(
                    4, "collision dims unchanged",
                    f"{REFERENCE_JSON_PATH.name} predates owner-tracking "
                    f"(schema_version={ref_payload.get('schema_version')!r}, need "
                    f"{REFERENCE_SCHEMA_VERSION!r}) -- run --write-reference to re-baseline",
                    blind_spot="did not run: the stored reference predates owner tracking and "
                                "records only a bare size/center multiset -- comparing against "
                                "it could not detect two identically-shaped colliders swapping "
                                "owners, which is the exact defect this schema exists to close. "
                                "Re-baseline with --write-reference before this gate can compare "
                                "again.",
                ))
            else:
                ref_by_owner = {
                    c["owner"]: {
                        "size": tuple(round(float(v), 6) for v in c["size"]),
                        "center": tuple(round(float(v), 6) for v in c["center"]),
                        "position": tuple(round(float(v), 6) for v in c["position"]) if c.get("position") else None,
                    }
                    for c in ref_payload["colliders"]
                }
                ref_owners = set(ref_by_owner)
                cur_owners = set(current_dims)

                mismatches = []
                for name in sorted(ref_owners - cur_owners):
                    mismatches.append(f"'{name}': in reference, missing from current scene")
                for name in sorted(cur_owners - ref_owners):
                    mismatches.append(f"'{name}': in current scene, not in reference (new BoxCollider or renamed/re-parented owner)")
                for name in sorted(ref_owners & cur_owners):
                    ref = ref_by_owner[name]["size"] + ref_by_owner[name]["center"]
                    cur = current_dims[name]["size"] + current_dims[name]["center"]
                    if any(abs(a - b) > DIMENSION_TOLERANCE for a, b in zip(ref, cur)):
                        mismatches.append(f"'{name}': reference size/center {ref} vs current {cur}")
                    rp, cp = ref_by_owner[name]["position"], current_dims[name]["position"]
                    if rp is None or cp is None:
                        mismatches.append(f"'{name}': owner transform position unreadable (ref={rp}, cur={cp})")
                    elif any(abs(a - b) > DIMENSION_TOLERANCE for a, b in zip(rp, cp)):
                        mismatches.append(f"'{name}': MOVED - reference position {rp} vs current {cp}")

                gate4_ok = not mismatches
                results.append(GateResult(
                    4, "collision dims unchanged",
                    "PASS" if gate4_ok else "FAIL",
                    f"matches {REFERENCE_JSON_PATH.name} ({len(ref_by_owner)} colliders, keyed by owner)",
                    f"{len(current_dims)} colliders, {len(mismatches)} mismatch(es)",
                    mismatches[:10],
                    blind_spot="compares BoxCollider size/center BY OWNER NAME against the stored "
                               "reference -- says nothing about MeshCollider, SphereCollider, or "
                               "CapsuleCollider dimensions, and cannot detect a real change that "
                               "was also written into the reference (e.g. by a stale or mistaken "
                               "--write-reference run). It CAN now name a changed collider by owner "
                               "and CAN detect an owner-swap between colliders whose local size/center "
                               "differ. It remains BLIND to a swap between two colliders that already "
                               "share identical local size AND center (e.g. WallLeft/WallRight, "
                               "Floor/Ceiling, or the four DeskLeg* colliders in this scene all do) -- "
                               "Since R22 it ALSO compares each owner's Transform m_LocalPosition, so "
                               "a collider that RELOCATED is now named and failed -- before that a "
                               "moved collider passed this gate untouched, because nothing it read "
                               "had changed. Remaining holes, stated: it reads the owner's OWN local "
                               "position, so a move applied to an ANCESTOR transform is still "
                               "invisible; it ignores rotation and scale entirely; and a same-shaped, "
                               "same-position pair swapping owners stays undetectable.",
                ))

        # --- Gate 5: no dangling mesh references ----------------------------
        dangling = gate5_dangling_mesh_refs(scene_text)
        results.append(GateResult(
            5, "dangling mesh refs",
            "PASS" if dangling == 0 else "FAIL",
            "0", str(dangling),
            blind_spot="counts null (fileID: 0) mesh references anywhere in the scene text -- "
                       "detects a MISSING mesh, never a WRONG one; a component pointing at a "
                       "valid but incorrect mesh asset reads as a full pass.",
        ))

        # --- Gates 6, 7, 8: human-only, certified by walkthrough -------------
        # These three cannot be run by any tool -- R22 ruled the only instrument
        # is a human walking the build. Before 2026-08-03 they were hard-VOID.
        # Allen walked HEAD 9e1b4e4 and passed all three, so the harness now
        # REPORTS that verdict rather than contradicting the register.
        #
        # It is reported with an expiry, not as a standing pass. C18: a gate
        # certifies the geometry it ran against and any change voids it. The
        # certification records the scene's content fingerprint, and if the
        # current scene's content differs by so much as one resolved record,
        # these three snap back to VOID automatically. That is the whole point:
        # a human verdict that cannot expire is the stale-gate defect R22 was
        # raised about in the first place.
        cert = {}
        if REFERENCE_JSON_PATH.is_file():
            try:
                cert = json.loads(REFERENCE_JSON_PATH.read_text(encoding="utf-8")).get("human_gates") or {}
            except (json.JSONDecodeError, OSError):
                cert = {}
        current_fp = scene_content_fingerprint(docs)
        cert_fp = cert.get("content_fingerprint")
        cert_ref = cert.get("certified_commit", "?")
        cert_when = cert.get("certified_at", "?")
        certified = bool(cert_fp) and cert_fp == current_fp

        for num, gname in ((6, "UI/HUD readability"), (7, "UI/HUD contrast"), (8, "structural-only check")):
            if certified:
                results.append(GateResult(
                    num, gname, "PASS",
                    f"human walkthrough certification, scene content unchanged since {cert_ref}",
                    f"certified {cert_when} at {cert_ref}; content fingerprint matches; "
                    f"basis: {cert.get('basis', 'unrecorded')}",
                    blind_spot=(
                        "THIS TOOL RAN NO CHECK, and the verdict it replays may not be a walk of "
                        "THIS build -- read the 'basis' in the observed column. It is replaying a "
                        "human verdict recorded in "
                        f"{REFERENCE_JSON_PATH.name}, and the only thing it verified itself is that "
                        "the scene's content fingerprint still matches the one certified -- so it "
                        "covers the GEOMETRY walked, not the current captures, not the screens' "
                        "content, and not anything outside the scene file (materials, textures, APV "
                        "bake). A change to any of those leaves this line reading PASS while the "
                        "thing the human actually judged has moved. Re-walk on any doubt."
                    ),
                ))
            elif cert_fp:
                results.append(void(
                    num, gname,
                    f"scene content CHANGED since the walkthrough at {cert_ref} -- certification expired",
                    blind_spot=(
                        "auto-VOIDed: a human certified this gate against a scene whose content "
                        "fingerprint no longer matches, so the verdict no longer covers this build "
                        "(C18). No tool can re-issue it -- R22's instrument is a human walking the "
                        "room. Treat as fully unknown until re-walked."
                    ),
                ))
            else:
                results.append(void(
                    num, gname, "voided by R22 pending human re-verification",
                    blind_spot=(
                        "VOIDed by R22: this run performs NO check for this gate, automated or "
                        "human. A pass recorded before the ruling must not be trusted, and none is "
                        "issued here -- treat this line as fully unknown, not as a historical pass "
                        "carried forward."
                    ),
                ))

        # --- R9-A: bunk 2 mattress luminance ---------------------------------
        current_img = load_capture(captures_dir, R9A_IMAGE)
        r9a_mean = region_mean_luminance(current_img, R9A_BOX)
        r9a_ok = abs(r9a_mean - R9A_EXPECTED_MEAN) <= R9A_TOLERANCE
        results.append(GateResult(
            "R9-A", "bunk 2 mattress luminance",
            "PASS" if r9a_ok else "FAIL",
            f"{R9A_EXPECTED_MEAN} +/- {R9A_TOLERANCE}",
            f"{r9a_mean:.2f}",
            blind_spot="samples one fixed pixel box and reports mean luminance only -- sees "
                       "nothing outside that box, says nothing about colour/hue, and a box that "
                       "no longer frames the intended surface (e.g. after a geometry move) would "
                       "still report a plausible-looking number.",
        ))

        # --- R9-B: region means within 10% of reference ----------------------
        if reference_captures_dir is None:
            results.append(skip(
                "R9-B", "region means vs reference", "no --reference given",
                blind_spot="did not run: with no --reference, this run checks nothing about "
                            "whether any sampled surface's brightness changed from a baseline.",
            ))
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
                blind_spot="compares mean luminance only, in fixed boxes -- sees nothing outside "
                           "those boxes, cannot detect a colour/hue shift at equal luminance, and "
                           "a box that has drifted off its intended surface still reports a number.",
            ))

        # --- R10: relief% -- fine surface detail, INFORMATIONAL ONLY ---------
        # Mean luminance (R9-B above) cannot see relief at all; this is the metric
        # the room is actually judged on. Always reported, never fails the run and
        # never affects the exit code -- there is no ratified tolerance for it yet,
        # only the hand-measured sanity values this was built to reproduce.
        if reference_captures_dir is None:
            results.append(skip(
                "R10", "relief% (informational)", "no --reference given",
                blind_spot="did not run: with no --reference, no relief/surface-detail ratio is "
                            "measured this run at all.",
            ))
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
                blind_spot="never fails the run regardless of value -- measures local luminance "
                           "contrast in the same fixed boxes as R9-A/R9-B, which does not "
                           "distinguish INTENDED surface detail from any other kind of contrast, "
                           "and inherits the same fixed-box blind spot as those two.",
            ))

        # --- R23: §1.1 conformance, screens dark --------------------------------
        if not args.conformance:
            results.append(skip(
                "R23", "law 1.1 cast (screens dark)", "no --conformance given",
                blind_spot="did not run: with no --conformance, the law 1.1 cool-cast measurement "
                            "does not happen this run, so nothing about the room's actual colour "
                            "cast -- with or without screens -- is checked.",
            ))
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
                blind_spot="measures chroma/hue in fixed boxes on the screens-DARK render only -- "
                           "sees nothing outside those boxes, says nothing about the room WITH "
                           "screens on (deliberately excluded to isolate the room from the "
                           "emissive screens), and a box that no longer frames its intended "
                           "surface would still report a number.",
            ))

            # --- R19: the institution's metal, informational ------------------
            detail = []
            for name, box in R19_REGIONS.items():
                lstar, chroma, hue = region_cast(graded, box)
                line = (f"{name:24s} L*={lstar:5.2f} chroma={chroma:5.2f} "
                        f"hue={hue:6.1f}deg  {cast_verdict(chroma, hue)}")
                if ungraded is not None:
                    uc, uh = region_cast(ungraded, box)[1:]
                    line += f"   | ungraded chroma={uc:5.2f} hue={uh:6.1f}deg {cast_verdict(uc, uh)}"
                detail.append(line)
            detail.append(
                "reported, never judged: cool metal is the institutional palette landing, not a "
                "law 1.1 failure -- §1.1 names a blue-tinted ROOM, and these are dark fixtures.")
            # --- R20: does the wear read? ------------------------------------
            bench = None
            r20 = []
            for name, box in R20_REGIONS.items():
                vals = []
                x0, y0, x1, y1 = box
                pix = graded.load()
                for yy in range(y0, y1):
                    for xx in range(x0, x1):
                        r, gg, b = pix[xx, yy]
                        vals.append(0.2126 * r + 0.7152 * gg + 0.0722 * b)
                vals.sort()
                n = len(vals)
                sp = vals[int(0.95 * n)] - vals[int(0.05 * n)]
                mean = sum(vals) / n
                if "BENCHMARK" in name:
                    bench = sp
                r20.append((name, mean, sp))
            detail20 = []
            for name, mean, sp in r20:
                verdict = "" if bench is None or "BENCHMARK" in name else (
                    "  READS" if sp >= bench else "  below benchmark")
                detail20.append(f"{name:28s} mean={mean:6.2f} spread(p95-p5)={sp:6.2f}{verdict}")
            detail20.append(
                "benchmark is the ceiling stain, the surface §1.7 names as the one that "
                "demonstrably reads at review distance. Sparse wear is expected to be flat in "
                "most patches; what matters is the contrast where it lands.")
            results.append(GateResult(
                "R20", "wear reads? (informational)", "INFO",
                "-", "chipped paint + battered desk vs the ceiling benchmark", detail20,
                blind_spot="reports luminance spread inside fixed boxes on the screens-dark "
                           "render. It cannot tell WHY a surface is varied -- a lighting gradient, "
                           "an edge or a neighbouring object inside the box all raise spread just "
                           "as wear does, which is why the desk is sampled twice, away from the "
                           "lamp pool. It says nothing about whether the wear is in a camera's "
                           "frustum in the three review poses (R7's actual failure), nothing about "
                           "hue, and nothing about whether the wear is well PLACED -- only whether "
                           "it is visible where it was put.",
            ))

            results.append(GateResult(
                "R19", "metal cast (informational)", "INFO",
                "-", "steel + conduit, surface-pure boxes", detail,
                blind_spot="four fixed boxes on the screens-dark render, reported without a "
                           "verdict. It cannot tell you whether the metal reads institutional -- "
                           "R19(b)-am moved that read onto VALUE and FINISH, and this instrument "
                           "measures neither. Hue here is a diagnostic, not a requirement. The "
                           "conduit body strip samples ONE face of a cylinder (the shaded one); "
                           "its full-width twin is carried beside it because the two disagree, "
                           "and the full-width figure includes edge pixels bleeding the warm wall "
                           "behind the pipe -- which is exactly how the superseded first-pass "
                           "boxes came to report the wall's hue as the metal's.",
            ))

        # --- IDEM: scene content vs a second build (§1.5 / §9.2) -------------
        if args.compare_scene:
            other_path = Path(args.compare_scene)
            other_docs = split_documents(load_scene_text(other_path))
            mine, theirs = canonical_scene_records(docs), canonical_scene_records(other_docs)
            only_mine, only_theirs = mine - theirs, theirs - mine
            n_mine, n_theirs = sum(only_mine.values()), sum(only_theirs.values())
            detail = [f"compared against {other_path}",
                      f"{sum(mine.values())} records here, {sum(theirs.values())} there",
                      f"{n_mine} only here, {n_theirs} only there"]
            for rec, c in list(only_mine.most_common(3)):
                detail.append(f"  only HERE  x{c}: {rec.split('||')[0]}")
            for rec, c in list(only_theirs.most_common(3)):
                detail.append(f"  only THERE x{c}: {rec.split('||')[0]}")
            results.append(GateResult(
                "IDEM", "scene content vs second build",
                "PASS" if not (n_mine or n_theirs) else "FAIL",
                "identical content ignoring fileID renumbering",
                "identical" if not (n_mine or n_theirs) else f"{n_mine + n_theirs} differing record(s)",
                detail,
                blind_spot="compares a multiset of per-document records with every bare fileID "
                           "RESOLVED to a stable label, so anchor renumbering is ignored by design "
                           "while a reference that changes what it points at is caught. It sorts "
                           "lines within a document, so a pure reordering inside one document is "
                           "invisible; it says nothing about assets outside the scene file "
                           "(meshes, materials, textures, the APV bake data), and nothing about "
                           "whether either scene is CORRECT -- only that the two agree.",
            ))
        else:
            results.append(skip(
                "IDEM", "scene content vs second build", "no --compare-scene given",
                blind_spot="did not run: without a second build to compare, this run says nothing "
                           "about whether the builder reproduces the room (§1.5). Note a byte "
                           "comparison would NOT substitute -- the builder is byte-unstable and "
                           "content-stable, which is exactly why this gate resolves fileIDs.",
            ))

    except (FileNotFoundError, ValueError, AssertionError, KeyError, json.JSONDecodeError) as exc:
        print(f"ERROR: {exc}", file=sys.stderr)
        sys.exit(1)

    # --- Header (C18 + Task C): state the build this run certifies, and the run's
    # total blind area, up front -- printed from the final `results` list (rather
    # than from args) so it can never drift from what the table/summary below show.
    print_header(scene_path, captures_dir, results)
    print_table(results)

    # "INFO" (R10) is deliberately excluded from ever failing the run -- it has no
    # ratified tolerance, only sanity values it should reproduce approximately.
    # "VOID" (Gates 6/7/8, R22) is likewise excluded -- a ruling took them off
    # the board pending human re-verification; that is not the same as a pass,
    # but it must not hard-fail the exit code either. Both are reported
    # distinctly in the summary line below, never silently merged into PASS.
    exit_code = 0 if all(r.status in ("PASS", "SKIP", "INFO", "VOID") for r in results) else 1
    print_summary(results, exit_code)

    if report_file is not None:
        sys.stdout.flush()
        sys.stdout = sys.__stdout__
        report_file.close()
        print(f"\nreport written: {args.report}")

    sys.exit(exit_code)


if __name__ == "__main__":
    main()
