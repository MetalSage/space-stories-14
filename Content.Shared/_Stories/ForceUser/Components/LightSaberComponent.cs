using Robust.Shared.GameStates;

namespace Content.Shared._Stories.Force.Lightsaber;

[RegisterComponent, AutoGenerateComponentState]
public sealed partial class LightsaberComponent : Component
{
    [DataField("lightSaberOwner"), AutoNetworkedField]
    public EntityUid? LightsaberOwner;

    [DataField("deactivateProb")]
    public float DeactivateProb = 0.5f;
}
