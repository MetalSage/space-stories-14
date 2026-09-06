using Content.Server.Administration.Managers;
using Content.Server.DeviceNetwork.Systems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Damage.Systems;
using Content.Shared.Emag.Systems;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Content.Shared.PowerCell;
using Content.Shared.Roles;
using Content.Shared.Silicons.Borgs;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Trigger.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.Silicons.Borgs;

/// <inheritdoc/>
public sealed partial class BorgSystem : SharedBorgSystem
{
    [Dependency] private IBanManager _banManager = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private DeviceNetworkSystem _deviceNetwork = default!;
    [Dependency] private TriggerSystem _trigger = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedBatterySystem _battery = default!;
    [Dependency] private EmagSystem _emag = default!;
    [Dependency] private MobThresholdSystem _mobThresholdSystem = default!;
    [Dependency] private ItemSlotsSystem _itemSlotsSystem = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private PowerCellSystem _powerCell = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    // Stories-Borg-Start
    [Dependency] private SharedHandsSystem _hands = default!;
    private float _rechargeRestockTimer;
    // Stories-Borg-End

    public static readonly ProtoId<JobPrototype> BorgJobId = "Borg";

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        InitializeTransponder();
    }

    public override bool CanPlayerBeBorged(ICommonSession session)
    {
        if (_banManager.GetJobBans(session.UserId)?.Contains(BorgJobId) == true)
            return false;

        return true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateTransponder(frameTime);
        UpdateRestock(frameTime); // Stories-Borg
    }

    // Stories-Borg-Start
    private void UpdateRestock(float frameTime)
    {
        _rechargeRestockTimer += frameTime;
        if (_rechargeRestockTimer < 2.0f)
            return;

        _rechargeRestockTimer = 0f;

        var query = EntityQueryEnumerator<BorgChassisComponent, InsideChargerComponent>();
        while (query.MoveNext(out var uid, out var chassis, out _))
        {
            RestockBorgModules((uid, chassis));
        }
    }

    public void RestockBorgModules(Entity<BorgChassisComponent> borg)
    {
        if (!_container.TryGetContainer(borg.Owner, borg.Comp.ModuleContainerId, out var container))
            return;

        var restockedAny = false;

        foreach (var moduleUid in container.ContainedEntities)
        {
            if (!TryComp<ItemBorgModuleComponent>(moduleUid, out var itemModule))
                continue;

            if (!_container.TryGetContainer(moduleUid, itemModule.HoldingContainer, out var holdingContainer))
                continue;

            var xform = Transform(borg.Owner);

            for (var i = 0; i < itemModule.Hands.Count; i++)
            {
                var hand = itemModule.Hands[i];
                if (hand.Item == null)
                    continue;

                var handId = $"{GetNetEntity(moduleUid)}-hand-{i}";

                var needsReplenish = false;

                if (!itemModule.StoredItems.TryGetValue(handId, out var storedItem) ||
                    Deleted(storedItem) ||
                    Terminating(storedItem))
                {
                    needsReplenish = true;
                }
                else
                {
                    var inModuleContainer = Transform(storedItem).ParentUid == holdingContainer.Owner;
                    var inBorgHand = _hands.IsHolding(borg.Owner, storedItem);

                    if (!inModuleContainer && !inBorgHand)
                        needsReplenish = true;
                }

                if (needsReplenish)
                {
                    var newItem = Spawn(hand.Item, xform.Coordinates);
                    itemModule.StoredItems[handId] = newItem;
                    itemModule.Spawned = true;

                    if (TryComp<HandsComponent>(borg.Owner, out var handsComp) &&
                        _hands.TryGetHand((borg.Owner, handsComp), handId, out _) &&
                        !_hands.TryGetHeldItem((borg.Owner, handsComp), handId, out _))
                    {
                        _hands.DoPickup(borg.Owner, handId, newItem, handsComp);
                        if (!hand.ForceRemovable && hand.Hand.Whitelist == null && hand.Hand.Blacklist == null)
                        {
                            EnsureComp<UnremoveableComponent>(newItem);
                        }
                    }
                    else
                    {
                        _container.Insert(newItem, holdingContainer);
                    }

                    Dirty(moduleUid, itemModule);
                    restockedAny = true;
                }
            }
        }

        if (restockedAny)
        {
            _popup.PopupEntity(Loc.GetString("stories-borg-charger-restocked-tools"), borg.Owner, borg.Owner, PopupType.Small);
        }
    }
    // Stories-Borg-End
}
