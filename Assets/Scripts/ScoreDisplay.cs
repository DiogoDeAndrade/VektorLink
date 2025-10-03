using TMPro;
using UnityEngine;

public class ScoreDisplay : MonoBehaviour
{
    TMP_Text            text;
    PlayerConstraint    player;
    Color               baseColor;

    void Start()
    {
        player = FindFirstObjectByType<PlayerConstraint>();
        text = GetComponent<TMP_Text>();
        baseColor = text.color;

        player.onChangeScore += OnScoreChange;
    }

    private void OnDestroy()
    {
        if (player) player.onChangeScore -= OnScoreChange;
    }

    private void OnScoreChange(int score)
    {
        text.text = string.Format("{0:000000}", score);
    }
}
