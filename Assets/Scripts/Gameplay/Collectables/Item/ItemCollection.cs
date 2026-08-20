using CopperRunner.Gameplay.Item;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(
    fileName = "NewItemCollection",
    menuName = "ScriptableObjects/Collectables/Collections/ItemCollection"
)]
public class ItemCollection : ScriptableObject 
{
    [SerializeField]
    List<List<ItemData>> items;
}
