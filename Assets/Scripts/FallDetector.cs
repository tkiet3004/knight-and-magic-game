using UnityEngine;

/// <summary>
/// Component này phát hiện khi character (player hoặc enemy) rơi tự do quá lâu
/// Gắn vào Player hoặc Enemy GameObject
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class FallDetector : MonoBehaviour
{
    [Header("Character Type")]
    [Tooltip("Đây có phải là Player không? (nếu false thì là Enemy)")]
    [SerializeField] private bool isPlayer = true;
    
    [Header("Fall Detection Settings")]
    [Tooltip("Thời gian rơi liên tục trước khi chết (giây)")]
    [SerializeField] private float maxFallTime = 5f;
    
    [Tooltip("Vận tốc Y tối thiểu để coi là đang rơi")]
    [SerializeField] private float fallingVelocityThreshold = -10f;
    
    [Tooltip("Vị trí Y tối thiểu trước khi chết")]
    [SerializeField] private float minYPosition = -20f;

    private Rigidbody2D rb;
    private TouchingDirections touchingDirections;
    private Damageable damageable;
    private float currentFallTime = 0f;
    private bool isFalling = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        touchingDirections = GetComponent<TouchingDirections>();
        damageable = GetComponent<Damageable>();
        
        // Auto detect: Chắc chắn rằng chỉ có object tag "Player" mới được tính là Player
        // Ghi đè giá trị inspector nếu cần thiết để fix lỗi enemies bị tính là player
        isPlayer = CompareTag("Player");
    }

    private void Update()
    {
        // Không kiểm tra nếu đã chết
        if (damageable != null && !damageable.IsAlive)
        {
            this.enabled = false;
            return;
        }
        
        // Kiểm tra xem có đang rơi không
        // Nếu không có TouchingDirections, chỉ dựa vào velocity
        bool isGrounded = touchingDirections != null ? touchingDirections.IsGrounded : false;
        bool isCurrentlyFalling = !isGrounded && rb.linearVelocity.y < fallingVelocityThreshold;

        if (isCurrentlyFalling)
        {
            if (!isFalling)
            {
                // Bắt đầu rơi
                isFalling = true;
                currentFallTime = 0f;
            }

            currentFallTime += Time.deltaTime;

            // Kiểm tra rơi quá lâu
            if (currentFallTime >= maxFallTime)
            {
                OnFellTooLong();
            }
        }
        else
        {
            // Đã chạm đất hoặc không rơi nữa
            if (isFalling)
            {
                isFalling = false;
                currentFallTime = 0f;
            }
        }

        // Kiểm tra rơi xuống dưới map
        if (transform.position.y < minYPosition)
        {
            OnFellOutOfMap();
        }
    }

    private void OnFellTooLong()
    {
        string characterType = isPlayer ? "Player" : "Enemy";
        Debug.Log($"{characterType} {gameObject.name} rơi tự do quá lâu!");
        
        if (isPlayer)
        {
            // Player game over
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver("Rơi tự do quá lâu!");
            }
        }
        else
        {
            // Enemy chết
            KillCharacter();
        }
        
        // Vô hiệu hóa component này để không trigger nhiều lần
        this.enabled = false;
    }

    private void OnFellOutOfMap()
    {
        string characterType = isPlayer ? "Player" : "Enemy";
        Debug.Log($"{characterType} {gameObject.name} rơi ra ngoài map!");
        
        if (isPlayer)
        {
            // Player game over
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver("Rơi xuống hố!");
            }
        }
        else
        {
            // Enemy chết
            KillCharacter();
        }
        
        // Vô hiệu hóa component này
        this.enabled = false;
    }

    private void KillCharacter()
    {
        // Giết character bằng cách set health = 0
        if (damageable != null && damageable.IsAlive)
        {
            // Gây damage lớn để chết chắc chắn
            damageable.Hit(9999, Vector2.zero);
        }
    }

    // Hiển thị debug info trong Scene view
    private void OnDrawGizmosSelected()
    {
        // Vẽ đường giới hạn Y tối thiểu
        Gizmos.color = Color.red;
        Vector3 leftPoint = new Vector3(-100, minYPosition, 0);
        Vector3 rightPoint = new Vector3(100, minYPosition, 0);
        Gizmos.DrawLine(leftPoint, rightPoint);

        // Hiển thị thông tin fall time
        if (isFalling)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}
