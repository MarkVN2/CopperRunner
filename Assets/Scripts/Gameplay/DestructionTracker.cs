using UnityEngine;

public class DestructionTracker : MonoBehaviour
{
    private bool isQuitting = false;

    private void OnDisable()
    {
        // Only log if the application is actually playing and we aren't just changing scenes/shutting down
        if (gameObject.scene.isLoaded && Application.isPlaying)
        {
            Debug.LogWarning(
                $"[Tracker] GameObject '{gameObject.name}' was disabled or hidden!\nStack Trace:\n{UnityEngine.StackTraceUtility.ExtractStackTrace()}"
            );
        }
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }

    private void OnDestroy()
    {
        if (isQuitting || !gameObject.scene.isLoaded)
            return;

        // Print a standard Unity stack trace which correctly rolls up through callers
        Debug.LogWarning(
            $"[Interceptor] '{gameObject.name}' is being destroyed! Trace:\n{UnityEngine.StackTraceUtility.ExtractStackTrace()}"
        );
    }
}
