namespace Content.Shared._Stories.Holy;

[RegisterComponent]
public sealed partial class UnholyComponent : Component
{

    /// <summary>
    /// Коэффициент сопротивления светлым силам. 0, чтобы быть полностью неуязвимым.
    /// </summary>
    [DataField]
    public float ResistanceCoefficient = 1f;

    [DataField]
    public bool IgnoreProtectionImpulse = false;

    /// <summary>
    /// Могут ли святые предметы обнаружить тьму.
    /// </summary>
    [DataField]
    public bool Detectable = true;

}
