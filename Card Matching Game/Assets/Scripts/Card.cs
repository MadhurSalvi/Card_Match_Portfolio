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

    private int cardId;
    private bool isFlipping = false;
    private bool isFlipped = false;
    public bool IsMatched { get; private set; } = false;

    public int CardId => cardId;
    public bool IsFlipped => isFlipped;

    public void SetCard(Sprite frontSprite, int id)
    {
        frontRenderer.sprite = frontSprite;
        cardId = id;
        backRenderer.enabled = true;
        frontRenderer.enabled = false;
        isFlipped = false;
        IsMatched = false;
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

        if (flipSound != null && GameManager.Instance != null)
            GameManager.Instance.PlaySound(flipSound);

        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(0, showFront ? 180 : 0, 0);

        while (elapsedTime < flipDuration)
        {
            transform.rotation = Quaternion.Lerp(startRotation, endRotation, elapsedTime / flipDuration);

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
            StartCoroutine(FlipCard(false));
    }

    public void HideCardInstant()
    {
        backRenderer.enabled = true;
        frontRenderer.enabled = false;
        isFlipped = false;
    }

    public void SetMatched()
    {
        IsMatched = true;
        isFlipped = true;
        frontRenderer.enabled = true;
        backRenderer.enabled = false;
    }

    public void DisableCard()
    {
        if (TryGetComponent<Collider2D>(out Collider2D col))
            col.enabled = false;
        IsMatched = true;
    }
}




// using UnityEngine;
// using System.Collections;

// public class Card : MonoBehaviour
// {
//     [Header("Visual Settings")]
//     [SerializeField] private SpriteRenderer frontRenderer;  
//     [SerializeField] private SpriteRenderer backRenderer;
//     [SerializeField] private float flipDuration = 0.5f;

//     [Header("Audio")]
//     public AudioClip flipSound;

//     private int cardId;
//     private bool isFlipping = false;
//     private bool isFlipped = false;

//     public int CardId { get { return cardId; } }
//     public bool IsFlipped { get { return isFlipped; } }

//     public void SetCard(Sprite frontSprite, int id)
//     {
//         frontRenderer.sprite = frontSprite;
//         cardId = id;
//         backRenderer.enabled = true;
//         frontRenderer.enabled = false;
//     }

//     private void OnMouseDown()
//     {
//         if (!isFlipping && !GameManager.Instance.IsWaiting && !isFlipped)
//         {
//             StartCoroutine(FlipCard(true));
//             GameManager.Instance.CardClicked(this);
//         }
//     }

//     public IEnumerator FlipCard(bool showFront)
//     {
//         isFlipping = true;
//         float elapsedTime = 0f;

//         // Play sound if available
//         if (flipSound != null && GameManager.Instance != null)
//         {
//             GameManager.Instance.PlaySound(flipSound);
//         }

//         Quaternion startRotation = transform.rotation;
//         Quaternion endRotation = Quaternion.Euler(0, showFront ? 180 : 0, 0);

//         while (elapsedTime < flipDuration)
//         {
//             transform.rotation = Quaternion.Lerp(
//                 startRotation,
//                 endRotation,
//                 elapsedTime / flipDuration
//             );

//             // Switch sprite at halfway point
//             if (elapsedTime >= flipDuration / 2)
//             {
//                 backRenderer.enabled = !showFront;
//                 frontRenderer.enabled = showFront;
//             }

//             elapsedTime += Time.deltaTime;
//             yield return null;
//         }

//         transform.rotation = endRotation;
//         isFlipped = showFront;
//         isFlipping = false;
//     }

//     public void HideCard()
//     {
//         if (isFlipped && !isFlipping)
//         {
//             StartCoroutine(FlipCard(false));
//         }
//     }
// }
