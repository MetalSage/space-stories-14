using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions;

[UsedImplicitly]
public sealed partial class BZFormationReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture,
        IGasMixtureHolder? holder,
        AtmosphereSystem atmosphereSystem,
        float heatScale)
    {
        var initN2O = mixture.GetMoles(Gas.NitrousOxide);
        var initPlasma = mixture.GetMoles(Gas.Plasma);
        var pressure = mixture.Pressure;
        var volume = mixture.Volume;

        var environmentEfficiency = volume / pressure;
        var ratioEfficiency = Math.Min(initN2O / initPlasma, 1);

        var totalRate = environmentEfficiency * ratioEfficiency / Atmospherics.BZFormationRate;

        var n2oRemoved = totalRate * 2f;
        var plasmaRemoved = totalRate * 4f;
        var bzFormed = totalRate * 5f;

        if (n2oRemoved > initN2O || plasmaRemoved > initPlasma)
            return ReactionResult.NoReaction;

        mixture.AdjustMoles(Gas.NitrousOxide, -n2oRemoved);
        mixture.AdjustMoles(Gas.Plasma, -plasmaRemoved);
        mixture.AdjustMoles(Gas.STBZ, bzFormed);

        var energyReleased = bzFormed * Atmospherics.BZFormationEnergy;
        var heatCap = atmosphereSystem.GetHeatCapacity(mixture, true);
        if (heatCap > Atmospherics.MinimumHeatCapacity)
        {
            mixture.Temperature =
                Math.Max((mixture.Temperature * heatCap + energyReleased) / heatCap, Atmospherics.TCMB);
        }

        return ReactionResult.Reacting;
    }
}
