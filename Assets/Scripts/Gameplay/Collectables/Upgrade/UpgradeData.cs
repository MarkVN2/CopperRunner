using System.Collections.Generic;
using UnityEngine;

namespace CopperRunner.Gameplay.Upgrade
{
	[CreateAssetMenu(
		fileName = "NewUpgrade",
		menuName = "ScriptableObjects/Collectables/Upgrade"
	)]
	public class UpgradeData : CollectableData
	{
		private int currentRank = 1;
		[Range(1,5)]
		public int maxRank;
		public int price;
		[SerializeReference] // FIX the list to be able to add the Upgrade Actions
		public List<UpgradeAction> upgradesActions;
		public void ActivateUpgrades()
		{
			foreach (UpgradeAction action in upgradesActions)
			{
				action.ActivateUpgrade(this);
			}
		}
		public void AddRank()
		{
			if (currentRank < maxRank)
			currentRank += 1;
		}
		public void IncreasePrice()
		{

		}

	}
}
