using NaughtyAttributes;
using UC;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Enemy : MonoBehaviour
{
    [SerializeField] 
    protected float     maxSpeed;
    [SerializeField] 
    protected float     angularSpeed;
    [SerializeField] 
    protected float     radius = 10.0f;
    [SerializeField] 
    protected bool      canCapture;
    [SerializeField, ShowIf(nameof(canCapture))] 
    protected Color     captureColor = Color.white;
    [SerializeField, ShowIf(nameof(canCapture))] 
    protected float     immunityDuration = 2.0f;
    [SerializeField] 
    protected Color     hostileColor = Color.red;
    [SerializeField] 
    protected LayerMask wallMask;
    [SerializeField] 
    protected LayerMask playerBeamMask;
    [SerializeField]
    protected GameObject blast;
    [SerializeField]
    protected int       captureScore = 50;
    [SerializeField]
    protected int       killScore = 100;

    protected Vector2           velocity;
    protected SpriteRenderer    spriteRenderer;
    protected float             immunityTimer;
    protected float             angle;
    protected bool              initialCanCapture;
    protected PlayerConstraint  player;

    protected virtual void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        velocity = maxSpeed * Random.insideUnitCircle.normalized;
        angle = Random.Range(0.0f, 360.0f);

        initialCanCapture = canCapture;
        player = FindFirstObjectByType<PlayerConstraint>();
    }

    protected virtual void Update()
    {
        if (canCapture)
        {
            spriteRenderer.color = captureColor;

            if (immunityTimer > 0)
            {
                immunityTimer -= Time.deltaTime;
            }
            else
            {
                var collision = LineCollisionDetector.IsColliding(transform.position, radius, playerBeamMask);
                if (collision != null)
                {
                    PlayerConstraint player = collision.detector.GetComponentInParent<PlayerConstraint>();
                    if (player)
                    {
                        player.Capture(this);
                    }

                    canCapture = false;
                    immunityTimer = immunityDuration;

                    var psObj = Instantiate(blast, transform.position, Quaternion.identity);
                    var ps = psObj.GetComponent<ParticleSystem>();
                    ps?.SetColor(captureColor);
                    ps?.Play();

                    player.ChangeScore(captureScore, true);
                }
                }
        }
        else
        {
            spriteRenderer.color = hostileColor;

            if (immunityTimer > 0)
            {
                immunityTimer -= Time.deltaTime;
            }
            else
            {
                var collision = LineCollisionDetector.IsColliding(transform.position, radius, playerBeamMask);
                if (collision != null)
                {
                    PlayerConstraint player = collision.detector.GetComponentInParent<PlayerConstraint>();
                    if (player)
                    {
                        player.Hurt(this);
                    }
                    immunityTimer = immunityDuration;

                    var psObj = Instantiate(blast, transform.position, Quaternion.identity);
                    var ps = psObj.GetComponent<ParticleSystem>();
                    ps?.SetColor(hostileColor);
                    ps?.Play();
                }
            }
        }

        var delta = velocity.xy0() * Time.deltaTime;

        transform.position += delta;
        var hit = LineCollisionDetector.IsColliding(transform.position, radius, wallMask);
        if (hit != null)
        {
            transform.position -= delta;

            var n = hit.normal;                  
            velocity = Vector2.Reflect(velocity, n);

            Vector2 pos2D = transform.position;
            float dist = Vector2.Distance(pos2D, hit.position);
            float penetration = radius - dist;  
            if (penetration > 0f)
            {
                transform.position += (Vector3)(n * (penetration + 0.001f));
            }
        }

        angle += angularSpeed * Time.deltaTime;
        while (angle < 0.0f) angle += 360.0f;
        while (angle >= 360.0f) angle -= 360.0f;

        transform.rotation = Quaternion.Euler(0.0f, 0.0f, angle);
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (immunityTimer > 0) return;
        if (canCapture) return;

        PlayerConstraint player = collision.GetComponentInParent<PlayerConstraint>();
        if (player != null)
        {
            player.Hurt(this, collision.transform);
            canCapture = initialCanCapture;
            immunityTimer = immunityDuration;
        }
    }

    public bool isGoal => !canCapture;

    protected void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    public void Kill()
    {
        var psObj = Instantiate(blast, transform.position, Quaternion.identity);
        var ps = psObj.GetComponent<ParticleSystem>();
        ps?.SetColor(hostileColor);
        ps?.Play();

        Destroy(gameObject);

        player.ChangeScore(killScore, false);
    }
}

[System.Serializable]
public class EnemyList : ProbList<Enemy>
{

}
