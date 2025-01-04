using Content.Shared.Throwing;
using Content.Server.Explosion.EntitySystems;
using Robust.Shared.Random;




namespace Content.Server._Stories.TriggerOnLand
{
    public sealed class TriggerOnLandSystem : EntitySystem
    {
        [Dependency] private readonly TriggerSystem _triggerSystem = default!;
        [Dependency] private readonly IRobustRandom _random = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<TriggerOnLandComponent, LandEvent>(TriggerOnLand);
        }




        private void TriggerOnLand(EntityUid uid, TriggerOnLandComponent component, ref LandEvent args)
        {
            if (_random.Prob(component.Prob))
            {
                _triggerSystem.Trigger(uid, args.User);
            }
        }

    }

}