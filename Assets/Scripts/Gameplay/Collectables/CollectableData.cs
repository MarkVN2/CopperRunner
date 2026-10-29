using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Serialization;

public class CollectableData : ScriptableObject
{
    public LocalizedString collectableName;
	public LocalizedString description;
	[FormerlySerializedAs("icon")]
	public Sprite inventoryTexture;
	public Rarity rarity;

    public LocalizedString GetName()
    {
        return collectableName;
    }
    
    public LocalizedString GetDescription()
    {
        return description;
    }

    public Sprite GetIcon()
    {
        return inventoryTexture;
    }

    public Sprite GetInventoryTexture()
    {
        return inventoryTexture;
    }

	public Rarity GetRarity()
    {
        return rarity;
    }

}
