using System.Collections;
using TMPro;
using UC;
using UnityEngine;

public class WaveDisplay : MonoBehaviour
{
    TMP_Text            text;
    CanvasGroup         canvasGroup;
    Coroutine           animationCR;

    void Start()
    {
        text = GetComponent<TMP_Text>();
        canvasGroup = GetComponent<CanvasGroup>();

        GameManager.Instance.onChangeWave += OnWaveChange;

        OnWaveChange(0);
    }

    private void OnDestroy()
    {
        GameManager.Instance.onChangeWave -= OnWaveChange;
    }

    private void OnWaveChange(int wave)
    {
        text.text = $"Wave {wave + 1}";

        if (animationCR != null)
            StopCoroutine(animationCR);

        animationCR = StartCoroutine(AnimationCR());
    }

    IEnumerator AnimationCR()
    { 
        float animDuration = 0.5f;

        canvasGroup.alpha = 0.0f;
        canvasGroup.FadeIn(animDuration);
        transform.localScale = Vector3.zero;
        transform.LocalScaleTo(Vector3.one, animDuration);
        
        yield return new WaitForSeconds(animDuration + 0.5f);
        
        canvasGroup.FadeOut(animDuration);

        animationCR = null;
    }
}
