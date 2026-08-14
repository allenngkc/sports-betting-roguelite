using System;
using System.Linq;
using SBR.Engine;

namespace SBR.Engine.Tests;

/// <summary>
/// The draws design rulings (Design Director batch 49, canon 8a8e7a6), as tests. These are not
/// engineering preferences — they are ratified design canon, and each one below is a place where a
/// well-meaning future change would quietly violate it. Guarding them here is cheaper than
/// re-litigating them after a surface ships.
/// </summary>
public class DrawRulingTests
{
    private static Matchup Board() => new Run("DRAW-RULING").CurrentSlate.Matchups[0];

    /// <summary>RATIFIED: the draw is not a team, EVER. The engine throwing rather than answering
    /// is the design, not a defensive accident — so a future "fix" that makes Side return a value
    /// for the draw is a canon violation and fails here.</summary>
    [Fact]
    public void Side_throws_on_a_draw_because_a_draw_is_not_a_team()
    {
        var pick = new Pick(0, MarketSelection.MoneylineDraw());
        Assert.Throws<InvalidOperationException>(() => pick.Side);

        Matchup m = Board();
        var leg = new Leg(m, MarketSelection.MoneylineDraw(), m.DrawOdds);
        Assert.Throws<InvalidOperationException>(() => leg.Side);

        // The team sides still answer — the throw is specific to the draw, not blanket.
        Assert.Equal(Side.Home, new Pick(0, Side.Home).Side);
        Assert.Equal(Side.Away, new Pick(0, Side.Away).Side);
    }

    /// <summary>RATIFIED: three offers, DRAW IN THE MIDDLE. The order is canon, not incidental —
    /// it is the order a book prints and the order the surface will render.</summary>
    [Fact]
    public void The_moneyline_board_is_three_offers_with_the_draw_in_the_middle()
    {
        MarketSelection[] moneyline = Board().Markets
            .Where(o => o.Selection.Kind == MarketKind.Moneyline)
            .Select(o => o.Selection)
            .ToArray();

        Assert.Equal(3, moneyline.Length);
        Assert.Equal(MarketChoice.Home, moneyline[0].Choice);
        Assert.Equal(MarketChoice.Draw, moneyline[1].Choice);
        Assert.Equal(MarketChoice.Away, moneyline[2].Choice);
    }

    /// <summary>RATIFIED: "no team anything" on the draw row. The engine emits fields and the
    /// surface composes them (S22), so the guarantee has to hold HERE — a surface cannot avoid
    /// printing a team name the engine handed it.</summary>
    [Fact]
    public void The_draw_offer_carries_no_team_identity()
    {
        Matchup m = Board();
        MatchModel.MarketFields fields = MatchModel.Fields(m, MarketSelection.MoneylineDraw());

        Assert.Equal("", fields.Subject);
        Assert.Equal("", fields.Role);
        Assert.DoesNotContain(m.Home.Name, fields.Subject);
        Assert.DoesNotContain(m.Away.Name, fields.Subject);
        Assert.DoesNotContain(m.Home.Name, fields.Line);
        Assert.DoesNotContain(m.Away.Name, fields.Line);
        // The fixture is the matchup, and it legitimately names both teams for every market.
        Assert.Contains(m.Home.Name, fields.Fixture);
    }

    /// <summary>THE NAMED RISK, guarded: "decisive" is an ENGINE term that must never print.
    ///
    /// The DD flagged it because a scoreless draw ending on a beat tagged Decisive is exactly where
    /// an engine word leaks into player-facing copy. Beat selection may read the flag internally —
    /// that is explicitly allowed — but no string the engine hands a surface may contain it.
    ///
    /// Written against the ENUM rather than the literal "Decisive" so a tag added later is covered
    /// without anyone remembering to come back here.</summary>
    [Fact]
    public void No_engine_tension_tag_name_leaks_into_a_player_facing_string()
    {
        Matchup m = Board();
        string[] tagNames = Enum.GetNames(typeof(TensionTag));

        foreach (MarketOffer offer in m.Markets)
        {
            MatchModel.MarketFields f = MatchModel.Fields(m, offer.Selection);
            string label = MatchModel.DisplayLabel(m, offer.Selection);
            foreach (string surface in new[] { f.Market, f.Subject, f.Line, f.Fixture, f.Role, label })
                foreach (string tag in tagNames)
                    Assert.False(
                        surface.Contains(tag, StringComparison.OrdinalIgnoreCase),
                        $"engine term '{tag}' reached a player-facing string: \"{surface}\" "
                        + $"({offer.Selection.Kind}/{offer.Selection.Choice})");
        }
    }
}
