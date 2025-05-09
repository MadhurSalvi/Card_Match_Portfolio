using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Card Settings")]
    public GameObject cardPrefab;
    public Sprite[] cardSprites;
    public Vector2 gridSize = new Vector2(4, 4);
    private float cardSize;
    private float spacing;

    [Header("Game Settings")]
    public int score = 0;
    public int baseMatchScore = 100;
    public int mismatchPenalty = 10;
    public int comboMultiplier = 1;
    private int consecutiveMatches = 0;
    private int matchesFound = 0;
    public bool IsWaiting { get; private set; }

    [Header("UI Elements")]
    public Text scoreText;
    public Text highScoreText;
    public GameObject gameOverPanel;
    public Text completionText;

    [Header("Audio")]
    public AudioClip matchSound;
    public AudioClip mismatchSound;
    public AudioClip gameOverSound;
    private AudioSource audioSource;

    private List<Card> cards = new List<Card>();
    private List<Card> flippedCards = new List<Card>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        ValidateSprites();
        CalculateCardDimensions();
        GenerateCardGrid();
        LoadHighScore();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    void ValidateSprites()
    {
        if (cardSprites.Length % 2 != 0)
            Debug.LogError("Need even number of sprites for matching pairs!");
    }

    void CalculateCardDimensions()
    {
        float screenHeight = Camera.main.orthographicSize * 2;
        float screenWidth = screenHeight * Camera.main.aspect;

        float maxCardWidth = (screenWidth * 0.9f) / gridSize.x;
        float maxCardHeight = (screenHeight * 0.8f) / gridSize.y;
        cardSize = Mathf.Min(maxCardWidth, maxCardHeight);
        spacing = cardSize * 0.15f;
    }

    void GenerateCardGrid()
    {
        // Clear existing cards
        foreach (var card in cards)
            Destroy(card.gameObject);
        cards.Clear();

        int totalCards = (int)(gridSize.x * gridSize.y);
        if (totalCards % 2 != 0)
        {
            Debug.LogError("Grid must have even number of cards!");
            gridSize = new Vector2(4, 4);
            totalCards = 16;
        }

        // Create card pairs
        List<int> cardIds = new List<int>();
        for (int i = 0; i < totalCards / 2; i++)
        {
            cardIds.Add(i);
            cardIds.Add(i); // Add pair
        }

        // Shuffle cards
        cardIds = ShuffleList(cardIds);

        // Calculate grid position
        Vector2 startPos = new Vector2(
            -((gridSize.x - 1) * (cardSize + spacing)) / 2,
            -((gridSize.y - 1) * (cardSize + spacing)) / 2
        );

        // Create cards
        for (int i = 0; i < cardIds.Count; i++)
        {
            GameObject newCard = Instantiate(cardPrefab, transform);
            newCard.transform.localScale = Vector3.one * cardSize;

            // Position card
            int row = i / (int)gridSize.x;
            int col = i % (int)gridSize.x;
            newCard.transform.position = new Vector2(
                startPos.x + col * (cardSize + spacing),
                startPos.y + row * (cardSize + spacing)
            );

            // Set card properties
            Card card = newCard.GetComponent<Card>();
            card.SetCard(cardSprites[cardIds[i]], cardIds[i]);
            cards.Add(card);
        }
    }

    List<T> ShuffleList<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
        return list;
    }

    public void CardClicked(Card card)
    {
        if (IsWaiting || flippedCards.Contains(card))
            return;

        flippedCards.Add(card);

        if (flippedCards.Count == 2)
            StartCoroutine(CheckMatch());
    }

    IEnumerator CheckMatch()
    {
        IsWaiting = true;
        yield return new WaitForSeconds(0.5f);

        bool isMatch = flippedCards[0].CardId == flippedCards[1].CardId;

        if (isMatch)
        {
            // Correct match
            score += baseMatchScore * comboMultiplier;
            consecutiveMatches++;
            matchesFound++;
            PlaySound(matchSound);

            if (consecutiveMatches >= 2)
                comboMultiplier = Mathf.Min(comboMultiplier + 1, 3);

            foreach (var card in flippedCards)
                card.GetComponent<Collider2D>().enabled = false;
        }
        else
        {
            // Wrong match
            score -= mismatchPenalty;
            consecutiveMatches = 0;
            comboMultiplier = 1;
            PlaySound(mismatchSound);

            foreach (var card in flippedCards)
                card.HideCard();
        }

        flippedCards.Clear();
        IsWaiting = false;
        UpdateScoreUI();

        // Check for game completion
        if (matchesFound == (gridSize.x * gridSize.y) / 2)
        {
            PlaySound(gameOverSound);
            CheckHighScore();
            ShowGameComplete();
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score + " (Combo: x" + comboMultiplier + ")";
    }

    void LoadHighScore()
    {
        if (highScoreText != null)
            highScoreText.text = "High Score: " + PlayerPrefs.GetInt("HighScore", 0);
    }

    void CheckHighScore()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (score > highScore)
        {
            PlayerPrefs.SetInt("HighScore", score);
            PlayerPrefs.Save();
            LoadHighScore();
        }
    }

    void ShowGameComplete()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            completionText.text = "Game Completed!\nFinal Score: " + score + "\nHigh Score: " + PlayerPrefs.GetInt("HighScore");
        }
    }

    public void ResetGame()
    {
        score = 0;
        comboMultiplier = 1;
        consecutiveMatches = 0;
        matchesFound = 0;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        GenerateCardGrid();
        UpdateScoreUI();
    }
}
