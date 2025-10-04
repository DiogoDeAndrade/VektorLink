using TMPro;
using UnityEngine;

public class HighscoreDisplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI    playerRank;
    [SerializeField] TextMeshProUGUI    playerName;
    [SerializeField] TextMeshProUGUI    playerScore;
    [SerializeField] TMP_ColorGradient  highlightGradient;

    public void UpdateEntry(int rank, string name, int score)
    {
        playerRank.text = $"{rank}.";
        playerName.text = name.ToUpper();
        playerScore.text = string.Format("{0:000000}", score);
    }

    public void Highlight()
    {
        playerRank.enableVertexGradient = true;
        playerRank.colorGradientPreset = highlightGradient;
        playerRank.color = Color.white;

        playerName.enableVertexGradient = true;
        playerName.colorGradientPreset = highlightGradient;
        playerName.color = Color.white;

        playerScore.enableVertexGradient = true;
        playerScore.colorGradientPreset = highlightGradient;
        playerScore.color = Color.white;
    }
}
