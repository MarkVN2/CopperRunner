using System.Collections;
using UnityEngine;

public class UIFader : MonoBehaviour
{
    [SerializeField]
    private CanvasGroup canvasGroup;

    [SerializeField]
    [Min(0f)]
    private float fadeDuration = 0.5f;

    private Coroutine activeFade;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            Debug.LogError("[UIFader] A CanvasGroup is required.", this);
            enabled = false;
        }
    }

    public void FadeIn()
    {
        BeginFade(1f);
    }

    public void FadeOut()
    {
        BeginFade(0f);
    }

    public IEnumerator FadeToAlpha(float targetAlpha)
    {
        return FadeToAlpha(targetAlpha, fadeDuration);
    }

    public IEnumerator FadeToAlpha(float targetAlpha, float duration)
    {
        if (canvasGroup == null)
        {
            Debug.LogError("[UIFader] Cannot fade without a CanvasGroup.", this);
            yield break;
        }

        targetAlpha = Mathf.Clamp01(targetAlpha);
        float startAlpha = canvasGroup.alpha;
        duration = Mathf.Max(0f, duration);

        if (duration == 0f)
        {
            ApplyAlpha(targetAlpha);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            ApplyAlpha(Mathf.Lerp(startAlpha, targetAlpha, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }

        ApplyAlpha(targetAlpha);
    }

    private void BeginFade(float targetAlpha)
    {
        if (canvasGroup == null)
        {
            Debug.LogError("[UIFader] Cannot fade without a CanvasGroup.", this);
            return;
        }

        if (activeFade != null)
            StopCoroutine(activeFade);

        activeFade = StartCoroutine(FadeAndClear(targetAlpha));
    }

    private IEnumerator FadeAndClear(float targetAlpha)
    {
        yield return FadeToAlpha(targetAlpha, fadeDuration);
        activeFade = null;
    }

    private void ApplyAlpha(float alpha)
    {
        canvasGroup.alpha = alpha;
        canvasGroup.interactable = alpha >= 1f;
        canvasGroup.blocksRaycasts = alpha > 0f;
    }
}
