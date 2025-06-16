namespace Content.Shared._Stories.Pontific;

[RegisterComponent]
public sealed partial class PontificComponent : Component
{
    [DataField]
    public HashSet<string> Actions = new()
    {
        "ActionPontificPrayer",
        "ActionPontificBloodyAltar",
        "ActionPontificSpawnMonk",
        "ActionPontificSpawnGuardian",
        "ActionPontificFelLightning",
        "ActionPontificFlameSwords",
        "ActionPontificKudzu",
    };

    [DataField]
    public HashSet<EntityUid> GrantedActions = new();
}
