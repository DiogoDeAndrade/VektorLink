using UC;
using UnityEngine;

public class LeaderboardButton : MonoBehaviour
{
    [SerializeField] private LeaderboardDisplay leaderboardDisplay;
    [SerializeField] private UIButton           playButton;

    CanvasGroup mainMenuCanvas;
    UIGroup mainUIGroup;

    void Start()
    {
        var leaderboardButton = GetComponent<UIButton>();
        leaderboardButton.onInteract += ShowLeaderboard;
        mainUIGroup = GetComponentInParent<UIGroup>();
        mainMenuCanvas = mainUIGroup.GetComponent<CanvasGroup>();
    }

    private void ShowLeaderboard(BaseUIControl control)
    {
        mainUIGroup.EnableUI(false);

        mainMenuCanvas.FadeOut(0.5f);

        leaderboardDisplay.RefreshData(LeaderboardManager.GetTop());
        var canvasGroup = leaderboardDisplay.GetComponent<CanvasGroup>();
        canvasGroup.FadeIn(0.5f);

        leaderboardDisplay.onBackPressed += BackToMenu;
    }

    private void BackToMenu()
    {
        mainMenuCanvas.FadeIn(0.5f);

        var canvasGroup = leaderboardDisplay.GetComponent<CanvasGroup>();
        canvasGroup.FadeOut(0.5f);

        mainUIGroup.EnableUI(true);
        mainUIGroup.selectedControl = playButton;

        leaderboardDisplay.onBackPressed -= BackToMenu;
    }
}
