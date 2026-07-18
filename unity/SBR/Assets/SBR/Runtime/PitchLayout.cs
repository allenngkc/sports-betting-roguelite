using UnityEngine;

namespace SBR.Game
{
    /// <summary>
    /// The stage's geometry (F_0.2.0 M-T2). Normalized pitch space: x 0→1 runs left→right,
    /// y 0→1 bottom→top. Formation slots are authored in the ATTACKING-RIGHT frame (own goal
    /// at x=0) and mirrored for the team attacking left. The PICKED team always attacks
    /// RIGHT — one consistent reading: the ball near the right goal is money coming home.
    /// 8 outfield dots per side (readability at the couch's seated FOV beats 11-a-side
    /// authenticity — a dial ratified at the M-T2 gate) in a 3-3-2, plus a keeper.
    /// </summary>
    public static class PitchLayout
    {
        public const int OutfieldPerTeam = 8;

        // 3-3-2 in the attacking-right frame.
        private static readonly Vector2[] Slots =
        {
            new Vector2(0.18f, 0.25f), new Vector2(0.16f, 0.50f), new Vector2(0.18f, 0.75f), // back line
            new Vector2(0.42f, 0.20f), new Vector2(0.40f, 0.50f), new Vector2(0.42f, 0.80f), // midfield
            new Vector2(0.66f, 0.35f), new Vector2(0.66f, 0.65f),                             // front two
        };

        private static readonly Vector2 KeeperSlot = new Vector2(0.045f, 0.5f);

        /// <summary>
        /// The formation anchor for an outfield dot. <paramref name="attackBias"/> in [-1, 1]
        /// shifts the whole shape toward the opponent goal (+) or its own (−) by up to 0.16
        /// of the pitch — territory made spatial.
        /// </summary>
        public static Vector2 FormationSlot(int index, bool attacksRight, float attackBias)
        {
            Vector2 s = Slots[Mathf.Clamp(index, 0, Slots.Length - 1)];
            s.x = Mathf.Clamp(s.x + attackBias * 0.16f, 0.06f, 0.94f);
            if (!attacksRight) s.x = 1f - s.x;
            return s;
        }

        public static Vector2 Keeper(bool attacksRight)
            => attacksRight ? KeeperSlot : new Vector2(1f - KeeperSlot.x, KeeperSlot.y);

        /// <summary>Territory center for a live win probability of the picked (attacking-right)
        /// side: 0.5 → midfield, high prob → camped in the opponent third.</summary>
        public static float TerritoryX(float pickedProb)
            => Mathf.Lerp(0.22f, 0.78f, Mathf.Clamp01(pickedProb));
    }
}
