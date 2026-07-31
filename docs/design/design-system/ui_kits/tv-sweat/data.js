/* One ticket swept beat by beat. Every figure here would come from the engine's revealed payload in
   the real product; the TV is the only surface allowed to show score, clock and probability at all.
   Fictional teams and players only. */
window.TVData = {
  away: "Pressmen", home: "Foundry",
  ticket: { index: "1/2", risk: "$50", pays: "$462" },
  /* Statements are VISUAL-DESIGN §6's lines verbatim. The scorer line wraps to two, which §3 allows. */
  legs: [
    { market: "MONEYLINE", price: "-155", statement: "Foundry to win", backed: "home" },
    { market: "TOTAL GOALS", price: "-110", statement: "Over 2.5 goals" },
    { market: "ANYTIME SCORER", price: "+210", statement: "Marcus Vale to score" }
  ],
  beats: [
    { clock: "PRE", score: [0, 0], legIndex: "1/3", event: "Teams out — kick-off shortly", momentum: [0, 0, 0, 0, 0, 0, 0, 0],
      states: ["NEXT", "NEXT", "NEXT"], progress: [null, null, null],
      cash: { state: "unavailable" },
      actors: [{ x: 30, y: 46, team: "a" }, { x: 44, y: 50, team: "b", number: "9" }, { x: 62, y: 44, team: "b" }], ball: { x: 50, y: 50 } },
    { clock: "12'", score: [0, 0], legIndex: "1/3", event: "Foundry build through the middle", momentum: [0, .1, .2, .1, .3, .4, .3, .5],
      states: ["LIVE", "LIVE", "LIVE"],
      progress: ["LIVE • LEVEL 0–0", "LIVE • 0 GOALS • 3 MORE", "LIVE • WAITING FOR VALE"],
      cash: { state: "actionable", amount: "$148" },
      actors: [{ x: 34, y: 40, team: "a" }, { x: 48, y: 52, team: "b", number: "9" }, { x: 58, y: 38, team: "b" }, { x: 70, y: 56, team: "a" }], ball: { x: 49, y: 51 } },
    { clock: "34'", score: [0, 1], legIndex: "2/3", event: "Vale finds the net", goal: true, momentum: [.2, .1, .3, .4, .3, .5, .7, .9],
      states: ["LIVE", "LIVE", "W"],
      progress: ["LIVE • LEADING 1–0", "LIVE • 1 GOAL • 2 MORE", null],
      cash: { state: "updating", amount: "$212" },
      actors: [{ x: 62, y: 44, team: "b", number: "9" }, { x: 74, y: 38, team: "b" }, { x: 80, y: 52, team: "a" }], ball: { x: 88, y: 47, payoff: true } },
    { clock: "58'", score: [1, 1], legIndex: "2/3", event: "Pressmen equalise from the spot", goal: true, momentum: [.5, .3, 0, -.3, -.5, -.7, -.8, -.6],
      states: ["LIVE", "LIVE", "W"],
      progress: ["LIVE • LEVEL 1–1", "LIVE • 2 GOALS • 1 MORE", null],
      cash: { state: "updating", amount: "$176" },
      actors: [{ x: 24, y: 48, team: "a" }, { x: 40, y: 42, team: "a" }, { x: 56, y: 54, team: "b", number: "9" }], ball: { x: 18, y: 48 } },
    { clock: "71'", score: [1, 1], legIndex: "2/3", event: "VAR — checking the second goal", momentum: [.3, 0, -.3, -.5, -.7, -.6, -.4, -.2],
      states: ["LIVE", "LIVE", "W"],
      progress: ["LIVE • LEVEL 1–1", "LIVE • 2 GOALS • 1 MORE", null],
      cash: { state: "suspended" },
      actors: [{ x: 40, y: 44, team: "a" }, { x: 52, y: 50, team: "b", number: "9" }], ball: { x: 46, y: 47 } },
    { clock: "78'", score: [1, 2], legIndex: "3/3", event: "Foundry back in front — cutback finished", goal: true, momentum: [-.5, -.3, 0, .2, .5, .7, .8, 1],
      states: ["LIVE", "W", "W"],
      progress: ["LIVE • LEADING 2–1", null, null],
      cash: { state: "updating", amount: "$340" },
      actors: [{ x: 66, y: 40, team: "b", number: "9" }, { x: 78, y: 52, team: "b" }, { x: 84, y: 44, team: "a" }], ball: { x: 90, y: 50, payoff: true } },
    { clock: "FT", score: [1, 2], legIndex: "3/3", event: "Full time", momentum: [0, .2, .5, .7, .8, 1, .6, .3],
      states: ["W", "W", "W"], progress: [null, null, null],
      cash: { state: "unavailable" },
      actors: [{ x: 48, y: 46, team: "b", number: "9" }, { x: 56, y: 52, team: "a" }], ball: { x: 50, y: 50 } }
  ],
  stats: [
    { label: "SHOTS", away: "7", home: "14" },
    { label: "SHOTS ON TARGET", away: "2", home: "6" },
    { label: "CORNERS", away: "3", home: "8" },
    { label: "CARDS", away: "2", home: "1" }
  ],
  nextTicket: { heading: "TICKET 2 OF 2", risk: "$50", pays: "$462",
    legs: ["JUNCTION ML -110", "BTTS YES -105", "OVER 8.5 CORNERS +100"] }
};
