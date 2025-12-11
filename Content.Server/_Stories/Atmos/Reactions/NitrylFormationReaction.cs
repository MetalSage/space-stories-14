using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions;

/// <summary>
///     Forms nitryl from mixing Healium, BZ and Nitrogen at high temperatures.
/// </summary>
[UsedImplicitly]
public sealed partial class NitrylFormationReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        var initHealium = mixture.GetMoles(Gas.STHealium);
        var initBZ = mixture.GetMoles(Gas.STBZ);
        var initNitrogen = mixture.GetMoles(Gas.Nitrogen);

        var rate = mixture.Temperature / Atmospherics.NitrylProductionMaxEfficiencyTemperature; // higher temperature gives higher speed

        var healiumRemoved = rate * 2f;
        var bzRemoved = rate * 5f;
        var nitrogenRemoved = rate * 10f;
        var nitrylFormed = rate * 10f;

        if (healiumRemoved > initHealium || bzRemoved > initBZ || nitrogenRemoved > initNitrogen)
            return ReactionResult.NoReaction;

        mixture.AdjustMoles(Gas.STHealium, -healiumRemoved);
        mixture.AdjustMoles(Gas.STBZ, -bzRemoved);
        mixture.AdjustMoles(Gas.Nitrogen, -nitrogenRemoved);
        mixture.AdjustMoles(Gas.STNitryl, nitrylFormed);

        var energyConsumed = nitrylFormed * Atmospherics.NitrylProductionEnergy;
        var heatCap = atmosphereSystem.GetHeatCapacity(mixture, true);
        if (heatCap > Atmospherics.MinimumHeatCapacity)
            mixture.Temperature = Math.Max((mixture.Temperature * heatCap + energyConsumed) / heatCap, Atmospherics.TCMB);

        return ReactionResult.Reacting;
    }
}