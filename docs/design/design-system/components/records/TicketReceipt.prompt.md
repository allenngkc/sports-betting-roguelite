A placed ticket as a printed receipt. Shown on ENTRY after PLACE TICKET, in MY BETS during the sweat, and in the Ledger once settled.

```jsx
<TicketReceipt number="TICKET 01" stake="$200" combined="+259" payout="$718"
  legs={[{team:"Bricklayers",market:"MONEYLINE",price:"-260"},
         {team:"Longhaulers",market:"MONEYLINE",price:"+180"}]} />
```

Renders `Run.Tickets` and nothing else — no invented date, settlement or outcome. Staging one clears the working marks and is what unlocks LOCK IT IN.
