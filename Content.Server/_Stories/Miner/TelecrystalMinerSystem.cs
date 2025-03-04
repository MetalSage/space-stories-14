using Content.Server.Power.Components;
using Content.Server.Chat.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using Content.Server.Station.Systems;
using Content.Server.Power.EntitySystems;
using Robust.Shared.Containers;
using Content.Shared.Stacks;
using Content.Server.Stack;
using Content.Server.Pinpointer;
using Robust.Shared.Utility;
using Robust.Shared.Player;

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
    [Dependency] private readonly BatterySystem _batterySystem = default!;
    [Dependency] private readonly NavMapSystem _navMap = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TelecrystalMinerComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, TelecrystalMinerComponent component, ComponentStartup args)
    {
        component.StartTime = _gameTiming.CurTime;
    }
    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<TelecrystalMinerComponent, BatteryComponent, PowerConsumerComponent>();
        var currentTime = _gameTiming.CurTime;

        while (query.MoveNext(out var entity, out var miner, out var battery, out var powerConsumer))
        {
            powerConsumer.NetworkLoad.DesiredPower = miner.PowerDraw;
            if (powerConsumer.NetworkLoad.ReceivingPower < miner.PowerDraw)
            {
                // p.s. если вы что-то не понимаете то можете спокойно плакать в лс @LuciferEOS
                miner.StartTime = null;
                continue;
            }
            if (miner.StartTime == null)
                miner.StartTime = currentTime;

            var elapsed = (currentTime - miner.LastUpdate).TotalSeconds;
            if (elapsed < 10) // проверяем каждые 10сек чтобы спаммить звуками ибо надо громко
                continue;

            miner.LastUpdate = currentTime;

            _audio.PlayPvs(miner.MiningSound, entity);
            // проверка на добычу тк раз в 50 сек
            if ((currentTime - miner.StartTime.Value).TotalSeconds >= 50)
            {
                miner.StartTime = currentTime;
                miner.AccumulatedTC += 1;

                if (!_containerSystem.TryGetContainer(entity, "tc_slot", out var container))
                    continue;

                if (container.ContainedEntities.Count == 0)
                {
                    var newTC = _entityManager.SpawnEntity("Telecrystal", Transform(entity).Coordinates);
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
            // объявление когда 12тк намайнил майнер (типо 10 минут)
            if (!miner.Notified && miner.AccumulatedTC >= 12)
            {
                miner.Notified = true;
                var station = _station.GetOwningStation(entity);
                if (station != null)
                {
                    var msg = Loc.GetString("announcement-tc-miner-10mins", // щиткод с dragonriftsystem ибо мне лень разбиратся в аннонсах
                        ("location", FormattedMessage.RemoveMarkupOrThrow(_navMap.GetNearestBeaconString((entity, Transform(entity))))));
                    _chat.DispatchGlobalAnnouncement(msg, playSound: false, colorOverride: Color.Red);
                    _audio.PlayGlobal("/Audio/Misc/notice1.ogg", Filter.Broadcast(), true);
                }
            }
        }
    }
}
