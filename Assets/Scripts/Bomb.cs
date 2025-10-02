using UnityEngine;

public class Bomb : MonoBehaviour
{
    [SerializeField]
    private float           duration = 2.0f;
    [SerializeField] 
    private Gradient        colorGradient;
    [SerializeField] 
    private AnimationCurve  scaleCurve;
    [SerializeField]
    private float           finalScale = 2.0f;

    float           timer = 0.0f;
    SpriteRenderer  spriteRenderer;
    float           prevDist = 0.0f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        float t = timer / duration;
        spriteRenderer.color = colorGradient.Evaluate(t);

        transform.localScale = Vector3.one * finalScale * scaleCurve.Evaluate(t);

        float currentDist = spriteRenderer.sprite.textureRect.width * transform.localScale.x * 0.5f;

        var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            float dist = Vector3.Distance(enemy.transform.position, transform.position);
            if ((dist >= prevDist) && (dist < currentDist))
            {
                enemy.Kill();
            }
        }

        prevDist = currentDist;
    }
}
