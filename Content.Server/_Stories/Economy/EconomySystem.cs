using Content.Server._Stories.Economy.Components;
using Content.Server.CartridgeLoader;
using Content.Shared.CartridgeLoader;
using Robust.Shared.Containers;

namespace Content.Server._Stories.Economy;

public sealed class EconomySystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem _cartridgeLoader = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public bool TrySendNotification(EntityUid mindId, string title, string message)
    {
        if (!TryComp<MindBankAccountComponent>(mindId, out var bankAccount) || !bankAccount.NotificationsEnabled)
            return false;

        if (bankAccount.LinkedIdCard == null || !Exists(bankAccount.LinkedIdCard))
            return false;

        var idCardUid = bankAccount.LinkedIdCard.Value;

        if (!_container.TryGetContainingContainer(idCardUid, out var container))
            return false;

        var pdaUid = container.Owner;

        if (!HasComp<CartridgeLoaderComponent>(pdaUid))
            return false;

        _cartridgeLoader.SendNotification(pdaUid, title, message);
        return true;
    }
}
