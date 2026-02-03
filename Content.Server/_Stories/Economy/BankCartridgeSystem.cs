using Content.Server._Stories.Economy.Components;
using Content.Server.CartridgeLoader;
using Content.Server.Station.Systems;
using Content.Shared._Stories.Economy;
using Content.Shared._Stories.Economy.Components;
using Content.Shared.CartridgeLoader;
using Content.Shared.Mind;
using Content.Shared.PDA;

namespace Content.Server._Stories.Economy;

public sealed class BankCartridgeSystem : EntitySystem
{
    [Dependency] private readonly BankSystem _bank = default!;
    [Dependency] private readonly CartridgeLoaderSystem _cartridgeLoader = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BankCartridgeComponent, CartridgeUiReadyEvent>(OnUiReady);
        SubscribeLocalEvent<BankCartridgeComponent, CartridgeMessageEvent>(OnMessage);

        SubscribeLocalEvent<BankCartridgeComponent, CartridgeLoaderExternalContainerChangedEvent>(
            OnExternalContainerChanged);
    }

    private void OnExternalContainerChanged(Entity<BankCartridgeComponent> ent,
        ref CartridgeLoaderExternalContainerChangedEvent args)
    {
        if (args.ContainerId != PdaComponent.PdaIdSlotId)
            return;

        if (TryComp<CartridgeComponent>(ent, out var cartridge) && cartridge.LoaderUid == args.Loader)
            UpdateUi(ent.Owner, args.Loader);
    }

    private void OnMessage(EntityUid uid, BankCartridgeComponent component, CartridgeMessageEvent args)
    {
        switch (args)
        {
            case BankTransferMessage transferMsg:
                OnTransfer(uid, component, transferMsg);
                break;
            case BankLinkIdMessage linkMsg:
                OnLink(uid, component, linkMsg);
                break;
            case BankUnlinkIdMessage unlinkMsg:
                OnUnlink(uid, component, unlinkMsg);
                break;
            case BankToggleNotificationsMessage toggleMsg:
                OnToggleNotifications(uid, component, toggleMsg);
                break;
        }
    }

    private void OnUiReady(EntityUid uid, BankCartridgeComponent component, CartridgeUiReadyEvent args)
    {
        UpdateUi(uid, args.Loader);
    }

    private void UpdateUi(EntityUid cartridgeUid, EntityUid loaderUid)
    {
        var accNum = "----";
        var ownerName = "N/A";
        var balance = 0;
        var isLinked = false;
        var notifications = false;

        if (TryComp<PdaComponent>(loaderUid, out var pda) && pda.ContainedId != null)
        {
            if (TryComp<IdBankAccountComponent>(pda.ContainedId.Value, out var idBank))
            {
                var station = _station.GetOwningStation(loaderUid);
                if (station.HasValue && _bank.TryGetAccount(station.Value, idBank.AccountNumber, out var account))
                {
                    accNum = account.AccountNumber;
                    ownerName = account.OwnerName;
                    balance = account.Balance;
                    isLinked = true;
                }
                else
                    ownerName = "Account Error";
            }
            else
                ownerName = "No Account";
        }
        else
            ownerName = "Insert ID";

        if (isLinked && !string.IsNullOrEmpty(accNum))
        {
            var query = EntityQueryEnumerator<MindBankAccountComponent>();
            while (query.MoveNext(out _, out var mindBank))
            {
                if (mindBank.AccountNumber == accNum)
                {
                    notifications = mindBank.NotificationsEnabled;
                    break;
                }
            }
        }

        _cartridgeLoader.UpdateCartridgeUiState(loaderUid,
            new BankCartridgeUiState(accNum, ownerName, balance, isLinked, notifications));
    }

    private void OnTransfer(EntityUid uid, BankCartridgeComponent component, BankTransferMessage args)
    {
        if (!TryComp<CartridgeComponent>(uid, out var cartridge) || !cartridge.LoaderUid.HasValue)
            return;

        var loaderUid = cartridge.LoaderUid.Value;
        var station = _station.GetOwningStation(loaderUid);
        if (!station.HasValue)
            return;

        if (TryComp<PdaComponent>(loaderUid, out var pda) &&
            pda.ContainedId != null &&
            TryComp<IdBankAccountComponent>(pda.ContainedId.Value, out var idBank))
        {
            if (_bank.TryTransfer(station.Value, idBank.AccountNumber, args.TargetAccount, args.Amount))
            {
                UpdateUi(uid, loaderUid);
                _cartridgeLoader.SendNotification(loaderUid, "Bank", Loc.GetString("bank-app-transfer-success"));
            }
            else
                _cartridgeLoader.SendNotification(loaderUid, "Bank", Loc.GetString("bank-app-transfer-fail"));
        }
    }

    private void OnLink(EntityUid uid, BankCartridgeComponent component, BankLinkIdMessage args)
    {
        if (!Exists(args.Actor))
            return;

        if (!_mind.TryGetMind(args.Actor, out var mindId, out _) ||
            !TryComp<MindBankAccountComponent>(mindId, out var mindBank))
            return;

        if (!TryComp<CartridgeComponent>(uid, out var cartridge) || !cartridge.LoaderUid.HasValue)
            return;

        var loaderUid = cartridge.LoaderUid.Value;

        if (TryComp<PdaComponent>(loaderUid, out var pda) && pda.ContainedId != null)
        {
            _bank.AttachBankToId(mindId, pda.ContainedId.Value, mindBank);

            UpdateUi(uid, loaderUid);
            _cartridgeLoader.SendNotification(loaderUid, "Bank", Loc.GetString("bank-app-link-success"));
        }
    }

    private void OnUnlink(EntityUid uid, BankCartridgeComponent component, BankUnlinkIdMessage args)
    {
        if (!TryComp<CartridgeComponent>(uid, out var cartridge) || !cartridge.LoaderUid.HasValue)
            return;

        var loaderUid = cartridge.LoaderUid.Value;

        if (TryComp<PdaComponent>(loaderUid, out var pda) &&
            pda.ContainedId != null &&
            TryComp<IdBankAccountComponent>(pda.ContainedId.Value, out var idBank))
        {
            RemComp<IdBankAccountComponent>(pda.ContainedId.Value);

            UpdateUi(uid, loaderUid);
            _cartridgeLoader.SendNotification(loaderUid, "Bank", Loc.GetString("bank-app-unlink-success"));
        }
    }

    private void OnToggleNotifications(EntityUid uid,
        BankCartridgeComponent component,
        BankToggleNotificationsMessage args)
    {
        if (!Exists(args.Actor))
            return;

        if (_mind.TryGetMind(args.Actor, out var mindId, out _) &&
            TryComp<MindBankAccountComponent>(mindId, out var mindBank))
        {
            mindBank.NotificationsEnabled = !mindBank.NotificationsEnabled;

            if (TryComp<CartridgeComponent>(uid, out var cartridge) && cartridge.LoaderUid.HasValue)
                UpdateUi(uid, cartridge.LoaderUid.Value);
        }
    }
}
