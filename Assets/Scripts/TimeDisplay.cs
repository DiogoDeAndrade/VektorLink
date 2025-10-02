using TMPro;
using UnityEngine;

public class TimeDisplay : MonoBehaviour
{
    TextMeshProUGUI     text;
    PlayerConstraint    player;

    void Start()
    {
        player = FindFirstObjectByType<PlayerConstraint>();
        text = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player.isDead)
        {
            text.text = "GAME OVER";
        }
        else
        {
            text.text = string.Format("{0:000}", Mathf.CeilToInt(player.lifetime));
        }
    }
}
