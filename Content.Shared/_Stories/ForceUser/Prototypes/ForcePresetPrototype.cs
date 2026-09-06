using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.ForceUser;

[Prototype, DataDefinition] 
public sealed partial class ForcePresetPrototype : IPrototype
{
    [DataField("componentsToAdd"), AlwaysPushInheritance]
    public ComponentRegistry ToAdd = new();

    [DataField("componentsToRemove"), AlwaysPushInheritance]
    public ComponentRegistry ToRemove = new();

    [DataField("name", required: true)]
    public string Name { get; private set; } = string.Empty;

    [DataField("side", required: true)]
    public ForceSide Side { get; private set; } = ForceSide.Debug;

    [DataField("alert")]
    public string AlertType { get; private set; }

    [ViewVariables, IdDataField]  public string ID { get; private set; } = default!;

    #region ForceComponent

    [DataField("volume")] public float Volume = 200f;
    [DataField("passiveVolume")] public float PassiveVolume = 30f;
    [DataField("maxVolume")] public float MaxVolume = 200f;

    #endregion
}

public enum ForceSide : byte
{
    Dark,
    Grey,
    Light,
    Debug,
}
