using Content.Shared.Emag.Systems;
using Content.Shared.Inventory.Events;
using Content.Shared.NPC.Components;
using Content.Shared.Popups;
using Content.Shared.Trigger.Components;
using Content.Shared.Trigger.Systems;

namespace Content.Server._Stories.ClothingWhitelist;

public sealed class ClothingWhitelistSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly TriggerSystem _trigger = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ClothingWhitelistComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<ClothingWhitelistComponent, GotUnequippedEvent>(OnUnequipped);
        SubscribeLocalEvent<ClothingWhitelistComponent, GotEmaggedEvent>(OnEmagged);
    }

    private void OnEquipped(EntityUid uid, ClothingWhitelistComponent comp, GotEquippedEvent args)
    {
        if (!TryComp<NpcFactionMemberComponent>(args.Equipee, out var npc))
            return;

        var fs = npc.Factions;
        if ((comp.FactionsWhitelist == null || fs.Overlaps(comp.FactionsWhitelist))
            && (comp.FactionsBlacklist == null || !fs.Overlaps(comp.FactionsBlacklist)))
            return;

        _popupSystem.PopupEntity(Loc.GetString("Ошибка доступа! Активация протоколов защиты.."),
            args.Equipee,
            args.Equipee,
            PopupType.LargeCaution);

        var timer = EnsureComp<TimerTriggerComponent>(uid);

        timer.Delay = TimeSpan.FromSeconds(comp.Delay);
        timer.BeepInterval = TimeSpan.FromSeconds(comp.BeepInterval);

        if (comp.InitialBeepDelay.HasValue)
            timer.InitialBeepDelay = TimeSpan.FromSeconds(comp.InitialBeepDelay.Value);
        else
            timer.InitialBeepDelay = null;

        timer.BeepSound = comp.BeepSound;

        _trigger.ActivateTimerTrigger(uid, args.Equipee);
    }

    private void OnUnequipped(EntityUid uid, ClothingWhitelistComponent comp, GotUnequippedEvent args)
    {
        _trigger.StopTimerTrigger(uid);
    }

    private void OnEmagged(EntityUid uid, ClothingWhitelistComponent comp, GotEmaggedEvent args)
    {
        _popupSystem.PopupEntity(Loc.GetString("Сброс протоколов защиты.."), uid);
        RemComp<ClothingWhitelistComponent>(uid);
    }
}
