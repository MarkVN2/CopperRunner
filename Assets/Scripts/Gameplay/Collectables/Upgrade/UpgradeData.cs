using System.Collections.Generic;
using UnityEngine;

namespace CopperRunner.Gameplay.Upgrade
{
	public class UpgradeData : CollectableData
	{
		private int currentRank = 1;
		[Range(1,5)]
		public int maxRank;
		public List<UpgradeAction> upgrades;
		public void ActivateUpgrades()
		{
			foreach (UpgradeAction action in upgrades)
			{
				action.ActivateUpgrade(this);
			}
		}
		public void AddRank()
		{
			if (currentRank < maxRank)
			currentRank += 1;
		}

	}
}
