#!/usr/bin/env python3
"""
Derive a tabular-figures TrueType file from a committed source font.

T82 (batch 38): the owning doc makes tabular figures mandatory; the glyphs are in the file, reachable
only through the `tnum` GSUB feature; and TextMeshPro cannot ask for one — its OTL_FeatureTag declares
only kern, liga, mark and mkmk. The substitution therefore has to be resolved before TMP sees the
font. This resolves it.

WHAT IT CHANGES, AND WHAT IT DELIBERATELY DOES NOT
    Only `cmap`. The ten digit codepoints U+0030..U+0039 are re-pointed at the glyph ids `tnum`
    substitutes them with; every other codepoint keeps the glyph it had. Nothing else in the file is
    touched — not `glyf`, not `loca`, not `hmtx`, not `fvar`/`gvar`/`HVAR`.

    That restraint is the whole design. Glyph ids stay put, so every table keyed by glyph id — above
    all a variable font's variation deltas — stays correct by construction. Swapping outlines or
    metrics between glyph ids would have desynced the variation data silently, and this family is
    variable with 45 named instances that the build selects by style name.

WHY A DERIVED FILE RATHER THAN PATCHING THE FONT ASSET
    Tried first, and it cannot work: a Dynamic TMP font asset does not serialize its character or
    glyph tables at all. It ships empty and populates at runtime, so anything written into those
    tables at build time is discarded on save — `m_Unicode` entries in the saved asset: 0. Measured,
    not assumed.

WHY THIS IS NOT THE "FROZEN INSTANCE" THE RULING REFUSED
    The refusal was of a binary nobody can re-derive. This script IS the derivation, it is committed
    beside the source font it reads, and it re-runs to a byte-identical result at any time. The output
    is a build product with its provenance in the repo, not an artifact from somebody's tool session.

USAGE
    python tools/tnum_font.py <source.ttf> <output.ttf>
    python tools/tnum_font.py \
        unity/SBR/Assets/SBR/Resources/Tv/Fonts/EncodeSans.ttf \
        unity/SBR/Assets/SBR/Resources/Tv/Fonts/EncodeSans-Tabular.ttf

    Exits non-zero, loudly, if the font declares no `tnum` or if any of the ten digits has no
    substitution. Nine of ten tabular is not tabular.
"""
import struct
import sys

DIGITS = [ord(c) for c in "0123456789"]


# ----------------------------------------------------------------- sfnt plumbing
def read_tables(buf):
    if buf[:4] == b"ttcf":
        raise SystemExit("TrueType collection - not handled")
    num = struct.unpack(">H", buf[4:6])[0]
    out = {}
    for i in range(num):
        o = 12 + i * 16
        tag, _cs, off, ln = struct.unpack(">4sIII", buf[o:o + 16])
        out[tag.decode("latin-1")] = (off, ln)
    return out


def checksum(data):
    if len(data) % 4:
        data = data + b"\0" * (4 - len(data) % 4)
    total = 0
    for i in range(0, len(data), 4):
        total = (total + struct.unpack(">I", data[i:i + 4])[0]) & 0xFFFFFFFF
    return total


