using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

[RequireComponent(typeof(Button))]
public class CardController : MonoBehaviour
{
    public int CardID { get; private set; }
    public bool IsFlipped { get; private set; } = false;
    public bool IsMatched { get; private set; } = false;

    [Header("UI Elements")]
    [SerializeField] private Image cardFront;
    [SerializeField] private Image cardBack;

    [Header("Animation")]
    [SerializeField] private float flipDuration = 0.25f;

    [Header("Audio")]
    [SerializeField] private AudioClip flipSound;
    [SerializeField] private AudioClip matchSound;
    [SerializeField] private AudioClip mismatchSound;

    private AudioSource audioSource;
    private Button button;

    private Action<CardController> onCardSelected;

    public void Initialize(int id, Sprite frontImage, Action<CardController> selectionCallback)
    {
        CardID = id;
        cardFront.sprite = frontImage;
        onCardSelected = selectionCallback;
        IsFlipped = false;
        IsMatched = false;
        SetVisualState(false);

        //debugText.text = id.ToString();
        Debug.Log($"Init Card initialized with ID: {id}");
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        button = GetComponent<Button>();
        button.onClick.AddListener(OnCardClicked);

        Debug.Log("Awake Button listener attached and audio source set.");
    }

    private void OnCardClicked()
    {
        Debug.Log($"Click Card {CardID} clicked. Flipped: {IsFlipped}, Matched: {IsMatched}");

        if (IsFlipped || IsMatched)
        {
            Debug.Log($"Click Blocked Card {CardID} is already flipped or matched.");
            return;
        }

        Flip(true);
        PlaySound(flipSound);
        onCardSelected?.Invoke(this);
    }

    public void Flip(bool showFront)
    {
        Debug.Log($"Flip Card {CardID} → ShowFront: {showFront}");
        IsFlipped = showFront;
        StopAllCoroutines();
        StartCoroutine(FlipAnimation(showFront));
    }

    public void SetMatched(bool matched)
    {
        IsMatched = matched;
        Debug.Log($"MatchStatus Card {CardID} matched: {matched}");
    }
    public void HideCard()
    {
        //Or use CanvasGroup fade for smooth gameplay//
        gameObject.SetActive(false);
    }

    public void ResetCard()
    {
        IsMatched = false;
        IsFlipped = false;
        SetVisualState(false);
        Debug.Log($"Reset Card {CardID} reset.");
    }

    private void SetVisualState(bool showFront)
    {
        Debug.Log($"SetVisualState Card {CardID} → ShowFront: {showFront}");

        if (cardFront == null || cardBack == null)
        {
            Debug.LogWarning($"SetVisualState cardFront or cardBack is missing on Card {CardID}");
            return;
        }

        cardFront.gameObject.SetActive(showFront);
        cardBack.gameObject.SetActive(!showFront);

        Debug.Log($"SetVisualState cardFront.SetActive({showFront}), cardBack.SetActive({!showFront})");
    }

    private System.Collections.IEnumerator FlipAnimation(bool showFront)
    {
        Debug.Log($"Animation Card {CardID} flipping → ShowFront: {showFront}");

        float time = 0f;
        Vector3 start = transform.localScale;
        Vector3 mid = new Vector3(0f, start.y, start.z);
        Vector3 end = start;

        //Shrink phase//
        while (time < flipDuration / 2f)
        {
            transform.localScale = Vector3.Lerp(start, mid, time / (flipDuration / 2f));
            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = mid;
        Debug.Log($"Animation Card {CardID} midpoint reached — toggling visual state");
        SetVisualState(showFront);

        time = 0f;

        //Expand phase//
        while (time < flipDuration / 2f)
        {
            transform.localScale = Vector3.Lerp(mid, end, time / (flipDuration / 2f));
            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = end;
        Debug.Log($"Animation Card {CardID} flip complete.");
    }

    public void PlayMatchSound(bool matched)
    {
        Debug.Log($"Sound Playing {(matched ? "match" : "mismatch")} sound for Card {CardID}");
        PlaySound(matched ? matchSound : mismatchSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"Audio AudioClip missing or AudioSource not set on Card {CardID}");
        }
    }
}
