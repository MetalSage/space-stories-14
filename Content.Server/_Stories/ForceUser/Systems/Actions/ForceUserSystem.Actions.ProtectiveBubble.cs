using Content.Server._Stories.ForceUser.ProtectiveBubble.Components;
using Content.Shared._Stories.ForceUser;
using Content.Shared._Stories.ForceUser.Actions.Events;

namespace Content.Server._Stories.ForceUser;

public sealed partial class ForceUserSystem
{
    public void InitializeProtectiveBubble()
    {
        SubscribeLocalEvent<ForceUserComponent, CreateProtectiveBubbleEvent>(OnProtectiveBubble);
    }

    private void OnProtectiveBubble(EntityUid uid, ForceUserComponent comp, CreateProtectiveBubbleEvent args)
    {
        if (args.Handled)
            return;

        if (HasComp<ProtectiveBubbleUserComponent>(uid))
            return;

        _bubble.StartBubbleWithUser(args.Proto, uid);

        args.Handled = true;
    }
}
