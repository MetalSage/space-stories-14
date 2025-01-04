using Content.Shared.Chemistry.Components;
using Content.Shared.Actions;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Whitelist;
using Content.Shared.Chemistry.Components.SolutionManager;

namespace Content.Shared._Stories.InjectReagents;
public sealed partial class InjectReagentsSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutions = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<InjectReagentsEvent>(OnInjectReagentsEvent);
        SubscribeLocalEvent<InjectReagentsToTargetEvent>(OnIjectReagentsToTargetEvent);
        SubscribeLocalEvent<InjectReagentsInRangeEvent>(OnInjectReagentsInRangeEvent);
    }
    private void OnInjectReagentsEvent(InjectReagentsEvent args)
    {
        if (args.Handled || !_solutions.TryGetSolution(args.Performer, args.SolutionTarget, out var solution)) return;
        _solutions.TryAddSolution(solution.Value, args.Solution);
        args.Handled = true;
    }
    private void OnIjectReagentsToTargetEvent(InjectReagentsToTargetEvent args)
    {
        if (args.Handled || !_solutions.TryGetSolution(args.Target, args.SolutionTarget, out var solution)) return;
        _solutions.TryAddSolution(solution.Value, args.Solution);
        args.Handled = true;
    }
    private void OnInjectReagentsInRangeEvent(InjectReagentsInRangeEvent args)
    {
        if (args.Handled)
            return;

        var entitis = _entityLookup.GetEntitiesInRange<SolutionContainerManagerComponent>(Transform(args.Performer).Coordinates, args.Range);
        foreach (var (entity, component) in entitis)
        {

            if (entity == args.Performer && !args.InjectToPerformer)
                continue;

            if (_whitelist.IsWhitelistFail(args.Whitelist, entity))
                continue;

            if (_whitelist.IsBlacklistPass(args.Blacklist, entity))
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
    public string SolutionTarget { get; set; } = "chemicals";
}
public sealed partial class InjectReagentsToTargetEvent : EntityTargetActionEvent
{
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("solution")]
    public Solution Solution { get; set; } = new();

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("solutionTarget")]
    public string SolutionTarget { get; set; } = "chemicals";
}
public sealed partial class InjectReagentsInRangeEvent : InstantActionEvent
{
    [DataField]
    public bool InjectToPerformer { get; set; } = false;

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
