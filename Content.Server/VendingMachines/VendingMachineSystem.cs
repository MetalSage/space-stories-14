// Stories-Economy
using System.Linq;
using Content.Shared.Roles;
using Content.Server._Stories.Economy;
using Content.Server.Cargo.Systems;
using Content.Server.Power.Components;
using Content.Server.Station.Systems;
using Content.Server.Vocalization.Systems;
using Content.Server.Power.EntitySystems;
using Content.Shared._Stories.Economy.Components;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Cargo;
using Content.Shared.Damage.Systems;
using Content.Shared.Emp;
using Content.Shared.GameTicking;
using Content.Shared.PDA;
using Content.Shared.Power;
using Content.Shared.Throwing;
using Content.Shared.UserInterface;
using Content.Shared.VendingMachines;
using Content.Shared.Wall;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Server.Access.Components;
using System.Numerics;

namespace Content.Server.VendingMachines;

public sealed partial class VendingMachineSystem : SharedVendingMachineSystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private PricingSystem _pricing = default!;
    [Dependency] private ThrowingSystem _throwingSystem = default!;
    [Dependency] private BankSystem _bank = default!;
    [Dependency] private StationSystem _station = default!;
    [Dependency] private SharedIdCardSystem _idCard = default!;
    [Dependency] private PowerReceiverSystem _power = default!;

    private const float WallVendEjectDistanceFromWall = 1f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VendingMachineComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<VendingMachineComponent, DamageChangedEvent>(OnDamageChanged);
        SubscribeLocalEvent<VendingMachineComponent, PriceCalculationEvent>(OnVendingPrice);
        SubscribeLocalEvent<VendingMachineComponent, TryVocalizeEvent>(OnTryVocalize);
        SubscribeLocalEvent<VendingMachineComponent, VendingMachineSelfDispenseEvent>(OnSelfDispense);
        SubscribeLocalEvent<VendingMachineRestockComponent, PriceCalculationEvent>(OnPriceCalculation);

        SubscribeLocalEvent<VendingMachineComponent, BeforeActivatableUIOpenEvent>(
            (uid, comp, args) => UpdateVendingUI(uid, args.User, comp));
        SubscribeLocalEvent<VendingMachineComponent, BoundUIOpenedEvent>(OnBoundUIOpened);
        SubscribeLocalEvent<BankBalanceChangedEventArgs>(OnBalanceChanged);
    }

    private void OnVendingPrice(EntityUid uid, VendingMachineComponent component, ref PriceCalculationEvent args)
    {
        var price = 0.0;

        foreach (var entry in component.Inventory.Values)
        {
            if (!ProtoMan.TryIndex<EntityPrototype>(entry.ID, out var proto))
            {
                Log.Error($"Unable to find entity prototype {entry.ID} on {ToPrettyString(uid)} vending.");
                continue;
            }

            price += entry.Amount * _pricing.GetEstimatedPrice(proto);
        }

        args.Price += price;
    }

    protected override void OnMapInit(EntityUid uid, VendingMachineComponent component, MapInitEvent args)
    {
        base.OnMapInit(uid, component, args);

        if (HasComp<ApcPowerReceiverComponent>(uid))
        {
            TryUpdateVisualState((uid, component));
        }

        if (ProtoMan.TryIndex(component.PackPrototypeId, out VendingMachineInventoryPrototype? packPrototype) // Stories-Economy
            && packPrototype!.ItemPrices.Count > 0)
        {
            ApplyPricesToInventory(component.Inventory, packPrototype.ItemPrices);
            ApplyPricesToInventory(component.EmaggedInventory, packPrototype.ItemPrices);
            ApplyPricesToInventory(component.ContrabandInventory, packPrototype.ItemPrices);
            Dirty(uid, component);
        }
    }

    private void OnPowerChanged(EntityUid uid, VendingMachineComponent component, ref PowerChangedEvent args)
    {
        TryUpdateVisualState((uid, component));
    }

    private void OnDamageChanged(EntityUid uid, VendingMachineComponent component, DamageChangedEvent args)
    {
        if (!args.DamageIncreased && component.Broken)
        {
            component.Broken = false;
            Dirty(uid, component);
            TryUpdateVisualState((uid, component));
            return;
        }

        if (component.Broken || component.DispenseOnHitCoolingDown || component.DispenseOnHitChance == null || args.DamageDelta == null)
            return;

        if (args.DamageIncreased && args.DamageDelta.GetTotal() >= component.DispenseOnHitThreshold && _random.Prob(component.DispenseOnHitChance.Value))
        {
            if (component.DispenseOnHitCooldown != null)
                component.DispenseOnHitEnd = Timing.CurTime + component.DispenseOnHitCooldown.Value;
            EjectRandom(uid, throwItem: true, forceEject: true, component);
        }
    }

    private void OnSelfDispense(EntityUid uid, VendingMachineComponent component, VendingMachineSelfDispenseEvent args)
    {
        if (args.Handled) return;
        args.Handled = true;
        EjectRandom(uid, throwItem: true, forceEject: false, component);
    }

    public void SetShooting(EntityUid uid, bool canShoot, VendingMachineComponent? component = null)
    {
        if (!Resolve(uid, ref component)) return;
        component.CanShoot = canShoot;
    }

    public void SetContraband(EntityUid uid, bool contraband, VendingMachineComponent? component = null)
    {
        if (!Resolve(uid, ref component)) return;
        component.Contraband = contraband;
        Dirty(uid, component);
    }

    public void EjectRandom(EntityUid uid, bool throwItem, bool forceEject = false, VendingMachineComponent? vendComponent = null)
    {
        if (!Resolve(uid, ref vendComponent)) return;
        var availableItems = GetAvailableInventory(uid, vendComponent);
        if (availableItems.Count <= 0) return;
        var item = _random.Pick(availableItems);

        if (forceEject)
        {
            vendComponent.NextItemToEject = item.ID;
            vendComponent.ThrowNextItem = throwItem;
            var entry = GetEntry(uid, item.ID, item.Type, vendComponent);
            if (entry != null) entry.Amount--;
            EjectItem(uid, vendComponent, forceEject);
        }
        else
        {
            TryEjectVendorItem(uid, item.Type, item.ID, throwItem, user: null, vendComponent: vendComponent);
        }
    }

    protected override void EjectItem(EntityUid uid, VendingMachineComponent? vendComponent = null, bool forceEject = false)
    {
        if (!Resolve(uid, ref vendComponent)) return;
        if (!forceEject) TryUpdateVisualState((uid, vendComponent));
        if (string.IsNullOrEmpty(vendComponent.NextItemToEject))
        {
            vendComponent.ThrowNextItem = false;
            return;
        }

        var xform = Transform(uid);
        var spawnCoordinates = xform.Coordinates;

        if (TryComp<WallMountComponent>(uid, out var wallMountComponent))
        {
            var offset = (wallMountComponent.Direction + xform.LocalRotation - Math.PI / 2).ToVec() * WallVendEjectDistanceFromWall;
            spawnCoordinates = spawnCoordinates.Offset(offset);
        }

        var ent = Spawn(vendComponent.NextItemToEject, spawnCoordinates);

        if (vendComponent.ThrowNextItem)
        {
            var range = vendComponent.NonLimitedEjectRange;
            var direction = new Vector2(_random.NextFloat(-range, range), _random.NextFloat(-range, range));
            _throwingSystem.TryThrow(ent, direction, vendComponent.NonLimitedEjectForce);
        }

        vendComponent.NextItemToEject = null;
        vendComponent.ThrowNextItem = false;
    }

    private void OnPriceCalculation(EntityUid uid, VendingMachineRestockComponent component, ref PriceCalculationEvent args)
    {
        var priceSets = new List<double>();
        foreach (var vendingInventory in component.CanRestock)
        {
            double total = 0;
            if (ProtoMan.TryIndex(vendingInventory, out VendingMachineInventoryPrototype? inventoryPrototype))
            {
                foreach (var (item, amount) in inventoryPrototype.StartingInventory)
                {
                    if (ProtoMan.TryIndex(item, out EntityPrototype? entity))
                        total += _pricing.GetEstimatedPrice(entity) * amount;
                }
            }
            priceSets.Add(total);
        }
        if (priceSets.Any()) args.Price += priceSets.Max();
    }

    private void OnTryVocalize(Entity<VendingMachineComponent> ent, ref TryVocalizeEvent args)
    {
        args.Cancelled |= ent.Comp.Broken;
    }

    /// <summary>
    /// Checks if the user gets free items from this vending machine.
    /// Returns true if the user's ID card has a PresetIdCardComponent and its JobName is in the machine's FreeJobs list.
    /// </summary>
    private bool IsFreeForUser(EntityUid uid, EntityUid user, VendingMachineComponent component)
    {
        if (component.FreeJobs.Count == 0)
            return false;

        if (!_idCard.TryFindIdCard(user, out var idCard))
            return false;

        ProtoId<JobPrototype>? jobId = null;
        if (idCard.Comp.JobPrototype != null)
        {
            jobId = idCard.Comp.JobPrototype;
        }
        else if (TryComp<PresetIdCardComponent>(idCard, out var preset) && preset.JobName != null)
        {
            jobId = preset.JobName;
        }

        if (jobId == null)
            return false;

        foreach (var freeJob in component.FreeJobs)
        {
            if (freeJob.Id == jobId.Value.Id)
                return true;
        }

        return false;
    }

    private void UpdateVendingUI(EntityUid uid, EntityUid user, VendingMachineComponent? component = null)
    {
        if (!Resolve(uid, ref component)) return;

        var authorized = IsAuthorized(uid, user, component);
        var inventory = GetAllInventory(uid, component);

        // If the user is authorized via access card, show prices as 0
        if (IsFreeForUser(uid, user, component))
        {
            var freeInventory = new List<VendingMachineInventoryEntry>();
            foreach (var entry in inventory)
                freeInventory.Add(new VendingMachineInventoryEntry(entry.Type, entry.ID, entry.Amount, 0));
            inventory = freeInventory;
        }

        var state = new VendingMachineUIState(inventory, authorized);
        UISystem.SetUiState(uid, VendingMachineUiKey.Key, state);

        if (authorized) SendBalanceToUser(uid, user, component);
    }

    protected override void UpdateUI(Entity<VendingMachineComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp)) return;

        var actors = UISystem.GetActors(entity.Owner, VendingMachineUiKey.Key).ToList();
        if (actors.Count == 0) return;

        // Re-use UpdateVendingUI so each actor gets correct (possibly free) inventory
        foreach (var actor in actors)
        {
            UpdateVendingUI(entity.Owner, actor, entity.Comp);
        }
    }

    private bool TryGetAccountNumber(EntityUid user, out string accountNumber)
    {
        accountNumber = string.Empty;
        if (_idCard.TryFindIdCard(user, out var idCard) && TryComp<IdBankAccountComponent>(idCard, out var bankComp))
        {
            accountNumber = bankComp.AccountNumber;
            return true;
        }
        return false;
    }



    private void AddInventoryFromPrototype(EntityUid uid, Dictionary<string, uint>? entries, InventoryType type, VendingMachineComponent? component = null, float restockQuality = 1.0f)
    {
        if (!Resolve(uid, ref component) || entries == null) return;

        Dictionary<string, VendingMachineInventoryEntry> inventory;

        if (type == InventoryType.Regular)
            inventory = component.Inventory;
        else if (type == InventoryType.Emagged)
            inventory = component.EmaggedInventory;
        else if (type == InventoryType.Contraband)
            inventory = component.ContrabandInventory;
        else
            return;

        foreach (var (id, amount) in entries)
        {
            if (!ProtoMan.HasIndex<EntityPrototype>(id)) continue;
            var restock = amount;
            if (restockQuality < 1.0f && !_random.Prob(restockQuality))
                restock = (uint)_random.Next(0, (int)amount);

            if (inventory.TryGetValue(id, out var entry))
                entry.Amount = Math.Min(entry.Amount + restock, amount * 3);
            else
                inventory.Add(id, new VendingMachineInventoryEntry(type, id, restock));
        }
    }

    public new void RestockInventoryFromPrototype(EntityUid uid, VendingMachineComponent? component = null, float restockQuality = 1f)
    {
        if (!Resolve(uid, ref component)) return;
        if (!ProtoMan.TryIndex(component.PackPrototypeId, out VendingMachineInventoryPrototype? packPrototype)) return;

        AddInventoryFromPrototype(uid, packPrototype!.StartingInventory, InventoryType.Regular, component, restockQuality);
        AddInventoryFromPrototype(uid, packPrototype.EmaggedInventory, InventoryType.Emagged, component, restockQuality);
        AddInventoryFromPrototype(uid, packPrototype.ContrabandInventory, InventoryType.Contraband, component, restockQuality);

        if (packPrototype.ItemPrices.Count > 0)
        {
            ApplyPricesToInventory(component.Inventory, packPrototype.ItemPrices);
            ApplyPricesToInventory(component.EmaggedInventory, packPrototype.ItemPrices);
            ApplyPricesToInventory(component.ContrabandInventory, packPrototype.ItemPrices);
        }
        Dirty(uid, component);
    }

    private void ApplyPricesToInventory(Dictionary<string, VendingMachineInventoryEntry> inventory, Dictionary<string, uint> prices)
    {
        foreach (var (itemId, entry) in inventory)
        {
            if (prices.TryGetValue(itemId, out var price))
                entry.Price = price;
        }
    }

    private void OnBoundUIOpened(EntityUid uid, VendingMachineComponent component, BoundUIOpenedEvent args)
    {
        if (args.UiKey is not VendingMachineUiKey key || key != VendingMachineUiKey.Key) return;
        UpdateVendingUI(uid, args.Actor, component);
    }

    private void SendBalanceToUser(EntityUid machine, EntityUid user, VendingMachineComponent component)
    {
        var station = _station.GetOwningStation(machine);
        if (station == null) return;

        var personalBal = 0;

        if (TryGetAccountNumber(user, out var accountNumber) && _bank.TryGetAccount(station.Value, accountNumber, out var account))
            personalBal = account!.Balance;

        UISystem.ServerSendUiMessage(machine, VendingMachineUiKey.Key, new VendingMachineBalanceMessage(personalBal), user);
    }

    protected override void AuthorizedVend(EntityUid uid, EntityUid sender, InventoryType type, string itemId, VendingMachineComponent component)
    {
        if (component.Ejecting || component.Broken || !_power.IsPowered(uid))
            return;

        if (!IsAuthorized(uid, sender, component))
        {
            Popup.PopupEntity(Loc.GetString("vending-machine-component-try-eject-access-denied"), uid, sender);
            Deny((uid, component), sender);
            return;
        }

        var entry = GetEntry(uid, itemId, type, component);

        if (entry == null || entry.Amount <= 0)
        {
            Deny((uid, component), sender);
            return;
        }

        if (entry.Price > 0 && !component.FreeVend)
        {
            // Free for authorized department staff only
            if (!IsFreeForUser(uid, sender, component) &&
                !TryProcessPayment(sender, uid, component, (int)entry.Price, itemId, !component.DisableFinancialLogging))
            {
                Popup.PopupEntity(Loc.GetString("stories-vending-machine-insufficient-funds"), uid, sender);
                Deny((uid, component), sender);
                return;
            }
        }

        TryEjectVendorItem(uid, type, itemId, component.CanShoot, sender, component);
    }

    private bool TryProcessPayment(EntityUid user, EntityUid machine, VendingMachineComponent component, int amount, string itemId, bool logTrans)
    {
        var station = _station.GetOwningStation(machine);
        if (station == null) return false;

        string itemName = ProtoMan.HasIndex<EntityPrototype>(itemId) ? ProtoMan.Index<EntityPrototype>(itemId).Name : itemId;
        string machineName = Name(machine);

        if (TryGetAccountNumber(user, out var userAcc))
        {
            if (_bank.TryChangeBalance(station.Value, userAcc, -amount))
            {
                if (logTrans)
                    _bank.LogTransaction(station.Value, userAcc, machineName, amount, Loc.GetString("stories-bank-log-purchase", ("item", itemName)));

                SendBalanceToUser(machine, user, component);
                return true;
            }
        }

        return false;
    }

    private void OnBalanceChanged(BankBalanceChangedEventArgs ev)
    {
        var query = EntityQueryEnumerator<VendingMachineComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            foreach (var actor in UISystem.GetActors(uid, VendingMachineUiKey.Key))
            {
                if (TryGetAccountNumber(actor, out var accNum) && accNum == ev.AccountNumber)
                {
                    SendBalanceToUser(uid, actor, component);
                }
            }
        }
    }

}
