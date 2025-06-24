using Content.Shared._Stories.ProtectiveBubble.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Map;
using Content.Shared.Toggleable;

namespace Content.Server._Stories.ProtectiveBubble;

public sealed partial class ProtectiveBubbleSystem
{
    #region color
    private void UpdateAppearance(EntityUid uid, ProtectiveBubbleComponent? component = null, AppearanceComponent? appearanceComponent = null)
    {
        if (!Resolve(uid, ref component) || !Resolve(uid, ref appearanceComponent))
            return;

        _appearance.SetData(uid, ToggleableLightVisuals.Color, component.Color, appearanceComponent);
    }

    public void SetColor(EntityUid uid, Color color, ProtectiveBubbleComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.Color = color;

        UpdateAppearance(uid, component);
    }

    #endregion

    #region bubble
    public EntityUid StartBubble(EntityCoordinates coords, EntProtoId protoId, EntityUid? user, out EntityUid uid, out ProtectiveBubbleComponent component)
    {
        uid = SpawnAtPosition(protoId, coords);
        component = EnsureComp<ProtectiveBubbleComponent>(uid);

        if (user is { } userId)
        {
            component.User = userId;
            EnsureComp<ProtectiveBubbleUserComponent>(userId).ProtectiveBubble = uid;
            if (_container.TryGetOuterContainer(userId, Transform(userId), out var container))
                _xform.SetParent(uid, container.Owner);
            else
                _xform.SetParent(uid, userId);
        }
        return uid;
    }

    public void StopBubble(EntityUid uid, ProtectiveBubbleComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (component.User is { } userId)
        {
            RemComp<ProtectiveBubbleUserComponent>(userId);
            component.User = null;
        }

        foreach (var ent in component.ProtectedEntities)
        {
            StopProtect(ent, uid, component);
        }

        QueueDel(uid);
    }

    #endregion

    #region protect
    public bool IsProtected(EntityUid uid, EntityUid? bubble = null)
    {
        if (!TryComp<ProtectedByProtectiveBubbleComponent>(uid, out var comp))
            return false;

        return !(bubble is { } bubbleId) || comp.ProtectiveBubble == bubbleId;
    }

    private void StartProtect(EntityUid uid, EntityUid bubble, ProtectiveBubbleComponent? component = null)
    {
        if (!Resolve(bubble, ref component) || IsProtected(uid))
            return;

        component.ProtectedEntities.Add(uid);
        AddComp(uid, new ProtectedByProtectiveBubbleComponent() { ProtectiveBubble = bubble }, true);
    }

    private void StopProtect(EntityUid uid, EntityUid bubble, ProtectiveBubbleComponent? component = null)
    {
        if (!Resolve(bubble, ref component) || !IsProtected(uid, bubble))
            return;

        component.ProtectedEntities.Remove(uid);
        RemComp<ProtectedByProtectiveBubbleComponent>(uid);
    }

    #endregion
}
