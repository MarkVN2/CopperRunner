using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponentInParent<Player>();
        if (player == null || player.IsDead)
            return;

        UpgradeShopMenu shopMenu = UpgradeShopMenu.Instance;
        if (shopMenu == null)
        {
            Debug.LogWarning("[ShopTrigger] No UpgradeShopMenu is available.", this);
            return;
        }

        Transform shopSection = transform.parent;
        if (shopSection == null)
        {
            Debug.LogWarning("[ShopTrigger] The shop trigger has no section parent.", this);
            return;
        }

        shopMenu.Open(shopSection);
    }
}
