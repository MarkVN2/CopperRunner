using UnityEngine;
using UnityEngine.Events;

public class EndRunTrigger : MonoBehaviour
{
    [SerializeField]
    private UnityEvent onEndRunTriggered;
    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggered)
            return;

        if (collision.GetComponent<Player>() != null)
        {
            EndRunMenu endRunMenu = FindFirstObjectByType<EndRunMenu>();
            if (endRunMenu == null)
            {
                GameObject endRunMenuObject = new GameObject("End Run Menu");
                endRunMenu = endRunMenuObject.AddComponent<EndRunMenu>();
            }

            if (endRunMenu != null)
                endRunMenu.Show();

            triggered = true;

            Time.timeScale = 0f;

            onEndRunTriggered?.Invoke();

            Debug.Log("[EndRunTrigger] Player reached the end of the run!");
        }
    }
}
