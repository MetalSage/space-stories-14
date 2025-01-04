using Content.Server.Explosion.EntitySystems;
using Content.Shared.Emag.Systems;
using Content.Shared.Explosion.Components;
using Content.Shared.Inventory.Events;
using Content.Shared.NPC.Components;
using Content.Shared.Popups;

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
        if (!TryComp<NpcFactionMemberComponent>(args.Equipee, out var npc)) return;

        var fs = npc.Factions;
        if ((comp.FactionsWhitelist == null || fs.Overlaps(comp.FactionsWhitelist))
        && (comp.FactionsBlacklist == null || !fs.Overlaps(comp.FactionsBlacklist))) return;

        _popupSystem.PopupEntity(Loc.GetString("Ошибка доступа! Активация протоколов защиты.."), args.Equipee, args.Equipee, PopupType.LargeCaution);

        _trigger.HandleTimerTrigger(
            uid,
            args.Equipee,
            comp.Delay,
            comp.BeepInterval,
            comp.InitialBeepDelay,
            comp.BeepSound
        );
    }

    private void OnUnequipped(EntityUid uid, ClothingWhitelistComponent comp, GotUnequippedEvent args)
    {
        RemComp<ActiveTimerTriggerComponent>(uid);
    }

    private void OnEmagged(EntityUid uid, ClothingWhitelistComponent comp, GotEmaggedEvent args)
    {
        _popupSystem.PopupEntity(Loc.GetString("Сброс протоколов защиты.."), uid);
        RemComp<ClothingWhitelistComponent>(uid);
    }
}
