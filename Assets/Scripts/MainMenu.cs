using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform menuContainer; // Container for buttons
    [SerializeField] private Button buttonPrefab; // Prefab for menu buttons
    [SerializeField] private TMP_Text titleText; // Game Title Text
    
    [Header("Prinbles Sprites")]
    [Tooltip("Drag 'Prinbles_Asset_Robin (v 1.1)/png/Button/Rect/Fill/Default.png' here")]
    [SerializeField] private Sprite buttonNormal;
    [Tooltip("Drag 'Prinbles_Asset_Robin (v 1.1)/png/Button/Rect/Fill/Hover.png' here")]
    [SerializeField] private Sprite buttonHover;

    [Header("Appearance")]
    [SerializeField] private TMP_FontAsset customFont;
    [SerializeField] private float buttonScale = 3f;
    [SerializeField] private float fontSize = 80f; // Increased default size
    [SerializeField] private float spacing = 40f;
    [SerializeField] private Color buttonTextColor = new Color(0.3f, 0.2f, 0.1f, 1f); // Dark Brown default

    private GameProgressManager gameProgressManager;

    private void Awake()
    {
        // Ensure GameProgressManager exists
        if (GameProgressManager.Instance == null)
        {
            GameObject progressManager = new GameObject("GameProgressManager");
            progressManager.AddComponent<GameProgressManager>();
        }
        gameProgressManager = GameProgressManager.Instance;
    }

    private void Start()
    {
        SetupResponsiveUI();
        SetupLayout();
        CreateMenuButtons();
        SetupTitle();
    }

    private void SetupTitle()
    {
        if (titleText != null)
        {
            titleText.text = "KNIGHT & MAGE";
            if (customFont != null) titleText.font = customFont;
            titleText.fontSize = 110f; // Big title
            titleText.color = Color.white;
            titleText.fontStyle = FontStyles.Bold; // Bold title
            titleText.alignment = TextAlignmentOptions.Center;
            
            // Add outline for better visibility
            titleText.outlineWidth = 0.2f;
            titleText.outlineColor = new Color(0.2f, 0.1f, 0f, 1f); // Dark outline
        }
    }

    private void SetupLayout()
    {
        if (menuContainer == null) return;

        // Clean up previous layouts if any
        LayoutGroup existingLayout = menuContainer.GetComponent<LayoutGroup>();
        if (existingLayout != null && !(existingLayout is VerticalLayoutGroup))
        {
             DestroyImmediate(existingLayout);
        }

        VerticalLayoutGroup layout = menuContainer.GetComponent<VerticalLayoutGroup>();
        if (layout == null) layout = menuContainer.gameObject.AddComponent<VerticalLayoutGroup>();

        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = spacing;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
    }

    private void CreateMenuButtons()
    {
        if (menuContainer == null || buttonPrefab == null)
        {
            Debug.LogError("[MainMenu] Menu Container or Button Prefab is missing!");
            return;
        }

        // Clear existing buttons
        foreach (Transform child in menuContainer) Destroy(child.gameObject);

        // Create START Button
        CreateButton("START", PlayGame);

        // Create CONTINUE Button (Always visible)
        Button continueBtn = CreateButton("CONTINUE", null); // Callback set later to capture button ref
        continueBtn.onClick.AddListener(() => OnContinueClicked(continueBtn));

        // Create SHOP Button
        CreateButton("SHOP", EnterShop);

        // Create SETTINGS Button
        CreateButton("SETTINGS", CreateSettingsMenu);

        // Create QUIT Button
        CreateButton("QUIT", QuitGame);
    }

    private void CreateSettingsMenu()
    {
        if (menuContainer == null || buttonPrefab == null) return;

        // Clear existing buttons
        foreach (Transform child in menuContainer) Destroy(child.gameObject);

        // Music Toggle
        Button musicBtn = CreateButton(GetMusicText(), null);
        musicBtn.onClick.AddListener(() => {
             SoundSettings.MusicEnabled = !SoundSettings.MusicEnabled;
             UpdateBtnText(musicBtn, GetMusicText());
        });

        // SFX Toggle
        Button sfxBtn = CreateButton(GetSfxText(), null);
        sfxBtn.onClick.AddListener(() => {
             SoundSettings.SFXEnabled = !SoundSettings.SFXEnabled;
             UpdateBtnText(sfxBtn, GetSfxText());
        });

        // Back Button
        CreateButton("BACK", CreateMenuButtons);
    }

    private string GetMusicText()
    {
        return SoundSettings.MusicEnabled ? "MUSIC: ON" : "MUSIC: OFF";
    }

    private string GetSfxText()
    {
        return SoundSettings.SFXEnabled ? "SFX: ON" : "SFX: OFF";
    }

    private void UpdateBtnText(Button btn, string text)
    {
        TMP_Text txt = btn.GetComponentInChildren<TMP_Text>();
        if(txt) txt.text = text;
    }

    private void OnContinueClicked(Button btn)
    {
        if (gameProgressManager != null && gameProgressManager.HasCheckpoint())
        {
            gameProgressManager.ContinueGame();
        }
        else
        {
            Debug.Log("[MainMenu] No checkpoint found!");
            StartCoroutine(ShowNoCheckpointFeedback(btn));
        }
    }

    private System.Collections.IEnumerator ShowNoCheckpointFeedback(Button btn)
    {
        TMP_Text btnText = btn.GetComponentInChildren<TMP_Text>();
        string originalText = "";
        
        if (btnText != null)
        {
            originalText = btnText.text;
            btnText.text = "NO DATA";
            btnText.color = Color.red; // Visual feedback
        }

        yield return new WaitForSeconds(1f);

        if (btnText != null)
        {
            btnText.text = originalText;
            btnText.color = buttonTextColor; // Restore color
        }
    }

    private Button CreateButton(string text, UnityEngine.Events.UnityAction onClickAction)
    {
        Button btn = Instantiate(buttonPrefab, menuContainer);
        
        // Scale and Size
        float baseWidth = 160f; // Approx width of the sprite
        float baseHeight = 60f; // Approx height
        Vector2 size = new Vector2(baseWidth * buttonScale, baseHeight * buttonScale);
        
        RectTransform rt = btn.GetComponent<RectTransform>();
        if (rt != null) rt.sizeDelta = size;


        LayoutElement le = btn.GetComponent<LayoutElement>();
        if (le == null) le = btn.gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = size.x;
        le.preferredHeight = size.y;
        le.minWidth = size.x;
        le.minHeight = size.y;

        // Setup Image
        Image img = btn.GetComponent<Image>();
        if (img != null)
        {
            img.type = Image.Type.Simple;
            img.preserveAspect = true;
            if (buttonNormal != null) img.sprite = buttonNormal;
            
            // Transitions
            btn.transition = Selectable.Transition.SpriteSwap;
            SpriteState ss = new SpriteState();
            if (buttonHover != null)
            {
                ss.highlightedSprite = buttonHover;
                ss.pressedSprite = buttonHover;
                ss.selectedSprite = buttonHover;
            }
            btn.spriteState = ss;
        }

        // Setup Text
        TMP_Text tmpText = btn.GetComponentInChildren<TMP_Text>();
        Text legacyText = btn.GetComponentInChildren<Text>();

        if (tmpText != null)
        {
            tmpText.text = text.ToUpper();
            tmpText.fontSize = fontSize;
            tmpText.color = buttonTextColor; // Use custom color
            tmpText.fontStyle = FontStyles.Bold; // Make Bold
            tmpText.alignment = TextAlignmentOptions.Center;
            
            if (customFont != null) tmpText.font = customFont;
            
            // Visual centering tweak
            RectTransform txtRt = tmpText.GetComponent<RectTransform>();
            if (txtRt != null) txtRt.anchoredPosition = new Vector2(0, 5);
        }
        else if (legacyText != null)
        {
            legacyText.text = text.ToUpper();
            legacyText.fontSize = (int)fontSize;
            legacyText.color = buttonTextColor; // Use custom color
            legacyText.fontStyle = FontStyle.Bold; // Make Bold
            legacyText.alignment = TextAnchor.MiddleCenter;
        }

        if (onClickAction != null)
        {
            btn.onClick.AddListener(onClickAction);
        }

        return btn;
    }

    private void SetupResponsiveUI()
    {
        CanvasScaler scaler = FindFirstObjectByType<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }
    }

    public void PlayGame()
    {
        if (gameProgressManager.IsFirstTime())
        {
            Debug.Log("[MainMenu] First time, loading Tutorial");
            SceneManager.LoadSceneAsync(1);
        }
        else
        {
            Debug.Log("[MainMenu] Returning player, loading Level Select");
            SceneManager.LoadSceneAsync(2);
        }
    }

    public void EnterShop()
    {
        Debug.Log("[MainMenu] Entering Shop...");
        // Assuming the Shop scene is named "Shop"
        SceneManager.LoadScene("Shop");
    }

    public void QuitGame()
    {
        Debug.Log("[MainMenu] Quitting Game...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
