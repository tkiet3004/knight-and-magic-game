using System.Collections;
using UnityEngine;

/// <summary>
/// Component này có thể gắn vào bất kỳ UI element nào để tạo fade effect
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class UIFadeEffect : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private bool fadeInOnEnable = true;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        
        if (fadeInOnEnable)
        {
            canvasGroup.alpha = 0f;
        }
    }

    private void OnEnable()
    {
        if (fadeInOnEnable && canvasGroup != null)
        {
            StartCoroutine(FadeIn());
        }
    }

    public void FadeInUI()
    {
        StartCoroutine(FadeIn());
    }

    public void FadeOutUI()
    {
        StartCoroutine(FadeOut());
    }

    public IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsedTime / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    public IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeOutDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}
