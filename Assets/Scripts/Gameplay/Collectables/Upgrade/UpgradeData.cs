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
		public List<UpgradeAction> upgradesActions = new List<UpgradeAction>();
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
		public bool CanAddRank()
		{
			return currentRank < maxRank;
		}
		public void IncreasePrice()
		{

		}

	}
}
