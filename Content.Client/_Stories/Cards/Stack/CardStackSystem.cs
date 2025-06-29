using Robust.Client.GameObjects;

using Content.Shared._Stories.Cards.Stack;
using Content.Shared.Examine;


namespace Content.Client._Stories.Cards.Stack;
public sealed class CardStackSystem : SharedCardStackSystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    public override void Initialize()
    {
        base.Initialize();
    }
}
