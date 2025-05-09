using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

// Main class controlling the card matching game
public class GameManager : MonoBehaviour
{
    // Singleton instance for global access
    public static GameManager Instance { get; private set; }

    [Header("Card Configuration")]
    public GameObject cardPrefab;          // Prefab for each card
    public Sprite[] cardSprites;           // Array of images for matching
    public Vector2 gridSize = new Vector2(8, 8); // Grid dimension
    private float cardSize, spacing;       // Calculated size and spacing

    [Header("Gameplay Parameters")]
    public int score = 0;
    public int matchpoint = 100;
    public int mismatchPenalty = 10;
    public int comboMultiplier = 1;
    private int consecutiveMatches = 0;
    private int matchesFound = 0;
    public bool IsWaiting { get; private set; } // Lock during match check

    [Header("UI References")]
    public Text scoreText;
    public Text highScoreText;
    public Text ComboScore;
    public GameObject gameOverPanel;
    public Text completionText;


    public GameObject Panel;

    [Header("Audio Clips")]
    public AudioClip matchSound;
    public AudioClip mismatchSound;
    public AudioClip gameOverSound;
    private AudioSource audioSource;

    private List<Card> cards = new List<Card>();         // All cards in scene
    private List<Card> flippedCards = new List<Card>();  // Cards currently face-up

    [Header("Save System")]
    private const string SaveKey = "GameSave"; // PlayerPrefs key

    // Data structure to store save information
    [System.Serializable]
    class SaveData
    {
        public int score;
        public int comboMultiplier;
        public int matchesFound;
        public List<int> matchedCardIds;
    }

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Ensure audio source is present
        if (!TryGetComponent<AudioSource>(out audioSource))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Start()
    {
        ValidateSprites();
        CalculateCardDimensions();
        GenerateCardGrid();
        DisplayHighScore();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Delay load to ensure cards exist
        Invoke(nameof(LoadGame), 0.5f);
    }

    // Ensure number of sprites is even (pairs)
    void ValidateSprites()
    {
        if (cardSprites.Length % 2 != 0)
            Debug.LogError("You need an even number of sprites for pairing.");
    }

    // Calculate appropriate card size based on screen and grid
    void CalculateCardDimensions()
    {
        float screenHeight = Camera.main.orthographicSize * 2;
        float screenWidth = screenHeight * Camera.main.aspect;

        float maxCardWidth = (screenWidth * 0.9f) / gridSize.x;
        float maxCardHeight = (screenHeight * 0.8f) / gridSize.y;
        cardSize = Mathf.Min(maxCardWidth, maxCardHeight);
        spacing = cardSize * 0.15f;
    }

    // Spawn and lay out all cards in the grid
    void GenerateCardGrid()
    {
        foreach (var card in cards)
            Destroy(card.gameObject);
        cards.Clear();

        int totalCards = (int)(gridSize.x * gridSize.y);
        if (totalCards % 2 != 0)
        {
            Debug.LogError("Grid must have an even number of cards.");
            gridSize = new Vector2(4, 4);
            totalCards = 16;
        }

        // Generate card pairs and shuffle
        List<int> cardIds = new List<int>();
        for (int i = 0; i < totalCards / 2; i++)
        {
            cardIds.Add(i);
            cardIds.Add(i);
        }

        cardIds = ShuffleList(cardIds);

        Vector2 startPos = new Vector2(
            -((gridSize.x - 1) * (cardSize + spacing)) / 2,
            -((gridSize.y - 1) * (cardSize + spacing)) / 2
        );

        for (int i = 0; i < cardIds.Count; i++)
        {
            GameObject newCard = Instantiate(cardPrefab, transform);
            newCard.transform.localScale = Vector3.one * cardSize;

            int row = i / (int)gridSize.x;
            int col = i % (int)gridSize.x;
            newCard.transform.position = new Vector2(
                startPos.x + col * (cardSize + spacing),
                startPos.y + row * (cardSize + spacing)
            );

            Card card = newCard.GetComponent<Card>();
            card.SetCard(cardSprites[cardIds[i]], cardIds[i]);
            cards.Add(card);
        }
    }

