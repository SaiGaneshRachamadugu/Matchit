using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

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

    private List<CardController> activeCards = new List<CardController>();
    private CardController firstSelected = null;
    private CardController secondSelected = null;

    private int totalCards = 0;
    private int totalPairs = 0;
    private int matchedPairs = 0;

    private int comboStreak = 0;
    private int highestStreak = 0;

    private void Start()
    {
        int rows = PlayerPrefs.GetInt("rows", 3);
        int cols = PlayerPrefs.GetInt("cols", 4);
        //Card grid set to 3/4 by default//
        totalCards = rows * cols;

        if (totalCards % 2 != 0)
        {
            Debug.LogWarning("Grid Setup Total cards must be even. Reducing by 1.");
            totalCards -= 1;
        }

        totalPairs = totalCards / 2;

        //Setup Grid layout//
        GridLayoutGroup grid = layoutGroup.GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = cols;
        }

        GenerateCards(totalCards);
        UpdateStatsUI();
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
            GameObject newCard = Instantiate(cardPrefab, layoutGroup);
            CardController controller = newCard.GetComponent<CardController>();
            Sprite faceSprite = cardFaceSprites[cardIDs[i] % cardFaceSprites.Length];

            controller.Initialize(cardIDs[i], faceSprite, OnCardSelected);
            activeCards.Add(controller);
        }
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
            statsText.text = $"Matches: {matchedPairs}\nRemaining: {remainingCards}\n Streak: {comboStreak}";
        }
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

    public void OnQuitBtn()
    {
        Application.Quit();
    }

    public void MainMenuBtn()
    {
        SceneManager.LoadScene(0);
    }
}
