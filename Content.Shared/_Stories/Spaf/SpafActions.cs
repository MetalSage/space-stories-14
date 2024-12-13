using Content.Shared.Actions;
using Content.Shared.Chemistry.Components;
using Content.Shared.DoAfter;
using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Stories.Spaf;

public interface ISpafAction
{
    [DataField("cost")]
    public float HungerCost { get; set; }
}

public sealed partial class SpafCreateEntityEvent : InstantActionEvent, ISpafAction
{
    [DataField("cost")]
    public float HungerCost { get; set; } = 20f;

    [DataField("proto", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string Prototype;
}

public sealed partial class SpafSpillSolutionEvent : InstantActionEvent, ISpafAction
{
    [DataField("cost")]
    public float HungerCost { get; set; } = 10f;

    [DataField("solution")]
    public Solution Solution { get; set; } = new();
}

public sealed partial class SpafPolymorphEvent : InstantActionEvent, ISpafAction
{
    [DataField("cost")]
    public float HungerCost { get; set; } = 70f;

    [DataField]
    public ProtoId<PolymorphPrototype> ProtoId;
}

public sealed partial class SpafStealthEvent : InstantActionEvent, ISpafAction
{
    [DataField("cost")]
    public float HungerCost { get; set; } = 15f;

    [DataField]
    public float Seconds { get; set; } = 5f;
}

[Serializable, NetSerializable]
public sealed partial class SpafStealthDoAfterEvent : SimpleDoAfterEvent { }

public sealed partial class FoodPopupEvent : InstantActionEvent { }
