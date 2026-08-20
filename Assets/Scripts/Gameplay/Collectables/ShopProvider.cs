using System.Collections.Generic;
using CopperRunner.Gameplay.Upgrade;
using NUnit.Framework;
using UnityEngine;

public class ShopProvider : MonoBehaviour
{
    [SerializeField]
    private UpgradeDatabase currentPool;

    [SerializeField]
    private RegionID[] unlockedRegions = {RegionID.BABYLON};

    [ContextMenu("Test")]
    public void TestListGetting()
    {
        foreach(UpgradeData data in currentPool.GetRandomFromActivePool(3, unlockedRegions))
        {
            Debug.Log(data.ToString());
        }
    }
}