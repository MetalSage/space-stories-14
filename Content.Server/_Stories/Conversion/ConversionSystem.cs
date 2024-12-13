using Content.Shared.Mind;
using Content.Server.Antag;
using Content.Server.Roles;
using Content.Shared._Stories.Conversion;
using Robust.Shared.Timing;
using Content.Server.Administration.Logs;
using Robust.Shared.Prototypes;

namespace Content.Server._Stories.Conversion;

public sealed partial class ConversionSystem : SharedConversionSystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly RoleSystem _role = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    public override void Initialize()
    {
        base.Initialize();
        InitializeMindShield();
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ConversionableComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            foreach (var (key, conversion) in comp.ActiveConversions)
            {
                if (conversion.EndTime == null)
                    continue;
                if (conversion.EndTime > _timing.CurTime)
                    continue;
                var proto = _prototype.Index(conversion.Prototype);
                DoRevert(uid, proto);
            }
        }
    }
}
