using System;
using UC;
using UnityEngine;

public class CreditsButton : MonoBehaviour
{
    [SerializeField] private BigTextScroll  creditsScroll;
    [SerializeField] private UIButton       playButton;

    CanvasGroup mainMenuCanvas;
    UIGroup     mainUIGroup;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var creditsButton = GetComponent<UIButton>();
        creditsButton.onInteract += ShowCredits;
        mainMenuCanvas = GetComponentInParent<CanvasGroup>();
        mainUIGroup = GetComponentInParent<UIGroup>();
    }

    private void ShowCredits(BaseUIControl control)
    {
        mainUIGroup.SetEnable(false);

        mainMenuCanvas.FadeOut(0.5f);

        var canvasGroup = creditsScroll.GetComponent<CanvasGroup>();
        canvasGroup.FadeIn(0.5f);

        creditsScroll.Reset();

        creditsScroll.onEndScroll += BackToMenu;
    }

    private void BackToMenu()
    {
        mainMenuCanvas.FadeIn(0.5f);

        var canvasGroup = creditsScroll.GetComponent<CanvasGroup>();
        canvasGroup.FadeOut(0.5f);

        mainUIGroup.SetEnable(true);
        mainUIGroup.selectedControl = playButton;

        creditsScroll.onEndScroll -= BackToMenu;
    }

}
