using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Whitelist;

namespace Content.Shared._Stories.InjectReagents;

public sealed partial class InjectReagentsSystem : EntitySystem
{
    [Dependency] private EntityLookupSystem _entityLookup = default!;
    [Dependency] private SharedSolutionContainerSystem _solutions = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ActionsComponent, InjectReagentsEvent>(OnInjectReagentsEvent);
        SubscribeLocalEvent<ActionsComponent, InjectReagentsToTargetEvent>(OnInjectReagentsToTargetEvent);
        SubscribeLocalEvent<ActionsComponent, InjectReagentsInRangeEvent>(OnInjectReagentsInRangeEvent);
    }

    private void OnInjectReagentsEvent(EntityUid uid, ActionsComponent comp, InjectReagentsEvent args)
    {
        if (args.Handled || !_solutions.TryGetSolution(args.Performer, args.SolutionTarget, out var solution))
            return;
        _solutions.TryAddSolution(solution.Value, args.Solution);
        args.Handled = true;
    }

    private void OnInjectReagentsToTargetEvent(EntityUid uid, ActionsComponent comp, InjectReagentsToTargetEvent args)
    {
        if (args.Handled || !_solutions.TryGetSolution(args.Target, args.SolutionTarget, out var solution))
            return;
        _solutions.TryAddSolution(solution.Value, args.Solution);
        args.Handled = true;
    }

    private void OnInjectReagentsInRangeEvent(EntityUid uid, ActionsComponent comp, InjectReagentsInRangeEvent args)
    {
        if (args.Handled)
            return;

        var entities =
            _entityLookup.GetEntitiesInRange<SolutionManagerComponent>(Transform(args.Performer).Coordinates,
                args.Range);

        foreach (var (entity, component) in entities)
        {
            if (entity == args.Performer && !args.InjectToPerformer)
                continue;

            if (_whitelist.IsWhitelistFail(args.Whitelist, entity))
                continue;

            if (_whitelist.IsWhitelistPass(args.Blacklist, entity))
                continue;

            if (!_solutions.TryGetSolution((entity, component), args.SolutionTarget, out var solution))
                continue;

            _solutions.TryAddSolution(solution.Value, args.Solution);
        }

        args.Handled = true;
    }
}

public sealed partial class InjectReagentsEvent : InstantActionEvent
{
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("solution")]
    public Solution Solution { get; set; } = new();

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("solutionTarget")]
    public string SolutionTarget { get; set; } = "bloodstream";
}

public sealed partial class InjectReagentsToTargetEvent : EntityTargetActionEvent
{
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("solution")]
    public Solution Solution { get; set; } = new();

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("solutionTarget")]
    public string SolutionTarget { get; set; } = "bloodstream";
}

public sealed partial class InjectReagentsInRangeEvent : InstantActionEvent
{
    [DataField]
    public bool InjectToPerformer { get; set; }

    [DataField]
    public float Range { get; set; } = 7.5f;

    [DataField]
    public EntityWhitelist? Whitelist { get; set; }

    [DataField]
    public EntityWhitelist? Blacklist { get; set; }

    [DataField]
    public Solution Solution { get; set; } = new();

    [DataField]
    public string SolutionTarget { get; set; } = "chemicals";
}
