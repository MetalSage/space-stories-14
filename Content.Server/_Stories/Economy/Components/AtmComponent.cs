using Robust.Shared.Audio;

namespace Content.Server._Stories.Economy.Components;

[RegisterComponent]
public sealed partial class AtmComponent : Component
{
    [ViewVariables]
    public string? LoggedInAccountNumber;

    [DataField]
    public SoundSpecifier SoundCash = new SoundPathSpecifier("/Audio/Effects/kaching.ogg");

    [DataField]
    public SoundSpecifier SoundError = new SoundPathSpecifier("/Audio/Machines/button.ogg");
}
