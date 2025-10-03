using Dan.Main;
using Dan.Models;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class LeaderboardManager : MonoBehaviour
{
    static LeaderboardManager Instance;

    class HighScore
    {
        public float    score;
        public string   name;
    }

    List<HighScore> highScoreTable;

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
            if (entries.Length == 0)
            {
                InitializeLeaderboard();
            }
            else
            {
                highScoreTable = new();
                for (int i = 0; i < Mathf.Max(entries.Length, 10); i++)
                {
                    highScoreTable.Add(new HighScore()
                    {
                        name = entries[i].Username,
                        score = entries[i].Score
                    });
                }

                foreach (var c in highScoreTable)
                {
                    Debug.Log($"{c.name} {c.score}");
                }
            }
        });
    }

    void InitializeLeaderboard()
    {
        Debug.Log("InitializeLeaderboard");
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
        for (int i = 0; i < playerNames.Length; i++)
        {
            UploadScore(playerNames[i], i * 500, isSuccessful =>
            {
                if (i == playerNames.Length - 1)
                {
                    UpdateEntries();
                }
            });
        }
    }

    public delegate void DoneAction(bool b);

    public void UploadScore(string name, int score, DoneAction doneAction)
    {
        Leaderboards.VektorLinkLeaderboard.UploadNewEntry(name, score, isSuccessful =>
        {
            doneAction?.Invoke(isSuccessful);
        });
    }
}
