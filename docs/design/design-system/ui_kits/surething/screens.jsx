const { ColumnHead, FormEntry, MarketOffer, MoreButton, TicketReceipt, RevealedLeg,
        OfferEntry, LedgerEntry, RunFigure, StampReason, InkMark } = window.SureThingDesignSystem_6e1eb3;

const sheet = { width: 700, borderRight: "2px solid var(--rule)", display: "flex", flexDirection: "column" };
const key = { fontSize: 13, letterSpacing: ".12em", color: "var(--toner-3)", whiteSpace: "nowrap" };

/* FORM — the house's document. Six ruled two-line entries, moneyline and MORE only. */
function FormScreen({ selections, onSelect, onMore }) {
  return (
    <div style={sheet}>
      <ColumnHead />
      <div style={{ flex: 1, display: "flex", flexDirection: "column" }}>
        {window.STData.slate.map((m, i) => (
          <FormEntry key={m.id} index={i} number={m.no} away={m.away} home={m.home}
            selected={selections[m.id] && selections[m.id].market === "MONEYLINE" ? selections[m.id].side : null}
            onSelect={(side) => onSelect(m, side)} onMore={() => onMore(m)} />
        ))}
      </div>
    </div>
  );
}

/* ENTRY — event detail. Only the market body changes when a destination is switched; the header and
   the working margin persist. A staged ticket is shown as a receipt before the action area. */
function EntryScreen({ matchup, destination, onDestination, selection, onSelectMarket, onBack, tickets }) {
  const markets = window.STData.markets[destination] || [];
  return (
    <div style={sheet}>
      <div style={{ display: "flex", alignItems: "center", gap: 14, padding: "0 14px", height: 44,
        borderBottom: "1px solid var(--rule)", background: "var(--ground-2)" }}>
        <button type="button" onClick={onBack} style={{ height: 32, minWidth: 96, background: "transparent",
          border: "1px solid var(--rule)", color: "var(--toner-2)", fontFamily: "var(--font-data)",
          fontSize: 13, letterSpacing: ".06em", cursor: "pointer" }}>‹ BACK TO FORM</button>
        <div style={{ display: "flex", alignItems: "baseline", gap: 10 }}>
          <span style={{ fontFamily: "var(--font-cond)", fontSize: 19, letterSpacing: ".03em",
            color: "var(--toner)", textTransform: "uppercase" }}>{matchup.away.name} at {matchup.home.name}</span>
          <span style={{ ...key, letterSpacing: ".08em" }}>{matchup.away.record} · {matchup.home.record} · ENTRY {matchup.no}</span>
        </div>
      </div>
      <div style={{ display: "flex", gap: 2, padding: "8px 14px 0", borderBottom: "1px solid var(--rule)" }}>
        {window.STData.destinations.map((d) => (
          <button key={d} type="button" onClick={() => onDestination(d)}
            style={{ height: 27, padding: "0 13px", background: d === destination ? "var(--ground)" : "transparent",
              border: "1px solid " + (d === destination ? "var(--rule)" : "var(--rule-soft)"), borderBottom: 0,
              color: d === destination ? "var(--toner)" : "var(--toner-3)", fontFamily: "var(--font-data)",
              fontSize: 13, letterSpacing: ".11em", cursor: "pointer" }}>{d}</button>
        ))}
      </div>
      <div style={{ flex: 1, overflowY: "auto" }}>
        {tickets.length > 0 && (
          <div style={{ padding: "12px 14px", borderBottom: "1px solid var(--rule)" }}>
            <div style={{ ...key, marginBottom: 7 }}>PLACED THIS ROUND</div>
            {tickets.map((t) => (
              <TicketReceipt key={t.number} number={t.number} legs={t.legs} stake={t.stake}
                combined={t.combined} payout={t.payout} style={{ marginBottom: 8 }} />
            ))}
          </div>
        )}
        {markets.map((mk) => {
          const picked = selection && selection.line === mk.line;
          const other = selection && selection.line !== mk.line;
          return (
            <div key={mk.line} style={{ display: "flex", alignItems: "center", padding: "0 14px", height: 54,
              borderBottom: "1px solid var(--rule-soft)",
              background: picked ? "linear-gradient(90deg,var(--marked-wash),transparent 70%)" : "transparent" }}>
              <div style={{ flex: 1, fontFamily: "var(--font-cond)", fontSize: 19, letterSpacing: ".03em",
                color: "var(--toner)", textTransform: "uppercase" }}>{mk.line}</div>
              <MarketOffer line="" price={mk.price} state={picked ? "picked" : other ? "replace" : "default"}
                onSelect={() => onSelectMarket(mk)} style={{ width: 176, justifyContent: "flex-end" }} />
            </div>
          );
        })}
      </div>
    </div>
  );
}

