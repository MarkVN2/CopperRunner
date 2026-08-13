
namespace CopperRunner.Gameplay.Upgrade
{

	[System.Serializable]
	public abstract class UpgradeAction
	{
		public abstract void ActivateUpgrade(UpgradeData upgradeData);

	}
}