# ----------------------------------------------------------------- cmap in
def parse_cmap(buf, off):
    """Every (codepoint -> glyph) the font declares, merged across its subtables."""
    n = struct.unpack(">H", buf[off + 2:off + 4])[0]
    mapping = {}
    for i in range(n):
        p = off + 4 + i * 8
        _pid, _eid, sub = struct.unpack(">HHI", buf[p:p + 8])
        o = off + sub
        fmt = struct.unpack(">H", buf[o:o + 2])[0]
        if fmt == 4:
            seg2 = struct.unpack(">H", buf[o + 6:o + 8])[0]
            seg = seg2 // 2
            ends = [struct.unpack(">H", buf[o + 14 + 2 * k:o + 16 + 2 * k])[0] for k in range(seg)]
            sp = o + 16 + seg2
            starts = [struct.unpack(">H", buf[sp + 2 * k:sp + 2 + 2 * k])[0] for k in range(seg)]
            dp = sp + seg2
            deltas = [struct.unpack(">h", buf[dp + 2 * k:dp + 2 + 2 * k])[0] for k in range(seg)]
            rp = dp + seg2
            ranges = [struct.unpack(">H", buf[rp + 2 * k:rp + 2 + 2 * k])[0] for k in range(seg)]
            for k in range(seg):
                for c in range(starts[k], min(ends[k], 0xFFFF) + 1):
                    if ranges[k] == 0:
                        g = (c + deltas[k]) & 0xFFFF
                    else:
                        gp = rp + 2 * k + ranges[k] + 2 * (c - starts[k])
                        if gp + 2 > len(buf):
                            continue
                        g = struct.unpack(">H", buf[gp:gp + 2])[0]
                        if g:
                            g = (g + deltas[k]) & 0xFFFF
                    if g:
                        mapping[c] = g
        elif fmt == 12:
            ngroups = struct.unpack(">I", buf[o + 12:o + 16])[0]
            for k in range(ngroups):
                gp = o + 16 + k * 12
                s, e, sg = struct.unpack(">III", buf[gp:gp + 12])
                for c in range(s, e + 1):
                    mapping[c] = sg + (c - s)
    return mapping


# ----------------------------------------------------------------- GSUB tnum
def read_coverage(buf, off):
    fmt = struct.unpack(">H", buf[off:off + 2])[0]
    glyphs = []
    if fmt == 1:
        n = struct.unpack(">H", buf[off + 2:off + 4])[0]
        for i in range(n):
            glyphs.append(struct.unpack(">H", buf[off + 4 + i * 2:off + 6 + i * 2])[0])
    elif fmt == 2:
        n = struct.unpack(">H", buf[off + 2:off + 4])[0]
        for i in range(n):
            r = off + 4 + i * 6
            s, e, _si = struct.unpack(">HHH", buf[r:r + 6])
            glyphs.extend(range(s, e + 1))
    return glyphs


def read_tnum(buf, gsub_off):
    feature_list = gsub_off + struct.unpack(">H", buf[gsub_off + 6:gsub_off + 8])[0]
    lookup_list = gsub_off + struct.unpack(">H", buf[gsub_off + 8:gsub_off + 10])[0]

    lookups = set()
    count = struct.unpack(">H", buf[feature_list:feature_list + 2])[0]
    for i in range(count):
        rec = feature_list + 2 + i * 6
        if buf[rec:rec + 4] != b"tnum":
            continue
        feat = feature_list + struct.unpack(">H", buf[rec + 4:rec + 6])[0]
        n = struct.unpack(">H", buf[feat + 2:feat + 4])[0]
        for k in range(n):
            lookups.add(struct.unpack(">H", buf[feat + 4 + k * 2:feat + 6 + k * 2])[0])

    subst, skipped = {}, 0
    for li in sorted(lookups):
        lk = lookup_list + struct.unpack(">H", buf[lookup_list + 2 + li * 2:lookup_list + 4 + li * 2])[0]
        ltype = struct.unpack(">H", buf[lk:lk + 2])[0]
        nsub = struct.unpack(">H", buf[lk + 4:lk + 6])[0]
        if ltype != 1:
            skipped += 1
            continue
        for s in range(nsub):
            sub = lk + struct.unpack(">H", buf[lk + 6 + s * 2:lk + 8 + s * 2])[0]
            fmt = struct.unpack(">H", buf[sub:sub + 2])[0]
            cov = read_coverage(buf, sub + struct.unpack(">H", buf[sub + 2:sub + 4])[0])
            if fmt == 1:
                delta = struct.unpack(">h", buf[sub + 4:sub + 6])[0]
                for g in cov:
                    subst[g] = (g + delta) & 0xFFFF
            elif fmt == 2:
                n = struct.unpack(">H", buf[sub + 4:sub + 6])[0]
                for k in range(min(n, len(cov))):
                    subst[cov[k]] = struct.unpack(">H", buf[sub + 6 + k * 2:sub + 8 + k * 2])[0]
    return subst, skipped


