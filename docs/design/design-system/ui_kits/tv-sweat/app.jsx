const { TvScorebug, TvLegRow, TvRiskPays, TvCashOutSlot, TvEventStrip, TvStage,
        TvStatsPanel, TvTicketCard, TvMomentumTape } = window.SureThingDesignSystem_6e1eb3;

const D = window.TVData;
const ctl = {
  height: 28, padding: "0 13px", background: "transparent", border: "1px solid var(--tv-rule)",
  color: "var(--tv-context)", fontFamily: "var(--font-tv)", fontSize: 12,
  letterSpacing: ".12em", cursor: "pointer", textTransform: "uppercase"
};

function App() {
  const [i, setI] = React.useState(1);
  const [stats, setStats] = React.useState(false);
  const [card, setCard] = React.useState(false);
  const [cashedOut, setCashedOut] = React.useState(null);
  const [punch, setPunch] = React.useState(true);
  const b = D.beats[i];
  /* DESIGN.md §7: if concurrency exceeds the column's height, resolved rows collapse first, then
     pending. A live row is never truncated. With this three-leg ticket the column never runs out of
     room — the one beat with three concurrent live legs has no resolved rows left to collapse — so
     this is a defensive capability, wired and ready for a taller ticket rather than exercised here. */
  const liveCount = b.states.filter((s) => s === "LIVE").length;
  /* VISUAL-DESIGN §5: a backed-team marker is honest for moneyline only. For totals, BTTS, corners,
     cards and scorer props the scorebug shows both identities and the rail says MARKET PICK. */
  const activeLeg = D.legs[Math.max(0, parseInt(b.legIndex, 10) - 1)] || D.legs[0];
  const underPressure = liveCount >= 3 || D.legs.length > 3;

  React.useEffect(() => {
    setPunch(true);
    const t = window.setTimeout(() => setPunch(false), 900);
    return () => window.clearTimeout(t);
  }, [i]);

  const cash = cashedOut ? { state: "accepted", amount: cashedOut } : b.cash;
  const advance = () => { setCard(false); setI((n) => Math.min(n + 1, D.beats.length - 1)); };
  const back = () => { setCard(false); setI((n) => Math.max(n - 1, 0)); };

  return (
    <div>
      <div style={{ width: 980, height: 550, position: "relative", overflow: "hidden",
        background: "var(--tv-substrate)", fontFamily: "var(--font-tv)",
        fontVariantNumeric: "tabular-nums", display: "flex" }}>
        <div style={{ width: "var(--tv-ticket-col-w)", display: "flex", flexDirection: "column",
          borderRight: "1px solid var(--tv-rule)", position: "relative" }}>
          <button type="button" onClick={() => setStats((s) => !s)}
            style={{ ...ctl, height: 34, border: 0, borderBottom: "1px solid var(--tv-rule)",
              textAlign: "left", fontSize: 15, letterSpacing: ".16em", color: "var(--tv-structure)" }}>
            TICKET {D.ticket.index} · STATS
          </button>
          {D.legs.map((l, n) => (
            <TvLegRow key={n} market={l.market} price={l.price} statement={l.statement}
              state={b.states[n]} progress={b.progress[n]} />
          ))}
          <div style={{ marginTop: "auto" }}>
            <TvRiskPays risk={D.ticket.risk} pays={D.ticket.pays} />
            <div onClick={() => { if (cash.state === "actionable") setCashedOut(cash.amount); }}
              style={{ cursor: cash.state === "actionable" ? "pointer" : "default" }}>
              <TvCashOutSlot state={cash.state} amount={cash.amount} />
            </div>
          </div>
          {stats && (
            <TvStatsPanel away={D.away} home={D.home} rows={D.stats} onClose={() => setStats(false)} />
          )}
        </div>
        <div style={{ flex: 1, display: "flex", flexDirection: "column", position: "relative" }}>
          {/* The beat IS the goal callback here, so the score holds L4 for the goal beat rather than
              for a timing window — a stepped kit has no frames to punch across. The one-L4 invariant
              still holds: every goal beat's cash-out is legitimately `updating` at L3. */}
          <TvScorebug ticket={D.ticket.index} leg={b.legIndex} away={D.away} home={D.home}
            score={b.score} clock={b.clock} backed={activeLeg.backed || null}
            marketPick={!activeLeg.backed} goal={b.goal ? true : undefined} />
          <TvMomentumTape samples={b.momentum} />
          <TvStage actors={b.actors} ball={b.ball} />
          <TvEventStrip text={b.event} punched={punch} />
          {card && <TvTicketCard {...D.nextTicket} />}
        </div>
      </div>
      <div style={{ width: 980, display: "flex", alignItems: "center", gap: 8, marginTop: 12 }}>
        <span style={{ fontFamily: "var(--font-tv)", fontSize: 12, letterSpacing: ".1em",
          color: "var(--tv-structure)", marginRight: "auto" }}>
          980 × 550 REFERENCE CANVAS · THE IN-ROOM RENDER AT THE SEATED CAMERA IS THE ONLY VALID ACCEPTANCE VIEW
        </span>
        <button type="button" style={ctl} onClick={back}>◄ Beat</button>
        <button type="button" style={ctl} onClick={advance}>Beat ►</button>
        <button type="button" style={ctl} onClick={() => setStats((s) => !s)}>Stats</button>
        <button type="button" style={ctl} onClick={() => setCard((c) => !c)}>Ticket card</button>
        <button type="button" style={ctl} onClick={() => { setI(1); setCashedOut(null); setCard(false); setStats(false); }}>Reset</button>
      </div>
    </div>
  );
}

ReactDOM.createRoot(document.getElementById("root")).render(<App />);
