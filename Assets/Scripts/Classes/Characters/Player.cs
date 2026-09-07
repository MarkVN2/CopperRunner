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

	private float weight = 0;

    void Update()
    {
        
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
		coinAmount = Mathf.Max(0, coinAmount + amount);
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
		return true;
	}

	public void UseItem()
	{
		if (equippedItem == null)
			return;

		equippedItem.RunActions();
	}

}
