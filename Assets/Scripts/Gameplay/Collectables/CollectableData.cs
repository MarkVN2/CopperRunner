using UnityEngine;
using UnityEngine.Localization;

public class CollectableData : ScriptableObject
{
    public LocalizedString collectableName;
	public LocalizedString description;
	public Sprite icon;
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
        return icon;    
    }

	public Rarity GetRarity()
    {
        return rarity;
    }

}
