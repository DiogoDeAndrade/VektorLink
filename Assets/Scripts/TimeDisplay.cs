using System;
using TMPro;
using UC;
using UnityEngine;

public class TimeDisplay : MonoBehaviour
{
    [SerializeField] private Gradient alertColor;
    [SerializeField] private float    alertSpeed = 4.0f;

    TextMeshPro         text;
    PlayerConstraint    player;
    Color               baseColor;

    float timer = 0.0f;

    void Start()
    {
        player = FindFirstObjectByType<PlayerConstraint>();
        text = GetComponent<TextMeshPro>();
        baseColor = text.color;

        player.onHurt += OnHurt;
    }

    private void OnDestroy()
    {
        if (player) player.onHurt -= OnHurt;
    }

    private void OnHurt(Enemy enemy)
    {
        if (enemy)
        {
            text.FlashColor(GetColor(), Color.red, 0.4f);
            transform.localScale = Vector2.one * 1.5f;
            transform.LocalScaleTo(Vector2.one, 0.35f, "ScaleText").EaseFunction(Ease.Sqrt);
        }
    }

    // Update is called once per frame
    void Update()
    {
        text.color = GetColor();
        if (player.isDead)
        {
            text.text = "GAME OVER";
        }
        else
        {
            text.text = string.Format("{0:000}", Mathf.CeilToInt(player.lifetime));
        }
    }

    Color GetColor()
    {
        if (player.isDead) return baseColor;
        if (player.lifetime > 5) return baseColor;

        timer += Time.deltaTime;
        
        Color ac = alertColor.Evaluate(Mathf.Sin(timer * alertSpeed) * 0.5f + 0.5f);

        float t = Mathf.Clamp01((player.lifetime - 4.5f) / 0.5f);

        return Color.Lerp(ac, baseColor, t);
    }
}