/* MY BETS — the revealed mirror. Read-only, TV-owned, and never ahead of the broadcast. */
function MyBetsScreen({ tickets, revealed }) {
  return (
    <div style={sheet}>
      <div style={{ display: "flex", alignItems: "center", gap: 12, padding: "0 14px", height: 44,
        borderBottom: "1px solid var(--rule)", background: "var(--ground-2)" }}>
        <span style={key}>READ-ONLY MIRROR</span>
        <StampReason reason="THE TV OWNS THE REVEAL — THIS SHEET ONLY COPIES IT" />
      </div>
      <div style={{ flex: 1, overflowY: "auto", padding: "0 14px" }}>
        {tickets.length === 0 && (
          <div style={{ padding: "18px 0", fontSize: 13, color: "var(--toner-3)", lineHeight: 1.5 }}>
            No tickets locked this round.
          </div>
        )}
        {tickets.map((t) => (
          <div key={t.number} style={{ padding: "14px 0", borderBottom: "1px solid var(--rule)" }}>
            <div style={{ display: "flex", alignItems: "baseline", gap: 12, marginBottom: 4 }}>
              <span style={{ fontFamily: "var(--font-cond)", fontSize: 19, letterSpacing: ".14em",
                color: "var(--toner)", textTransform: "uppercase" }}>{t.number}</span>
              <span style={key}>STAKE {t.stake} · PAYS {t.payout}</span>
            </div>
            {t.legs.map((l, i) => (
              <RevealedLeg key={i} team={l.team} price={l.price} market={l.market}
                state={revealed[t.number + "/" + i] || "PENDING"} />
            ))}
          </div>
        ))}
      </div>
    </div>
  );
}

/* REWARDS — the shop. Ruled offer entries, never a promotional rail. */
function RewardsScreen({ result, onBuy }) {
  return (
    <div style={sheet}>
      <div style={{ display: "flex", alignItems: "center", gap: 12, padding: "0 14px", height: 44,
        borderBottom: "1px solid var(--rule)", background: "var(--ground-2)" }}>
        <span style={key}>OFFERS THIS ROUND</span>
        <span style={{ ...key, marginLeft: "auto" }}>RELICS HELD 2 / 5</span>
      </div>
      <div style={{ flex: 1, overflowY: "auto" }}>
        {window.STData.offers.map((o) => (
          <OfferEntry key={o.name} {...o} onBuy={() => onBuy(o)} />
        ))}
        {result && (
          <div style={{ padding: "13px 14px" }}><StampReason reason={result} /></div>
        )}
      </div>
    </div>
  );
}

/* LEDGER — Old Slips. The settled record, read-only, same document grammar, no live styling. */
function LedgerScreen() {
  return (
    <div style={sheet}>
      <div style={{ display: "flex", alignItems: "center", gap: 12, padding: "0 14px", height: 44,
        borderBottom: "1px solid var(--rule)", background: "var(--ground-2)" }}>
        <span style={key}>SETTLED TICKETS · THIS RUN</span>
        <span style={{ ...key, marginLeft: "auto" }}>3 RECORDS</span>
      </div>
      <div style={{ flex: 1, overflowY: "auto" }}>
        {window.STData.ledger.map((l) => <LedgerEntry key={l.number} {...l} />)}
      </div>
    </div>
  );
}

Object.assign(window, { FormScreen, EntryScreen, MyBetsScreen, RewardsScreen, LedgerScreen });
