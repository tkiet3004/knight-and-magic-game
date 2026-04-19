using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class UIManager : MonoBehaviour
{
    public GameObject damageTextPrefab;
    public GameObject healthTextPrefab;

    public Canvas gameCanvas;

    private void Awake()
    {
        gameCanvas = FindFirstObjectByType<Canvas>();
    }

    public TMP_Text coinText;
    public Sprite coinIconSprite; // Assign in Inspector

    private void Start()
    {
        if (coinText == null && gameCanvas != null)
        {
            // Create Coin HUD Container
            GameObject coinContainer = new GameObject("CoinHUD");
            coinContainer.transform.SetParent(gameCanvas.transform, false);
            RectTransform containerRT = coinContainer.AddComponent<RectTransform>();
            
            // Position Bottom Right - Closer to corner
            containerRT.anchorMin = new Vector2(1, 0);
            containerRT.anchorMax = new Vector2(1, 0);
            containerRT.pivot = new Vector2(1, 0);
            containerRT.anchoredPosition = new Vector2(-20, 20); 
            containerRT.sizeDelta = new Vector2(150, 40);

            // Create Icon
            GameObject iconObj = new GameObject("CoinIcon");
            iconObj.transform.SetParent(coinContainer.transform, false);
            UnityEngine.UI.Image iconImg = iconObj.AddComponent<UnityEngine.UI.Image>();
            if (coinIconSprite != null) iconImg.sprite = coinIconSprite;
            
            RectTransform iconRT = iconObj.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0, 0.5f);
            iconRT.anchorMax = new Vector2(0, 0.5f);
            iconRT.pivot = new Vector2(0, 0.5f);
            iconRT.anchoredPosition = new Vector2(0, 0); 
            iconRT.sizeDelta = new Vector2(30, 30);

            // Create Text
            GameObject textObj = new GameObject("CoinCount");
            textObj.transform.SetParent(coinContainer.transform, false);
            coinText = textObj.AddComponent<TextMeshProUGUI>();
            
            coinText.fontSize = 24; 
            coinText.fontStyle = FontStyles.Bold; // Make Bold
            coinText.color = new Color(1f, 0.84f, 0f); // Gold color
            coinText.alignment = TextAlignmentOptions.Left;
            
            RectTransform textRT = textObj.GetComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0, 0);
            textRT.anchorMax = new Vector2(1, 1);
            textRT.pivot = new Vector2(0.5f, 0.5f);
            textRT.offsetMin = new Vector2(35, 0); // Offset for icon (30 + padding)
            textRT.offsetMax = new Vector2(0, 0);
        }

        UpdateCoinText(PlayerPrefs.GetInt("Coins", 0));
    }

    private void OnEnable()
    {
        CharacterEvents.characterDamaged += CharacterTookDamage;
        CharacterEvents.characterHealed += CharacterHealed;
        CharacterEvents.characterCoinCollected += OnCoinCollected;
    }

    private void OnDisable()
    {
        CharacterEvents.characterDamaged -= CharacterTookDamage;
        CharacterEvents.characterHealed -= CharacterHealed;
        CharacterEvents.characterCoinCollected -= OnCoinCollected;
    }

    private void OnCoinCollected(GameObject character, int amount)
    {
        int totalCoins = PlayerPrefs.GetInt("Coins", 0);
        UpdateCoinText(totalCoins);
    }

    private void UpdateCoinText(int amount)
    {
        if (coinText != null)
        {
            coinText.text = amount.ToString(); // Just the number
        }
    }

    public void CharacterTookDamage(GameObject character, int damageReceived)
    {
        // Don't show damage text if shielded
        ShieldEffect shield = character.GetComponent<ShieldEffect>();
        if (shield != null && shield.IsShieldActive())
        {
            return;
        }

        // Create text at character hit
        Vector3 spawnPosition = Camera.main.WorldToScreenPoint(character.transform.position);

        TMP_Text tmpText = Instantiate(damageTextPrefab, spawnPosition, Quaternion.identity, gameCanvas.transform)
            .GetComponent<TMP_Text>();

        tmpText.text = damageReceived.ToString();
    }

    public void CharacterHealed(GameObject character, int healthRestored)
    {
        // Create text at character hit
        Vector3 spawnPosition = Camera.main.WorldToScreenPoint(character.transform.position);

        TMP_Text tmpText = Instantiate(healthTextPrefab, spawnPosition, Quaternion.identity, gameCanvas.transform)
            .GetComponent<TMP_Text>();

        tmpText.text = healthRestored.ToString();
    }


}