# ----------------------------------------------------------------- cmap out
def build_cmap(mapping):
    """One format-12 subtable, reached by both a Unicode and a Windows encoding record.

    Format 12 rather than 4 because it covers the whole repertoire with no BMP ceiling, and because
    its group encoding is simple enough to emit without a second implementation of segment packing.
    """
    codes = sorted(mapping)
    groups = []
    for c in codes:
        g = mapping[c]
        if groups and c == groups[-1][1] + 1 and g == groups[-1][2] + (c - groups[-1][0]):
            groups[-1][1] = c
        else:
            groups.append([c, c, g])

    body = struct.pack(">HHIII", 12, 0, 16 + 12 * len(groups), 0, len(groups))
    for s, e, g in groups:
        body += struct.pack(">III", s, e, g)

    n_records = 2
    sub_off = 4 + n_records * 8
    head = struct.pack(">HH", 0, n_records)
    head += struct.pack(">HHI", 0, 4, sub_off)    # Unicode, full repertoire
    head += struct.pack(">HHI", 3, 10, sub_off)   # Windows, UCS-4
    return head + body, len(groups)


def rebuild(buf, tables, new_cmap):
    out = {tag: bytearray(buf[off:off + ln]) for tag, (off, ln) in tables.items()}
    out["cmap"] = bytearray(new_cmap)

    # head.checkSumAdjustment must be zero while the file checksum is computed.
    if "head" not in out:
        raise SystemExit("no head table")
    struct.pack_into(">I", out["head"], 8, 0)

    tags = sorted(out)
    n = len(tags)
    entry_selector = max(0, (n).bit_length() - 1)
    search_range = 16 * (1 << entry_selector)
    offset_table = struct.pack(">IHHHH", 0x00010000, n, search_range, entry_selector, 16 * n - search_range)

    offset = len(offset_table) + 16 * n
    records, blobs, head_pos = [], [], None
    for tag in tags:
        data = bytes(out[tag])
        if tag == "head":
            head_pos = offset
        records.append((tag, checksum(data), offset, len(data)))
        blobs.append(data)
        padded = len(data) + ((4 - len(data) % 4) % 4)
        offset += padded

    file_bytes = bytearray(offset_table)
    for tag, cs, off, ln in records:
        file_bytes += struct.pack(">4sIII", tag.encode("latin-1"), cs, off, ln)
    for data in blobs:
        file_bytes += data
        file_bytes += b"\0" * ((4 - len(data) % 4) % 4)

    adjustment = (0xB1B0AFBA - checksum(bytes(file_bytes))) & 0xFFFFFFFF
    struct.pack_into(">I", file_bytes, head_pos + 8, adjustment)
    return bytes(file_bytes)


def main(src, dst):
    buf = open(src, "rb").read()
    if buf[:5] == b"versi":
        raise SystemExit(f"{src} is an LFS POINTER, not a font. Restore it by checkout, never by cat-file.")
    tables = read_tables(buf)
    if "GSUB" not in tables:
        raise SystemExit(f"{src} has no GSUB, so it declares no tnum")

    mapping = parse_cmap(buf, tables["cmap"][0])
    subst, skipped = read_tnum(buf, tables["GSUB"][0])
    if not subst:
        raise SystemExit(f"{src} declares no `tnum` substitutions")

    moved = []
    for cp in DIGITS:
        g = mapping.get(cp)
        if g is None:
            raise SystemExit(f"{src} has no glyph for U+{cp:04X}")
        t = subst.get(g)
        if t is None:
            raise SystemExit(f"U+{cp:04X} (glyph {g}) has no `tnum` substitution. "
                             "Nine of ten tabular is not tabular.")
        mapping[cp] = t
        moved.append(f"{chr(cp)}:{g}->{t}")

    new_cmap, groups = build_cmap(mapping)
    open(dst, "wb").write(rebuild(buf, tables, new_cmap))

    print(f"source : {src}")
    print(f"output : {dst}")
    print(f"tnum   : {len(subst)} substitutions" + (f", {skipped} non-SingleSubst lookup(s) skipped" if skipped else ""))
    print(f"digits : {' '.join(moved)}")
    print(f"cmap   : {len(mapping)} codepoints in {groups} format-12 groups")
    print("only cmap changed — glyph ids, metrics and variation data are byte-identical")


if __name__ == "__main__":
    if len(sys.argv) != 3:
        raise SystemExit(__doc__)
    main(sys.argv[1], sys.argv[2])
