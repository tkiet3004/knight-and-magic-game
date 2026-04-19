using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Quản lý tất cả shader trong game - Tự động load và cache shader
/// Cung cấp API để apply shader cho bất kỳ object nào
/// </summary>
public class ShaderManager : MonoBehaviour
{
    // Singleton pattern
    private static ShaderManager _instance;
    public static ShaderManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("ShaderManager");
                _instance = go.AddComponent<ShaderManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    [Header("Shader Paths")]
    [SerializeField] private string sprite2DGlowPath = "Custom/Sprite2D_Glow";
    [SerializeField] private string sprite2DDissolvePath = "Custom/Sprite2D_Dissolve";
    [SerializeField] private string sprite2DHitFlashPath = "Custom/Sprite2D_HitFlash";
    [SerializeField] private string uiGlowPath = "Custom/UI_Glow";

    // Cached shaders
    private Dictionary<string, Shader> shaderCache = new Dictionary<string, Shader>();
    
    // Material cache - để tránh tạo material mới nhiều lần
    private Dictionary<string, Material> materialCache = new Dictionary<string, Material>();

    // Shader property IDs (optimized)
    private static class ShaderProps
    {
        // Glow
        public static readonly int GlowColor = Shader.PropertyToID("_GlowColor");
        public static readonly int GlowIntensity = Shader.PropertyToID("_GlowIntensity");
        public static readonly int GlowSpeed = Shader.PropertyToID("_GlowSpeed");
        public static readonly int GlowMin = Shader.PropertyToID("_GlowMin");
        public static readonly int GlowMax = Shader.PropertyToID("_GlowMax");

        // Wave
        public static readonly int WaveAmplitude = Shader.PropertyToID("_WaveAmplitude");
        public static readonly int WaveFrequency = Shader.PropertyToID("_WaveFrequency");
        public static readonly int WaveSpeed = Shader.PropertyToID("_WaveSpeed");
        public static readonly int WaveDirection = Shader.PropertyToID("_WaveDirection");

        // Dissolve
        public static readonly int DissolveTex = Shader.PropertyToID("_DissolveTex");
        public static readonly int DissolveAmount = Shader.PropertyToID("_DissolveAmount");
        public static readonly int DissolveEdgeColor = Shader.PropertyToID("_DissolveEdgeColor");
        public static readonly int DissolveEdgeWidth = Shader.PropertyToID("_DissolveEdgeWidth");
        public static readonly int DissolveEdgeIntensity = Shader.PropertyToID("_DissolveEdgeIntensity");

        // Rainbow
        public static readonly int RainbowSpeed = Shader.PropertyToID("_RainbowSpeed");
        public static readonly int RainbowSaturation = Shader.PropertyToID("_RainbowSaturation");
        public static readonly int RainbowBrightness = Shader.PropertyToID("_RainbowBrightness");
        public static readonly int RainbowScale = Shader.PropertyToID("_RainbowScale");

        // Flash
        public static readonly int FlashColor = Shader.PropertyToID("_FlashColor");
        public static readonly int FlashAmount = Shader.PropertyToID("_FlashAmount");

        // Outline
        public static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
        public static readonly int OutlineWidth = Shader.PropertyToID("_OutlineWidth");
    }

    public enum ShaderType
    {
        Sprite2D_Glow,
        Sprite2D_Dissolve,
        Sprite2D_HitFlash,
        UI_Glow
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllShaders();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Load tất cả shader vào cache
    /// </summary>
    private void LoadAllShaders()
    {
        LoadShader(ShaderType.Sprite2D_Glow, sprite2DGlowPath);
        LoadShader(ShaderType.Sprite2D_Dissolve, sprite2DDissolvePath);
        LoadShader(ShaderType.Sprite2D_HitFlash, sprite2DHitFlashPath);
        LoadShader(ShaderType.UI_Glow, uiGlowPath);

        Debug.Log($"[ShaderManager] Loaded {shaderCache.Count} shaders");
    }

    /// <summary>
    /// Load một shader vào cache
    /// </summary>
    private void LoadShader(ShaderType type, string path)
    {
        Shader shader = Shader.Find(path);
        if (shader != null)
        {
            shaderCache[type.ToString()] = shader;
            Debug.Log($"[ShaderManager] Loaded shader: {type}");
        }
        else
        {
            Debug.LogError($"[ShaderManager] Failed to load shader: {path}");
        }
    }

    /// <summary>
    /// Lấy shader từ cache
    /// </summary>
    public Shader GetShader(ShaderType type)
    {
        string key = type.ToString();
        if (shaderCache.ContainsKey(key))
        {
            return shaderCache[key];
        }
        Debug.LogError($"[ShaderManager] Shader not found: {type}");
        return null;
    }

    /// <summary>
    /// Tạo hoặc lấy material từ cache
    /// </summary>
    public Material GetOrCreateMaterial(ShaderType type)
    {
        string key = type.ToString();
        
        if (materialCache.ContainsKey(key))
        {
            return materialCache[key];
        }

        Shader shader = GetShader(type);
        if (shader != null)
        {
            Material mat = new Material(shader);
            materialCache[key] = mat;
            return mat;
        }

        return null;
    }

