/* The TV's one law: brightness is the primary semantic channel, hue is secondary.
   At most ONE L4 element exists on the surface at any instant. If two things want full brightness,
   the design has not decided what matters. */
export const TIER = { L4: 1, L3: 0.7, L2: 0.4, L1: 0.15, L0: 0 };
export const tier = (level) => (TIER[level] == null ? 1 : TIER[level]);
