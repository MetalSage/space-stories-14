using Content.Server.Antag;
using Content.Server.Roles;
using Content.Shared._Stories.Conversion;
using Content.Shared.Mind;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._Stories.Conversion;

public sealed partial class ConversionSystem : SharedConversionSystem
{
    [Dependency] private AntagSelectionSystem _antag = default!;
    [Dependency] private SharedMindSystem _mind = default!;
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private RoleSystem _role = default!;
    [Dependency] private IGameTiming _timing = default!;

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
