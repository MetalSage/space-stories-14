using Content.Client.Chemistry.Containers.EntitySystems;
using Content.Shared._Stories.ReagentStatusIcon;
using Content.Shared.Ghost;
using Content.Shared.StatusIcon.Components;
using Content.Shared.Whitelist;
using Robust.Client.Player;
using Robust.Shared.Prototypes;

namespace Content.Client._Stories.ReagentStatusIcon
{
    public sealed partial class ReagentStatusIconSystem : EntitySystem
    {
        [Dependency] private readonly SolutionContainerSystem _solution = default!;
        [Dependency] private readonly IPrototypeManager _prototype = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ReagentStatusIconComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
        }

        private void OnGetStatusIconsEvent(EntityUid uid, ReagentStatusIconComponent component, ref GetStatusIconsEvent args)
        {
            var viewer = _playerManager.LocalSession?.AttachedEntity;
            var showTo = _prototype.Index(component.StatusIcon).ShowTo;

            if (!(_prototype.Index(component.StatusIcon).VisibleToGhosts && HasComp<GhostComponent>(viewer)))
            {
                if (showTo != null && !_entityWhitelist.IsValid(showTo, viewer))
                    return;
            }

            if (_solution.TryGetSolution(uid, component.Solution, out var solution) && solution.Value.Comp.Solution.ContainsReagent(component.Reagent))
                args.StatusIcons.Add(_prototype.Index(component.StatusIcon));
        }

    }
}
