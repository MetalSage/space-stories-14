using Content.Shared.Mind.Components;

namespace Content.Server._Stories.Language.Systems;

public sealed partial class MindLanguageSystem : EntitySystem
{
    [Dependency] private LanguageSystem _language = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MindContainerComponent, MindAddedMessage>(OnMindAdded);
    }

    private void OnMindAdded(Entity<MindContainerComponent> ent, ref MindAddedMessage args)
    {
        if (args.TransferEntity is not { } previous || previous == ent.Owner)
            return;

        if (TerminatingOrDeleted(ent.Owner))
            return;

        _language.TransferMindBoundLanguages(previous, ent.Owner);
    }
}
