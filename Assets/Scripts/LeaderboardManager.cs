using Dan.Main;
using Dan.Models;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public delegate void DoneAction(bool b);

    public class HighScore
    {
        public int      rank;
        public float    score;
        public string   name;
    }

    List<HighScore> highScoreTable;
    bool            _scoresAvailable = false;

    static LeaderboardManager Instance;
    static public bool isScoresAvailable => (Instance) && (Instance._scoresAvailable);

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

        _UpdateEntries();
    }


    void _UpdateEntries()
    {
        var query = LeaderboardSearchQuery.Paginated(0, 10);

        Leaderboards.VektorLinkLeaderboard.GetEntries(query, entries =>
        {
            highScoreTable = new();
            for (int i = 0; i < Mathf.Min(entries.Length, 10); i++)
            {
                Debug.Log($"Updating entry {i}");
                highScoreTable.Add(new HighScore()
                {
                    rank = entries[i].Rank,
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

    public void _UploadScore(string name, int score, DoneAction doneAction, bool resetPlayer)
    {
        if (resetPlayer)
        {
            Leaderboards.VektorLinkLeaderboard.ResetPlayer(() =>
            {
                _UploadScore(name, score, doneAction, false);
            });
            return;
        }
        Leaderboards.VektorLinkLeaderboard.UploadNewEntry(name, score, isSuccessful =>
        {
            doneAction?.Invoke(isSuccessful);
        });
    }

    void _GetEntry(int index, out string name, out int score)
    {
        if ((highScoreTable == null) || (index >= highScoreTable.Count))
        {
            name = "UNKNOWN";
            score = (index + 1) * 1000;
            return;
        }

        name = highScoreTable[index].name;
        score = Mathf.FloorToInt(highScoreTable[index].score);
    }

    public delegate void LocalVicinityDone(int ourRank, List<HighScore> localVicinity);

    void _GetLocalVicinity(LocalVicinityDone doneFunction)
    {
        // 1) Get this player's own entry to know their 1-based rank
        Leaderboards.VektorLinkLeaderboard.GetPersonalEntry(myEntry =>
        {
            if (myEntry.Rank <= 0)
            {
                // No entry yet
                doneFunction?.Invoke(0, null);
                return;
            }

            // 2) Get total entries so we can clamp window
            Leaderboards.VektorLinkLeaderboard.GetEntryCount(totalCount =>
            {
                int prev = 5, next = 4;
                int take = Mathf.Max(1, prev + 1 + next);

                // 0-based index of the player
                int myIdx = myEntry.Rank - 1;

                // First, place the window so that `myIdx` is inside it
                int skip = myIdx - prev;

                // Clamp to valid range [0, totalCount - take]
                skip = Mathf.Clamp(skip, 0, Mathf.Max(0, totalCount - take));

                // If, after clamping, the player would still fall outside the window
                // (can happen when we were forced near the end), pull the window down
                if (myIdx > skip + (take - 1))
                    skip = myIdx - (take - 1);

                // Final clamp (in case the adjustment went negative)
                skip = Mathf.Clamp(skip, 0, Mathf.Max(0, totalCount - take));

                // Now query exactly this window
                var query = LeaderboardSearchQuery.Paginated(skip, take);

                // 3) Fetch the window
                Leaderboards.VektorLinkLeaderboard.GetEntries(
                    query,
                    entries =>
                    {
                        List<HighScore> localVicinity = new();
                        foreach (var e in entries)
                        {
                            localVicinity.Add(new HighScore
                            {
                                rank = e.Rank,
                                name = e.Username,
                                score = e.Score
                            });
                        }
                        doneFunction?.Invoke(myEntry.Rank, localVicinity);
                    },
                    err => doneFunction?.Invoke(0, null)
                );
            },
            err => doneFunction?.Invoke(0, null));
        },
        err => doneFunction?.Invoke(0, null));
    }

    public static void GetEntry(int index, out string name, out int score)
    {
        Instance._GetEntry(index, out name, out score);
    }

    public static void UploadScore(string name, int score, DoneAction doneAction, bool resetPlayer)
    {
        Instance._UploadScore(name, score, doneAction, resetPlayer);
    }

    public static void GetLocalVicinity(LocalVicinityDone doneFunction)
    {
        Instance._GetLocalVicinity(doneFunction);
    }

    public static List<HighScore> GetTop()
    {
        return Instance.highScoreTable;
    }
}
