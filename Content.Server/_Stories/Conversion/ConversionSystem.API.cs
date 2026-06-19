using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared._Stories.Conversion;
using Content.Shared.Mind;
using Content.Shared.Radio;
using Content.Shared.Radio.Components;
using Content.Shared.Roles;
using Content.Shared.Roles.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._Stories.Conversion;

public sealed partial class ConversionSystem
{
    public HashSet<EntityUid> GetEntitiesConvertedBy(EntityUid? uid, ProtoId<ConversionPrototype> prototype)
    {
        HashSet<EntityUid> entities = new();
        var query = AllEntityQuery<ConversionableComponent>();

        while (query.MoveNext(out var entity, out var comp))
        {
            foreach (var conversion in comp.ActiveConversions)
            {
                if (conversion.Key == prototype.Id && conversion.Value.Owner != null && GetEntity(conversion.Value.Owner.Value) == uid)
                    entities.Add(entity);
            }
        }

        return entities;
    }

    public bool TryGetConversion(EntityUid uid,
        string id,
        [NotNullWhen(true)] out ConversionData? conversion,
        ConversionableComponent? component = null)
    {
        conversion = null;

        if (!Resolve(uid, ref component, false))
            return false;

        return component.ActiveConversions.TryGetValue(id, out conversion);
    }

    public bool TryRevert(EntityUid target,
        ProtoId<ConversionPrototype> prototype,
        EntityUid? performer = null,
        ConversionableComponent? component = null)
    {
        if (!_prototype.TryIndex(prototype, out var proto))
            return false;

        if (!CanRevert(target, prototype, performer, component))
            return false;

        DoRevert(target, proto, performer, component);
        return true;
    }

    private void DoRevert(EntityUid target,
        ConversionPrototype proto,
        EntityUid? performer = null,
        ConversionableComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return;

        if (!component.ActiveConversions.TryGetValue(proto.ID, out var data))
            return;

        var ev = new RevertedEvent(target, performer, data);
        RaiseLocalEvent(target, (object)ev, true);

        _mind.TryGetMind(target, out var mindId, out var mind);

        if (proto.EndBriefing != null && mindId != default)
        {
            _antag.SendBriefing(target,
                Loc.GetString(proto.EndBriefing.Value.Text ?? ""),
                proto.EndBriefing.Value.Color,
                proto.EndBriefing.Value.Sound);
        }

        EntityManager.RemoveComponents(target, proto.Components);

        if (mindId != default)
            MindRemoveRoles(mindId, proto.MindRoles);

        if (proto.Channels.Count > 0)
        {
            var channelProtoIds = proto.Channels.Select(id => new ProtoId<RadioChannelPrototype>(id));
            EnsureComp<IntrinsicRadioReceiverComponent>(target);
            EnsureComp<IntrinsicRadioTransmitterComponent>(target).Channels.ExceptWith(channelProtoIds);
            EnsureComp<ActiveRadioComponent>(target).Channels.ExceptWith(channelProtoIds);
        }

        component.ActiveConversions.Remove(proto.ID);

        Dirty(target, component);
    }

    public bool TryConvert(EntityUid target,
        ProtoId<ConversionPrototype> prototype,
        EntityUid? performer = null,
        ConversionableComponent? component = null)
    {
        if (!_prototype.TryIndex(prototype, out var proto))
            return false;

        if (!CanConvert(target, prototype, performer, component))
            return false;

        DoConvert(target, proto, performer, component);
        return true;
    }

    private void DoConvert(EntityUid target,
        ConversionPrototype proto,
        EntityUid? performer = null,
        ConversionableComponent? component = null)
    {
        component ??= EnsureComp<ConversionableComponent>(target);

        _mind.TryGetMind(target, out var mindId, out var mind);

        if (proto.Briefing != null && mindId != default)
        {
            _antag.SendBriefing(target,
                Loc.GetString(proto.Briefing.Value.Text ?? ""),
                proto.Briefing.Value.Color,
                proto.Briefing.Value.Sound);
        }

        EntityManager.AddComponents(target, proto.Components);

        if (proto.MindRoles != null && mindId != default)
        {
            foreach (var role in proto.MindRoles)
            {
                _role.MindAddRole(mindId, role);
            }
        }

        if (proto.Channels.Count > 0)
        {
            var channelProtoIds = proto.Channels.Select(id => new ProtoId<RadioChannelPrototype>(id));
            EnsureComp<IntrinsicRadioReceiverComponent>(target);
            EnsureComp<IntrinsicRadioTransmitterComponent>(target).Channels.UnionWith(channelProtoIds);
            EnsureComp<ActiveRadioComponent>(target).Channels.UnionWith(channelProtoIds);
        }

        var conversion = new ConversionData
        {
            Owner = GetNetEntity(performer),
            Prototype = proto.ID,
            StartTime = _timing.CurTime,
            EndTime = proto.Duration == null ? null : _timing.CurTime + TimeSpan.FromSeconds(proto.Duration.Value),
        };

        component.ActiveConversions.Add(proto.ID, conversion);

        var ev = new ConvertedEvent(target, performer, conversion);
        RaiseLocalEvent(target, (object)ev, true);
        Dirty(target, component);
    }

    public void MindRemoveRoles(EntityUid mindId, List<EntProtoId>? roles, MindComponent? mind = null)
    {
        if (!Resolve(mindId, ref mind))
            return;

        if (roles == null)
            return;

        var rolesUid = mind.MindRoleContainer.ContainedEntities
            .Where(role => EntityPrototyped(role, roles))
            .ToList();

        rolesUid.ForEach(mindRole =>
        {
            var antagonist = Comp<MindRoleComponent>(mindRole).Antag;

            QueueDel(mindRole);

            var message = new RoleRemovedEvent(mindId, mind, antagonist);

            if (mind.OwnedEntity != null)
                RaiseLocalEvent(mind.OwnedEntity.Value, message, true);
        });
    }

    public bool EntityPrototyped(EntityUid role, List<EntProtoId> roles)
    {
        var proto = MetaData(role).EntityPrototype;

        if (proto == null)
            return false;

        return roles.Contains(proto);
    }
}
