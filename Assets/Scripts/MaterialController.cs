using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Component để điều khiển Material properties runtime với animations
/// Gắn vào bất kỳ GameObject nào có SpriteRenderer hoặc Image
/// </summary>
public class MaterialController : MonoBehaviour
{
    [Header("Target Component")]
    [SerializeField] private bool autoDetect = true;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Image image;
    [SerializeField] private Text text;
    
    [Header("Shader Type")]
    [SerializeField] private ShaderManager.ShaderType shaderType = ShaderManager.ShaderType.Sprite2D_HitFlash;

    private Material material;
    private Renderer targetRenderer;

    // Property IDs cache
    private static class Props
    {
        public static readonly int GlowColor = Shader.PropertyToID("_GlowColor");
        public static readonly int GlowIntensity = Shader.PropertyToID("_GlowIntensity");
        public static readonly int GlowSpeed = Shader.PropertyToID("_GlowSpeed");
        public static readonly int GlowMin = Shader.PropertyToID("_GlowMin");
        public static readonly int GlowMax = Shader.PropertyToID("_GlowMax");
        
        public static readonly int WaveAmplitude = Shader.PropertyToID("_WaveAmplitude");
        public static readonly int WaveFrequency = Shader.PropertyToID("_WaveFrequency");
        public static readonly int WaveSpeed = Shader.PropertyToID("_WaveSpeed");
        public static readonly int WaveDirection = Shader.PropertyToID("_WaveDirection");
        
        public static readonly int DissolveAmount = Shader.PropertyToID("_DissolveAmount");
        public static readonly int DissolveEdgeColor = Shader.PropertyToID("_DissolveEdgeColor");
        public static readonly int DissolveEdgeWidth = Shader.PropertyToID("_DissolveEdgeWidth");
        
        public static readonly int RainbowSpeed = Shader.PropertyToID("_RainbowSpeed");
        public static readonly int RainbowSaturation = Shader.PropertyToID("_RainbowSaturation");
        public static readonly int RainbowBrightness = Shader.PropertyToID("_RainbowBrightness");
        
        public static readonly int FlashAmount = Shader.PropertyToID("_FlashAmount");
        public static readonly int FlashColor = Shader.PropertyToID("_FlashColor");
        
        public static readonly int OutlineWidth = Shader.PropertyToID("_OutlineWidth");
        public static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
    }

    private void Awake()
    {
        if (autoDetect)
        {
            DetectComponents();
        }
        
        InitializeMaterial();
    }

    private void DetectComponents()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (image == null)
            image = GetComponent<Image>();
        
