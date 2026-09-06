using Content.Shared.Maps;
using Robust.Shared.Configuration;

namespace Content.Server.Maps.Conditions;

public sealed partial class CVarMapCondition : GameMapCondition
{
    [DataField("cvar", required: true)]
    public string CVar { get; private set; } = default!;

    public override bool Check(GameMapPrototype map)
    {
        var cfg = IoCManager.Resolve<IConfigurationManager>();
        return cfg.GetCVar<bool>(CVar) ^ Inverted;
    }
}
