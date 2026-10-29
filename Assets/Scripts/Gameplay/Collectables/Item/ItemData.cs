using System.Collections.Generic;
using UnityEngine;

namespace CopperRunner.Gameplay.Item
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "ScriptableObjects/Collectables/Item")]
    public class ItemData : CollectableData
    {
        public Sprite worldTexture;
        public int price;

        [SerializeReference, SerializeReferenceDropdown]
        public List<ItemAction> actions = new();

        public Sprite GetWorldTexture()
        {
            return worldTexture;
        }

        public void RunActions()
        {
            foreach (ItemAction action in actions)
            {
                action.ActivateItem(this);
            }
        }
    }
}
