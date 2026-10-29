using System;
using System.Collections.Generic;
using CopperRunner.Gameplay.Item;
using CopperRunner.Gameplay.Upgrade;
using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
public class Player : Actor
{
    private int coinAmount;
    private ItemData equippedItem;
    private List<UpgradeData> upgrades = new List<UpgradeData>();
    private bool isDead;

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
    private float weight;

    public event Action<int> CoinsChanged;

    public float CarryingCapacity => carryingCapacity;

    public bool IsDead
    {
        get { return isDead; }
    }

    void Update() { }

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
        return true;
    }

    public void AddItem(ItemData newItem)
    {
        if (newItem == null)
            return;

        equippedItem = newItem;
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
        if (equippedItem == null)
            return;

        equippedItem.RunActions();
    }

    public void Die()
    {
        if (cheatDeath)
        {
            cheatDeath = false;
            if (cheatDeathSource != null)
                upgrades.Remove(cheatDeathSource);
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
