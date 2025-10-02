using NaughtyAttributes;
using UnityEngine;

public class EnemyChaser : Enemy
{
    [SerializeField] SpriteRenderer innerSprite;
    [SerializeField] bool           fleeWhenCapturable;
    [SerializeField, ShowIf(nameof(fleeWhenCapturable))]
    private float maxFleeRange = 50.0f;

    Transform        target;

    protected override void Start()
    {
        base.Start();

        velocity = Vector2.zero;

        // Select
        int idx = Random.Range(0, 2);
        target = player.GetTransform(idx);

        innerSprite.color = target.GetComponent<SpriteRenderer>().color;
    }

    protected override void Update()
    {
        if (player != null)
        {
            if ((canCapture) && (fleeWhenCapturable))
            {
                if (Vector3.Distance(target.position, transform.position) < maxFleeRange)
                {
                    velocity = -(target.position - transform.position).normalized * maxSpeed;
                }
                else
                {
                    velocity = Vector3.zero;
                }
            }
            else
            {
                velocity = (target.position - transform.position).normalized * maxSpeed;
            }
        }
        else
        {
            velocity = Vector2.zero;
        }

        base.Update();
    }
}
