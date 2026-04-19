using UnityEngine;

/// <summary>
/// Các chức năng debug và testing - Xóa dữ liệu, cheat, v.v.
/// </summary>
public class DebugManager : MonoBehaviour
{
    [Header("Debug Settings")]
    [Tooltip("Nhấn phím này để xóa TOÀN BỘ dữ liệu game")]
    public KeyCode clearAllDataKey = KeyCode.F10;

    [Tooltip("Nhấn phím này để xóa CHỈ tiến trình level")]
    public KeyCode clearProgressKey = KeyCode.F9;

    [Tooltip("Nhấn phím này để xóa CHỈ tiền và upgrades")]
    public KeyCode clearShopDataKey = KeyCode.F8;

    [Tooltip("Hiện thông báo khi xóa dữ liệu")]
    public bool showDebugMessages = true;

    private void Update()
    {
        // Xóa TOÀN BỘ dữ liệu
        if (Input.GetKeyDown(clearAllDataKey))
        {
            ClearAllData();
        }

        // Xóa tiến trình level
        if (Input.GetKeyDown(clearProgressKey))
        {
            ClearProgressData();
        }

        // Xóa tiền và upgrades
        if (Input.GetKeyDown(clearShopDataKey))
        {
            ClearShopData();
        }
    }

    /// <summary>
    /// Xóa TOÀN BỘ dữ liệu game
    /// </summary>
    public void ClearAllData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        if (showDebugMessages)
        {
            Debug.Log("✅ [DEBUG] Đã xóa TOÀN BỘ dữ liệu game!");
            Debug.Log("    - Tiến trình level");
            Debug.Log("    - Tiền coins");
            Debug.Log("    - Upgrades");
            Debug.Log("    - Checkpoint");
            Debug.Log("    - Cài đặt âm thanh");
            Debug.Log("    => Game đã về trạng thái ban đầu!");
        }
    }

    /// <summary>
    /// Xóa CHỈ dữ liệu tiến trình level
    /// </summary>
    public void ClearProgressData()
    {
        // Keys từ GameProgressManager
        PlayerPrefs.DeleteKey("FirstTime");
        PlayerPrefs.DeleteKey("LastLevelCompleted");
        PlayerPrefs.DeleteKey("HighestLevelUnlocked");
        
        // Checkpoint keys
        PlayerPrefs.DeleteKey("CP_Scene");
        PlayerPrefs.DeleteKey("CP_X");
        PlayerPrefs.DeleteKey("CP_Y");
        PlayerPrefs.DeleteKey("CP_Health");
        PlayerPrefs.DeleteKey("HasCheckpoint");
        PlayerPrefs.DeleteKey("CP_DeadEnemies");
        PlayerPrefs.DeleteKey("CP_PickedItems");

        PlayerPrefs.Save();

        if (showDebugMessages)
        {
            Debug.Log("✅ [DEBUG] Đã xóa dữ liệu tiến trình level!");
            Debug.Log("    => Chỉ giữ lại: Tiền, Upgrades, Settings");
        }
    }

    /// <summary>
    /// Xóa CHỈ dữ liệu shop (tiền và upgrades)
    /// </summary>
    public void ClearShopData()
    {
        // Keys từ ShopManager
        PlayerPrefs.DeleteKey("Coins");
        PlayerPrefs.DeleteKey("HealthUpgrades");
        PlayerPrefs.DeleteKey("SpeedLevel");
        PlayerPrefs.DeleteKey("DamageLevel");

        PlayerPrefs.Save();

        if (showDebugMessages)
        {
            Debug.Log("✅ [DEBUG] Đã xóa dữ liệu shop!");
            Debug.Log("    => Chỉ giữ lại: Tiến trình level, Settings");
        }
    }

    /// <summary>
    /// Cheat: Thêm tiền
    /// </summary>
    public void AddCoins(int amount)
    {
        int currentCoins = PlayerPrefs.GetInt("Coins", 0);
        PlayerPrefs.SetInt("Coins", currentCoins + amount);
        PlayerPrefs.Save();

        if (showDebugMessages)
        {
            Debug.Log($"✅ [DEBUG] Đã thêm {amount} coins! Tổng: {currentCoins + amount}");
        }
    }

    /// <summary>
    /// Cheat: Unlock tất cả level
    /// </summary>
    public void UnlockAllLevels()
    {
        PlayerPrefs.SetInt("HighestLevelUnlocked", 99);
        PlayerPrefs.Save();

        if (showDebugMessages)
        {
            Debug.Log("✅ [DEBUG] Đã unlock tất cả level!");
        }
    }

    // ============================================
    // Các hàm tiện ích để gọi từ Editor hoặc UI
    // ============================================

    [ContextMenu("Clear All Data")]
    private void ContextMenu_ClearAll()
    {
        ClearAllData();
    }

    [ContextMenu("Clear Progress Only")]
    private void ContextMenu_ClearProgress()
    {
        ClearProgressData();
    }

    [ContextMenu("Clear Shop Data Only")]
    private void ContextMenu_ClearShop()
    {
        ClearShopData();
    }

    [ContextMenu("Add 1000 Coins")]
    private void ContextMenu_AddCoins()
    {
        AddCoins(1000);
    }

    [ContextMenu("Unlock All Levels")]
    private void ContextMenu_UnlockAll()
    {
        UnlockAllLevels();
    }
}
