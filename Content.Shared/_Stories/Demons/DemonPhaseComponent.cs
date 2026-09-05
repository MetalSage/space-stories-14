using Content.Shared.Actions;
using Content.Shared.Polymorph;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.Demons;

[RegisterComponent]
public sealed partial class DemonPhaseComponent : Component
{
    [DataField(required: true)]
    public ProtoId<PolymorphPrototype> PhasePolymorph;

    [DataField]
    public DemonPhaseAnchor Anchor = DemonPhaseAnchor.Blood;

    [DataField]
    public float SearchRange = 1.5f;

    [DataField]
    public float DarknessThreshold = 1f;

    [DataField]
    public LocId FailPopup = "demon-phase-fail";

    [DataField(required: true)]
    public EntProtoId PhaseOutEffect;

    [DataField(required: true)]
    public EntProtoId PhaseInEffect;

    [DataField]
    public TimeSpan RiseDuration = TimeSpan.FromSeconds(2);

    [DataField]
    public SoundSpecifier? PhaseOutSound;

    [DataField]
    public SoundSpecifier? PhaseInSound;

    [DataField]
    public float HallucinationChance;

    [DataField]
    public SoundSpecifier? HallucinationSounds;
}

public enum DemonPhaseAnchor : byte
{
    Blood,

    Darkness,
}

public sealed partial class DemonPhaseActionEvent : InstantActionEvent;
