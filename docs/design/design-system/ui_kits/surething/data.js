/* The Round 3 slate from SHARED-SPEC.md, plus the event-detail markets the engine already prices:
   Moneyline, TotalGoals, BothTeamsToScore, TotalCorners, TotalCards, AnytimeScorer.
   Fictional leagues, teams and players only — IP safety and the comedy both require it. */
window.STData = {
  slate: [
    { id: 1, no: "01", away: { name: "Yams", record: "7-2", price: "-145" }, home: { name: "Startups", record: "4-5", price: "+125" } },
    { id: 2, no: "02", away: { name: "Mallards", record: "3-6", price: "+210" }, home: { name: "Bricklayers", record: "8-1", price: "-260" } },
    { id: 3, no: "03", away: { name: "Nighthawks", record: "5-4", price: "+135" }, home: { name: "Foundry", record: "6-3", price: "-155" } },
    { id: 4, no: "04", away: { name: "Longhaulers", record: "6-3", price: "+180" }, home: { name: "Tidewater", record: "2-7", price: "-215" } },
    { id: 5, no: "05", away: { name: "Saltmen", record: "4-5", price: "-110" }, home: { name: "Junction", record: "5-4", price: "-110" } },
    { id: 6, no: "06", away: { name: "Kestrels", record: "8-1", price: "-300" }, home: { name: "Pressmen", record: "3-6", price: "+240" } }
  ],
  destinations: ["GOALS", "BTTS", "CORNERS", "CARDS", "PLAYERS"],
  markets: {
    GOALS: [
      { line: "Over 1.5 goals", price: "-190" }, { line: "Under 1.5 goals", price: "+155" },
      { line: "Over 2.5 goals", price: "-110" }, { line: "Under 2.5 goals", price: "-110" },
      { line: "Over 3.5 goals", price: "+185" }, { line: "Under 3.5 goals", price: "-225" }
    ],
    BTTS: [{ line: "Both teams to score — Yes", price: "-105" }, { line: "Both teams to score — No", price: "-115" }],
    CORNERS: [
      { line: "Over 8.5 corners", price: "+100" }, { line: "Under 8.5 corners", price: "-125" },
      { line: "Over 10.5 corners", price: "+195" }, { line: "Under 10.5 corners", price: "-240" }
    ],
    CARDS: [
      { line: "Over 3.5 cards", price: "-130" }, { line: "Under 3.5 cards", price: "+105" },
      { line: "Over 4.5 cards", price: "+165" }, { line: "Under 4.5 cards", price: "-200" }
    ],
    PLAYERS: [
      { line: "Marcus Vale anytime", price: "+210" }, { line: "Osric Kean anytime", price: "+265" },
      { line: "Dennis Prole anytime", price: "+320" }, { line: "Ivo Tanager anytime", price: "+410" }
    ]
  },
  offers: [
    { name: "Vig Rebate", price: "$140", description: "Returns 4% of every losing stake to the bank at round settlement.", affordable: true },
    { name: "The Shill's Notebook", price: "$260", description: "Reveals one guru's pick per round. The guru is wrong more often than not.", affordable: true },
    { name: "Late Line", price: "$420", description: "One selection per round may be swapped after the round is locked.", affordable: false, reason: "BANK TOO LOW" },
    { name: "Accounting Error", price: "$180", description: "A missed target books the shortfall at 1.35x instead of 1.5x.", affordable: true, owned: true }
  ],
  ledger: [
    { number: "R2 · TICKET 02", legs: "Kestrels ML −300 · Under 2.5 +105", terminal: "LOST", stake: "$180", payout: "$0" },
    { number: "R2 · TICKET 01", legs: "Foundry ML −155 · BTTS Yes −105", terminal: "WON", stake: "$120", payout: "$396" },
    { number: "R1 · TICKET 01", legs: "Junction ML −110 · Over 8.5 corners +100", terminal: "CASHED OUT", stake: "$100", payout: "$164" }
  ]
};
