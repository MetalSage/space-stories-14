using Content.Server.Power.Components;
using Content.Server.Chat.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using Content.Server.Station.Systems;
using Content.Shared.Popups;
using Robust.Shared.Containers;
using Content.Shared.Stacks;
using Content.Server.Stack;
using Content.Server.Pinpointer;
using Robust.Shared.Utility;
using Robust.Shared.Player;
using Content.Shared.Interaction;

namespace Content.Server._Stories.Miner;

public sealed class TelecrystalMinerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly StackSystem _stackSystem = default!;
    [Dependency] private readonly NavMapSystem _navMap = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TelecrystalMinerComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<TelecrystalMinerComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<TelecrystalMinerComponent, InteractHandEvent>(OnInteractHand);
    }

    private void OnStartup(EntityUid uid, TelecrystalMinerComponent component, ComponentStartup args)
    {
        component.StartTime = _gameTiming.CurTime;
        component.PowerDraw = component.DefaultPowerDraw;
        component.IsDisabled = false;
        var originStation = _station.GetOwningStation(uid);

        if (originStation != null)
        {
            component.OriginStation = originStation;
        } // щиткод взят с системы нюки
        else
        {
            var transform = Transform(uid);
            component.OriginMapGrid = (transform.MapID, transform.GridUid);
        }
    }

    private void OnShutdown(EntityUid uid, TelecrystalMinerComponent component, ComponentShutdown args)
    {
        component.PowerDraw = component.DefaultPowerDraw;
        component.IsDisabled = false;
    }

    private void OnInteractHand(EntityUid uid, TelecrystalMinerComponent component, InteractHandEvent args)
    {
        if (!component.IsDisabled)
            return;
        component.IsDisabled = false;
        component.PowerDraw = component.DefaultPowerDraw;
        _chat.TrySendInGameICMessage(uid, Loc.GetString("tc-miner-restarted"), InGameICChatType.Speak, false);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<TelecrystalMinerComponent, PowerConsumerComponent>();
        var currentTime = _gameTiming.CurTime;

        while (query.MoveNext(out var uid, out var miner, out var powerConsumer))
        {
            // чекаем станцию
            var currentStation = _station.GetOwningStation(uid);
            if (currentStation == null || miner.OriginStation != null && currentStation != miner.OriginStation && miner.IsDisabled == false)
            {
                if (!miner.IsDisabled) // если не на станции то увы :j_jokerge:
                {
                    miner.IsDisabled = true;
                    _chat.TrySendInGameICMessage(uid, Loc.GetString("tc-miner-nongrid"), InGameICChatType.Speak, false);
                }
                continue;
            }
            if (miner.IsDisabled)
            {
                powerConsumer.NetworkLoad.ReceivingPower = 0;
                continue;
            }

            powerConsumer.NetworkLoad.DesiredPower = miner.PowerDraw;
            if (miner.PowerDraw >= miner.MaxPowerDraw)
            {
                miner.IsDisabled = true;
                powerConsumer.NetworkLoad.DesiredPower = 0;
                _chat.TrySendInGameICMessage(uid, Loc.GetString("tc-miner-overload"), InGameICChatType.Speak, false);
                continue; // пусть игрок сам перезапускает майер
            }

            if (miner.StartTime == null)
                miner.StartTime = currentTime;

            var elapsed = (currentTime - miner.LastUpdate).TotalSeconds;
            if (elapsed < 10)
                continue;

            miner.LastUpdate = currentTime;
            _audio.PlayPvs(miner.MiningSound, uid);

            if ((currentTime - miner.StartTime.Value).TotalSeconds >= 50)
            {
                miner.StartTime = currentTime;
                miner.AccumulatedTC += 1;
                miner.PowerDraw = Math.Min(miner.PowerDraw + miner.PowerIncreasePerTC, miner.MaxPowerDraw);

                if (!_containerSystem.TryGetContainer(uid, "tc_slot", out var container))
                    continue;

                if (container.ContainedEntities.Count == 0)
                {
                    var newTC = _entityManager.SpawnEntity("Telecrystal", Transform(uid).Coordinates);
                    if (TryComp(newTC, out StackComponent? newStack))
                    {
                        _stackSystem.SetCount(newTC, 1);
                        _containerSystem.Insert(newTC, container);
                    }
                }
                else if (TryComp(container.ContainedEntities[0], out StackComponent? stack))
                {
                    _stackSystem.SetCount(container.ContainedEntities[0], stack.Count + 1);
                }
            }

            if (!miner.Notified && miner.AccumulatedTC >= 12)
            {
                miner.Notified = true;
                var station = _station.GetOwningStation(uid);
                if (station != null)
                {
                    var msg = Loc.GetString("announcement-tc-miner-10mins",
                        ("location", FormattedMessage.RemoveMarkupOrThrow(_navMap.GetNearestBeaconString((uid, Transform(uid))))));
                    _chat.DispatchGlobalAnnouncement(msg, playSound: false, colorOverride: Color.Red);
                    _audio.PlayGlobal("/Audio/Misc/notice1.ogg", Filter.Broadcast(), true); // пиздец.
                }
            }
        }
    }
}
