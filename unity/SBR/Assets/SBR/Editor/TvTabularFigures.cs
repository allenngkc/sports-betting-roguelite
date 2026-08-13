using System.Collections.Generic;
using System.IO;

namespace SBR.EditorTools
{
    /// <summary>
    /// Reads a TrueType file's <c>tnum</c> (tabular figures) substitution out of its GSUB table and
    /// returns it as default-glyph → tabular-glyph. T82's mechanism, in the form the DD granted:
    /// the substitution is RESOLVED AT BUILD TIME from the committed font, not baked into a frozen
    /// static instance that nobody could re-derive.
    ///
    /// <para><b>Why this exists at all.</b> Owning doc §4 makes tabular figures mandatory. The glyphs
    /// are in the file — <c>tnum</c> is in the GSUB of both TV faces — but TextMeshPro cannot ask for
    /// them: <c>OTL_FeatureTag</c> declares only <c>kern</c>, <c>liga</c>, <c>mark</c> and
    /// <c>mkmk</c>. So the feature is applied here, once, at generation, by pointing the digit
    /// characters at the glyphs the feature would have selected.</para>
    ///
    /// <para><b>Why it re-derives instead of hardcoding.</b> A table of glyph indices would be a set
    /// of magic numbers that rot the first time the font is updated and the glyph order changes —
    /// the same objection the generator's own docstring raises against selecting faces by index. This
    /// reads the font that is actually being built, every build.</para>
    ///
    /// <para>Deliberately narrow: it handles the lookup type <c>tnum</c> is expressed in — SingleSubst
    /// (type 1), both formats, both coverage formats — and reports anything else rather than guessing.
    /// A font whose tabular figures arrive some other way must fail loudly here, not silently ship
    /// proportional digits while claiming otherwise.</para>
    /// </summary>
    internal static class TvTabularFigures
    {
        private sealed class Reader
        {
            private readonly byte[] _b;
            public Reader(byte[] b) { _b = b; }
            public ushort U16(int o) => (ushort)((_b[o] << 8) | _b[o + 1]);
            public short S16(int o) => (short)((_b[o] << 8) | _b[o + 1]);
            public uint U32(int o) => (uint)((_b[o] << 24) | (_b[o + 1] << 16) | (_b[o + 2] << 8) | _b[o + 3]);
            public string Tag(int o) => System.Text.Encoding.ASCII.GetString(_b, o, 4);
            public int Length => _b.Length;
        }

        /// <summary>default glyph id → tabular glyph id, empty if the font declares no `tnum`.
        /// <paramref name="note"/> always says what happened, so a caller can log the reason a map is
        /// empty rather than discovering it as a measurement later.</summary>
        public static Dictionary<uint, uint> ReadTnumMap(string ttfPath, out string note)
        {
            var map = new Dictionary<uint, uint>();
            if (!File.Exists(ttfPath)) { note = $"font not found: {ttfPath}"; return map; }

            var r = new Reader(File.ReadAllBytes(ttfPath));
            int gsub = -1;
            int numTables = r.U16(4);
            for (int i = 0; i < numTables; i++)
            {
                int rec = 12 + i * 16;
                if (r.Tag(rec) == "GSUB") { gsub = (int)r.U32(rec + 8); break; }
            }
            if (gsub < 0) { note = "no GSUB table"; return map; }

            int featureListOff = gsub + r.U16(gsub + 6);
            int lookupListOff = gsub + r.U16(gsub + 8);

            // Collect every lookup index reached by a feature tagged `tnum`. A feature can appear once
            // per script/language, so the same lookup may arrive several times; the set dedupes.
            var lookupIndices = new HashSet<ushort>();
            int featureCount = r.U16(featureListOff);
            for (int i = 0; i < featureCount; i++)
            {
                int fr = featureListOff + 2 + i * 6;
                if (r.Tag(fr) != "tnum") continue;
                int feature = featureListOff + r.U16(fr + 4);
                int n = r.U16(feature + 2);
                for (int k = 0; k < n; k++) lookupIndices.Add(r.U16(feature + 4 + k * 2));
            }
            if (lookupIndices.Count == 0) { note = "GSUB present but declares no `tnum`"; return map; }

            int skippedNonSingle = 0;
            foreach (ushort li in lookupIndices)
            {
                int lookup = lookupListOff + r.U16(lookupListOff + 2 + li * 2);
                int type = r.U16(lookup);
                int subCount = r.U16(lookup + 4);
                if (type != 1) { skippedNonSingle++; continue; }

                for (int s = 0; s < subCount; s++)
                {
                    int sub = lookup + r.U16(lookup + 6 + s * 2);
                    int substFormat = r.U16(sub);
                    int coverage = sub + r.U16(sub + 2);
                    var covered = ReadCoverage(r, coverage);

                    if (substFormat == 1)
                    {
                        short delta = r.S16(sub + 4);
                        foreach (uint g in covered) map[g] = (uint)((g + delta) & 0xFFFF);
                    }
                    else if (substFormat == 2)
                    {
                        int glyphCount = r.U16(sub + 4);
                        for (int k = 0; k < glyphCount && k < covered.Count; k++)
                            map[covered[k]] = r.U16(sub + 6 + k * 2);
                    }
                }
            }

            note = skippedNonSingle == 0
                ? $"{map.Count} substitutions from {lookupIndices.Count} lookup(s)"
                : $"{map.Count} substitutions, but {skippedNonSingle} `tnum` lookup(s) were NOT " +
                  "SingleSubst and were skipped — this reader does not guess at other lookup types";
            return map;
        }

        /// <summary>Coverage table, both formats, in coverage-index order — which is the order
        /// SingleSubstFormat2's substitute array is indexed by.</summary>
        private static List<uint> ReadCoverage(Reader r, int off)
        {
            var glyphs = new List<uint>();
            int format = r.U16(off);
            if (format == 1)
            {
                int n = r.U16(off + 2);
                for (int i = 0; i < n; i++) glyphs.Add(r.U16(off + 4 + i * 2));
            }
            else if (format == 2)
            {
                int n = r.U16(off + 2);
                for (int i = 0; i < n; i++)
                {
                    int rec = off + 4 + i * 6;
                    uint start = r.U16(rec), end = r.U16(rec + 2);
                    for (uint g = start; g <= end; g++) glyphs.Add(g);
                }
            }
            return glyphs;
        }
    }
}
