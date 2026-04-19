using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [Header("UI Components")]
    public Slider healthSlider;
    public TMP_Text healthBarText;

    [Header("Configuration")]
    public string bossTag = "Boss";
    public string overrideName = "BOSS";
    public Color barColor = Color.red;

    [Header("Layout")]
    [Tooltip("If true, automatically anchors the Health Bar to Bottom Center")]
    public bool forceBottomCenter = true;
    public float bottomPadding = 30f;

    private Damageable bossDamageable;

    private void Reset()
    {
        healthSlider = GetComponentInChildren<Slider>();
        healthBarText = GetComponentInChildren<TMP_Text>();
    }

    private void Awake()
    {
        // Try to find the boss immediately
        FindBoss();
    }

    private void Start()
    {
        // Apply visual customization (Red Color, Position)
        CustomizeHealthBar();
        
        // Initial Update
        if (bossDamageable != null)
        {
            UpdateHealthBar(bossDamageable.Health, bossDamageable.MaxHealth);
        }
    }

    private void FindBoss()
    {
        GameObject boss = GameObject.FindGameObjectWithTag(bossTag);

        if (boss == null)
        {
            // If checking specifically for generic names if tag fails (optional safety net)
            boss = GameObject.Find("Slime Boss"); 
            if(boss == null) boss = GameObject.Find("Boss");
        }

        if (boss != null)
        {
            bossDamageable = boss.GetComponent<Damageable>();
        }
        else
        {
            Debug.LogWarning($"BossHealthBar: No GameObject found with tag '{bossTag}'. Display may not update.");
        }
    }

    private void OnEnable()
    {
        if(bossDamageable == null) FindBoss();

        if (bossDamageable != null)
        {
            bossDamageable.healthChanged.AddListener(OnHealthChanged);
        }
    }

    private void OnDisable()
    {
        if (bossDamageable != null)
        {
            bossDamageable.healthChanged.RemoveListener(OnHealthChanged);
        }
    }

    private void CustomizeHealthBar()
    {
        if (healthSlider == null) return;

        // 1. Disable Interaction (Prevents "Only red if we hover" issue)
        healthSlider.interactable = false;
        healthSlider.transition = Selectable.Transition.None;

        RectTransform sliderRect = healthSlider.GetComponent<RectTransform>();

        // 2. Widen the Bar and Position it
        if (sliderRect != null)
        {
            Vector2 size = sliderRect.sizeDelta;
            size.x = 600f; 
            sliderRect.sizeDelta = size;

            if (forceBottomCenter)
            {
                sliderRect.anchorMin = new Vector2(0.5f, 0f); 
                sliderRect.anchorMax = new Vector2(0.5f, 0f); 
                sliderRect.pivot = new Vector2(0.5f, 0f);     
                sliderRect.anchoredPosition = new Vector2(0, bottomPadding);
            }
        }

        // 3. Fix Background Color (The "Right one should be black" issue)
        // Usually, the background is a child named "Background"
        Transform bgTransform = healthSlider.transform.Find("Background");
        if (bgTransform != null)
        {
            Image bgImage = bgTransform.GetComponent<Image>();
            if (bgImage != null)
            {
                 // Create a runtime white sprite for background too if needed, 
                 // but typically setting color to black works fine on most sprites.
                 bgImage.color = Color.black;
                 
                 // If it has the green sprite, we might want to unset it too just in case
                 if (bgImage.sprite != null && bgImage.sprite.name != "RuntimeWhiteSprite")
                 {
                     // Use the same white sprite logic if we want pure black, 
                     // but usually tinting black works on anything.
                     // Let's safe-guard it by stripping the sprite if it looks like a UI sprite
                     bgImage.sprite = null; 
                 }
            }
        }

        // 4. Fix Fill Color (The "Black Bar" issue)
        if (healthSlider.fillRect != null)
        {
            Image fillImage = healthSlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
            {
                if (fillImage.sprite == null || fillImage.sprite.name != "RuntimeWhiteSprite")
                {
                    Texture2D tex = new Texture2D(2, 2);
                    Color[] colors = new Color[4];
                    for(int i=0; i<4; i++) colors[i] = Color.white;
                    tex.SetPixels(colors);
                    tex.Apply();
                    
                    Sprite whiteSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
                    whiteSprite.name = "RuntimeWhiteSprite";
                    fillImage.sprite = whiteSprite;
                }
                
                fillImage.color = barColor; 
            }
        }

        // 5. Fix Text Layout 
        if (healthBarText != null)
        {
            RectTransform textRect = healthBarText.GetComponent<RectTransform>();
            if (textRect != null)
            {
                textRect.anchorMin = new Vector2(0.5f, 1f); 
                textRect.anchorMax = new Vector2(0.5f, 1f); 
                textRect.pivot = new Vector2(0.5f, 0f);     
                
                textRect.sizeDelta = new Vector2(600f, 50f); 
                textRect.anchoredPosition = new Vector2(0, 10f); 
                
                healthBarText.alignment = TextAlignmentOptions.Center;
                healthBarText.textWrappingMode = TextWrappingModes.NoWrap;
            }

            // 6. Hide Text Background (The "Grey Box")
            // The text might be inside a panel/image that was used for the original background.
            // We want to hide that container so only the floating text remains.
            
            // Check Parent for Image (Common pattern: Panel -> Text)
            if (healthBarText.transform.parent != null && 
                healthBarText.transform.parent != healthSlider.transform && 
                healthBarText.transform.parent != this.transform)
            {
                Image bg = healthBarText.transform.parent.GetComponent<Image>();
                if (bg != null) bg.enabled = false;
            }

            // Check Self for Image (Less common but possible)
            Image selfBg = healthBarText.GetComponent<Image>();
            if (selfBg != null) selfBg.enabled = false;
        }
    }

    private void OnHealthChanged(int currentHealth, int maxHealth)
    {
        UpdateHealthBar(currentHealth, maxHealth);
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.value = (float)currentHealth / maxHealth;
        }

        if (healthBarText != null)
        {
            healthBarText.text = $"{overrideName} HP {currentHealth} / {maxHealth}";
        }
    }
}
