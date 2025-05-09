using UnityEngine;
using System.Collections;

// Handles individual card behavior
public class Card : MonoBehaviour
{
    [Header("Rendering")]
    [SerializeField] private SpriteRenderer frontRenderer;
    [SerializeField] private SpriteRenderer backRenderer;
    [SerializeField] private float flipDuration = 0.5f;

    [Header("Sound")]
    public AudioClip flipSound;

    private int cardId;
    private bool isFlipping = false;
    private bool isFlipped = false;
    public bool IsMatched { get; private set; } = false;

    public int CardId => cardId;
    public bool IsFlipped => isFlipped;

    // Set visuals and identity
    public void SetCard(Sprite frontSprite, int id)
    {
        frontRenderer.sprite = frontSprite;
        cardId = id;
        backRenderer.enabled = true;
        frontRenderer.enabled = false;
        isFlipped = false;
        IsMatched = false;
    }

    // On player click
    private void OnMouseDown()
    {
        if (!isFlipping && !GameManager.Instance.IsWaiting && !isFlipped)
        {
            StartCoroutine(FlipCard(true));
            GameManager.Instance.CardClicked(this);
        }
    }

    // Animate card rotation
    public IEnumerator FlipCard(bool showFront)
    {
        isFlipping = true;
        float elapsed = 0f;

        if (flipSound != null && GameManager.Instance != null)
            GameManager.Instance.PlaySound(flipSound);

        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(0, showFront ? 180 : 0, 0);

        while (elapsed < flipDuration)
        {
            transform.rotation = Quaternion.Lerp(startRot, endRot, elapsed / flipDuration);

            if (elapsed >= flipDuration / 2)
            {
                backRenderer.enabled = !showFront;
                frontRenderer.enabled = showFront;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRot;
        isFlipped = showFront;
        isFlipping = false;
    }

    // Flip card back
    public void HideCard()
    {
        if (isFlipped && !isFlipping)
            StartCoroutine(FlipCard(false));
    }

    // Instantly reset to back side
    public void HideCardInstant()
    {
        backRenderer.enabled = true;
        frontRenderer.enabled = false;
        isFlipped = false;
    }

    // Visually and logically set as matched
    public void SetMatched()
    {
        IsMatched = true;
        isFlipped = true;
        frontRenderer.enabled = true;
        backRenderer.enabled = false;
    }

    // Disable interactions on matched cards
    public void DisableCard()
    {
        if (TryGetComponent<Collider2D>(out Collider2D col))
            col.enabled = false;

        IsMatched = true;
    }
}
