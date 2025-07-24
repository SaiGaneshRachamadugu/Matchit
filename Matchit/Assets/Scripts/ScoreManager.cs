using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private const string ScoreKey = "player_score";

    public int CurrentScore { get; private set; }

    public static ScoreManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadScore();
    }

    public void AddScore(int amount)
    {
        CurrentScore += amount;
        SaveScore();
    }

    public void ResetScore()
    {
        CurrentScore = 0;
        SaveScore();
    }

    public void SaveScore()
    {
        PlayerPrefs.SetInt(ScoreKey, CurrentScore);
        PlayerPrefs.Save();
    }

    public void LoadScore()
    {
        CurrentScore = PlayerPrefs.GetInt(ScoreKey, 0);
    }
}
