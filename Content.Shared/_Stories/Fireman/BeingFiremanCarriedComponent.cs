using Robust.Shared.GameStates;

namespace Content.Shared._Stories.Fireman;

[RegisterComponent, NetworkedComponent]
[Access(typeof(StoriesFiremanCarrySystem))]
public sealed partial class BeingFiremanCarriedComponent : Component;
