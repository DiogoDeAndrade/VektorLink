using Dan.Main;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    static LeaderboardManager Instance;

    class HighScore
    {
        public float    score;
        public string   name;
    }

    List<HighScore> highScoreTable;
    bool            _scoresAvailable = false;

    public bool isScoresAvailable => _scoresAvailable;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        UpdateEntries();
    }


    void UpdateEntries()
    {
        Debug.Log("Update Entries");
        Leaderboards.VektorLinkLeaderboard.GetEntries(entries =>
        {
            highScoreTable = new();
            for (int i = 0; i < Mathf.Min(entries.Length, 10); i++)
            {
                highScoreTable.Add(new HighScore()
                {
                    name = entries[i].Username,
                    score = entries[i].Score
                });
            }

            _scoresAvailable = true;
        });
    }

    /*[Button("Initialize Leaderboard")]
    void InitializeLeaderboard()
    {
        
        InitializeLeaderboard(0);
    }

    void InitializeLeaderboard(int index)
    {
        Debug.Log($"InitializeLeaderboard for {index}");
        string[] playerNames = new string[]
        {  
            "NEONRIDER",
            "VTEKX",
            "ASTROX",
            "GRIDPHANTOM",
            "IONSTORM",
            "LZRHAWK",
            "VECTORIA",
            "BYTEFURY",
            "ECLIPZER",
            "QUANTOR"
        };

        Leaderboards.VektorLinkLeaderboard.ResetPlayer(() =>
        {
            UploadScore(playerNames[index], (index + 1) * 500, isSuccessful =>
            {
                if (index < 9)
                {
                    StartCoroutine(InitializeLeaderboardCR(index + 1));
                }
            });
        });
    }

    IEnumerator InitializeLeaderboardCR(int index)
    {
        yield return new WaitForSeconds(0.5f);
        InitializeLeaderboard(index);

    }*/

    public delegate void DoneAction(bool b);

    public void UploadScore(string name, int score, DoneAction doneAction)
    {
        Leaderboards.VektorLinkLeaderboard.UploadNewEntry(name, score, isSuccessful =>
        {
            doneAction?.Invoke(isSuccessful);
        });
    }
}
