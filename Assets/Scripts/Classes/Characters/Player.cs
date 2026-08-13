using System.Collections.Generic;
using CopperRunner.Gameplay.Item;
using CopperRunner.Gameplay.Upgrade;
using UnityEngine;

public class Player : Actor
{
    private int coinAmount;
    private ItemData equippedItem;
    private List<UpgradeData> upgrades;

	private float weight = 0;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

	public void AddUpgrade(UpgradeData upgrade)
	{

	}
	public void AddItem(ItemData newItem)
	{
		equippedItem = newItem;
	}
	public void AddCoin(int amount)
	{
		coinAmount += amount;
	}

	public void UseItem()
	{
		equippedItem.RunActions();
	}

}
