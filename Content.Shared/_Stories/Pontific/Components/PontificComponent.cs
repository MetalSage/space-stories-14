namespace Content.Shared._Stories.Pontific;

[RegisterComponent]
public sealed partial class PontificComponent : Component
{
    [DataField]
    public HashSet<string> Actions = new();

    [DataField]
    public HashSet<EntityUid> GrantedActions = new();
}
