using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Robust.Shared.Containers;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Fluids;
using Robust.Shared.Prototypes;
using Content.Shared.Stealth;
using Content.Shared.Chemistry.Components;
using Content.Shared.Devour;
using Content.Shared.Devour.Components;
using Content.Shared.Mobs;

namespace Content.Shared._Stories.Spaf;

public abstract partial class SharedSpafSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly SharedPuddleSystem _puddle = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpafComponent, ComponentInit>(OnInit);

        SubscribeLocalEvent<SpafComponent, SpafCreateEntityEvent>(OnCreateEntity);
        SubscribeLocalEvent<SpafComponent, SpafSpillSolutionEvent>(OnSpill);
        SubscribeLocalEvent<SpafComponent, SpafStealthEvent>(OnStealth);
        SubscribeLocalEvent<SpafComponent, SpafStealthDoAfterEvent>(OnStealthDoAfter);

        SubscribeLocalEvent<SpafComponent, DevourDoAfterEvent>(OnDevourDoAfter);
        SubscribeLocalEvent<SpafComponent, MobStateChangedEvent>(OnMobStateChanged);

        SubscribeLocalEvent<HungerComponent, FoodPopupEvent>(OnFood);
    }

    public bool TryModifyHunger(EntityUid uid, float amount, HungerComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (component.CurrentHunger - amount < 0)
        {
            _popup.PopupEntity(Loc.GetString("need-more-food"), uid, uid);
            return false;
        }

        _hunger.ModifyHunger(uid, -amount, component);

        return true;
    }

    private void OnDevourDoAfter(EntityUid uid, SpafComponent component, DevourDoAfterEvent args)
    {
        if (!args.Cancelled && TryComp<DevourerComponent>(uid, out var devourer))
            _hunger.ModifyHunger(uid, devourer.HealRate);
    }

    private void OnMobStateChanged(EntityUid uid, SpafComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead || !TryComp<DevourerComponent>(uid, out var devourer))
            return;

        _container.EmptyContainer(devourer.Stomach);
    }

    private void OnInit(EntityUid uid, SpafComponent component, ComponentInit args)
    {
        foreach (var action in component.Actions)
        {
            var actionId = _action.AddAction(uid, action);
            if (actionId.HasValue)
                component.GrantedActions.Add(actionId.Value);
        }
    }

    private void OnCreateEntity(EntityUid uid, SpafComponent component, SpafCreateEntityEvent args)
    {
        if (args.Handled || !TryModifyHunger(args.Performer, args.HungerCost))
            return;

        SpawnAtPosition(args.Prototype, Transform(args.Performer).Coordinates);

        args.Handled = true;
    }

    private void OnSpill(EntityUid uid, SpafComponent component, SpafSpillSolutionEvent args)
    {
        if (args.Handled || !TryModifyHunger(args.Performer, args.HungerCost))
            return;

        var solution = new Solution(args.Solution);

        _puddle.TrySpillAt(Transform(args.Performer).Coordinates, solution, out _);

        args.Handled = true;
    }

    private void OnStealth(EntityUid uid, SpafComponent component, SpafStealthEvent args)
    {
        if (args.Handled || !TryModifyHunger(args.Performer, args.HungerCost))
            return;

        // DoAfter с Hidden = true используется, чтобы спаф мог видеть сколько секунд
        // у него осталось. Достаточно удобно, не требует писать много кода для этого.

        _stealth.SetEnabled(uid, true);

        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.Performer, TimeSpan.FromSeconds(args.Seconds), new SpafStealthDoAfterEvent(), args.Performer, args.Performer)
        {
            Hidden = true,
            BreakOnHandChange = false,
            BreakOnDropItem = false,
            BreakOnWeightlessMove = false,
            RequireCanInteract = false,
        });
    }

    private void OnStealthDoAfter(EntityUid uid, SpafComponent component, SpafStealthDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        _stealth.SetEnabled(uid, false);

        args.Handled = true;
    }

    private void OnFood(EntityUid uid, HungerComponent component, FoodPopupEvent args)
    {
        if (args.Handled)
            return;

        _popup.PopupEntity("" + component.CurrentHunger, uid, uid);

        args.Handled = true;
    }
}
