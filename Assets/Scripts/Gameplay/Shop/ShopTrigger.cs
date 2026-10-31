using Unity.Cinemachine;
using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    private float defaultFixedDeltaTime;

    private void Awake()
    {
        defaultFixedDeltaTime = Time.fixedDeltaTime;
    }

    private void ShopTriggerOnClose()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = defaultFixedDeltaTime;
        UpgradeShopMenu.Instance.OnClose.RemoveListener(ShopTriggerOnClose);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        UpgradeShopMenu.Instance.Open();
        UpgradeShopMenu.Instance.OnClose.AddListener(ShopTriggerOnClose);

        float shopDuration = UpgradeShopMenu.Instance.RemainingTime;

        float targetTimeScale = Mathf.Clamp(shopDuration / 10f, 0.1f, 0.6f);

        Time.timeScale = targetTimeScale;
        Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;
    }
}
