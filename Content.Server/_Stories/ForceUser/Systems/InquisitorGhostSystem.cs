using Content.Server._Stories.ForceUser.Components;
using Content.Server.Light.EntitySystems;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Light.Components;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Polymorph;

namespace Content.Server._Stories.ForceUser.Systems;

public sealed class InquisitorGhostSystem : EntitySystem
{
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly PoweredLightSystem _poweredLight = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<InquisitorGhostComponent, MindAddedMessage>(OnInit);
        SubscribeLocalEvent<InquisitorGhostComponent, RevertPolymorphActionEvent>(OnRevert);
    }

    private void OnInit(EntityUid uid, InquisitorGhostComponent component, MindAddedMessage args)
    {
        if (_mind.TryGetMind(uid, out var mind, out _))
            _actions.RemoveProvidedActions(uid, mind);
        if (TryComp<ActionsContainerComponent>(uid, out var container))
        {
            foreach (var ent in container.Container.ContainedEntities)
            {
                _actionContainer
                    .RemoveAction(ent);
            }
        }

        _actions.AddAction(uid, component.RevertActionPrototype);
    }

    private void OnRevert(EntityUid uid, InquisitorGhostComponent component, RevertPolymorphActionEvent args)
    {
        foreach (var (ent, comp) in _lookup.GetEntitiesInRange<PoweredLightComponent>(_xform.GetMapCoordinates(uid),
                     component.Range))
        {
            _poweredLight.TryDestroyBulb(ent, comp);
        }
    }
}
