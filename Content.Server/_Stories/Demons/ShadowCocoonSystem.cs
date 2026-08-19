using Content.Server.Popups;
using Content.Shared._Stories.Demons;
using Content.Shared.Destructible;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._Stories.Demons;

public sealed partial class ShadowCocoonSystem : EntitySystem
{
    [Dependency] private ShadowGrappleSystem _grapple = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowCocoonComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ShadowCocoonComponent, DestructionEventArgs>(OnDestruction);
        SubscribeLocalEvent<ShadowCocoonComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAltVerbs);
    }

    private void OnGetAltVerbs(Entity<ShadowCocoonComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess || !HasComp<DemonImmunitiesComponent>(args.User))
            return;

        var user = args.User;
        var silent = ent.Comp.Silent;

        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString(silent ? "shadow-cocoon-lure-enable" : "shadow-cocoon-lure-disable"),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/smite.svg.192dpi.png")),
            Act = () =>
            {
                ent.Comp.Silent = !silent;
                _popup.PopupEntity(
                    Loc.GetString(silent ? "shadow-cocoon-lure-enabled-popup" : "shadow-cocoon-lure-disabled-popup"),
                    ent,
                    user);
            },
        });
    }

    private void OnMapInit(Entity<ShadowCocoonComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.BodyContainer = _container.EnsureContainer<Container>(ent, ShadowCocoonComponent.ContainerId);
    }

    private void OnDestruction(Entity<ShadowCocoonComponent> ent, ref DestructionEventArgs args)
    {
        if (ent.Comp.BodyContainer is not { } container)
            return;

        _container.EmptyContainer(container);
    }

    public bool TryInsertBody(Entity<ShadowCocoonComponent> ent, EntityUid body)
    {
        if (ent.Comp.BodyContainer is not { } container)
            return false;

        return _container.Insert(body, container);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ShadowCocoonComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.NextPulse > _timing.CurTime)
                continue;

            comp.NextPulse = _timing.CurTime + comp.PulseInterval;
            var coords = Transform(uid).Coordinates;
            _grapple.ExtinguishNearby(coords, comp.ExtinguishRange);

            if (!comp.Silent && comp.HallucinationSounds != null && _random.Prob(comp.HallucinationChance))
                _audio.PlayPvs(comp.HallucinationSounds, coords);
        }
    }
}
