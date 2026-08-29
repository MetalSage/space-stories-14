using Content.Shared.Actions;
using Content.Shared.Chemistry.Components;
using Content.Shared.Devour;
using Content.Shared.Devour.Components;
using Content.Shared.DoAfter;
using Content.Shared.Fluids;
using Content.Shared.Mobs;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Stealth;
using Robust.Shared.Containers;

namespace Content.Shared._Stories.Spaf;

public abstract partial class SharedSpafSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _action = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SatiationSystem _satiation = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedPuddleSystem _puddle = default!;
    [Dependency] private SharedStealthSystem _stealth = default!;

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

        SubscribeLocalEvent<SatiationComponent, FoodPopupEvent>(OnFood);
    }

    public bool TryModifyHunger(EntityUid uid, float amount, SatiationComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        var curValue = _satiation.GetValueOrNull((uid, component), SatiationSystem.Hunger) ?? 0f;
        if (curValue - amount < 0)
        {
            _popup.PopupEntity(Loc.GetString("need-more-food"), uid, uid);
            return false;
        }

        _satiation.ModifyValue((uid, component), SatiationSystem.Hunger, -amount);

        return true;
    }

    private void OnDevourDoAfter(EntityUid uid, SpafComponent component, DevourDoAfterEvent args)
    {
        if (!args.Cancelled && TryComp<DevourerComponent>(uid, out var devourer) && TryComp<SatiationComponent>(uid, out var satiation))
            _satiation.ModifyValue((uid, satiation), SatiationSystem.Hunger, devourer.HealRate);
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

        args.Handled = _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager,
            args.Performer,
            TimeSpan.FromSeconds(args.Seconds),
            new SpafStealthDoAfterEvent(),
            args.Performer,
            args.Performer)
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

    private void OnFood(EntityUid uid, SatiationComponent component, FoodPopupEvent args)
    {
        if (args.Handled)
            return;

        var val = _satiation.GetValueOrNull((uid, component), SatiationSystem.Hunger) ?? 0f;
        _popup.PopupEntity("" + val, uid, uid);

        args.Handled = true;
    }
}