        if (text == null)
            text = GetComponent<Text>();
    }

    private void InitializeMaterial()
    {
        if (spriteRenderer != null)
        {
            // Apply shader theo loại được chọn
            Shader shader = ShaderManager.Instance.GetShader(shaderType);
            if (shader != null)
            {
                Material newMat = new Material(shader);
                newMat.mainTexture = spriteRenderer.sprite.texture;
                spriteRenderer.material = newMat;
                material = newMat;
                Debug.Log($"[MaterialController] Applied {shaderType} shader to {gameObject.name}");
            }
            else
            {
                material = spriteRenderer.material;
                Debug.LogWarning($"[MaterialController] {shaderType} shader not found for {gameObject.name}");
            }
            targetRenderer = spriteRenderer;
        }
        else if (image != null)
        {
            material = image.material;
        }
        else if (text != null)
        {
            material = text.material;
        }

        if (material == null)
        {
            Debug.LogWarning($"[MaterialController] No material found on {gameObject.name}");
        }
    }
    
    /// <summary>
    /// Thay đổi shader type và apply lại
    /// </summary>
    public void SetShaderType(ShaderManager.ShaderType type)
    {
        shaderType = type;
        InitializeMaterial();
    }

    // ===== GLOW EFFECTS =====

    /// <summary>
    /// Bật/tắt glow effect
    /// </summary>
    public void SetGlow(bool enabled, Color glowColor, float intensity = 1.5f)
    {
        if (material == null) return;
        
        material.SetColor(Props.GlowColor, glowColor);
        material.SetFloat(Props.GlowIntensity, enabled ? intensity : 0f);
    }

    /// <summary>
    /// Animate glow intensity
    /// </summary>
    public void AnimateGlowIntensity(float targetIntensity, float duration)
    {
        StartCoroutine(AnimateFloatProperty(Props.GlowIntensity, targetIntensity, duration));
    }

    /// <summary>
    /// Pulse glow effect một lần
    /// </summary>
    public void PulseGlow(float maxIntensity = 3f, float duration = 0.5f)
    {
        StartCoroutine(PulseEffect(Props.GlowIntensity, maxIntensity, duration));
    }

    // ===== WAVE EFFECTS =====

    /// <summary>
    /// Bật/tắt wave effect
    /// </summary>
    public void SetWave(bool enabled, float amplitude = 0.1f, float frequency = 5f, float speed = 2f)
    {
        if (material == null) return;
        
        material.SetFloat(Props.WaveAmplitude, enabled ? amplitude : 0f);
        material.SetFloat(Props.WaveFrequency, frequency);
        material.SetFloat(Props.WaveSpeed, speed);
    }

    /// <summary>
    /// Animate wave amplitude
    /// </summary>
    public void AnimateWaveAmplitude(float targetAmplitude, float duration)
    {
        StartCoroutine(AnimateFloatProperty(Props.WaveAmplitude, targetAmplitude, duration));
    }

    // ===== DISSOLVE EFFECTS =====

    /// <summary>
    /// Dissolve object từ 0 -> 1
    /// </summary>
    public void Dissolve(float duration, System.Action onComplete = null)
    {
        StartCoroutine(DissolveCoroutine(0f, 1f, duration, onComplete));
    }

    /// <summary>
    /// Appear từ dissolve (1 -> 0)
    /// </summary>
    public void AppearFromDissolve(float duration, System.Action onComplete = null)
    {
        StartCoroutine(DissolveCoroutine(1f, 0f, duration, onComplete));
    }

    /// <summary>
    /// Set dissolve amount trực tiếp
    /// </summary>
    public void SetDissolveAmount(float amount)
    {
        if (material == null) return;
        material.SetFloat(Props.DissolveAmount, amount);
    }

    private IEnumerator DissolveCoroutine(float from, float to, float duration, System.Action onComplete)
    {
        if (material == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float value = Mathf.Lerp(from, to, t);
            material.SetFloat(Props.DissolveAmount, value);
            yield return null;
        }

        material.SetFloat(Props.DissolveAmount, to);
        onComplete?.Invoke();
    }

    // ===== RAINBOW EFFECTS =====

    /// <summary>
    /// Bật/tắt rainbow effect
    /// </summary>
    public void SetRainbow(bool enabled, float speed = 1f, float saturation = 0.8f, float brightness = 1f)
    {
        if (material == null) return;
        
        material.SetFloat(Props.RainbowSpeed, enabled ? speed : 0f);
        material.SetFloat(Props.RainbowSaturation, saturation);
        material.SetFloat(Props.RainbowBrightness, brightness);
    }

    // ===== FLASH EFFECTS =====

    /// <summary>
    /// Flash effect một lần (hit effect)
    /// </summary>
    public void Flash(Color flashColor, float duration = 0.2f)
    {
        StartCoroutine(FlashCoroutine(flashColor, duration));
    }

    /// <summary>
    /// Flash nhiều lần
    /// </summary>
    public void FlashMultiple(Color flashColor, int times = 3, float flashDuration = 0.1f, float interval = 0.1f)
    {
        StartCoroutine(FlashMultipleCoroutine(flashColor, times, flashDuration, interval));
    }

    private IEnumerator FlashCoroutine(Color flashColor, float duration)
    {
        if (material == null) yield break;

        material.SetColor(Props.FlashColor, flashColor);
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - (elapsed / duration);
            material.SetFloat(Props.FlashAmount, t);
            yield return null;
        }

        material.SetFloat(Props.FlashAmount, 0f);
    }

    private IEnumerator FlashMultipleCoroutine(Color flashColor, int times, float flashDuration, float interval)
    {
        for (int i = 0; i < times; i++)
        {
            yield return FlashCoroutine(flashColor, flashDuration);
            yield return new WaitForSeconds(interval);
        }
    }

    // ===== OUTLINE EFFECTS =====

    /// <summary>
    /// Bật/tắt outline
    /// </summary>
    public void SetOutline(bool enabled, Color outlineColor, float width = 1f)
    {
        if (material == null) return;
        
        material.SetColor(Props.OutlineColor, outlineColor);
        material.SetFloat(Props.OutlineWidth, enabled ? width : 0f);
    }

    /// <summary>
    /// Animate outline width
    /// </summary>
    public void AnimateOutlineWidth(float targetWidth, float duration)
    {
        StartCoroutine(AnimateFloatProperty(Props.OutlineWidth, targetWidth, duration));
    }

    /// <summary>
    /// Pulse outline effect
    /// </summary>
    public void PulseOutline(float maxWidth = 3f, float duration = 0.5f)
    {
        StartCoroutine(PulseEffect(Props.OutlineWidth, maxWidth, duration));
    }

    // ===== GENERIC ANIMATION HELPERS =====

    private IEnumerator AnimateFloatProperty(int propertyID, float targetValue, float duration)
    {
        if (material == null) yield break;

        float startValue = material.GetFloat(propertyID);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float value = Mathf.Lerp(startValue, targetValue, t);
            material.SetFloat(propertyID, value);
            yield return null;
        }

        material.SetFloat(propertyID, targetValue);
    }

    private IEnumerator AnimateColorProperty(int propertyID, Color targetColor, float duration)
    {
        if (material == null) yield break;

        Color startColor = material.GetColor(propertyID);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            Color color = Color.Lerp(startColor, targetColor, t);
            material.SetColor(propertyID, color);
            yield return null;
        }

        material.SetColor(propertyID, targetColor);
    }

    private IEnumerator PulseEffect(int propertyID, float maxValue, float duration)
    {
        if (material == null) yield break;

        float startValue = material.GetFloat(propertyID);
        float halfDuration = duration / 2f;

        // Pulse up
        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            float value = Mathf.Lerp(startValue, maxValue, t);
            material.SetFloat(propertyID, value);
            yield return null;
        }

        // Pulse down
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            float value = Mathf.Lerp(maxValue, startValue, t);
            material.SetFloat(propertyID, value);
            yield return null;
        }

        material.SetFloat(propertyID, startValue);
    }

    // ===== UTILITY METHODS =====

    /// <summary>
    /// Reset tất cả shader properties về mặc định
    /// </summary>
    public void ResetAllProperties()
    {
        if (material == null) return;

        // Reset glow
        material.SetFloat(Props.GlowIntensity, 0f);
        
        // Reset wave
        material.SetFloat(Props.WaveAmplitude, 0f);
        
        // Reset dissolve
        material.SetFloat(Props.DissolveAmount, 0f);
        
        // Reset flash
        material.SetFloat(Props.FlashAmount, 0f);
        
        // Reset outline
        material.SetFloat(Props.OutlineWidth, 0f);
    }

    /// <summary>
    /// Lấy material hiện tại
    /// </summary>
    public Material GetMaterial()
    {
        return material;
    }

    /// <summary>
    /// Set material mới
    /// </summary>
    public void SetMaterial(Material newMaterial)
    {
        if (newMaterial == null) return;

        if (spriteRenderer != null)
        {
            spriteRenderer.material = newMaterial;
        }
        else if (image != null)
        {
            image.material = newMaterial;
        }
        else if (text != null)
        {
            text.material = newMaterial;
        }

        material = newMaterial;
    }

    private void OnDestroy()
    {
        // Cleanup material if needed
        if (material != null && spriteRenderer != null)
        {
            // Don't destroy shared materials
            if (material != spriteRenderer.sharedMaterial)
            {
                Destroy(material);
            }
        }
    }
}