    /// <summary>
    /// Apply shader cho SpriteRenderer
    /// </summary>
    public void ApplyShaderToSprite(SpriteRenderer spriteRenderer, ShaderType type)
    {
        if (spriteRenderer == null) return;

        Material mat = new Material(GetShader(type));
        spriteRenderer.material = mat;
        Debug.Log($"[ShaderManager] Applied {type} to {spriteRenderer.gameObject.name}");
    }

    /// <summary>
    /// Apply shader cho UI Image
    /// </summary>
    public void ApplyShaderToImage(Image image, ShaderType type)
    {
        if (image == null) return;

        Material mat = new Material(GetShader(type));
        image.material = mat;
        Debug.Log($"[ShaderManager] Applied {type} to {image.gameObject.name}");
    }

    /// <summary>
    /// Apply shader cho Text
    /// </summary>
    public void ApplyShaderToText(Text text, ShaderType type)
    {
        if (text == null) return;

        Material mat = new Material(GetShader(type));
        text.material = mat;
        Debug.Log($"[ShaderManager] Applied {type} to {text.gameObject.name}");
    }

    /// <summary>
    /// Apply shader cho tất cả SpriteRenderer trong object và children
    /// </summary>
    public void ApplyShaderToAllSprites(GameObject root, ShaderType type)
    {
        SpriteRenderer[] sprites = root.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sprite in sprites)
        {
            ApplyShaderToSprite(sprite, type);
        }
        Debug.Log($"[ShaderManager] Applied {type} to {sprites.Length} sprites in {root.name}");
    }

    /// <summary>
    /// Apply shader cho tất cả UI Image trong object và children
    /// </summary>
    public void ApplyShaderToAllImages(GameObject root, ShaderType type)
    {
        Image[] images = root.GetComponentsInChildren<Image>(true);
        foreach (var image in images)
        {
            ApplyShaderToImage(image, type);
        }
        Debug.Log($"[ShaderManager] Applied {type} to {images.Length} images in {root.name}");
    }

    // ===== GLOW SHADER METHODS =====
    
    public void SetGlowProperties(Material mat, Color glowColor, float intensity = 1.5f, float speed = 2f, float min = 0.3f, float max = 1f)
    {
        if (mat == null) return;
        mat.SetColor(ShaderProps.GlowColor, glowColor);
        mat.SetFloat(ShaderProps.GlowIntensity, intensity);
        mat.SetFloat(ShaderProps.GlowSpeed, speed);
        mat.SetFloat(ShaderProps.GlowMin, min);
        mat.SetFloat(ShaderProps.GlowMax, max);
    }

    // ===== WAVE SHADER METHODS =====
    
    public void SetWaveProperties(Material mat, float amplitude = 0.1f, float frequency = 5f, float speed = 2f, Vector2 direction = default)
    {
        if (mat == null) return;
        if (direction == default) direction = Vector2.right;
        
        mat.SetFloat(ShaderProps.WaveAmplitude, amplitude);
        mat.SetFloat(ShaderProps.WaveFrequency, frequency);
        mat.SetFloat(ShaderProps.WaveSpeed, speed);
        mat.SetVector(ShaderProps.WaveDirection, new Vector4(direction.x, direction.y, 0, 0));
    }

    // ===== DISSOLVE SHADER METHODS =====
    
    public void SetDissolveProperties(Material mat, Texture2D dissolveTex, float amount = 0f, Color edgeColor = default, float edgeWidth = 0.1f, float edgeIntensity = 2f)
    {
        if (mat == null) return;
        if (edgeColor == default) edgeColor = new Color(1f, 0.5f, 0f, 1f);
        
        mat.SetTexture(ShaderProps.DissolveTex, dissolveTex);
        mat.SetFloat(ShaderProps.DissolveAmount, amount);
        mat.SetColor(ShaderProps.DissolveEdgeColor, edgeColor);
        mat.SetFloat(ShaderProps.DissolveEdgeWidth, edgeWidth);
        mat.SetFloat(ShaderProps.DissolveEdgeIntensity, edgeIntensity);
    }

    // ===== RAINBOW SHADER METHODS =====
    
    public void SetRainbowProperties(Material mat, float speed = 1f, float saturation = 0.8f, float brightness = 1f, float scale = 1f)
    {
        if (mat == null) return;
        mat.SetFloat(ShaderProps.RainbowSpeed, speed);
        mat.SetFloat(ShaderProps.RainbowSaturation, saturation);
        mat.SetFloat(ShaderProps.RainbowBrightness, brightness);
        mat.SetFloat(ShaderProps.RainbowScale, scale);
    }

    // ===== FLASH SHADER METHODS =====
    
    public void SetFlashProperties(Material mat, Color flashColor, float amount = 0f)
    {
        if (mat == null) return;
        mat.SetColor(ShaderProps.FlashColor, flashColor);
        mat.SetFloat(ShaderProps.FlashAmount, amount);
    }

    // ===== OUTLINE SHADER METHODS =====
    
    public void SetOutlineProperties(Material mat, Color outlineColor, float width = 1f)
    {
        if (mat == null) return;
        mat.SetColor(ShaderProps.OutlineColor, outlineColor);
        mat.SetFloat(ShaderProps.OutlineWidth, width);
    }

    /// <summary>
    /// Clear material cache để giải phóng memory
    /// </summary>
    public void ClearMaterialCache()
    {
        foreach (var mat in materialCache.Values)
        {
            if (mat != null)
            {
                Destroy(mat);
            }
        }
        materialCache.Clear();
        Debug.Log("[ShaderManager] Material cache cleared");
    }
}
