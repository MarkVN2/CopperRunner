using System.Collections.Generic;
namespace CopperRunner.Gameplay.Item
{
	public class ItemData : CollectableData
	{
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
