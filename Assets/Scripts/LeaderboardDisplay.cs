using UC;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class LeaderboardDisplay : MonoBehaviour
{
    public delegate void OnBackPressed();
    public event OnBackPressed onBackPressed;

    [SerializeField] 
    private PlayerInput        playerInput;
    [SerializeField, InputPlayer(nameof(playerInput)), InputButton] 
    private UC.InputControl    backControl;

    HighscoreDisplay[] highscoreDisplays;

    void Start()
    {
        highscoreDisplays = GetComponentsInChildren<HighscoreDisplay>();
        backControl.playerInput = playerInput;
    }

    public void RefreshData(List<LeaderboardManager.HighScore> highscores, int highlightRank = -1)
    {
        for (int i = 0; i < Mathf.Min(highscoreDisplays.Length, highscores.Count); i++)
        {
            var hs = highscores[i];
            highscoreDisplays[i].UpdateEntry(hs.rank, hs.name, Mathf.FloorToInt(hs.score));
            if (highlightRank == hs.rank)
            {
                highscoreDisplays[i].Highlight();
            }
        }
    }

    void Update()
    {
        if (backControl.IsDown())
        {
            onBackPressed?.Invoke();
        }
    }
}
