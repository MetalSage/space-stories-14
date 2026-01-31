namespace Content.Shared._Stories.Force.Lightsaber;

[RegisterComponent] [AutoGenerateComponentState]
public sealed partial class LightsaberComponent : Component
{
    [DataField("deactivateProb")]
    public float DeactivateProb = 0.5f;

    [DataField("lightSaberOwner")] [AutoNetworkedField]
    public EntityUid? LightsaberOwner;
}
