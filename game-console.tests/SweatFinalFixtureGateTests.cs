using SBR.ConsoleGame;
using SBR.Engine;
using Xunit.Abstractions;

namespace SBR.ConsoleGame.Tests;

/// <summary>
/// THE FINAL-TELLING GATE — <c>T140</c> arm A, on the console's own fast-forward rule.
///
/// <para><b>The rule being protected is not pacing.</b> <c>SweatRenderer.SweatOne</c> clears
/// <c>fastForward</c> the moment the final telling arrives — *"reached the final leg — it must be
/// sweated"* — and <c>Hold</c> refuses the fast-forward key there, printing
/// *"(the final leg must be sweated — no fast-forward)"*. If the predicate never goes true, the
/// player fast-forwards through the last match of the ticket: the one thing the console says it
/// will not allow.</para>
///
/// <para><b>Why this is asserted rather than shot.</b> <c>Hold</c> opens with
/// <c>if (Console.IsInputRedirected) return Input.None;</c> — piping stdin at the exe short-circuits
/// the entire fast-forward path, so no piped-input run can demonstrate the defect or the fix. The
/// evidence has to be the predicate's value on a real event stream, which is what this file is.</para>
///
/// <para><b>The mutation is encoded, not described.</b> <see cref="OldPredicate"/> is the struck
/// line, kept executable, and the gate asserts it FAILS on the very ticket the new one handles. A
/// gate that only checks the fix would pass just as happily against the defect on any ordinary
/// ticket, because on those two the predicates agree — which is exactly how this survived.</para>
/// </summary>
public class SweatFinalFixtureGateTests
{
    private readonly ITestOutputHelper _output;

    public SweatFinalFixtureGateTests(ITestOutputHelper output) => _output = output;

    /// <summary>The struck predicate, verbatim: <c>evt.LegIndex == ticket.Legs.Count - 1</c>.
    /// Kept executable so the gate can prove it wrong rather than assert that it is.</summary>
    private static bool OldPredicate(DramaEvent e, Ticket ticket) => e.LegIndex == ticket.Legs.Count - 1;

    /// <summary>An INTERLEAVED ticket — <c>[matchA, matchB, matchA]</c> — searched off the board.
    /// Fixture grouping is first-appearance, so fixture 0 is legs {0, 2} and fixture 1 is leg {1};
    /// the anchors are therefore 0 and 1 and never 2. The same-match pair is the nested goal pair
    /// (over the higher line entails over the lower), pure set containment, so no board change can
    /// refuse it.</summary>
    /// <summary>What one drive of an interleaved ticket produced. The session is DRAINED inside the
    /// search — driving it is the only way to know whether it reaches its last telling — so the
    /// measurements travel out rather than the spent session.</summary>
    private sealed record Drive(
        Ticket Ticket, int FixtureCount, int Beats, List<int> Anchors,
        int NewTrue, int OldTrue, int LastFixtureSeen);

    /// <summary>An INTERLEAVED ticket — <c>[matchA, matchB, matchA]</c> — <b>that survives fixture 0
    /// and is therefore still being told when its last telling arrives</b>, searched off the board.
    ///
    /// <para>Fixture grouping is first-appearance, so fixture 0 is legs {0, 2} and fixture 1 is leg
    /// {1}; the anchors are 0 and 1 and never 2. The same-match pair is the nested goal pair (over
    /// the higher line entails over the lower), pure set containment, so no board change refuses
    /// it.</para>
    ///
    /// <para><b>SURVIVING FIXTURE 0 IS PART OF THE FIXTURE, NOT A DETAIL.</b> The first candidate
    /// this search produced busted on its FIRST telling: legs {0,2} resolved, the ticket died, and
    /// the sweat ended with leg 1's match never told at all — six beats, one anchor, no final
    /// telling. On that ticket <c>OnFinalFixture</c> correctly never fires, and so did the struck
    /// predicate, so it cannot distinguish them. **A ticket must reach its last telling for the
    /// fast-forward rule to have anything to protect.**</para></summary>
    private static Drive? SurvivingInterleaved(params string[] seeds)
    {
        foreach (string seed in seeds)
        {
            int matchups = new Run(seed).CurrentSlate.Matchups.Count;
            for (int a = 0; a < matchups; a++)
                for (int b = 0; b < matchups; b++)
                {
                    if (a == b) continue;
                    var run = new Run(seed);
                    RunConfig cfg = run.Config;
                    Pick[] picks =
                    {
                        new Pick(a, MarketSelection.TotalGoals(cfg.GoalLines[1], true)),
                        new Pick(b, MarketSelection.Moneyline(Side.Home)),
                        new Pick(a, MarketSelection.TotalGoals(cfg.GoalLines[0], true)),
                    };
                    if (run.RefusalFor(picks) != null) continue;

                    Ticket t = run.PlaceTicket(picks, 10);
                    run.LockRound();
                    if (t.Legs.Count != 3) continue;
                    if (!ReferenceEquals(t.Legs[0].Matchup, t.Legs[2].Matchup)) continue;
                    if (ReferenceEquals(t.Legs[0].Matchup, t.Legs[1].Matchup)) continue;

                    SweatSession session = run.Sweats[0];
                    if (session.FixtureCount >= t.Legs.Count) continue;

                    var anchors = new List<int>();
                    int beats = 0, newTrue = 0, oldTrue = 0, lastFixture = -1;
                    while (session.MoveNext(out DramaEvent? e))
                    {
                        if (e is null) break;
                        beats++;
                        if (!anchors.Contains(e.LegIndex)) anchors.Add(e.LegIndex);
                        lastFixture = Math.Max(lastFixture, e.FixtureIndex);
                        if (SweatLines.OnFinalFixture(e, session)) newTrue++;
                        if (OldPredicate(e, t)) oldTrue++;
                        if (session.HasPendingLoss) session.DeclinePendingLoss();
                    }

                    // Must have been told all the way to its last telling, or the rule under test
                    // never engages and the two predicates are indistinguishable here.
                    if (lastFixture != session.FixtureCount - 1) continue;

                    return new Drive(t, session.FixtureCount, beats, anchors, newTrue, oldTrue, lastFixture);
                }
        }

        return null;
    }

