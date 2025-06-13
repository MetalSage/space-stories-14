using Content.Shared._Stories.ProtectiveBubble.Components;
using Content.Shared.Damage;
using Content.Server._Stories.ForceUser.Components;
using Content.Shared._Stories.ForceUser;
using Content.Shared.Weapons.Melee.EnergySword;

namespace Content.Server._Stories.ForceUser;

public sealed partial class ForceUserSystem
{
    // TODO: Перенести в компонент ForceUserProtectiveBubble.
    public const float ProtectiveBubbleVolumeCost = 5f;

    public readonly DamageSpecifier ProtectiveBubbleRegeneration = new()
    {
        DamageDict = {
        { "Blunt", -2.5f },
        { "Slash", -2.5f },
        { "Piercing", -5f },
        { "Heat", -2.5f }
        }
    };

    public readonly DamageSpecifier ProtectiveBubbleDegradation = new()
    {
        DamageDict = {
        { "Blunt", 5f },
        { "Slash", 5f },
        { "Piercing", 10f },
        { "Heat", 5f }
        }
    };

    public void UpdateProtectiveBubble(float frameTime)
    {
        var query = EntityQueryEnumerator<ForceUserProtectiveBubbleComponent, ProtectiveBubbleComponent>();
        while (query.MoveNext(out var uid, out var forceBubble, out var bubble))
        {
            if (!(bubble.User is { } bubbleUser))
                continue;

            // FIXME: Очень глупо делать это каждый тик.
            if (TryComp<ForceUserComponent>(bubbleUser, out var forceUser) && TryComp<EnergySwordComponent>(forceUser.Lightsaber, out var sword))
                _bubble.SetColor(uid, sword.ActivatedColor);

            if (_force.TryRemoveVolume(bubbleUser, ProtectiveBubbleVolumeCost * frameTime))
                _damageable.TryChangeDamage(uid, ProtectiveBubbleRegeneration * frameTime, true);
            else
                _damageable.TryChangeDamage(uid, ProtectiveBubbleDegradation * frameTime, true);
        }
    }
}
