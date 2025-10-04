using UnityEngine;
using UC;

public class EnableOnLeaderboardData : MonoBehaviour
{
    void Start()
    {
        var uiControl = GetComponent<BaseUIControl>();
        uiControl.canSelect += (control) =>
        {
            return LeaderboardManager.isScoresAvailable;
        };
    }
}
