using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Card Setup")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform layoutGroup;
    [SerializeField] private Sprite[] cardFaceSprites;

    [Header("UI")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private ParticleSystem Confetti;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Hint System")]
    [SerializeField] private Button hintButton;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private int maxHints = 3;
    [SerializeField] private float hintDuration = 0.8f;
    [SerializeField] private float hintCooldown = 5f;

    private int hintsUsed = 0;
    private bool isHintOnCooldown = false;


    private List<CardController> activeCards = new List<CardController>();
    private CardController firstSelected = null;
    private CardController secondSelected = null;

    private int totalCards = 0;
    private int totalPairs = 0;
    private int matchedPairs = 0;

    private int comboStreak = 0;
    private int highestStreak = 0;
    [SerializeField] private CardPool cardPool;

    private void Start()
    {
        int rows = PlayerPrefs.GetInt("rows", 3);
        int cols = PlayerPrefs.GetInt("cols", 4);
        totalCards = rows * cols;

        if (totalCards % 2 != 0) totalCards--;

        totalPairs = totalCards / 2;

        var grid = layoutGroup.GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = cols;
        }

        GenerateCards(totalCards);
        UpdateStatsUI();
        StartCoroutine(MemoryPreviewCoroutine());
    }

    private IEnumerator MemoryPreviewCoroutine()
    {
        foreach (var card in activeCards)
            card.Flip(true);

        //Preview duration//
        yield return new WaitForSeconds(0.8f);

        foreach (var card in activeCards)
            card.Flip(false);
    }

    private void GenerateCards(int totalCards)
    {
        List<int> cardIDs = new List<int>();

        for (int i = 0; i < totalCards / 2; i++)
        {
            cardIDs.Add(i);
            cardIDs.Add(i);
        }

        Shuffle(cardIDs);

        for (int i = 0; i < totalCards; i++)
        {
            GameObject cardGO = cardPool.GetCard();

            //Ensure correct parenting and reset scale//
            cardGO.transform.SetParent(layoutGroup, false);

            //Reset RectTransform in case of distortion from previous reuse//
            RectTransform rect = cardGO.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.localScale = Vector3.one;
                rect.anchoredPosition3D = Vector3.zero;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            CardController controller = cardGO.GetComponent<CardController>();
            if (controller == null)
            {
                Debug.LogError("GameManager Card prefab missing CardController component.");
                continue;
            }

            Sprite faceSprite = cardFaceSprites[cardIDs[i] % cardFaceSprites.Length];
            controller.Initialize(cardIDs[i], faceSprite, OnCardSelected);
            activeCards.Add(controller);
        }

        UpdateStatsUI();
    }

    private void OnCardSelected(CardController selected)
    {
        if (firstSelected == null)
        {
            firstSelected = selected;
        }
        else if (secondSelected == null && selected != firstSelected)
        {
            secondSelected = selected;
            StartCoroutine(CheckMatch());
        }
    }

    private System.Collections.IEnumerator CheckMatch()
    {
        yield return new WaitForSeconds(0.5f);

        bool isMatch = firstSelected.CardID == secondSelected.CardID;

        firstSelected.PlayMatchSound(isMatch);
        secondSelected.PlayMatchSound(isMatch);

        if (!isMatch)
        {
            firstSelected.Flip(false);
            secondSelected.Flip(false);
            comboStreak = 0;
        }
        else
        {
            firstSelected.SetMatched(true);
            secondSelected.SetMatched(true);

            firstSelected.HideCard();
            secondSelected.HideCard();

            matchedPairs++;
            comboStreak++;
            highestStreak = Mathf.Max(highestStreak, comboStreak);
            ScoreManager.Instance.AddScore(10);
            UpdateScoreUI();

            UpdateStatsUI();
            CheckVictory();
        }

        firstSelected = null;
        secondSelected = null;
    }

    private void UpdateStatsUI()
    {
        int matchedCards = matchedPairs * 2;
        int remainingCards = totalCards - matchedCards;

        if (statsText != null)
        {
            statsText.text = $"Card Matches: {matchedPairs}\nRemaining: {remainingCards}\n Streak: {comboStreak}";
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Total Score: {ScoreManager.Instance.CurrentScore}";
        }
    }

    public void RestartMatch()
    {
        ClearCards();

        int rows = PlayerPrefs.GetInt("rows", 3);
        int cols = PlayerPrefs.GetInt("cols", 4);
        totalCards = rows * cols;

        if (totalCards % 2 != 0)
            totalCards--;

        totalPairs = totalCards / 2;

        GridLayoutGroup grid = layoutGroup.GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = cols;
        }

        victoryPanel.SetActive(false);
        Confetti.Stop();
        GenerateCards(totalCards);
    }

    private void ClearCards()
    {
        foreach (var card in activeCards)
        {
            cardPool.ReturnCard(card.gameObject);
        }

        activeCards.Clear();
        matchedPairs = 0;
        comboStreak = 0;
        totalPairs = 0;
        totalCards = 0;
    }

    private void CheckVictory()
    {
        Debug.Log($"Victory Check matchedPairs: {matchedPairs} / totalPairs: {totalPairs}");

        if (matchedPairs >= totalPairs)
        {
            Debug.Log("Victory All pairs matched!");
            if (Confetti != null) Confetti.Play();
            victoryPanel.SetActive(true);
        }
    }

    private void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public void RevealHint()
    {
        if (hintsUsed >= maxHints || isHintOnCooldown) return;

        hintsUsed++;
        UpdateHintUI();
        StartCoroutine(RevealHintCoroutine());
    }

    private IEnumerator RevealHintCoroutine()
    {
        isHintOnCooldown = true;
        hintButton.interactable = false;

        Debug.Log("[Hint] Revealing all unmatched cards...");

        foreach (var card in activeCards)
        {
            if (!card.IsMatched)
                card.Flip(true);
        }

        yield return new WaitForSeconds(hintDuration);

        foreach (var card in activeCards)
        {
            if (!card.IsMatched && card != firstSelected && card != secondSelected)
                card.Flip(false);
        }

        Debug.Log("[Hint] Hint ended. Starting cooldown...");

        // Wait for cooldown
        yield return new WaitForSeconds(hintCooldown);

        isHintOnCooldown = false;

        // Re-enable only if hints are still available
        if (hintsUsed < maxHints)
            hintButton.interactable = true;
    }

    private void UpdateHintUI()
    {
        if (hintText != null)
            hintText.text = $"Hints Left: {maxHints - hintsUsed}";

        if (hintsUsed >= maxHints)
        {
            hintButton.interactable = false;
            Debug.Log("[Hint] All hints used.");
        }
    }


    public void OnQuitBtn()
    {
        Application.Quit();
    }

    public void MainMenuBtn()
    {
        SceneManager.LoadScene(0);
    }
}
