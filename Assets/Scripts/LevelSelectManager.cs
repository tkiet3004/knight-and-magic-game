using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro; // Added for TextMeshPro support

/// <summary>
/// Quản lý scene Level Select - hiển thị các level đã unlock
/// Updated to use Prinbles GUI assets and TextMeshPro
/// Includes Auto-Responsive UI setup, Custom Fonts, Scaled UI, and Layout fixes
/// </summary>
public class LevelSelectManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform levelButtonContainer; // Container chứa các button level
    [SerializeField] private Button levelButtonPrefab; // Prefab cho button level
    [SerializeField] private Button backButton; // Nút quay lại
    [SerializeField] private Image backgroundImage; // Background image to stretch
    [SerializeField] private TMP_Text titleText; // Title Text (e.g. "Level")

    [Header("Prinbles Sprites - Levels")]
    [Tooltip("Drag 'asset/png/Level/Button/Unlocked/Default.png' here")]
    [SerializeField] private Sprite unlockedNormal;
    [Tooltip("Drag 'asset/png/Level/Button/Unlocked/Hover.png' here")]
    [SerializeField] private Sprite unlockedHover;
    [Tooltip("Drag 'asset/png/Level/Button/Locked/Default.png' here")]
    [SerializeField] private Sprite lockedNormal;
    [Tooltip("Drag 'asset/png/Level/Button/Locked/Hover.png' here")]
    [SerializeField] private Sprite lockedHover;
    
    [Header("Prinbles Sprites - Back Button")]
    [Tooltip("Drag 'asset/png/Buttons/Square-Medium/ArrowLeft/Default.png' here")]
    [SerializeField] private Sprite backBtnNormal;
    [Tooltip("Drag 'asset/png/Buttons/Square-Medium/ArrowLeft/Hover.png' here")]
    [SerializeField] private Sprite backBtnHover;

    [Header("Appearance")]
    [SerializeField] private TMP_FontAsset customFont;
    [Range(0.5f, 3f)]
    [SerializeField] private float buttonScale = 1.5f;
    [SerializeField] private float fontSize = 36f;
    [SerializeField] private float titleFontSize = 72f;
    [SerializeField] private float layoutSpacing = 30f; // Increased spacing

    [Header("Settings")]
    [SerializeField] private int totalLevels = 3; // Tổng số level

    private List<Button> levelButtons = new List<Button>();

    private void Start()
    {
        // Auto-fix layout and responsiveness
        SetupResponsiveUI();
        SetupLayout();

        // Setup Title
        SetupTitle();

        // Setup Back Button Visuals
        SetupBackButton();

        // Tạo các button level
        CreateLevelButtons();

        // Setup back button event
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
    }

    private void SetupLayout()
    {
        if (levelButtonContainer == null) return;

        // Check for existing/conflicting layout groups
        LayoutGroup existingLayout = levelButtonContainer.GetComponent<LayoutGroup>();
        HorizontalLayoutGroup layout = null;

        if (existingLayout != null)
        {
            if (existingLayout is HorizontalLayoutGroup hg)
            {
                layout = hg;
            }
            else
            {
                // Found a different layout group (e.g., Grid or Vertical), remove it
                DestroyImmediate(existingLayout);
            }
        }

        // Add HorizontalLayoutGroup if missing
        if (layout == null)
        {
            layout = levelButtonContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        }

        // Configure Layout
        if (layout != null)
        {
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = layoutSpacing;
            layout.childControlWidth = false; // We set size manually
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            
            // Debug.Log("[LevelSelectManager] Auto-configured HorizontalLayoutGroup.");
        }
        else
        {
            //Debug.LogWarning("[LevelSelectManager] Could not configure HorizontalLayoutGroup. If UI looks okay, you can ignore this.");
        }
    }

    private void SetupTitle()
    {
        if (titleText == null)
        {
            GameObject titleObj = GameObject.Find("Title");
            if (titleObj != null) titleText = titleObj.GetComponent<TMP_Text>();
        }

        if (titleText != null)
        {
            titleText.text = "LEVEL";
            titleText.fontSize = titleFontSize; // Apply larger font size
            
            if (customFont != null) titleText.font = customFont;
        }
    }

    private void SetupBackButton()
    {
        if (backButton != null)
        {
            // 1. Hide Text
            TMP_Text btnText = backButton.GetComponentInChildren<TMP_Text>();
            if (btnText != null) btnText.gameObject.SetActive(false);
            Text legacyText = backButton.GetComponentInChildren<Text>();
            if (legacyText != null) legacyText.gameObject.SetActive(false);

            // 2. Set Scaled Size (Square)
            RectTransform rt = backButton.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.sizeDelta = new Vector2(100, 100); 
            }

            // 3. Set Sprites
            Image img = backButton.GetComponent<Image>();
            if (img != null)
            {
                img.type = Image.Type.Simple;
                img.preserveAspect = true;
                
                if (backBtnNormal != null) img.sprite = backBtnNormal;

                backButton.transition = Selectable.Transition.SpriteSwap;
                SpriteState ss = new SpriteState();
                if (backBtnHover != null)
                {
                    ss.highlightedSprite = backBtnHover;
                    ss.pressedSprite = backBtnHover;
                    ss.selectedSprite = backBtnHover;
                }
                backButton.spriteState = ss;
            }
        }
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

        if (backButton != null)
        {
            RectTransform rt = backButton.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(1, 1);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(1, 1);
                rt.anchoredPosition = new Vector2(-50, -50); 
            }
        }

        if (backgroundImage == null)
        {
            GameObject bgObj = GameObject.Find("Background");
            if (bgObj != null) backgroundImage = bgObj.GetComponent<Image>();
        }

        if (backgroundImage != null)
        {
            RectTransform rt = backgroundImage.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.SetAsFirstSibling();
            }
        }
    }

    private void CreateLevelButtons()
    {
        if (levelButtonContainer == null || levelButtonPrefab == null) return;

        foreach (Transform child in levelButtonContainer) Destroy(child.gameObject);
        levelButtons.Clear();

        GameProgressManager progressManager = GameProgressManager.Instance;
        if (progressManager == null) Debug.LogWarning("[LevelSelectManager] No ProgressManager found.");

        float baseSize = 100f; // Assume base sprite size
        Vector2 buttonSize = new Vector2(baseSize * buttonScale, baseSize * buttonScale);

        // Start from 0 (Tutorial)
        for (int i = 0; i <= totalLevels; i++)
        {
            Button button = Instantiate(levelButtonPrefab, levelButtonContainer);
            levelButtons.Add(button);

            // Apply Size manually via LayoutElement or resizing RectTransform directly
            LayoutElement le = button.GetComponent<LayoutElement>();
            if (le == null) le = button.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = buttonSize.x;
            le.preferredHeight = buttonSize.y;
            le.minWidth = buttonSize.x;
            le.minHeight = buttonSize.y;

            // Also set RectTransform directly just in case layout group is disabled/weird
            RectTransform rt = button.GetComponent<RectTransform>();
            if (rt != null) rt.sizeDelta = buttonSize;

            int levelIndex = i;
            bool isUnlocked = (progressManager != null) ? progressManager.IsLevelUnlocked(i) : true;
            
            // Tutorial (Level 0) is always unlocked
            if (i == 0) isUnlocked = true;

            // Setup button text
            TMP_Text tmpText = button.GetComponentInChildren<TMP_Text>();
            Text legacyText = button.GetComponentInChildren<Text>();

            if (isUnlocked)
            {
                string label = $"{i}";
                if (tmpText != null)
                {
                    tmpText.text = label;
                    tmpText.fontSize = fontSize; // Apply Font Size
                    tmpText.color = Color.white;
                    tmpText.alignment = TextAlignmentOptions.Center; // Center Text
                    
                    if (customFont != null) tmpText.font = customFont;
                    
                    // Nudge text down slightly because button 3D effect usually pushes visual center down
                    RectTransform textRt = tmpText.GetComponent<RectTransform>();
                    if (textRt != null) textRt.anchoredPosition = new Vector2(0, 5); 

                    tmpText.gameObject.SetActive(true);
                }
                else if (legacyText != null)
                {
                    legacyText.text = label;
                    legacyText.fontSize = (int)fontSize;
                    legacyText.color = Color.white;
                    legacyText.alignment = TextAnchor.MiddleCenter;
                    legacyText.gameObject.SetActive(true);
                }
            }
            else
            {
                if (tmpText != null) tmpText.gameObject.SetActive(false);
                if (legacyText != null) legacyText.gameObject.SetActive(false);
            }

            // Setup button image
            Image btnImage = button.GetComponent<Image>();
            if (btnImage != null)
            {
                btnImage.type = Image.Type.Simple;
                btnImage.preserveAspect = true;

                if (isUnlocked && unlockedNormal != null) btnImage.sprite = unlockedNormal;
                else if (!isUnlocked && lockedNormal != null) btnImage.sprite = lockedNormal;
                
                button.transition = Selectable.Transition.SpriteSwap;
                SpriteState spriteState = new SpriteState();
                
                if (isUnlocked)
                {
                    spriteState.highlightedSprite = unlockedHover;
                    spriteState.pressedSprite = unlockedHover;
                    spriteState.selectedSprite = unlockedHover;
                }
                else
                {
                    spriteState.highlightedSprite = lockedHover;
                    spriteState.pressedSprite = lockedHover;
                    spriteState.selectedSprite = lockedHover;
                }
                button.spriteState = spriteState;
            }

            button.interactable = isUnlocked;
            button.onClick.AddListener(() => OnLevelButtonClicked(levelIndex));
        }
    }

    private void OnLevelButtonClicked(int levelIndex)
    {
        int sceneIndex;
        if (levelIndex == 0)
        {
            sceneIndex = 1; // Tutorial Scene
        }
        else
        {
            sceneIndex = levelIndex + 2; // Level 1 -> Scene 3, Level 2 -> Scene 4, etc.
        }
        
        Debug.Log($"[LevelSelectManager] Load Level {levelIndex} (Scene {sceneIndex})");

        // FIX: Ensure fresh start by clearing session state and ensuring we don't load checkpoint
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.ClearSessionState();
            GameProgressManager.Instance.ShouldLoadFromCheckpoint = false;
        }

        SceneManager.LoadScene(sceneIndex);
    }

    private void OnBackButtonClicked()
    {
        Debug.Log("[LevelSelectManager] Quay lại Main Menu");
        SceneManager.LoadScene(0);
    }
}
