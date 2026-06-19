using Content.Shared.NPC.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._Stories.ClothingWhitelist;

[RegisterComponent]
public sealed partial class ClothingWhitelistComponent : Component
{
    [DataField("beepInterval")]
    public float BeepInterval = 1;

    [DataField("beepSound")]
    public SoundSpecifier? BeepSound = new SoundPathSpecifier("/Audio/Machines/Nuke/general_beep.ogg");

    [DataField("delay")]
    public float Delay = 3f;

    [DataField("factionsBlacklist")] [ViewVariables(VVAccess.ReadWrite)]
    public HashSet<ProtoId<NpcFactionPrototype>>? FactionsBlacklist = new();

    [DataField("factionsWhitelist")] [ViewVariables(VVAccess.ReadWrite)]
    public HashSet<ProtoId<NpcFactionPrototype>>? FactionsWhitelist = new();

    [DataField("initialBeepDelay")]
    public float? InitialBeepDelay = 0;
}
