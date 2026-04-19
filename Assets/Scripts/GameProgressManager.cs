using UnityEngine;

/// <summary>
/// Quản lý tiến trình game - Save/Load level đã unlock
/// </summary>
public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance { get; private set; }

    [SerializeField] private int totalLevels = 3; // Tổng số level trong game

    // Keys cho PlayerPrefs
    private const string FIRST_TIME_KEY = "FirstTime";
    private const string LAST_LEVEL_COMPLETED_KEY = "LastLevelCompleted";
    private const string HIGHEST_LEVEL_UNLOCKED_KEY = "HighestLevelUnlocked";

    // Keys cho Checkpoint
    private const string CP_SCENE_KEY = "CP_Scene";
    private const string CP_X_KEY = "CP_X";
    private const string CP_Y_KEY = "CP_Y";
    private const string CP_HEALTH_KEY = "CP_Health";
    private const string HAS_CP_KEY = "HasCheckpoint";
    
    // Keys for World State
    private const string CP_DEAD_ENEMIES_KEY = "CP_DeadEnemies";
    private const string CP_PICKED_ITEMS_KEY = "CP_PickedItems";

    private System.Collections.Generic.HashSet<string> sessionDeadEnemies = new System.Collections.Generic.HashSet<string>();
    private System.Collections.Generic.HashSet<string> sessionPickedItems = new System.Collections.Generic.HashSet<string>();

    // Runtime state (không lưu) để báo hiệu cho scene mới load biết cần restore checkpoint
    public bool ShouldLoadFromCheckpoint { get; set; } = false;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Khởi tạo tiến trình lần đầu nếu chưa có
        if (!PlayerPrefs.HasKey(FIRST_TIME_KEY))
        {
            InitializeFirstTime();
        }
    }

    /// <summary>
    /// Khởi tạo tiến trình lần đầu chơi
    /// </summary>
    private void InitializeFirstTime()
    {
        PlayerPrefs.SetInt(FIRST_TIME_KEY, 0); // Đánh dấu đã chơi rồi
        PlayerPrefs.SetInt(LAST_LEVEL_COMPLETED_KEY, 0); // Chưa hoàn thành level nào
        PlayerPrefs.SetInt(HIGHEST_LEVEL_UNLOCKED_KEY, 1); // Level 1 unlock ban đầu
        PlayerPrefs.Save();
        
        Debug.Log("[GameProgressManager] Game lần đầu - khởi tạo tiến trình");
    }

    /// <summary>
    /// Kiểm tra xem có phải lần đầu chơi không
    /// </summary>
    public bool IsFirstTime()
    {
        return !PlayerPrefs.HasKey(FIRST_TIME_KEY);
    }

    /// <summary>
    /// Lấy level cao nhất đã unlock
    /// </summary>
    public int GetHighestLevelUnlocked()
    {
        if (!PlayerPrefs.HasKey(HIGHEST_LEVEL_UNLOCKED_KEY))
        {
            return 1; // Mặc định level 1 unlock
        }
        return PlayerPrefs.GetInt(HIGHEST_LEVEL_UNLOCKED_KEY);
    }

    /// <summary>
    /// Lấy level cuối cùng đã hoàn thành
    /// </summary>
    public int GetLastLevelCompleted()
    {
        return PlayerPrefs.GetInt(LAST_LEVEL_COMPLETED_KEY, 0);
    }

    /// <summary>
    /// Kiểm tra level có unlock không
    /// </summary>
    public bool IsLevelUnlocked(int levelIndex)
    {
        return levelIndex <= GetHighestLevelUnlocked();
    }

    /// <summary>
    /// Hoàn thành một level - unlock level tiếp theo
    /// </summary>
    public void CompleteLevel(int levelIndex)
    {
        int currentHighest = GetHighestLevelUnlocked();
        
        // Lưu lại level đã hoàn thành
        PlayerPrefs.SetInt(LAST_LEVEL_COMPLETED_KEY, levelIndex);
        
        // Nếu hoàn thành level và nó là level cao nhất, unlock level tiếp theo
        if (levelIndex >= currentHighest && levelIndex < totalLevels)
        {
            PlayerPrefs.SetInt(HIGHEST_LEVEL_UNLOCKED_KEY, levelIndex + 1);
            Debug.Log($"[GameProgressManager] Level {levelIndex} hoàn thành! Unlock Level {levelIndex + 1}");
        }

        // Khi hoàn thành level, ta nên xóa checkpoint của level đó
        ClearCheckpoint();
        
        PlayerPrefs.Save();
    }

    // ================= CHECKPOINT SYSTEM =================

    public void SaveCheckpoint(int sceneIndex, Vector3 position, int health)
    {
        PlayerPrefs.SetInt(CP_SCENE_KEY, sceneIndex);
        PlayerPrefs.SetFloat(CP_X_KEY, position.x);
        PlayerPrefs.SetFloat(CP_Y_KEY, position.y);
        PlayerPrefs.SetInt(CP_HEALTH_KEY, health);
        PlayerPrefs.SetInt(HAS_CP_KEY, 1);

        // Save sets
        string deadEnemiesStr = string.Join(";", sessionDeadEnemies);
        string pickedItemsStr = string.Join(";", sessionPickedItems);
        PlayerPrefs.SetString(CP_DEAD_ENEMIES_KEY, deadEnemiesStr);
        PlayerPrefs.SetString(CP_PICKED_ITEMS_KEY, pickedItemsStr);

        PlayerPrefs.Save();

        Debug.Log($"[GameProgressManager] Saved Checkpoint at {position} in Scene {sceneIndex} with {sessionDeadEnemies.Count} dead enemies and {sessionPickedItems.Count} picked items.");
    }

    public void LoadCheckpointState()
    {
        sessionDeadEnemies.Clear();
        sessionPickedItems.Clear();

        string deadEnemiesStr = PlayerPrefs.GetString(CP_DEAD_ENEMIES_KEY, "");
        if (!string.IsNullOrEmpty(deadEnemiesStr))
        {
            string[] ids = deadEnemiesStr.Split(';');
            foreach (string id in ids) if(!string.IsNullOrEmpty(id)) sessionDeadEnemies.Add(id);
        }

        string pickedItemsStr = PlayerPrefs.GetString(CP_PICKED_ITEMS_KEY, "");
        if (!string.IsNullOrEmpty(pickedItemsStr))
        {
            string[] ids = pickedItemsStr.Split(';');
            foreach (string id in ids) if(!string.IsNullOrEmpty(id)) sessionPickedItems.Add(id);
        }
        
        Debug.Log($"[GameProgressManager] Loaded Checkpoint State: {sessionDeadEnemies.Count} dead enemies, {sessionPickedItems.Count} picked items.");
    }

    public void ClearSessionState()
    {
        sessionDeadEnemies.Clear();
        sessionPickedItems.Clear();
        Debug.Log("[GameProgressManager] Session state cleared.");
    }

    public void MarkEnemyDead(string id)
    {
        if (!string.IsNullOrEmpty(id) && !sessionDeadEnemies.Contains(id))
        {
            sessionDeadEnemies.Add(id);
        }
    }

    public bool IsEnemyDead(string id)
    {
        return sessionDeadEnemies.Contains(id);
    }

    public void MarkItemPicked(string id)
    {
        if (!string.IsNullOrEmpty(id) && !sessionPickedItems.Contains(id))
        {
            sessionPickedItems.Add(id);
        }
    }

    public bool IsItemPicked(string id)
    {
        return sessionPickedItems.Contains(id);
    }

    public bool HasCheckpoint()
    {
        return PlayerPrefs.GetInt(HAS_CP_KEY, 0) == 1;
    }

    public int GetCheckpointSceneIndex()
    {
        return PlayerPrefs.GetInt(CP_SCENE_KEY, -1);
    }

    public Vector3 GetCheckpointPosition()
    {
        float x = PlayerPrefs.GetFloat(CP_X_KEY, 0);
        float y = PlayerPrefs.GetFloat(CP_Y_KEY, 0);
        return new Vector3(x, y, 0);
    }

    public int GetCheckpointHealth()
    {
        return PlayerPrefs.GetInt(CP_HEALTH_KEY, 100);
    }

    public void ClearCheckpoint()
    {
        PlayerPrefs.DeleteKey(HAS_CP_KEY);
        PlayerPrefs.DeleteKey(CP_DEAD_ENEMIES_KEY);
        PlayerPrefs.DeleteKey(CP_PICKED_ITEMS_KEY);
        PlayerPrefs.Save();
        Debug.Log("[GameProgressManager] Checkpoint cleared");
    }

    /// <summary>
    /// Continue Logic: Load scene và báo hiệu cần restore state
    /// </summary>
    public void ContinueGame()
    {
        if (HasCheckpoint())
        {
            int sceneIndex = GetCheckpointSceneIndex();
            // Đặt flag để GameManager biết cần restore vị trí
            ShouldLoadFromCheckpoint = true;
            Time.timeScale = 1f; 
            Debug.Log($"[GameProgressManager] Continuing game from Checkpoint in Scene {sceneIndex}...");
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
        }
        else
        {
            Debug.LogWarning("[GameProgressManager] Cannot continue: No checkpoint found!");
        }
    }

    /// <summary>
    /// Reset toàn bộ tiến trình (để test)
    /// </summary>
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(FIRST_TIME_KEY);
        PlayerPrefs.DeleteKey(LAST_LEVEL_COMPLETED_KEY);
        PlayerPrefs.DeleteKey(HIGHEST_LEVEL_UNLOCKED_KEY);
        ClearCheckpoint();
        ClearSessionState();
        
        // Clear Shop Data
        PlayerPrefs.DeleteKey("Coins");
        PlayerPrefs.DeleteKey("HealthUpgrades");
        PlayerPrefs.DeleteKey("RunUnlocked");
        PlayerPrefs.DeleteKey("ArcheryUnlocked");

        PlayerPrefs.Save();
        
        Debug.Log("[GameProgressManager] Đã reset toàn bộ tiến trình!");
    }

    /// <summary>
    /// In thông tin tiến trình hiện tại
    /// </summary>
    public void DebugPrintProgress()
    {
        Debug.Log($"[GameProgressManager] Lần đầu: {IsFirstTime()}");
        Debug.Log($"[GameProgressManager] Level cao nhất unlock: {GetHighestLevelUnlocked()}");
        Debug.Log($"[GameProgressManager] Level cuối cùng hoàn thành: {GetLastLevelCompleted()}");
    }
}
