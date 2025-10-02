using UC;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private AudioClip      musicTrack;
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField, GradientUsage(true)] private Gradient       backgroundColor;
    [SerializeField] private float          maxLevelTime = 10.0f;

    float levelTime = 0.0f;
    Material backgroundMaterial;

    void Start()
    {
        SoundManager.PlayMusic(musicTrack, 1.0f, 1.0f, 1.0f);

        backgroundMaterial = new Material(backgroundRenderer.material);
        backgroundRenderer.material = backgroundMaterial;
    }

    private void Update()
    {
        levelTime += Time.deltaTime;

        backgroundMaterial.SetColor("_Color2", backgroundColor.Evaluate(Mathf.Clamp01(levelTime / maxLevelTime)));
    }
}
