using Unity.Cinemachine;
using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    [SerializeField]
    private Transform cameraTarget;

    private CinemachineBrain cinemachineBrain;
    private CinemachineCamera vCam;
    private float defaultFixedDeltaTime;

    private void Awake()
    {
        defaultFixedDeltaTime = Time.fixedDeltaTime;
    }

    private void Start()
    {
        if (Camera.main != null)
        {
            vCam = Camera.main.GetComponent<CinemachineCamera>();
            cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();

            if (cinemachineBrain != null)
            {
                cinemachineBrain.UpdateMethod = CinemachineBrain.UpdateMethods.FixedUpdate;
            }
        }
    }

    private void ShopTriggerOnClose()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = defaultFixedDeltaTime;
        UpgradeShopMenu.Instance.OnClose.RemoveListener(ShopTriggerOnClose);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Time.timeScale = 0.5f;
        Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;
        UpgradeShopMenu.Instance.OnClose.AddListener(ShopTriggerOnClose);
        UpgradeShopMenu.Instance.Open();
    }
}