    // Randomize card order
    List<T> ShuffleList<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T temp = list[k];
            list[k] = list[n];
            list[n] = temp;
        }
        return list;
    }

    // Handle card click from Card script
    public void CardClicked(Card card)
    {
        if (IsWaiting || flippedCards.Contains(card) || card.IsFlipped)
            return;

        flippedCards.Add(card);

        if (flippedCards.Count >= 2)
            StartCoroutine(CheckMatch());
    }

    // Coroutine to handle matching logic
    IEnumerator CheckMatch()
    {
        IsWaiting = true;
        yield return new WaitForSeconds(0.5f);

        while (flippedCards.Count >= 2)
        {
            Card a = flippedCards[0];
            Card b = flippedCards[1];
            flippedCards.RemoveRange(0, 2);

            bool match = a.CardId == b.CardId;

            if (match)
            {
                score += matchpoint * comboMultiplier;
                consecutiveMatches++;
                matchesFound++;
                PlaySound(matchSound);

                if (consecutiveMatches >= 2)
                    comboMultiplier = Mathf.Min(comboMultiplier + 1, 3);

                a.DisableCard();
                b.DisableCard();
            }
            else
            {
                score -= mismatchPenalty;
                consecutiveMatches = 0;
                comboMultiplier = 1;
                PlaySound(mismatchSound);

                a.HideCard();
                b.HideCard();
            }

            UpdateScoreUI();
        }

        IsWaiting = false;

        if (matchesFound == (gridSize.x * gridSize.y) / 2)
        {
            PlaySound(gameOverSound);
            CheckHighScore();
            ShowGameComplete();
        }

        SaveGame();
    }

    // Play sound clips
    public void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    // Refresh score display
    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
        ComboScore.text = "Combo: x" + comboMultiplier;
    }

    // Show stored high score
    void DisplayHighScore()
    {
        if (highScoreText != null)
            highScoreText.text = "High Score: " + PlayerPrefs.GetInt("HighScore", 0);
    }

    // Save new high score
    void CheckHighScore()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (score > highScore)
        {
            PlayerPrefs.SetInt("HighScore", score);
            PlayerPrefs.Save();
            DisplayHighScore();
        }
    }

    // End game screen
    void ShowGameComplete()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            completionText.text = "Game Completed! Press Restart to Play again.";

            Panel.SetActive(false);
        }

        PlayerPrefs.DeleteKey(SaveKey); // Clear saved game data

    }

    // Reset all game values and cards
    public void ResetGame()
    {
        PlayerPrefs.DeleteKey(SaveKey);

        score = 0;
        comboMultiplier = 1;
        consecutiveMatches = 0;
        matchesFound = 0;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        Panel.SetActive(true);
        GenerateCardGrid();
        UpdateScoreUI();
    }

    // Exit game/application
    public void GameQuit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Save current progress to PlayerPrefs
    void SaveGame()
    {
        List<int> matched = new List<int>();
        foreach (Card c in cards)
            if (c.IsMatched) matched.Add(c.CardId);

        SaveData data = new SaveData
        {
            score = score,
            comboMultiplier = comboMultiplier,
            matchesFound = matchesFound,
            matchedCardIds = matched
        };

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    // Load previously saved data
    void LoadGame()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
            return;

        string json = PlayerPrefs.GetString(SaveKey);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        score = data.score;
        comboMultiplier = data.comboMultiplier;
        matchesFound = data.matchesFound;

        foreach (Card card in cards)
        {
            if (data.matchedCardIds.Contains(card.CardId))
            {
                card.SetMatched();
                card.DisableCard();
            }
            else
            {
                card.HideCardInstant();
            }
        }

        UpdateScoreUI();
    }
}
