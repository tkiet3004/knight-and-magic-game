using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Over Settings")]
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private CanvasGroup gameOverCanvasGroup;
    [SerializeField] private GameObject pauseButton; // Nút pause cần ẩn khi game over
    [SerializeField] private float fallDeathThreshold = -20f; // Y position để coi là rơi xuống hố
    [SerializeField] private float fallVelocityThreshold = -30f; // Vận tốc rơi quá nhanh
    
    [Header("Animation Settings")]
    [SerializeField] private float delayBeforeUI = 2f; // Delay 2s để xem animation player chết
    [SerializeField] private float fadeInDuration = 2f; // Thời gian fade in của UI
    
    private bool isGameOver = false;
    private GameObject player;
    private Damageable playerDamageable;
    private Rigidbody2D playerRb;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Tìm player
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerDamageable = player.GetComponent<Damageable>();
            playerRb = player.GetComponent<Rigidbody2D>();
        }

        // Ẩn UI game over ban đầu
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }

        // Setup canvas group nếu chưa có
        if (gameOverUI != null && gameOverCanvasGroup == null)
        {
            gameOverCanvasGroup = gameOverUI.GetComponent<CanvasGroup>();
            if (gameOverCanvasGroup == null)
            {
                gameOverCanvasGroup = gameOverUI.AddComponent<CanvasGroup>();
            }
        }

        // Set alpha ban đầu = 0
        if (gameOverCanvasGroup != null)
        {
            gameOverCanvasGroup.alpha = 0f;
        }

        // IMPORTANT: Manage Session State
        // If we are NOT loading from a checkpoint, we should clear the transient "dead enemies" list.
        // If we ARE loading from a checkpoint, we must load the state NOW (in Awake) so it's ready for other scripts' Start().
        if (GameProgressManager.Instance != null)
        {
            if (GameProgressManager.Instance.ShouldLoadFromCheckpoint)
            {
                GameProgressManager.Instance.LoadCheckpointState();
            }
            // else
            // {
            //     // COMMENTED OUT: We don't want to clear session state here blindly. 
            //     // If we are just transitioning scenes or reloading, we might want to keep the state 
            //     // if we are NOT using the "save" system but just a transient session state.
            //     // However, for a "Retry" or "Start New Game", we SHOULD clear it.
            //     // But since Save/Load is manual, let's rely on Explicit Resets or Load.
                
            //     // GameProgressManager.Instance.ClearSessionState();
            // }
        }
    }

    private void Start()
    {
        // Check checkpoint load
        if (GameProgressManager.Instance != null && GameProgressManager.Instance.ShouldLoadFromCheckpoint)
        {
            RestoreCheckpointState();
            // Reset flag đã xử lý
            GameProgressManager.Instance.ShouldLoadFromCheckpoint = false;
        }
    }

    private void RestoreCheckpointState()
    {
        // State is already loaded in Awake
        
        Vector3 checkpointPos = GameProgressManager.Instance.GetCheckpointPosition();
        int health = GameProgressManager.Instance.GetCheckpointHealth();
        
        if (player != null)
        {
            // Set Position
            player.transform.position = checkpointPos;
            
            // Set Health
            if (playerDamageable != null)
            {
                playerDamageable.Health = health;
                playerDamageable.IsAlive = true; // Ensure alive
                
                // Reset animation/state if needed
                Animator anim = player.GetComponent<Animator>();
                if(anim)
                {
                    anim.SetBool(AnimationStrings.isAlive, true);
                    anim.Play("knight_idle"); // Force idle - name might vary, "knight_idle" is standard in this pack
                }
            }
            
            Debug.Log($"[GameManager] Restored Game State: Pos {checkpointPos}, Health {health}");
        }
    }

    private void OnEnable()
    {
        // Đăng ký sự kiện khi player chết
        if (playerDamageable != null)
        {
            playerDamageable.damageableDeath.AddListener(OnPlayerDeath);
        }
    }

    private void OnDisable()
    {
        // Hủy đăng ký sự kiện
        if (playerDamageable != null)
        {
            playerDamageable.damageableDeath.RemoveListener(OnPlayerDeath);
        }
    }

    private void Update()
    {
        if (isGameOver || player == null) return;

        // Kiểm tra rơi xuống hố (vị trí Y quá thấp)
        if (player.transform.position.y < fallDeathThreshold)
        {
            TriggerGameOver("Rơi xuống hố!");
        }

        // Kiểm tra rơi tự do quá nhanh (có thể bỏ nếu không cần)
        if (playerRb != null && playerRb.linearVelocity.y < fallVelocityThreshold)
        {
            TriggerGameOver("Rơi quá nhanh!");
        }
    }

    private void OnPlayerDeath()
    {
        TriggerGameOver("Hết máu!");
    }

    public void TriggerGameOver(string reason = "")
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log("Game Over: " + reason);

        // Ẩn pause button ngay lập tức
        if (pauseButton != null)
        {
            pauseButton.SetActive(false);
        }

        // KHÔNG dừng thời gian ngay, để animation player chết chạy
        // Bắt đầu coroutine để delay và fade in UI
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        // Chờ 2s để xem animation player chết
        yield return new WaitForSeconds(delayBeforeUI);

        // Hiển thị UI game over (nhưng vẫn trong suốt)
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }

        // Fade in UI trong 2s
        if (gameOverCanvasGroup != null)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.unscaledDeltaTime; // Dùng unscaledDeltaTime để hoạt động cả khi timeScale = 0
                gameOverCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
                yield return null;
            }

            gameOverCanvasGroup.alpha = 1f; // Đảm bảo alpha = 1 cuối cùng
        }

        // SAU KHI fade in xong mới dừng thời gian (hoặc không dừng nếu muốn player vẫn có thể di chuyển)
        Time.timeScale = 0f;
    }

    // Các hàm cho UI buttons
    // Các hàm cho UI buttons
    public void SaveGameManual()
    {
        if (player != null && playerDamageable != null && GameProgressManager.Instance != null && playerDamageable.IsAlive)
        {
            int currentScene = SceneManager.GetActiveScene().buildIndex;
            Vector3 pos = player.transform.position;
            int health = playerDamageable.Health;
            
            GameProgressManager.Instance.SaveCheckpoint(currentScene, pos, health);
            Debug.Log("[GameManager] Partial manual save complete.");
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        
        // Reset flag to ensure we don't accidentally load a checkpoint
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.ClearSessionState(); // Clear temporary memory for a fresh start
            GameProgressManager.Instance.ShouldLoadFromCheckpoint = false;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync(0); // Scene 0 là main menu
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
