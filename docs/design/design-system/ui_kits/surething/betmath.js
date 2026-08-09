/* American odds helpers. In the real product /engine owns all of this and the UI never re-derives it;
   these exist only so the kit's figures move truthfully when you click. */
(function () {
  const dec = (american) => {
    const n = parseInt(String(american).replace("+", ""), 10);
    return n > 0 ? 1 + n / 100 : 1 + 100 / Math.abs(n);
  };
  const toAmerican = (d) => {
    if (d <= 1) return "—";
    const n = d >= 2 ? Math.round((d - 1) * 100) : -Math.round(100 / (d - 1));
    return (n > 0 ? "+" : "") + n;
  };
  const money = (n) => "$" + Math.round(n).toLocaleString("en-US");
  const combined = (legs) => legs.reduce((a, l) => a * dec(l.price), 1);
  window.STMath = {
    dec, toAmerican, money, combined,
    combinedAmerican: (legs) => (legs.length ? toAmerican(combined(legs)) : "—"),
    payout: (legs, stake) => (legs.length ? money(combined(legs) * stake) : "$0")
  };
})();
