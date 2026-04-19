using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý level progression - chuyển màn khi diệt hết quái
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Completion Settings")]
    [SerializeField] private GameObject levelCompleteUI;
    [SerializeField] private CanvasGroup levelCompleteCanvasGroup;
    [SerializeField] private GameObject pauseButton; // Ẩn khi level complete
    
    [Header("Enemy Tracking")]
    [Tooltip("Chọn cách tracking: Manual = dùng EnemyTracker component, Auto = tìm theo layer")]
    [SerializeField] private bool useManualTracking = true;
    [SerializeField] private LayerMask enemyLayer; // Dùng khi auto tracking
    [SerializeField] private bool trackEnemiesByLayer = true;
    [SerializeField] private string enemyTag = "Enemy"; // Backup: dùng tag nếu không dùng layer
    
    [Header("Animation Settings")]
    [SerializeField] private float delayBeforeUI = 1.5f; // Delay trước khi hiện UI
    [SerializeField] private float fadeInDuration = 1.5f; // Thời gian fade in
    [SerializeField] private float autoNextLevelDelay = 3f; // Tự động chuyển màn sau x giây (0 = tắt)

    private int totalEnemies = 0;
    private int enemiesKilled = 0;
    private bool levelCompleted = false;
    private System.Collections.Generic.List<EnemyTracker> registeredEnemies = new System.Collections.Generic.List<EnemyTracker>();
    // List to track enemies found via auto-detection (CountEnemies)
    private System.Collections.Generic.List<Damageable> autoTrackedEnemies = new System.Collections.Generic.List<Damageable>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Ẩn UI level complete ban đầu
        if (levelCompleteUI != null)
        {
            levelCompleteUI.SetActive(false);
        }

        // Setup canvas group
        if (levelCompleteUI != null && levelCompleteCanvasGroup == null)
        {
            levelCompleteCanvasGroup = levelCompleteUI.GetComponent<CanvasGroup>();
            if (levelCompleteCanvasGroup == null)
            {
                levelCompleteCanvasGroup = levelCompleteUI.AddComponent<CanvasGroup>();
            }
        }

        if (levelCompleteCanvasGroup != null)
        {
            levelCompleteCanvasGroup.alpha = 0f;
        }
    }

    private void Start()
    {
        // Chỉ đếm tự động nếu không dùng manual tracking
        if (!useManualTracking)
        {
            CountEnemies();
            // Đăng ký sự kiện khi enemy chết
            CharacterEvents.characterDamaged += OnCharacterDamaged;
        }
        else
        {
            Debug.Log("Using manual enemy tracking via EnemyTracker components");
        }
    }

    private void OnDestroy()
    {
        if (!useManualTracking)
        {
            CharacterEvents.characterDamaged -= OnCharacterDamaged;
        }
    }

    private void CountEnemies()
    {
        if (trackEnemiesByLayer)
        {
            // Tìm tất cả GameObject có component Damageable và nằm trên enemy layer
            Damageable[] allDamageables = FindObjectsByType<Damageable>(FindObjectsSortMode.None);
            
            foreach (Damageable damageable in allDamageables)
            {
                // Kiểm tra xem object có nằm trên enemy layer không
                if (((1 << damageable.gameObject.layer) & enemyLayer) != 0)
                {
                    totalEnemies++;
                    autoTrackedEnemies.Add(damageable);
                }
            }
        }
        else
        {
            // Backup: dùng tag
            GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
            totalEnemies = enemies.Length;
            foreach(GameObject go in enemies) {
                 Damageable d = go.GetComponent<Damageable>();
                 if(d != null) autoTrackedEnemies.Add(d);
            }
        }

        Debug.Log($"Level started with {totalEnemies} enemies");
    }

    private void OnCharacterDamaged(GameObject character, int damage)
    {
        // Kiểm tra xem character có phải enemy không
        bool isEnemy = false;
        
        if (trackEnemiesByLayer)
        {
            isEnemy = ((1 << character.layer) & enemyLayer) != 0;
        }
        else
        {
            isEnemy = character.CompareTag(enemyTag);
        }

        if (isEnemy)
        {
            Damageable damageable = character.GetComponent<Damageable>();
            if (damageable != null && !damageable.IsAlive)
            {
                OnEnemyKilled(character);
            }
        }
    }

    private void OnEnemyKilled(GameObject enemyObj = null)
    {
        enemiesKilled++;
        
        // Calculate and log enemies left
        int enemiesLeft = Mathf.Max(0, totalEnemies - enemiesKilled);
        string enemyName = (enemyObj != null) ? enemyObj.name : "Unknown Enemy";
        Debug.Log($"[LevelManager] ⚔️ Enemy Defeated: '{enemyName}'. Remaining: {enemiesLeft} / {totalEnemies}");

        // Mark persistence
        if (enemyObj != null)
        {
            var uid = enemyObj.GetComponent<UniqueId>();
            if (uid != null && GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.MarkEnemyDead(uid.Id);
            }
        }

        // Kiểm tra xem đã diệt hết chưa
        if (enemiesKilled >= totalEnemies && totalEnemies > 0 && !levelCompleted)
        {
            OnLevelComplete();
        }
    }

    private void OnLevelComplete()
    {
        if (levelCompleted) return;

        levelCompleted = true;
        Debug.Log("Level Complete! All enemies defeated!");

        // Lưu tiến trình
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        // Scene index: 0=Main Menu, 1=Tutorial, 3,4,5 = Level 1,2,3
        int levelIndex = currentSceneIndex - 2; // Tutorial là scene 1, Level 1 là scene 3 => levelIndex 1
        
        if (levelIndex > 0)
        {
            GameProgressManager.Instance.CompleteLevel(levelIndex);
        }

        StartCoroutine(LevelCompleteSequence());
    }

    private IEnumerator LevelCompleteSequence()
    {
        // Chờ một chút để player thấy enemy cuối cùng chết
        yield return new WaitForSeconds(delayBeforeUI);

        // Ẩn pause button
        if (pauseButton != null)
        {
            pauseButton.SetActive(false);
        }

        // Hiển thị UI (trong suốt)
        if (levelCompleteUI != null)
        {
            levelCompleteUI.SetActive(true);
        }

        // Fade in UI
        if (levelCompleteCanvasGroup != null)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                levelCompleteCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
                yield return null;
            }

            levelCompleteCanvasGroup.alpha = 1f;
        }

        // Tự động chuyển màn nếu được bật
        if (autoNextLevelDelay > 0)
        {
            yield return new WaitForSeconds(autoNextLevelDelay);
            LoadNextLevel();
        }
    }

    // Các hàm cho UI buttons
    public void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // Kiểm tra xem có scene tiếp theo không
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("No more levels! This is the last level.");
            // Có thể quay về main menu hoặc hiện credits
            ReturnToMainMenu();
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;

        // Ensure fresh start
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.ClearSessionState();
            GameProgressManager.Instance.ShouldLoadFromCheckpoint = false;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Scene 0 là main menu
    }

    // Debug: Force complete level (test)
    [ContextMenu("Force Complete Level")]
    public void ForceCompleteLevel()
    {
        OnLevelComplete();
    }

    // Debug: Check enemy count
    [ContextMenu("Log Enemy Count")]
    public void LogEnemyCount()
    {
        Debug.Log($"Enemies: {enemiesKilled}/{totalEnemies}");
    }

    // === MANUAL TRACKING METHODS ===
    // Được gọi bởi EnemyTracker khi enemy spawn
    public void RegisterEnemy(EnemyTracker enemy)
    {
        if (!registeredEnemies.Contains(enemy))
        {
            registeredEnemies.Add(enemy);
            totalEnemies++;
            Debug.Log($"Enemy registered. Total: {totalEnemies}");
        }
    }

    // Được gọi bởi EnemyTracker khi enemy bị destroy
    public void UnregisterEnemy(EnemyTracker enemy, bool wasKilled)
    {
        if (registeredEnemies.Contains(enemy))
        {
            registeredEnemies.Remove(enemy);
            
            if (wasKilled)
            {
                OnEnemyKilled(enemy != null ? enemy.gameObject : null);
            }
        }
    }

    [Header("Fall Detection")]
    [Tooltip("Global check: Vị trí Y mà enemy sẽ bị giết nếu rơi xuống thấp hơn")]
    [SerializeField] private float enemyFallDeathY = -20f;

    private void Update()
    {
        // Debug loop to check if we are tracking enemies
        if (registeredEnemies.Count > 0 && Time.frameCount % 120 == 0) // Low frequency check (2s)
        {
            Debug.Log($"[LevelManager] Debug: Tracking {registeredEnemies.Count} enemies active in scene.");
        }

        // Kiểm tra và xử lý enemies bị rơi khỏi map
        // Duyệt ngược để an toàn nếu list thay đổi (mặc dù kill không remove ngay lập tức)
        for (int i = registeredEnemies.Count - 1; i >= 0; i--)
        {
            EnemyTracker enemy = registeredEnemies[i];
            
            // Check null (object destroyed)
            if (enemy != null)
            {
                // Debug log specific to falling enemies to verify coordinates
                if (enemy.transform.position.y < -10f && enemy.transform.position.y > enemyFallDeathY)
                {
                     // Print occasionally to avoid spam
                     if (Time.frameCount % 60 == 0)
                     {
                        Debug.Log($"[LevelManager] Enemy {enemy.name} is falling... Y={enemy.transform.position.y:F2}");
                     }
                }

                if (enemy.transform.position.y < enemyFallDeathY)
                {
                    // Enemy đã rơi quá thấp
                    Debug.Log($"[LevelManager] ☠️ FORCE KILL: Enemy {enemy.name} fell to Y={enemy.transform.position.y} (Threshold: {enemyFallDeathY})");
                    
                    Damageable d = enemy.GetComponent<Damageable>();
                    if (d != null)
                    {
                        // Ensure it's dead before destroying
                        if (d.IsAlive)
                        {
                            d.IsAlive = false; // Force death status
                            Debug.Log($"[LevelManager] Set IsAlive=false for {enemy.name}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[LevelManager] Enemy {enemy.name} has no Damageable component!");
                    }
                    
                    // Force destroy to ensure UnregisterEnemy is called and death is counted
                    Destroy(enemy.gameObject);
                }
            }
        }

        // Check Auto-Tracked Enemies
        for (int i = autoTrackedEnemies.Count - 1; i >= 0; i--)
        {
            Damageable enemy = autoTrackedEnemies[i];
             
             // Check if object is destroyed (Unity overrides == null)
             if (enemy != null && enemy.gameObject != null)
             {
                 if (enemy.transform.position.y < enemyFallDeathY)
                 {
                     Debug.Log($"[LevelManager] ☠️ AUTO-TRACK KILL: Enemy {enemy.name} fell to Y={enemy.transform.position.y}");
                     
                     // Handle Logic
                     if (enemy.IsAlive)
                     {
                         enemy.IsAlive = false;
                         // Manually call OnEnemyKilled because there's no EnemyTracker to do it on Destroy
                         OnEnemyKilled(enemy.gameObject);
                     }
                     
                     Destroy(enemy.gameObject);
                 }
             }
             else
             {
                 // Cleanup null references
                 autoTrackedEnemies.RemoveAt(i);
             }
        }
    }
}
