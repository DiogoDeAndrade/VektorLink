
using UC;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] 
    private List<AudioClip>  musicTracks;
    [SerializeField] 
    private SpriteRenderer backgroundRenderer;
    [SerializeField, GradientUsage(true)] 
    private Gradient       backgroundColor;
    [SerializeField] 
    private float          maxLevelTime = 10.0f;
    [SerializeField]
    private GameObject     bombPrefab;

    float levelTime = 0.0f;
    Material backgroundMaterial;
    PlayerConstraint player;
    float colorAnimTime = 0.0f;
    WaveDef waveDef;
    bool levelHold = false;

    void Start()
    {
        ChangeSong();

        backgroundMaterial = new Material(backgroundRenderer.material);
        backgroundRenderer.material = backgroundMaterial;

        player = FindFirstObjectByType<PlayerConstraint>();
        player.onHurt += Player_onHurt;

        waveDef = GameManager.Instance.GetWave();
    }

    private void OnDestroy()
    {
        if (player) player.onHurt -= Player_onHurt;
    }

    private void Player_onHurt(Enemy enemy)
    {
        colorAnimTime = 0.25f;
    }

    private void Update()
    {
        if (levelHold)
        {
            var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            if (enemies.Length == 0)
            {
                ChangeSong();
                GameManager.Instance.NextWave();
                waveDef = GameManager.Instance.GetWave();
                levelHold = false;
            }
        }
        else
        {
            levelTime += Time.deltaTime;

            colorAnimTime = Mathf.Max(0, colorAnimTime - Time.deltaTime);
            var color = Color.Lerp(GetColor(), Color.red, colorAnimTime / 0.25f);
            backgroundMaterial.SetColor("_Color2", color);

            // Check for end of wave
            var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            if (enemies.Length == waveDef.maxCount)
            {
                bool allGoalComplete = true;
                foreach (var enemy in enemies)
                {
                    if (!enemy.isGoal)
                    {
                        allGoalComplete = false;
                        break;
                    }
                }
                if (allGoalComplete)
                {
                    // Spawn bomb
                    Instantiate(bombPrefab, Vector3.zero, Quaternion.identity);
                    levelHold = true;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            FullscreenFader.FadeOut(0.5f, Color.black, () =>
            {
                SceneManager.LoadScene("Title");
            });
        }
    }

    private void ChangeSong()
    {
        var song = musicTracks.Random();
        SoundManager.PlayMusic(song, 1.0f, 1.0f, 1.0f);
    }

    private Color GetColor()
    {
        return backgroundColor.Evaluate(Mathf.Clamp01(levelTime / maxLevelTime));
    }
}
