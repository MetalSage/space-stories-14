using Content.Shared.Mobs.Components;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.Conversion;

public abstract class SharedConversionSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    public bool IsConverted(EntityUid uid, ProtoId<ConversionPrototype> prototype, ConversionableComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        var proto = _prototype.Index(prototype);

        return component.ActiveConversions.ContainsKey(proto.ID);
    }
    public bool CanConvert(EntityUid target, ProtoId<ConversionPrototype> prototype, EntityUid? performer = null, ConversionableComponent? component = null)
    {
        if (!Resolve(target, ref component))
            return false;

        if (IsConverted(target, prototype))
            return false;

        var proto = _prototype.Index(prototype);

        if (TryComp<MobStateComponent>(target, out var mobState) &&
            proto.AllowedMobStates != null &&
            !proto.AllowedMobStates.Contains(mobState.CurrentState))
            return false;

        if (_entityWhitelist.IsWhitelistFail(proto.Whitelist, target))
            return false;

        if (_entityWhitelist.IsBlacklistPass(proto.Blacklist, target))
            return false;

        if (!component.AllowedConversions.Contains(proto.ID))
            return false;

        var ev = new ConvertAttemptEvent(target, performer, proto);
        RaiseLocalEvent(target, (object) ev);

        return !ev.Cancelled;
    }
    public bool CanRevert(EntityUid target, ProtoId<ConversionPrototype> prototype, EntityUid? performer = null, ConversionableComponent? component = null)
    {
        if (!Resolve(target, ref component))
            return false;

        if (!IsConverted(target, prototype))
            return false;

        var proto = _prototype.Index(prototype);

        var ev = new RevertAttemptEvent(target, performer, proto);
        RaiseLocalEvent(target, (object) ev);

        return !ev.Cancelled;
    }
}
