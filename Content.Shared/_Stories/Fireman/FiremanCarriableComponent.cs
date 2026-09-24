using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared._Stories.Fireman;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(StoriesFiremanCarrySystem))]
public sealed partial class FiremanCarriableComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan Delay = TimeSpan.FromSeconds(3);

    [DataField, AutoNetworkedField]
    public bool BeingCarried;

    [DataField, AutoNetworkedField]
    public TimeSpan BreakDelay = TimeSpan.FromSeconds(2);

    [DataField, AutoNetworkedField]
    public bool BreakingFree;

    [DataField, AutoNetworkedField]
    public EntityWhitelist? CarrierWhitelist;

    [DataField, AutoNetworkedField]
    public bool CanThrow = true;

    [DataField, AutoNetworkedField]
    public float ThrowDistanceModifier = 0.4f;

    [DataField, AutoNetworkedField]
    public System.Numerics.Vector2 CarriedOffset = System.Numerics.Vector2.Zero;
}
