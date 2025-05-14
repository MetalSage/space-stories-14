using Content.Server.Objectives.Components;
using Content.Server.Shuttles.Systems;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.Roles.Jobs;
using Robust.Shared.Configuration;
using Content.Server.Chat.Managers;
using Robust.Shared.Random;
using Content.Shared.Popups;
using Content.Server.Store.Systems;
using Content.Shared.FixedPoint;

namespace Content.Server.Objectives.Systems;

public sealed class PickRandomJobPersonSystem : EntitySystem
{
    [Dependency] private readonly EmergencyShuttleSystem _emergencyShuttle = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly SharedJobSystem _job = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly TargetObjectiveSystem _target = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;

    private const float UdateDelay = 10f;
    private float _updateTime = 0;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PickRandomJobPersonComponent, ObjectiveAssignedEvent>(OnHeadAssigned);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _updateTime += frameTime;
        if (_updateTime < UdateDelay)
            return;
        _updateTime -= UdateDelay;

        var query = EntityQueryEnumerator<PickRandomJobPersonComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Handled == false && TryComp<MindComponent>(comp.MindId, out var mind))
            {
                var ev = new ObjectiveAssignedEvent(comp.MindId, mind);
                RaiseLocalEvent(uid, ref ev);
            }
        }
    }

    private void OnHeadAssigned(EntityUid uid, PickRandomJobPersonComponent comp, ref ObjectiveAssignedEvent args)
    {
        comp.MindId = args.MindId;

        // invalid prototype
        if (!TryComp<TargetObjectiveComponent>(uid, out var target))
            return;

        // target already assigned
        if (comp.Handled)
            return;

        // no other humans to kill
        var allHumans = _mind.GetAliveHumans(args.MindId);
        if (allHumans.Count == 0)
            return;

        var allHeads = new HashSet<Entity<MindComponent>>();
        foreach (var mind in allHumans)
        {
            if (_job.MindTryGetJob(mind, out var job) && job.ID == comp.JobID)
                allHeads.Add(mind);
        }

        if (allHeads.Count == 0)
            allHeads = allHumans; // fallback to non-head target

        var targetMindUid = _random.Pick(allHeads);
        var targetUid = EnsureComp<MindComponent>(targetMindUid).CurrentEntity;

        _target.SetTarget(uid, targetMindUid, target);

        if (comp.JobID == "GuardianNt" && targetUid != null) // FIXME: SHITCODED
        {
            _store.TryAddCurrency(new Dictionary<string, FixedPoint2> { {"SkillPoint", 10} }, targetUid.Value);
            _popup.PopupEntity("Вы чувствуете зло и оно нацелено на вас... Проверьте магазин навыков.", targetUid.Value, targetUid.Value, PopupType.LargeCaution);
        }

        comp.Handled = true;
    }
}
