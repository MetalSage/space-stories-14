namespace Content.Shared._Stories.Demons;

[RegisterComponent]
public sealed partial class DemonImmunitiesComponent : Component
{
    [DataField]
    public TimeSpan? VanishDelay = TimeSpan.FromSeconds(3);

    [DataField]
    public bool ImmuneToKnockdown = true;
}
