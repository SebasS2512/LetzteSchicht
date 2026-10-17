using System.Collections;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;

    [Header("Einstellungen")]
    [Tooltip("Schwarzes Vollbild-Image mit CanvasGroup.")]
    [SerializeField] private CanvasGroup fadeGroup;

    [SerializeField] private float fadeDuration = 1.5f;

    private void Awake()
    {
        Instance = this;

        if (fadeGroup != null)
        {
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
        }
    }

    public IEnumerator FadeOut()
    {
        yield return FadeRoutine(0f, 1f);
    }

    public IEnumerator FadeIn()
    {
        yield return FadeRoutine(1f, 0f);
    }

    private IEnumerator FadeRoutine(float from, float to)
    {
        if (fadeGroup == null) yield break;

        fadeGroup.blocksRaycasts = true;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }

        fadeGroup.alpha = to;

        fadeGroup.blocksRaycasts = to > 0.5f;
    }
}