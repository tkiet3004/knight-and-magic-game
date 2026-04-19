using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Tạo hiệu ứng text animation cho UI Level Complete
/// Gắn vào Text component
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class LevelCompleteTextEffect : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float scaleInDuration = 0.8f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private Vector3 startScale = new Vector3(0.3f, 0.3f, 1f);
    [SerializeField] private Vector3 endScale = Vector3.one;
    [SerializeField] private bool animateOnEnable = true;

    private TMP_Text textComponent;
    private Vector3 originalScale;

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        if (animateOnEnable)
        {
            StartCoroutine(AnimateText());
        }
    }

    private IEnumerator AnimateText()
    {
        // Set initial state
        Color textColor = textComponent.color;
        textColor.a = 0f;
        textComponent.color = textColor;
        transform.localScale = startScale;

        float elapsedTime = 0f;
        float maxDuration = Mathf.Max(fadeInDuration, scaleInDuration);

        while (elapsedTime < maxDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            // Fade in
            if (elapsedTime < fadeInDuration)
            {
                textColor.a = Mathf.Clamp01(elapsedTime / fadeInDuration);
                textComponent.color = textColor;
            }

            // Scale in
            if (elapsedTime < scaleInDuration)
            {
                float scaleProgress = elapsedTime / scaleInDuration;
                float curveValue = scaleCurve.Evaluate(scaleProgress);
                transform.localScale = Vector3.Lerp(startScale, endScale, curveValue);
            }

            yield return null;
        }

        // Ensure final values
        textColor.a = 1f;
        textComponent.color = textColor;
        transform.localScale = endScale;
    }
}
