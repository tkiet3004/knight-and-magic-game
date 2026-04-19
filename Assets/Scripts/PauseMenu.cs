using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject pauseButton; // Nút pause cần ẩn khi menu hiện

    public void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        
        // Ẩn pause button khi menu hiện
        if (pauseButton != null)
        {
            pauseButton.SetActive(false);
        }
    }

    public void Home()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenu();
        }
        else
        {
            SceneManager.LoadSceneAsync(0);
            Time.timeScale = 1f;
        }
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        
        // Hiện lại pause button khi resume
        if (pauseButton != null)
        {
            pauseButton.SetActive(true);
        }
    }

    [Header("UI Resources")]
    [Tooltip("Drag 'Prinbles_Asset_Robin (v 1.1) (9_5_2023)/Save.png' here if possible")]
    [SerializeField] private Sprite saveButtonSprite; 

    private void Start()
    {
        // Ẩn text cũ nếu có (bỏ feature cũ)
        Transform t = transform.Find("AutoSaveText");
        if(t != null) Destroy(t.gameObject);

        // Tạo nút Save Button dynamically
        CreateSaveButton();
    }

    private void CreateSaveButton()
    {
        if (pauseMenu == null) return;

        // Tìm nút template. Ưu tiên tìm nút Resume hoặc Restart để copy
        // Các nút trong pause menu thường là con trực tiếp hoặc nằm trong panel
        Button[] buttons = pauseMenu.GetComponentsInChildren<Button>(true);
        Button templateButton = null;

        // Try to find a good template (prefer Restart or Resume over Home)
        foreach (var btn in buttons)
        {
            if (btn.name.Contains("Resume") || btn.name.Contains("Restart"))
            {
                templateButton = btn;
                break;
            }
        }
        
        // Fallback to any button
        if (templateButton == null && buttons.Length > 0) templateButton = buttons[0];
        
        if (templateButton == null)
        {
            // Debug.LogWarning("[PauseMenu] Could not find any button to clone for Save button!");
            return;
        }

        // Clone nút mới
        GameObject saveBtnObj = Instantiate(templateButton.gameObject, templateButton.transform.parent);
        saveBtnObj.name = "SaveButton";
        
        // Disable original text object if exists (since we use icon)
        foreach(Transform child in saveBtnObj.transform)
        {
            // Disable text or existing icon if we want to replace it
            // Assuming the structure is Button -> Image(Icon) or Button -> Text
            // We'll try to find the Image component on the button itself first
        }

        // Setup Button Event
        Button saveBtn = saveBtnObj.GetComponent<Button>();
        saveBtn.onClick.RemoveAllListeners();
        saveBtn.onClick.AddListener(Save);

        // Change Image
        Image btnImage = saveBtnObj.GetComponent<Image>();
        if (btnImage != null)
        {
             // If the button has an image component directly, set it
             // If we have a specific sprite, use it. 
             if (saveButtonSprite != null)
             {
                 btnImage.sprite = saveButtonSprite;
                 btnImage.type = Image.Type.Simple;
                 btnImage.preserveAspect = true;
             }
        }
        
        // Adjust Hierarchy Order (e.g. Place it before "Home" or at the end)
        // Let's try to place it before the "Home" button/Quit button if possible
        // Or just keep it at the end (which usually means rightmost or bottom)
        saveBtnObj.transform.SetAsLastSibling();
        
        // Log to confirm
        //Debug.Log($"[PauseMenu] Created Save Button from template: {templateButton.name}");
    }

    public void Save()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveGameManual();
            // Optional: Feedback (e.g. Flash text)
            Debug.Log("Game Saved Manually!");
        }
    }

    public void Restart()
    {
        Time.timeScale = 1f;

        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.ClearSessionState();
            GameProgressManager.Instance.ShouldLoadFromCheckpoint = false;
        }

        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }
}
