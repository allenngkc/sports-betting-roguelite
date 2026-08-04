const { OsRail, SectionTabs, Masthead, OsTray } = window.SureThingDesignSystem_6e1eb3;
const { WorkingMargin, PassiveMargin } = window;
const { FormScreen, EntryScreen, MyBetsScreen, RewardsScreen, LedgerScreen } = window;

const TABS = ["FORM", "ENTRY", "MY BETS", "REWARDS"];
const TARGET = 1900;

function App() {
  const [tab, setTab] = React.useState("FORM");
  const [tray, setTray] = React.useState("SURETHING");
  const [selections, setSelections] = React.useState({
    2: { side: "home", market: "MONEYLINE", team: "Bricklayers", price: "-260", entry: "02", id: 2 },
    4: { side: "away", market: "MONEYLINE", team: "Longhaulers", price: "+180", entry: "04", id: 4 }
  });
  const [stake, setStake] = React.useState(200);
  const [tickets, setTickets] = React.useState([]);
  const [detail, setDetail] = React.useState({ matchup: window.STData.slate[1], destination: "GOALS" });
  const [locked, setLocked] = React.useState(false);
  const [revealed, setRevealed] = React.useState({});
  const [shopResult, setShopResult] = React.useState(null);

  const bank = 1340;
  const legs = Object.values(selections);

  const select = (m, side) => {
    const t = side === "away" ? m.away : m.home;
    setSelections((s) => ({ ...s, [m.id]: { id: m.id, side, market: "MONEYLINE", team: t.name, price: t.price, entry: m.no } }));
  };
  const selectMarket = (mk) => {
    const m = detail.matchup;
    setSelections((s) => ({ ...s, [m.id]: { id: m.id, side: null, market: detail.destination, line: mk.line, team: mk.line, price: mk.price, entry: m.no } }));
  };
  const remove = (id) => setSelections((s) => { const n = { ...s }; delete n[id]; return n; });
  const fraction = (f) => {
    const pct = f === "MAX" ? 1 : parseInt(f, 10) / 100;
    setStake(Math.max(10, Math.round(bank * pct)));
  };
  const nudge = (n) => setStake((s) => Math.max(10, Math.min(bank, s + (n.indexOf("+") >= 0 ? 10 : -10))));

  const place = () => {
    if (!legs.length) return;
    setTickets((t) => t.concat([{
      number: "TICKET " + String(t.length + 1).padStart(2, "0"),
      legs: legs.map((l) => ({ team: l.team, market: l.market === "MONEYLINE" ? "MONEYLINE" : l.market, price: l.price })),
      stake: window.STMath.money(stake),
      combined: window.STMath.combinedAmerican(legs),
      payout: window.STMath.payout(legs, stake)
    }]));
    setSelections({});
  };

  const lock = () => {
    setLocked(true);
    setTab("MY BETS");
    const seq = [];
    tickets.forEach((t) => t.legs.forEach((_, i) => seq.push(t.number + "/" + i)));
    seq.forEach((k, i) => {
      window.setTimeout(() => setRevealed((r) => ({ ...r, [k]: "LIVE" })), 700 + i * 900);
      window.setTimeout(() => setRevealed((r) => ({ ...r, [k]: i % 3 === 2 ? "DEAD" : "GREEN" })), 1500 + i * 900);
    });
  };

  const reset = () => {
    setLocked(false); setRevealed({}); setTickets([]); setTab("FORM");
    setSelections({
      2: { side: "home", market: "MONEYLINE", team: "Bricklayers", price: "-260", entry: "02", id: 2 },
      4: { side: "away", market: "MONEYLINE", team: "Longhaulers", price: "+180", entry: "04", id: 4 }
    });
    setStake(200);
  };

  const figures = [
    { label: "BANK", value: window.STMath.money(bank) },
    { label: "TARGET", value: window.STMath.money(TARGET), tone: "wax" },
    { label: "RELICS", value: "2/5" },
    { label: "TICKETS", value: tickets.length + "/3" }
  ];

  const activeTab = tray === "LEDGER" ? "LEDGER" : tab;
  const body = () => {
    if (tray === "LEDGER") return <LedgerScreen />;
    if (tab === "ENTRY") return <EntryScreen matchup={detail.matchup} destination={detail.destination}
      onDestination={(d) => setDetail((x) => ({ ...x, destination: d }))}
      selection={selections[detail.matchup.id]} onSelectMarket={selectMarket}
      onBack={() => setTab("FORM")} tickets={tickets} />;
    if (tab === "MY BETS") return <MyBetsScreen tickets={tickets} revealed={revealed} />;
    if (tab === "REWARDS") return <RewardsScreen result={shopResult}
      onBuy={(o) => setShopResult(o.name.toUpperCase() + " BOUGHT — BANK REDUCED BY " + o.price)} />;
    return <FormScreen selections={selections} onSelect={select}
      onMore={(m) => { setDetail({ matchup: m, destination: "GOALS" }); setTab("ENTRY"); }} />;
  };

  const margin = () => {
    if (tray === "LEDGER") return <PassiveMargin title="Record" rows={[
      { label: "TICKETS SETTLED", value: "3" }, { label: "STAKED", value: "$400" },
      { label: "RETURNED", value: "$560", tone: "wax" }]}
      note="Read-only. The ledger copies settled tickets and derives nothing." />;
    if (tab === "MY BETS") return <PassiveMargin title="This round" rows={[
      { label: "TICKETS", value: tickets.length + "/3" },
      { label: "AT RISK", value: tickets.length ? tickets[0].stake : "$0" },
      { label: "IF EVERYTHING LANDS", value: tickets.length ? tickets[0].payout : "$0", tone: "wax" }]}
      note="No stake, selection or lock controls during the sweat. The board is frozen." />;
    if (tab === "REWARDS") return <PassiveMargin title="Run context" rows={[
      { label: "BANK", value: window.STMath.money(bank) },
      { label: "RELICS HELD", value: "2/5" }, { label: "ROUND", value: "3 OF 8" }]}
      note="Buying updates the existing tally. Errors are literal and are not given a remedy the engine did not supply." />;
    return <WorkingMargin legs={legs} stake={stake} tickets={tickets} locked={locked}
      onRemove={remove} onFraction={fraction} onNudge={nudge} onPlace={place} onLock={lock}
      onSkip={() => setTickets([])} />;
  };

  return (
    <div>
      <div style={{ width: 1024, height: 704, position: "relative", overflow: "hidden",
        background: "var(--ground)", color: "var(--toner)", boxShadow: "0 24px 64px rgba(0,0,0,.75)" }}>
        <div aria-hidden="true" style={{ position: "absolute", inset: 0, pointerEvents: "none", zIndex: 9,
          opacity: "var(--toner-grain-opacity)",
          backgroundImage: "url(\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='140' height='140'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='3'/%3E%3C/filter%3E%3Crect width='140' height='140' filter='url(%23n)'/%3E%3C/svg%3E\")" }} />
        <OsRail clock="02:47" />
        <SectionTabs tabs={TABS} active={activeTab} onSelect={(t) => { setTray("SURETHING"); setTab(t); }}
          meta={tray === "LEDGER" ? "READ ONLY" : "SHEET 1 OF 1"} />
        <Masthead title={tray === "LEDGER" ? "Ledger" : "SureThing Form"}
          dateline={"ROUND 3 OF 8 · SEED 8F3K-22 · " + (tray === "LEDGER" ? "SETTLED RECORD" : locked ? "ROUND LOCKED" : "PRICES FINAL")}
          figures={figures}
          note={tray === "LEDGER"
            ? "Every ticket this run, after it settled. Nothing here can be changed."
            : locked ? "The board is frozen. The TV owns the rest."
            : "Prices are final. Nothing you do moves them."} />
        <div style={{ height: 530, display: "flex", position: "relative", zIndex: 2 }}>
          {body()}{margin()}
        </div>
        <OsTray apps={[
          { label: "SURETHING", active: tray === "SURETHING" },
          { label: "LEDGER", active: tray === "LEDGER" },
          { label: "MESSAGES", badge: "1" }]}
          onSelect={(l) => { if (l === "LEDGER" || l === "SURETHING") setTray(l); }} />
      </div>
      <div style={{ width: 1024, display: "flex", alignItems: "center", gap: 14, marginTop: 12,
        fontFamily: "var(--font-data)", fontSize: 12, letterSpacing: ".1em", color: "var(--toner-3)" }}>
        <span>1024 × 704 · UNITY UGUI CANVAS ON A WORLD-SPACE LAPTOP · READ AT AN ANGLE</span>
        <button type="button" onClick={reset} style={{ marginLeft: "auto", height: 26, padding: "0 12px",
          background: "transparent", border: "1px solid var(--rule)", color: "var(--toner-3)",
          fontFamily: "var(--font-data)", fontSize: 12, letterSpacing: ".1em", cursor: "pointer" }}>RESET KIT</button>
      </div>
    </div>
  );
}

ReactDOM.createRoot(document.getElementById("root")).render(<App />);
