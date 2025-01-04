using Content.Server.Light.EntitySystems;
using Content.Shared.Actions;
using Content.Server.Light.Components;
using Content.Shared.Polymorph;
using Content.Server._Stories.ForceUser.Components;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;

namespace Content.Server._Stories.ForceUser.Systems;
public sealed partial class SithGhostSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly PoweredLightSystem _poweredLight = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SithGhostComponent, MindAddedMessage>(OnInit);
        SubscribeLocalEvent<SithGhostComponent, RevertPolymorphActionEvent>(OnRevert);
    }
    private void OnInit(EntityUid uid, SithGhostComponent component, MindAddedMessage args)
    {
        if (_mind.TryGetMind(uid, out var mind, out _))
            _actions.RemoveProvidedActions(uid, mind);
        if (TryComp<ActionsContainerComponent>(uid, out var container))
            foreach (var ent in container.Container.ContainedEntities)
            {
                _actionContainer.RemoveAction(ent); // Убираем подарок от оффов, так как магазин добавляет actions разуму.
            }
        _actions.AddAction(uid, component.RevertActionPrototype);
    }
    private void OnRevert(EntityUid uid, SithGhostComponent component, RevertPolymorphActionEvent args)
    {
        foreach (var (ent, comp) in _lookup.GetEntitiesInRange<PoweredLightComponent>(_xform.GetMapCoordinates(uid), component.Range))
        {
            _poweredLight.TryDestroyBulb(ent, comp);
        }
    }
}
