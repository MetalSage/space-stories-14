using Content.Shared.Polymorph;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Server._Stories.PolymorphClimbers;

[RegisterComponent]
public sealed partial class PolymorphClimbersComponent : Component
{

    [DataField(required: true)]
    public ProtoId<PolymorphPrototype> Polymorph;

    [DataField]
    public EntityWhitelist? Whitelist = null;

    [DataField]
    public EntityWhitelist? Blacklist = null;


}
