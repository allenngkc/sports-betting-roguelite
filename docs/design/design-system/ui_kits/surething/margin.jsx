/* The player margin — the right 324px, present on every surface. Its vertical order is fixed:
   header, legs, combined, stake, payout, actions. On MY BETS it becomes passive; on REWARDS it
   carries run context. It never turns into a floating drawer. */
const { MarginHeader, MarginLeg, MarginRow, StakeControls, PayoutFigure,
        PlaceAction, LockAction, SkipAction, RunFigure, StampReason } = window.SureThingDesignSystem_6e1eb3;

const marginShell = {
  width: 324, display: "flex", flexDirection: "column", padding: "0 14px",
  background: "repeating-linear-gradient(180deg,transparent 0 25px,var(--rule-soft) 25px 26px)"
};

function WorkingMargin({ legs, stake, tickets, onRemove, onFraction, onNudge, onPlace, onLock, onSkip, locked }) {
  const combined = window.STMath.combinedAmerican(legs);
  const payout = window.STMath.payout(legs, stake);
  const lockReason = legs.length
    ? "PLACE OR CLEAR THIS WORKING SLIP"
    : tickets.length === 0 ? "PLACE AT LEAST ONE TICKET" : null;
  return (
    <div style={marginShell}>
      <MarginHeader count={legs.length + (legs.length === 1 ? " SELECTION" : " SELECTIONS")} />
      <div style={{ paddingTop: 4 }}>
        {legs.length === 0 && (
          <div style={{ padding: "14px 0", fontSize: 13, lineHeight: 1.5, color: "var(--toner-3)", letterSpacing: ".02em" }}>
            No marks on this sheet. Circle a price to start a ticket.
          </div>
        )}
        {legs.map((l) => (
          <MarginLeg key={l.id} team={l.team} price={l.price} market={l.market} entry={l.entry}
            onRemove={() => onRemove(l.id)} />
        ))}
      </div>
      <MarginRow label="COMBINED" value={combined} />
      <StakeControls stake={window.STMath.money(stake)} onFraction={onFraction} onNudge={onNudge} />
      <div style={{ marginTop: 10, paddingTop: 9, borderTop: "1px solid var(--rule)" }}>
        <PayoutFigure value={payout} highlight={legs.length > 0} />
      </div>
      <div style={{ marginTop: "auto", paddingBottom: 13, display: "flex", flexDirection: "column", gap: 6 }}>
        {locked ? (
          <div style={{ padding: "10px 0", display: "flex", flexDirection: "column", gap: 7 }}>
            <div style={{ fontSize: 13, letterSpacing: ".12em", color: "var(--toner-3)" }}>ROUND LOCKED</div>
            <StampReason reason="BOARD FROZEN — WATCH THE TV" />
          </div>
        ) : (
          <>
            <PlaceAction onClick={onPlace} disabled={legs.length === 0} />
            <LockAction disabled={!!lockReason} reason={lockReason || undefined} onClick={onLock} />
            <SkipAction onSkip={onSkip} />
          </>
        )}
      </div>
    </div>
  );
}

function PassiveMargin({ title, rows, note, children }) {
  return (
    <div style={marginShell}>
      <MarginHeader title={title} />
      <div style={{ paddingTop: 4 }}>
        {(rows || []).map((r) => <MarginRow key={r.label} label={r.label} value={r.value} tone={r.tone} />)}
      </div>
      {children}
      {note && (
        <div style={{ marginTop: "auto", paddingBottom: 15, fontSize: 13, lineHeight: 1.5, color: "var(--toner-3)" }}>{note}</div>
      )}
    </div>
  );
}

Object.assign(window, { WorkingMargin, PassiveMargin, marginShell });
