using CopperRunner.Gameplay.Upgrade;

[System.Serializable]
public class ExtraLifeAction : UpgradeAction
{
    public override void ActivateUpgrade(UpgradeData upgradeData)
    {
        GameManager.Instance.ActivePlayer.GrantCheatDeath(upgradeData);
    }
}
