using Robust.Shared.Prototypes;
using Content.Shared.Alert;

namespace Content.Shared._Stories.ForceUser;

[Prototype("forcePreset"), DataDefinition]
public sealed partial class ForcePresetPrototype : IPrototype
{
    [ViewVariables][IdDataField] public string ID { get; private set; } = default!;

    /// <summary>
    ///     То имя, которое другие пользователи силы смогут почувствовать.
    ///     Это может быть джедай, инквизитор, древний ситх, адепт темной стороны.
    /// </summary>
    [DataField("name", required: true)]
    public string Name { get; private set; } = string.Empty;

    [DataField("side", required: true)]
    public ForceSide Side { get; private set; } = ForceSide.Debug;

    [DataField("alert")]
    public string AlertType { get; private set; }

    #region ForceComponent
    [DataField("volume")] public float Volume = 200f;
    [DataField("passiveVolume")] public float PassiveVolume = 30f;
    [DataField("maxVolume")] public float MaxVolume = 200f;
    #endregion
    // Эту нужно чтобы добавить магазин.
    [DataField("componentsToAdd")]
    [AlwaysPushInheritance]
    public ComponentRegistry ToAdd = new();

    [DataField("componentsToRemove")]
    [AlwaysPushInheritance]
    public ComponentRegistry ToRemove = new();
}
public enum ForceSide : byte
{
    Dark, // Темная сторона
    Grey, // Серая сторона
    Light, // Светлая сторона
    Debug // Сторона фикса багов // ! Absolute power!
}
