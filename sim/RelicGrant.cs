using System;
using System.Collections.Generic;
using System.Reflection;
using SBR.Engine;

namespace SBR.Sim;

/// <summary>
/// HARNESS-ONLY BACKDOOR — must never migrate into engine or game code.
///
/// The relic power audit and the combo scan need a run to start already owning specific relics. The
/// frozen engine has no "grant relic" API: RelicCatalog prices are fixed and Run.BuyRelic requires
/// Phase.Shop and a bank deduction, so buying is both stochastic (the shop may not offer it) and
/// bank-distorting. The only clean, non-distorting way is to reach into Run's private EffectEngine and
/// append the relic directly, exactly as a purchase would — reflection, which is legitimate in a test
/// harness (this project) but would be a layering violation anywhere else.
///
/// It also regenerates round-1 tout Intel: the engine only calls GenerateIntel at ExitShop, so a
/// tout_sheet granted before round 1 would otherwise produce no round-1 intel. GenerateIntel draws no
/// RNG unless tout_sheet is owned, so this is a no-op for every other relic.
/// </summary>
public static class RelicGrant
{
    private static readonly FieldInfo EffectsField =
        typeof(Run).GetField("_effects", BindingFlags.NonPublic | BindingFlags.Instance)
        ?? throw new InvalidOperationException("Run._effects not found — engine layout changed.");

    private static readonly FieldInfo IntelField =
        typeof(Run).GetField("_intel", BindingFlags.NonPublic | BindingFlags.Instance)
        ?? throw new InvalidOperationException("Run._intel not found — engine layout changed.");

    private static readonly Dictionary<string, RelicDefinition> ById = BuildIndex();

    private static Dictionary<string, RelicDefinition> BuildIndex()
    {
        var map = new Dictionary<string, RelicDefinition>();
        foreach (RelicDefinition d in RelicCatalog.All) map[d.Id] = d;
        return map;
    }

    /// <summary>Grants the given relics (in order → acquisition order) to a freshly constructed run
    /// sitting in round-1 Betting, then refreshes round-1 intel. Must be called before the first bet.</summary>
    public static void Grant(Run run, IReadOnlyList<string> relicIds)
    {
        var effects = (EffectEngine)EffectsField.GetValue(run)!;
        foreach (string id in relicIds)
        {
            if (!ById.TryGetValue(id, out RelicDefinition? def))
                throw new ArgumentException($"Unknown relic id '{id}'");
            effects.Add(def); // appends in acquisition order, resets its per-round charge — as BuyRelic does
        }

        // Round-1 intel: the engine only fires tout on ExitShop, so grant it here for round 1 too.
        IReadOnlyList<MatchupIntel> intel = effects.GenerateIntel(run.CurrentSlate, run.Rng.Relics);
        IntelField.SetValue(run, intel);
    }
}
