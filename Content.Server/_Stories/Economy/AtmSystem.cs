using Content.Server._Stories.Economy.Components;
using Content.Server.Inventory;
using Content.Server.Popups;
using Content.Server.Stack;
using Content.Server.Station.Systems;
using Content.Shared._Stories.Economy;
using Content.Shared._Stories.Economy.Components;
using Content.Shared.Cargo.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.PDA;
using Content.Shared.Stacks;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;

namespace Content.Server._Stories.Economy;

public sealed partial class AtmSystem : EntitySystem
{
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private BankSystem _bank = default!;
    [Dependency] private SharedHandsSystem _handsSystem = default!;
    [Dependency] private ServerInventorySystem _inventory = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private StackSystem _stack = default!;
    [Dependency] private StationSystem _station = default!;
    [Dependency] private UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AtmComponent, AtmLoginMessage>(OnLogin);
        SubscribeLocalEvent<AtmComponent, AtmWithdrawMessage>(OnWithdraw);
        SubscribeLocalEvent<AtmComponent, AtmLogoutMessage>(OnLogout);
        SubscribeLocalEvent<AtmComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<AtmComponent, ActivatableUIOpenAttemptEvent>(OnOpenAttempt);
        SubscribeLocalEvent<AtmComponent, AfterActivatableUIOpenEvent>(OnAfterOpen);
        SubscribeLocalEvent<BankBalanceChangedEventArgs>(OnBalanceChanged);
    }

    private void OnOpenAttempt(EntityUid uid, AtmComponent component, ActivatableUIOpenAttemptEvent args)
    {
        UpdateUi(uid, component);
    }

    private void OnAfterOpen(EntityUid uid, AtmComponent component, AfterActivatableUIOpenEvent args)
    {
        var (prefillAcc, prefillPin) = GetCardCredentials(args.User);
        UpdateUi(uid, component, prefillAccountNumber: prefillAcc, prefillPin: prefillPin);
    }

    private (string acc, string pin) GetCardCredentials(EntityUid user)
    {
        if (_inventory.TryGetSlotEntity(user, "id", out var idSlotItem))
        {
            if (TryComp<PdaComponent>(idSlotItem, out var pda) && pda.ContainedId != null)
            {
                if (TryComp<IdBankAccountComponent>(pda.ContainedId.Value, out var pdaBank))
                {
                    var mindPin = GetPinForAccount(pdaBank.AccountNumber);
                    return (pdaBank.AccountNumber, mindPin);
                }
            }
            else if (TryComp<IdBankAccountComponent>(idSlotItem, out var directBank))
            {
                var mindPin = GetPinForAccount(directBank.AccountNumber);
                return (directBank.AccountNumber, mindPin);
            }
        }
        return (string.Empty, string.Empty);
    }

    private string GetPinForAccount(string accountNumber)
    {
        var query = EntityQueryEnumerator<MindBankAccountComponent>();
        while (query.MoveNext(out _, out var mindBank))
        {
            if (mindBank.AccountNumber == accountNumber)
                return mindBank.Pin;
        }
        return string.Empty;
    }

    private void OnLogin(EntityUid uid, AtmComponent component, AtmLoginMessage args)
    {
        var station = _station.GetOwningStation(uid);
        if (station == null)
            return;

        if (_bank.TryGetAccount(station.Value, args.AccountNumber, out var account))
        {
            if (account!.Pin == args.Pin)
            {
                component.LoggedInAccountNumber = args.AccountNumber;
                var msg = Loc.GetString("stories-atm-msg-login-success");
                UpdateUi(uid, component, msg);
                _popup.PopupEntity(msg, uid, args.Actor);
            }
            else
            {
                var msg = Loc.GetString("stories-atm-msg-invalid-pin");
                UpdateUi(uid, component, msg);
                _audio.PlayPvs(component.SoundError, uid);
                _popup.PopupEntity(msg, uid, args.Actor);
            }
        }
        else
        {
            var msg = Loc.GetString("stories-atm-msg-account-not-found");
            UpdateUi(uid, component, msg);
            _audio.PlayPvs(component.SoundError, uid);
            _popup.PopupEntity(msg, uid, args.Actor);
        }
    }

    private void OnWithdraw(EntityUid uid, AtmComponent component, AtmWithdrawMessage args)
    {
        if (component.LoggedInAccountNumber == null || args.Amount <= 0)
            return;

        var station = _station.GetOwningStation(uid);
        if (station == null)
            return;

        if (_bank.TryChangeBalance(station.Value, component.LoggedInAccountNumber, -args.Amount))
        {
            SpawnCash(uid, args.Amount, args.Actor);
            _audio.PlayPvs(component.SoundCash, uid);
            var msg = Loc.GetString("stories-atm-msg-withdraw-success", ("amount", args.Amount));
            UpdateUi(uid, component, msg);
            _popup.PopupEntity(msg, uid, args.Actor);
        }
        else
        {
            var msg = Loc.GetString("stories-atm-msg-insufficient-funds");
            UpdateUi(uid, component, msg);
            _audio.PlayPvs(component.SoundError, uid);
            _popup.PopupEntity(msg, uid, args.Actor);
        }
    }

    private void OnLogout(EntityUid uid, AtmComponent component, AtmLogoutMessage args)
    {
        component.LoggedInAccountNumber = null;
        var msg = Loc.GetString("stories-atm-msg-logged-out");
        UpdateUi(uid, component, msg);
        _popup.PopupEntity(msg, uid, args.Actor);
    }

    private void OnInteractUsing(EntityUid uid, AtmComponent component, InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (component.LoggedInAccountNumber == null)
            return;

        if (TryComp<StackComponent>(args.Used, out var stack) && HasComp<CashComponent>(args.Used))
        {
            var station = _station.GetOwningStation(uid);
            if (station == null)
                return;

            var amount = _stack.GetCount(args.Used);
            if (_bank.TryChangeBalance(station.Value, component.LoggedInAccountNumber, amount))
            {
                _audio.PlayPvs(component.SoundCash, uid);

                _stack.SetCount(args.Used, 0);
                if (!Terminating(args.Used))
                    Del(args.Used);

                var msg = Loc.GetString("stories-atm-msg-deposit-success", ("amount", amount));
                UpdateUi(uid, component, msg);
                _popup.PopupEntity(msg, uid, args.User);
                args.Handled = true;
            }
        }
    }

    private void SpawnCash(EntityUid atmUid, int amount, EntityUid? user)
    {
        var coords = Transform(atmUid).Coordinates;
        var cash = Spawn("SpaceCash", coords);
        _stack.SetCount(cash, amount);

        if (user != null)
        {
            _handsSystem.TryPickupAnyHand(user.Value, cash);
        }
    }

    private void UpdateUi(EntityUid uid, AtmComponent component, string message = "",
        string prefillAccountNumber = "", string prefillPin = "")
    {
        var balance = 0;
        var isLoggedIn = component.LoggedInAccountNumber != null;
        var accNum = component.LoggedInAccountNumber ?? "";
        var ownerName = string.Empty;

        if (isLoggedIn)
        {
            var station = _station.GetOwningStation(uid);
            if (station != null &&
                _bank.TryGetAccount(station.Value, component.LoggedInAccountNumber!, out var account))
            {
                balance = account!.Balance;
                ownerName = account.OwnerName;
            }
        }

        var state = new AtmBoundUserInterfaceState(isLoggedIn, accNum, balance, message, ownerName,
            prefillAccountNumber, prefillPin);
        _ui.SetUiState(uid, AtmUiKey.Key, state);
    }

    private void OnBalanceChanged(BankBalanceChangedEventArgs ev)
    {
        var query = EntityQueryEnumerator<AtmComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (component.LoggedInAccountNumber == ev.AccountNumber)
            {
                var station = _station.GetOwningStation(uid);
                if (station == ev.Station)
                {
                    UpdateUi(uid, component);
                }
            }
        }
    }
}
