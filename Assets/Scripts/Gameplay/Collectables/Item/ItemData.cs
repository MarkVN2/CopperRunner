using System.Collections.Generic;
using UnityEngine;
namespace CopperRunner.Gameplay.Item
{
	[CreateAssetMenu(
		fileName = "NewItem",
		menuName = "ScriptableObjects/Collectables/Item"
	)]
	public class ItemData : CollectableData
	{
		[SerializeReference]
		public List<ItemAction> actions = new();

		public void RunActions()
		{
			foreach (ItemAction action in actions)
			{
				action.ActivateItem(this);
			}
		}

	}
}
