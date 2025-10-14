using UC;
using TMPro;
using UnityEngine;

public class MultiplierDisplay : MonoBehaviour
{
    TMP_Text            text;
    PlayerConstraint    player;

    void Start()
    {
        player = FindFirstObjectByType<PlayerConstraint>();
        text = GetComponent<TMP_Text>();

        player.onChangeMultiplier += OnMultiplierChange;
    }

    private void OnDestroy()
    {
        if (player) player.onChangeMultiplier -= OnMultiplierChange;
    }

    private void OnMultiplierChange(int multiplier)
    {
        text.text = $"<size=80%>x<size=100%>{multiplier}";

        transform.localScale = Vector2.one * 1.5f;
        transform.LocalScaleTo(Vector2.one, 0.35f, "ScaleText").EaseFunction(Ease.Sqrt);
    }
}
