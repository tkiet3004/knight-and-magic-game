using UnityEngine;

/// <summary>
/// Thư viện các hiệu ứng shader - CHỈ CÓ CODE, KHÔNG ADD COMPONENT
/// Chỉ chứa color presets đơn giản
/// </summary>
public static class ShaderEffectLibrary
{
    // ===== FLASH COLOR PRESETS =====
    
    public static class FlashColor
    {
        public static readonly Color White = Color.white;
        public static readonly Color Red = new Color(1f, 0f, 0f, 1f);
        public static readonly Color Blue = new Color(0.5f, 0.7f, 1f, 1f);
        public static readonly Color Yellow = new Color(1f, 1f, 0f, 1f);
        public static readonly Color Green = new Color(0f, 1f, 0f, 1f);
    }
}

