#!/usr/bin/env python3
"""
What faces does this TTF actually expose, and what is its DEFAULT?

Written for Phase T's step T-1, where "resolve faces by style name, never by index" needed to
stop being a rule quoted from another surface's scars and start being a measurement of this
surface's own font.

THE TRAP THIS EXISTS TO CATCH
    A variable font's default instance is not necessarily the one anyone means. The laptop
    learned it from Archivo, whose faceIndex 0 reports SemiBold 600 — the first cut of that
    generator took the default and shipped a whole surface's roman voice at 600, unchosen
    (`SureThingTmpFontAssets.cs:20`, and the DD's stroke ruling
    `docs/design/s2-am2-authored-stroke-2026-08-10.md` under "The trap this nearly walked into").

    Encode Sans is worse, and this script is how that was established rather than assumed:
    its axis defaults are wght=100 wdth=75, its OS/2 usWeightClass is 100, and nameID 1 reads
    "Encode Sans Condensed Thin". The default is wrong on BOTH axes. Unity's legacy `Font`
    renders the default instance, so that is what the TV surface has been rendering.

    It also reports 45 named instances with Regular 400 at index 21 — which is why the laptop
    generator's probe ceiling of 12 could not have found it.

WHY A SCRIPT AND NOT A NOTE (C34)
    A claim about a font that lives only in a comment cannot be re-checked when the font is
    updated and its instances reorder. This re-runs in a second, needs no editor and no Unity,
    and has no third-party dependency — it reads the sfnt table directory and then `name`,
    `fvar` and `OS/2` directly, because fontTools is not available in this environment.

USAGE
    python tools/ttf_faces.py <font.ttf> [more.ttf ...]

    python tools/ttf_faces.py unity/SBR/Assets/SBR/Resources/Tv/Fonts/EncodeSans.ttf

WHAT TO LOOK AT
    The line marked "<-- AT THE AXIS DEFAULT" is what you get by taking faceIndex 0. If that is
    not the face you intend to ship, the generator must resolve by style name — and its probe
    ceiling must be high enough to reach the instance you want.
"""
import struct
import sys


def read_tables(buf):
    if buf[:4] == b"ttcf":
        raise SystemExit("TrueType collection - not handled")
    num = struct.unpack(">H", buf[4:6])[0]
    tables = {}
    for i in range(num):
        off = 12 + i * 16
        tag, _cs, o, ln = struct.unpack(">4sIII", buf[off:off + 16])
        tables[tag.decode("latin-1").strip()] = (o, ln)
    return tables


def read_names(buf, off):
    _ver, count, storage = struct.unpack(">HHH", buf[off:off + 6])
    out = {}
    for i in range(count):
        rec = off + 6 + i * 12
        pid, _eid, _lid, nid, ln, so = struct.unpack(">HHHHHH", buf[rec:rec + 12])
        s = buf[off + storage + so: off + storage + so + ln]
        try:
            txt = s.decode("utf-16-be") if pid == 3 else s.decode("latin-1")
        except Exception:
            continue
        # Windows/English records win where a name is present on several platforms.
        if nid not in out or pid == 3:
            out[nid] = txt
    return out


def read_fvar(buf, off):
    (_major, _minor, axes_off, _res, axis_count, axis_size,
     inst_count, inst_size) = struct.unpack(">HHHHHHHH", buf[off:off + 16])
    axes = []
    for i in range(axis_count):
        a = off + axes_off + i * axis_size
        tag, mn, df, mx, _flags, name_id = struct.unpack(">4siiiHH", buf[a:a + 20])
        axes.append({"tag": tag.decode("latin-1"), "min": mn / 65536.0,
                     "default": df / 65536.0, "max": mx / 65536.0, "nameID": name_id})
    insts = []
    base = off + axes_off + axis_count * axis_size
    for i in range(inst_count):
        p = base + i * inst_size
        sub_id, _flags = struct.unpack(">HH", buf[p:p + 4])
        coords = [struct.unpack(">i", buf[p + 4 + j * 4: p + 8 + j * 4])[0] / 65536.0
                  for j in range(axis_count)]
        insts.append({"nameID": sub_id, "coords": coords})
    return axes, insts


def report(path):
    buf = open(path, "rb").read()
    if buf[:5] == b"versi":
        raise SystemExit(f"{path} is an LFS POINTER, not a font. Restore it by checkout (smudge), "
                         "never by cat-file, and verify by loading it.")
    tables = read_tables(buf)
    names = read_names(buf, tables["name"][0]) if "name" in tables else {}

    print("=" * 78)
    print(path)
    print("=" * 78)
    print(f"  tables: {len(tables)}   variable: {'YES (fvar present)' if 'fvar' in tables else 'no'}")
    print()
    print("  --- what the DEFAULT face reports (this is what faceIndex 0 gives you) ---")
    print(f"    nameID 1  family              : {names.get(1)!r}")
    print(f"    nameID 2  subfamily           : {names.get(2)!r}")
    print(f"    nameID 16 typographic family  : {names.get(16)!r}")
    print(f"    nameID 17 typographic subfam  : {names.get(17)!r}")
    print(f"    nameID 6  postscript          : {names.get(6)!r}")
    if "OS/2" in tables:
        o = tables["OS/2"][0]
        weight, width = struct.unpack(">HH", buf[o + 4:o + 8])
        print(f"    OS/2 usWeightClass          : {weight}   usWidthClass: {width}")

    if "fvar" in tables:
        axes, insts = read_fvar(buf, tables["fvar"][0])
        print()
        print("  --- axes ---")
        for a in axes:
            print(f"    {a['tag']}  min {a['min']:g}  DEFAULT {a['default']:g}  max {a['max']:g}")
        print()
        print(f"  --- {len(insts)} named instances (resolve BY THESE NAMES, never by index) ---")
        tags = [a["tag"] for a in axes]
        for i, ins in enumerate(insts):
            nm = names.get(ins["nameID"], f"<nameID {ins['nameID']}>")
            coord = "  ".join(f"{t}={c:g}" for t, c in zip(tags, ins["coords"]))
            dflt = all(abs(c - a["default"]) < 1e-6 for c, a in zip(ins["coords"], axes))
            print(f"    [{i:>2}] {nm:<28} {coord}{'   <-- AT THE AXIS DEFAULT' if dflt else ''}")
        print()
        print("  FreeType face index for named instance k is (k+1) << 16; index 0 is the default.")
    print()


if __name__ == "__main__":
    if len(sys.argv) < 2:
        raise SystemExit(__doc__)
    for p in sys.argv[1:]:
        report(p)
