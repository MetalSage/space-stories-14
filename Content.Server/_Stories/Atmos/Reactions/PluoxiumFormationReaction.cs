using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions;

/// <summary>
///     Forms pluoxium from mixing Oxygen and Tritium and Carbon Dioxide at low temperature.
/// </summary>
[UsedImplicitly]
public sealed partial class PluoxiumFormationReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        var initOxygen = mixture.GetMoles(Gas.Oxygen);
        var initCO2 = mixture.GetMoles(Gas.CarbonDioxide);
        var initTrit = mixture.GetMoles(Gas.Tritium);
        var temperature = mixture.Temperature;
        var volume = mixture.Volume;

        var environmentEfficiency = volume / temperature; // more volume and less temperature gives better rates

        var totalRate = environmentEfficiency / Atmospherics.PluoxiumProductionRate;

        var oxygenRemoved = totalRate * 100f;
        var co2Removed = totalRate * 50f;
        var tritRemoved = totalRate * 1f;
        var pluoxiumFormed = totalRate * 5f;

        if (oxygenRemoved > initOxygen || co2Removed > initCO2 || tritRemoved > initTrit)
            return ReactionResult.NoReaction;

        mixture.AdjustMoles(Gas.Oxygen, -oxygenRemoved);
        mixture.AdjustMoles(Gas.CarbonDioxide, -co2Removed);
        mixture.AdjustMoles(Gas.Tritium, -tritRemoved);
        mixture.AdjustMoles(Gas.Pluoxium, pluoxiumFormed);

        var energyReleased = pluoxiumFormed * Atmospherics.PluoxiumProductionEnergy;
        var heatCap = atmosphereSystem.GetHeatCapacity(mixture, true);
        if (heatCap > Atmospherics.MinimumHeatCapacity)
            mixture.Temperature = Math.Max((mixture.Temperature * heatCap + energyReleased) / heatCap, Atmospherics.TCMB);

        return ReactionResult.Reacting;
    }
}