    [Fact]
    public void The_final_telling_is_reached_on_an_interleaved_ticket_and_the_struck_predicate_never_was()
    {
        Drive? found = SurvivingInterleaved(
            "GATE-FINALFIX-A", "GATE-FINALFIX-B", "GATE-FINALFIX-C", "GATE-FINALFIX-D");
        Assert.True(found is not null,
            "no interleaved [A,B,A] ticket on these seeds survived fixture 0 to reach its last "
            + "telling. The case T140 arm A creates is then unreachable from this pool and this "
            + "gate would be vacuous — widen the seeds rather than relax the assertions");
        Drive d = found!;
        Ticket ticket = d.Ticket;

        _output.WriteLine($"legs {ticket.Legs.Count}  fixtures {d.FixtureCount}  beats {d.Beats}");
        _output.WriteLine($"anchors seen : {string.Join(", ", d.Anchors)}");
        _output.WriteLine($"OnFinalFixture true on {d.NewTrue} beats; struck predicate true on {d.OldTrue}");

        // ANTI-VACUITY: fewer TELLINGS than LEGS, or leg-index and fixture-index coincide and the
        // two predicates cannot be told apart.
        Assert.True(d.FixtureCount < ticket.Legs.Count,
            $"FixtureCount {d.FixtureCount} is not below Legs.Count {ticket.Legs.Count} — "
            + "this ticket has one leg per fixture and cannot distinguish the two predicates");
        Assert.True(d.Beats > 0, "C29: no beat was put through either predicate");

        // THE MUTATION. The struck line cannot fire on this ticket: the last LEG index is 2 and 2 is
        // never an anchor, because leg 2 shares fixture 0 with leg 0 and the anchor is the LOWEST
        // ticket-order leg on the fixture. This is the assertion that makes the gate a gate — remove
        // it and the file would pass against the defect it exists to catch.
        Assert.False(d.Anchors.Contains(ticket.Legs.Count - 1),
            $"the last leg index {ticket.Legs.Count - 1} appeared as an anchor, so the struck "
            + "predicate is reachable here after all and this gate is not measuring the defect");
        Assert.True(d.OldTrue == 0,
            $"the struck predicate fired on {d.OldTrue} beats — it was expected to be UNREACHABLE on "
            + "an interleaved ticket, which is the whole defect. If this now fires, arm A's fixture "
            + "grouping has changed and this gate needs re-deriving, not relaxing");

        // AND THE FIX. The final telling must actually be reached, on at least one beat.
        Assert.True(d.NewTrue > 0,
            "OnFinalFixture never went true, so fastForward is never cleared and Hold never refuses "
            + "the key — the player can fast-forward through the ticket's last match");
        Assert.Equal(d.FixtureCount - 1, d.LastFixtureSeen);
    }

    /// <summary>The two predicates AGREE on an ordinary ticket, which is the compatibility half:
    /// the fix must not move the console's behaviour on the shape that ships today.</summary>
    [Fact]
    public void On_an_ordinary_ticket_the_new_predicate_matches_the_struck_one_beat_for_beat()
    {
        var run = new Run("GATE-FINALFIX-B");
        Ticket ticket = run.PlaceTicket(new[]
        {
            new Pick(0, MarketSelection.Moneyline(Side.Home)),
            new Pick(1, MarketSelection.Moneyline(Side.Home)),
        }, 10);
        run.LockRound();
        SweatSession session = run.Sweats[0];

        Assert.Equal(ticket.Legs.Count, session.FixtureCount);

        int beats = 0, agreed = 0;
        while (session.MoveNext(out DramaEvent? e))
        {
            if (e is null) break;
            beats++;
            if (SweatLines.OnFinalFixture(e, session) == OldPredicate(e, ticket)) agreed++;
            if (session.HasPendingLoss) session.DeclinePendingLoss();
        }

        _output.WriteLine($"ordinary ticket: {beats} beats, {agreed} in agreement");
        Assert.True(beats > 0, "C29: no beat was compared");
        Assert.Equal(beats, agreed);
    }
}
