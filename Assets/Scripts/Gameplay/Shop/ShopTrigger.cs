using Unity.Cinemachine;
using UnityEngine;

public class ShopTrigger : MonoBehaviour {

    [SerializeField]
    private Transform cameraTarget;
    private void OnTriggerEnter2D(Collider2D collision)
    {

        Time.timeScale = 0.5f;
        //UpgradeShopMenu.Instance.OnClose = () =>
        //{
        //    CameraTarget previousTarget = Camera.main.GetComponent<CinemachineCamera>().Target;
        //    Camera.main.GetComponent<CinemachineCamera>().Target = previousTarget;
        //};
        Camera.main.GetComponent<CinemachineCamera>().Target.LookAtTarget = cameraTarget;

        //UpgradeShopMenu.Instance.Open();
    }
}
