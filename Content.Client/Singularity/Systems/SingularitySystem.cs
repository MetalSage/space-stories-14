using Content.Shared.CCVar;
using Content.Shared.Singularity;
using Content.Shared.Singularity.Components;
using Content.Shared.Singularity.EntitySystems;
using Robust.Client.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.GameStates;

namespace Content.Client.Singularity.Systems;

/// <summary>
/// The client-side version of <see cref="SharedSingularitySystem"/>.
/// Primarily manages <see cref="SingularityComponent"/>s.
/// </summary>
public sealed partial class SingularitySystem : SharedSingularitySystem
{
    // Stories-Singularity-Start
    [Dependency] private IConfigurationManager _config = default!;

    private bool _disableSinguloWarp;
    // Stories-Singularity-End

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SingularityComponent, ComponentHandleState>(HandleSingularityState);

        // Stories-Singularity-Start
        Subs.CVar(_config, CCVars.DisableSinguloWarp, OnDisableSinguloWarpChanged, true);
        // Stories-Singularity-End
    }

    // Stories-Singularity-Start
    protected override void OnSingularityStartup(EntityUid uid, SingularityComponent component, ComponentStartup args)
    {
        base.OnSingularityStartup(uid, component, args);

        if (TryComp<SpriteComponent>(uid, out var sprite))
        {
            sprite.Visible = _disableSinguloWarp;
        }
    }

    private void OnDisableSinguloWarpChanged(bool disabled)
    {
        _disableSinguloWarp = disabled;

        var query = EntityQueryEnumerator<SingularityComponent, SpriteComponent>();
        while (query.MoveNext(out _, out _, out var sprite))
        {
            sprite.Visible = disabled;
        }
    }
    // Stories-Singularity-End

    /// <summary>
    /// Handles syncing singularities with their server-side versions.
    /// </summary>
    /// <param name="uid">The uid of the singularity to sync.</param>
    /// <param name="comp">The state of the singularity to sync.</param>
    /// <param name="args">The event arguments including the state to sync the singularity with.</param>
    private void HandleSingularityState(EntityUid uid, SingularityComponent comp, ref ComponentHandleState args)
    {
        if (TerminatingOrDeleted(uid) || args.Current is not SingularityComponentState state)
            return;

        SetLevel(uid, state.Level, comp);
    }
}
