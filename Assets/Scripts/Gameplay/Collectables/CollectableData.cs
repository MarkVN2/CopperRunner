using UnityEngine;
public class CollectableData : ScriptableObject
{
    protected string collectableName;
    protected string description;
    protected Sprite icon;
    protected Rarity rarity;

    public string GetName()
    {
        return collectableName;
    }
    
    public string GetDescription()
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
