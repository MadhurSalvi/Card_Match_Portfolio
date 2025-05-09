using UnityEngine;
using System.Collections;

public class Card : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer frontRenderer;
    [SerializeField] private SpriteRenderer backRenderer;
    [SerializeField] private float flipDuration = 0.5f;

    [Header("Audio")]
    public AudioClip flipSound;

    public int cardId;
    private bool isFlipping = false;
    private bool isFlipped = false;

    public int CardId => cardId;
    public bool IsFlipped => isFlipped;

    public void SetCard(Sprite frontSprite, int id)
    {
        frontRenderer.sprite = frontSprite;
        cardId = id;
        backRenderer.enabled = true;
        frontRenderer.enabled = false;
    }

    private void OnMouseDown()
    {
        if (!isFlipping && !GameManager.Instance.IsWaiting && !isFlipped)
        {
            StartCoroutine(FlipCard(true));
            GameManager.Instance.CardClicked(this);
        }
    }

    public IEnumerator FlipCard(bool showFront)
    {
        isFlipping = true;
        float elapsedTime = 0f;

        // Play sound if available
        if (flipSound != null && GameManager.Instance != null)
        {
            GameManager.Instance.PlaySound(flipSound);
        }

        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(0, showFront ? 180 : 0, 0);

        while (elapsedTime < flipDuration)
        {
            transform.rotation = Quaternion.Lerp(
                startRotation,
                endRotation,
                elapsedTime / flipDuration
            );

            // Switch sprite at halfway point
            if (elapsedTime >= flipDuration / 2)
            {
                backRenderer.enabled = !showFront;
                frontRenderer.enabled = showFront;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRotation;
        isFlipped = showFront;
        isFlipping = false;
    }

    public void HideCard()
    {
        if (isFlipped && !isFlipping)
        {
            StartCoroutine(FlipCard(false));
        }
    }
}
