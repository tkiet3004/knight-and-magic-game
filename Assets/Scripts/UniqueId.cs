using UnityEngine;

public class UniqueId : MonoBehaviour
{
    [Tooltip("Unique ID for this object. If empty, it will be generated based on name and position.")]
    [SerializeField] private string _id;

    public string Id
    {
        get
        {
            if (string.IsNullOrEmpty(_id))
            {
                _id = GenerateId();
            }
            return _id;
        }
    }

    private void Awake()
    {
        if (string.IsNullOrEmpty(_id))
        {
            _id = GenerateId();
        }
    }

    private void Start()
    {
        // Self-Check for persistence
        if (GameProgressManager.Instance != null && !string.IsNullOrEmpty(Id))
        {
            // Check if this is an enemy that is dead
            if (GameProgressManager.Instance.IsEnemyDead(Id))
            {
                Destroy(gameObject);
                return;
            }

            // Check if this is a pickup that is picked (only if not handled by Pickup script)
            // Note: HealthPickup handles its own check, but double check doesn't hurt if we use different lists
            // Here we assume "Enemy" logic is the main missing piece.
        }
    }

    private string GenerateId()
    {
        // Generate a deterministic ID based on name and initial position
        // This ensures the ID remains the same across scene reloads
        // formatted to 1 decimal place to avoid tiny floating point differences
        return $"{gameObject.name}_{transform.position.x:F1}_{transform.position.y:F1}_{transform.position.z:F1}";
    }

    [ContextMenu("Generate ID")]
    private void GenerateGuid()
    {
        _id = System.Guid.NewGuid().ToString();
    }
}
