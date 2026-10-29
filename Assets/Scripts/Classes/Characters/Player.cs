using System;
using System.Collections.Generic;
using CopperRunner.Gameplay.Item;
using CopperRunner.Gameplay.Upgrade;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterMovement))]
public class Player : Actor
{
    private int coinAmount;
    private readonly List<ItemData> items = new List<ItemData>();
    private List<UpgradeData> upgrades = new List<UpgradeData>();
    private bool isDead;
    private InputSystem_Actions input;
    private InputAction useItemAction;

    private bool cheatDeath;
    private UpgradeData cheatDeathSource;
    public bool CheatDeath
    {
        get => cheatDeath;
        set => cheatDeath = value;
    }

    [SerializeField]
    [Min(0f)]
    private float carryingCapacity = 20f;
    public float CopperCarryingCapacity => carryingCapacity;
    private float weight;

    public event Action<int> CoinsChanged;
    public event Action<IReadOnlyList<ItemData>> ItemsChanged;
    public event Action<IReadOnlyList<UpgradeData>> UpgradesChanged;

    public float CarryingCapacity => carryingCapacity;

    public bool IsDead
    {
        get { return isDead; }
    }

    public int RemainingSteps { get; set; } = 10;
    public int ItemCount => items.Count;
    public IReadOnlyList<ItemData> Items => items;
    public IReadOnlyList<UpgradeData> Upgrades => upgrades;

    private void Awake()
    {
        input = new InputSystem_Actions();
        useItemAction = input.Player.Attack;
    }

    private void OnEnable()
    {
        useItemAction?.Enable();
    }

    private void OnDisable()
    {
        useItemAction?.Disable();
    }

    private void OnDestroy()
    {
        input?.Dispose();
    }

    private void Update()
    {
        if (useItemAction != null && useItemAction.WasPressedThisFrame())
            UseItem();
    }

    public bool CanAddUpgrade(UpgradeData upgrade)
    {
        if (upgrade == null)
            return false;

        return !upgrades.Contains(upgrade) || upgrade.CanAddRank();
    }

    public bool AddUpgrade(UpgradeData upgrade)
    {
        if (!CanAddUpgrade(upgrade))
            return false;

        if (upgrades.Contains(upgrade))
        {
            upgrade.AddRank();
        }
        else
        {
            upgrades.Add(upgrade);
        }

        upgrade.ActivateUpgrades();
        UpgradesChanged?.Invoke(upgrades);
        return true;
    }

    public void AddItem(ItemData newItem)
    {
        if (newItem == null)
            return;

        items.Add(newItem);
        ItemsChanged?.Invoke(items);
    }

    public void AddCoin(int amount)
    {
        long newAmount = (long)coinAmount + amount;
        coinAmount = (int)Math.Min(Math.Max(newAmount, 0), Mathf.FloorToInt(carryingCapacity));
        weight = coinAmount;
        CoinsChanged?.Invoke(coinAmount);
    }

    public bool CanAddCoin(int amount)
    {
        return amount >= 0 && weight + amount <= carryingCapacity;
    }

    public void IncreaseCarryingCapacity(float amount)
    {
        if (amount <= 0f)
        {
            Debug.LogWarning(
                "[Player] Carrying capacity increase must be greater than zero.",
                this
            );
            return;
        }

        carryingCapacity += amount;
    }

    public int GetCoinAmount()
    {
        return coinAmount;
    }

    public bool CanAfford(int price)
    {
        return price >= 0 && coinAmount >= price;
    }

    public bool TrySpendCoins(int amount)
    {
        if (!CanAfford(amount))
            return false;

        coinAmount -= amount;
        weight = coinAmount;
        CoinsChanged?.Invoke(coinAmount);
        return true;
    }

    public void GrantCheatDeath(UpgradeData source)
    {
        cheatDeath = true;
        cheatDeathSource = source;
    }

    public void UseItem()
    {
        if (isDead || items.Count == 0)
            return;

        ItemData item = items[0];
        items.RemoveAt(0);
        ItemsChanged?.Invoke(items);
        item.RunActions();
    }

    public void Die()
    {
        if (cheatDeath)
        {
            cheatDeath = false;
            if (cheatDeathSource != null)
            {
                upgrades.Remove(cheatDeathSource);
                UpgradesChanged?.Invoke(upgrades);
            }
            cheatDeathSource = null;
            return;
        }

        if (isDead)
            return;

        isDead = true;
        enabled = false;

        CharacterMovement movement = GetComponent<CharacterMovement>();
        if (movement != null)
            movement.enabled = false;

        Rigidbody2D body = GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
            body.simulated = false;
        }

        EndRunMenu endRunMenu = FindFirstObjectByType<EndRunMenu>();
        if (endRunMenu == null)
        {
            GameObject endRunMenuObject = new GameObject("End Run Menu");
            endRunMenu = endRunMenuObject.AddComponent<EndRunMenu>();
        }

        if (endRunMenu != null)
            endRunMenu.Show();
    }
}
