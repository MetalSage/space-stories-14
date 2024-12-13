using Content.Shared.NPC.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._Stories.ClothingWhitelist;

[RegisterComponent]
public sealed partial class ClothingWhitelistComponent : Component
{
    [DataField("factionsWhitelist"), ViewVariables(VVAccess.ReadWrite)]
    public HashSet<ProtoId<NpcFactionPrototype>>? FactionsWhitelist = new();

    [DataField("factionsBlacklist"), ViewVariables(VVAccess.ReadWrite)]
    public HashSet<ProtoId<NpcFactionPrototype>>? FactionsBlacklist = new();

    [DataField("delay")]
    public float Delay = 3f;

    [DataField("beepSound")]
    public SoundSpecifier? BeepSound = new SoundPathSpecifier("/Audio/Machines/Nuke/general_beep.ogg");

    [DataField("initialBeepDelay")]
    public float? InitialBeepDelay = 0;

    [DataField("beepInterval")]
    public float BeepInterval = 1;
}
