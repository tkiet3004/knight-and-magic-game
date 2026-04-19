using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    // --- Configuration & Assets (Drag these in Inspector) ---
    [Header("Assets (Optional)")]
    public TMP_FontAsset customFont;
    public Sprite backgroundSprite;
    public Sprite itemCardSprite;
    public Sprite buttonSprite;
    public Sprite buttonHoverSprite;
    public Sprite coinIcon;
    public Sprite healthIcon;
    public Sprite speedIcon; // Was runIcon
    public Sprite damageIcon; // Was archeryIcon

    [Header("Costs")]
    public int healthUpgradeBaseCost = 50;
    public int speedUpgradeBaseCost = 100;
    public int damageUpgradeBaseCost = 150;

    // --- Internal UI References (Built automatically) ---
    private TMP_Text coinText;
    
    private Button healthButton;
    private TMP_Text healthCostText;
    private TMP_Text healthLevelText;

    private Button speedButton;
    private TMP_Text speedCostText;
    private TMP_Text speedLevelText;

    private Button damageButton;
    private TMP_Text damageCostText;
    private TMP_Text damageLevelText;

    // PlayerPrefs Keys
    private const string COINS_KEY = "Coins";
    private const string HP_LEVEL_KEY = "HealthUpgrades";
    private const string SPEED_KEY = "SpeedLevel";
    private const string DAMAGE_KEY = "DamageLevel";

    private void Start()
    {
        BuildShopUI();
        UpdateUI();
    }

    private void Update()
    {
        // DEBUG: Cheat to add coins
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.mKey.wasPressedThisFrame)
        {
            Debug.Log("Cheat: Added 1000 Coins");
            int coins = PlayerPrefs.GetInt(COINS_KEY, 0);
            PlayerPrefs.SetInt(COINS_KEY, coins + 1000);
            PlayerPrefs.Save();
            UpdateUI();
        }

        // DEBUG: Cheat to reset upgrades
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.nKey.wasPressedThisFrame)
        {
            ResetUpgrades();
        }
    }

    private void UpdateUI()
    {
        int coins = PlayerPrefs.GetInt(COINS_KEY, 0);
        int hpLevel = PlayerPrefs.GetInt(HP_LEVEL_KEY, 0);
        int speedLevel = PlayerPrefs.GetInt(SPEED_KEY, 0);
        int damageLevel = PlayerPrefs.GetInt(DAMAGE_KEY, 0);

        if (coinText) coinText.text = coins.ToString();

        // 1. Health Upgrade
        UpdateUpgradeItem(healthButton, healthCostText, healthLevelText, coins, healthUpgradeBaseCost, hpLevel, "MAX HP");

        // 2. Speed Upgrade
        UpdateUpgradeItem(speedButton, speedCostText, speedLevelText, coins, speedUpgradeBaseCost, speedLevel, "SPEED");

        // 3. Damage Upgrade
        UpdateUpgradeItem(damageButton, damageCostText, damageLevelText, coins, damageUpgradeBaseCost, damageLevel, "DAMAGE");
    }

    private void UpdateUpgradeItem(Button btn, TMP_Text costText, TMP_Text levelText, int currentCoins, int baseCost, int currentLevel, string statName)
    {
        // Cost could scale, e.g., base + (level * 50). Sticking to base for now as per request simplicity, or simple scaling.
        // Let's do simple scaling so 0->1 is base, 1->2 is base+50.
        int currentCost = baseCost + (currentLevel * 50);

        if (costText) costText.text = currentCost.ToString();
        if (levelText) 
        {
             if(statName == "MAX HP")
                levelText.text = $"MAX HP: {100 + (currentLevel * 50)}";
             else
                levelText.text = $"LVL {currentLevel}";
        }

        if (btn)
        {
            bool canAfford = currentCoins >= currentCost;
            btn.interactable = canAfford;
            UpdateButtonVisuals(btn, canAfford);

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => BuyUpgrade(statName, currentCost));
        }
    }

    private void UpdateButtonVisuals(Button btn, bool interactable)
    {
        Image img = btn.GetComponent<Image>();
        if (img)
        {
            img.color = interactable ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f); 
        }
    }

    // --- Buying Logic ---

    public void BuyUpgrade(string upgradeType, int cost)
    {
        int coins = PlayerPrefs.GetInt(COINS_KEY, 0);
        if (coins >= cost)
        {
            coins -= cost;
            PlayerPrefs.SetInt(COINS_KEY, coins);

            if (upgradeType == "MAX HP")
            {
                int lvl = PlayerPrefs.GetInt(HP_LEVEL_KEY, 0);
                PlayerPrefs.SetInt(HP_LEVEL_KEY, lvl + 1);
            }
            else if (upgradeType == "SPEED")
            {
                int lvl = PlayerPrefs.GetInt(SPEED_KEY, 0);
                PlayerPrefs.SetInt(SPEED_KEY, lvl + 1);
            }
            else if (upgradeType == "DAMAGE")
            {
                int lvl = PlayerPrefs.GetInt(DAMAGE_KEY, 0);
                PlayerPrefs.SetInt(DAMAGE_KEY, lvl + 1);
            }

            PlayerPrefs.Save();
            UpdateUI();
        }
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Main Menu"); // Matches "Main Menu.unity"
    }


    // --- UI Construction ---

    private void BuildShopUI()
    {
        // 1. Find or Create Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("ShopCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Ensure EventSystem exists
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // 2. Background Panel
        GameObject panelObj = new GameObject("ShopPanel");
        panelObj.transform.SetParent(canvas.transform, false);
        Image panelImg = panelObj.AddComponent<Image>();
        panelImg.color = Color.white; 
        if (backgroundSprite) panelImg.sprite = backgroundSprite;
        RectTransform panelRT = panelObj.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        
        // 3. Title - Changed to "SHOP"
        TMP_Text title = CreateText(panelObj.transform, "SHOP", 120, new Vector2(0, -80), Color.yellow, true);
        title.outlineWidth = 0.25f;
        title.outlineColor = new Color(0.4f, 0.2f, 0f); 

        RectTransform titleRT = title.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.5f, 1f);
        titleRT.anchorMax = new Vector2(0.5f, 1f);
        titleRT.pivot = new Vector2(0.5f, 1f);
        titleRT.anchoredPosition = new Vector2(0, -80);

        // 4. Coin HUD
        GameObject coinHud = new GameObject("CoinHUD");
        coinHud.transform.SetParent(panelObj.transform, false);
        RectTransform coinRT = coinHud.AddComponent<RectTransform>();
        coinRT.anchorMin = Vector2.one;
        coinRT.anchorMax = Vector2.one;
        coinRT.pivot = Vector2.one;
        coinRT.anchoredPosition = new Vector2(-50, -50);
        
        // Coin Icon - Moved further left to give space
        GameObject cIcon = new GameObject("Icon");
        cIcon.transform.SetParent(coinHud.transform, false);
        Image cImg = cIcon.AddComponent<Image>();
        if (coinIcon) cImg.sprite = coinIcon;
        else cImg.color = Color.yellow;
        RectTransform cIconRT = cIcon.GetComponent<RectTransform>();
        cIconRT.sizeDelta = new Vector2(60, 60);
        cIconRT.anchoredPosition = new Vector2(-70, 0);

        // Coin Text - Increased Padding relative to icon
        coinText = CreateText(coinHud.transform, "0", 60, Vector2.zero, new Color(1f, 0.84f, 0f), true);
        coinText.alignment = TextAlignmentOptions.Right; // Align Right
        RectTransform coinTextRT = coinText.GetComponent<RectTransform>();
        coinTextRT.pivot = new Vector2(1, 0.5f);
        coinTextRT.anchoredPosition = new Vector2(-150, 0); // Much further left of icon

        // 5. Items Container
        GameObject container = new GameObject("ItemsContainer");
        container.transform.SetParent(panelObj.transform, false);
        RectTransform contRT = container.AddComponent<RectTransform>();
        contRT.anchorMin = new Vector2(0.5f, 0.5f);
        contRT.anchorMax = new Vector2(0.5f, 0.5f);
        contRT.sizeDelta = new Vector2(1200, 500);
        
        HorizontalLayoutGroup hlg = container.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.spacing = 50;
        hlg.childControlHeight = false;
        hlg.childControlWidth = false;

        // 6. Create Items
        CreateHealthItem(container.transform);
        CreateSpeedItem(container.transform);
        CreateDamageItem(container.transform);

        // 7. Back Button
        GameObject backBtnObj = CreateButton(panelObj.transform, "BACK", new Vector2(-800, -450), BackToMenu);
        RectTransform backRT = backBtnObj.GetComponent<RectTransform>();
        backRT.anchorMin = Vector2.zero;
        backRT.anchorMax = Vector2.zero;
        backRT.anchoredPosition = new Vector2(150, 100);
    }

    public void ResetUpgrades()
    {
        Debug.Log("Resetting Upgrades...");
        PlayerPrefs.SetInt(HP_LEVEL_KEY, 0);
        PlayerPrefs.SetInt(SPEED_KEY, 0);
        PlayerPrefs.SetInt(DAMAGE_KEY, 0);
        PlayerPrefs.Save();
        UpdateUI();
    }

    private void CreateHealthItem(Transform parent)
    {
        GameObject item = CreateItemCard(parent, "MAX HP", healthIcon);
        
        // Visually decrease Health Icon size
        Transform iconTr = item.transform.Find("Icon");
        if (iconTr)
        {
             RectTransform iconRT = iconTr.GetComponent<RectTransform>();
             iconRT.sizeDelta = new Vector2(90, 90); 
        }

        // Adjusted vertical spacing
        healthLevelText = CreateText(item.transform, "LVL 0", 30, new Vector2(0, -40), new Color(0.3f, 0.2f, 0.1f), true);
        healthCostText = CreateText(item.transform, "50", 45, new Vector2(0, -100), new Color(0.4f, 0.15f, 0.1f), true);
        GameObject btnObj = CreateButton(item.transform, "BUY", new Vector2(0, -170), null);
        healthButton = btnObj.GetComponent<Button>();
    }

    private void CreateSpeedItem(Transform parent)
    {
        GameObject item = CreateItemCard(parent, "SPEED", speedIcon);
        speedLevelText = CreateText(item.transform, "LVL 0", 30, new Vector2(0, -40), new Color(0.3f, 0.2f, 0.1f), true);
        speedCostText = CreateText(item.transform, "100", 45, new Vector2(0, -100), new Color(0.4f, 0.15f, 0.1f), true);
        GameObject btnObj = CreateButton(item.transform, "BUY", new Vector2(0, -170), null);
        speedButton = btnObj.GetComponent<Button>();
    }

    private void CreateDamageItem(Transform parent)
    {
        GameObject item = CreateItemCard(parent, "DAMAGE", damageIcon);
        
        // Visually increase Attack Icon size to match others
        Transform iconTr = item.transform.Find("Icon");
        if (iconTr)
        {
             RectTransform iconRT = iconTr.GetComponent<RectTransform>();
             iconRT.sizeDelta = new Vector2(140, 140); 
        }

        damageLevelText = CreateText(item.transform, "LVL 0", 30, new Vector2(0, -40), new Color(0.3f, 0.2f, 0.1f), true);
        damageCostText = CreateText(item.transform, "150", 45, new Vector2(0, -100), new Color(0.4f, 0.15f, 0.1f), true);
        GameObject btnObj = CreateButton(item.transform, "BUY", new Vector2(0, -170), null);
        damageButton = btnObj.GetComponent<Button>();
    }

    private GameObject CreateItemCard(Transform parent, string title, Sprite icon)
    {
        GameObject card = new GameObject("ItemCard");
        card.transform.SetParent(parent, false);
        Image cardImg = card.AddComponent<Image>();
        cardImg.color = Color.white;
        if (itemCardSprite)
        {
             cardImg.sprite = itemCardSprite;
             cardImg.type = Image.Type.Sliced;
        }
        else
        {
             cardImg.color = new Color(0.2f, 0.2f, 0.25f, 1f);
        }
        
        RectTransform rt = card.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300, 450); // Made slightly taller

        // Title at very top
        CreateText(card.transform, title, 36, new Vector2(0, 170), new Color(0.3f, 0.2f, 0.1f), true);

        // Icon in the upper middle
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(card.transform, false);
        Image iconImg = iconObj.AddComponent<Image>();
        if (icon) iconImg.sprite = icon;
        else iconImg.color = new Color(0.5f, 0.5f, 0.5f);
        RectTransform iconRT = iconObj.GetComponent<RectTransform>();
        iconRT.sizeDelta = new Vector2(100, 100);
        iconRT.anchoredPosition = new Vector2(0, 70); // Lowered from 100 to give "margin top"

        return card;
    }

    private TMP_Text CreateText(Transform parent, string content, float size, Vector2 pos, Color color, bool bold)
    {
        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(parent, false);
        TMP_Text tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        if (bold) tmp.fontStyle = FontStyles.Bold;
        if (customFont) tmp.font = customFont;

        RectTransform rt = txtObj.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(300, 100);
        return tmp;
    }

    private GameObject CreateButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction action)
    {
        GameObject btnObj = new GameObject("Button_" + label);
        btnObj.transform.SetParent(parent, false);
        
        Image img = btnObj.AddComponent<Image>();
        img.color = Color.white;
        if (buttonSprite) 
        {
            img.sprite = buttonSprite;
            img.type = Image.Type.Sliced; 
        }
        else 
        {
             img.color = new Color(0.3f, 0.3f, 0.3f);
        }

        Button btn = btnObj.AddComponent<Button>();
        if (action != null) btn.onClick.AddListener(action);

        if (buttonHoverSprite != null)
        {
            btn.transition = Selectable.Transition.SpriteSwap;
            SpriteState ss = new SpriteState();
            ss.highlightedSprite = buttonHoverSprite;
            ss.pressedSprite = buttonHoverSprite;
            ss.selectedSprite = buttonHoverSprite;
            btn.spriteState = ss;
        }

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(160, 50);
        rt.anchoredPosition = pos;

        // Label - Dark text for better contrast on light buttons
        CreateText(btnObj.transform, label, 30, Vector2.zero, new Color(0.2f, 0.1f, 0.05f), true); // Dark Brown

        return btnObj;
    }
}
