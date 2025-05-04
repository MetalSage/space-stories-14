using Content.Shared.Actions;
using Robust.Shared.Player;

namespace Content.Shared._Stories.Revenant;

public sealed partial class RevenantReapActionEvent : InstantActionEvent;

public sealed partial class RevenantGhostlyTouchActionEvent : EntityTargetActionEvent;

public sealed class RevenantDieEvent : EntityEventArgs;

public sealed partial class RevenantRebirthToggledEvent : InstantActionEvent;
