using NaughtyAttributes;
using UC;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] 
    private float     maxSpeed;
    [SerializeField] 
    private float     angularSpeed;
    [SerializeField] 
    private float     radius = 10.0f;
    [SerializeField] 
    private bool      canCapture;
    [SerializeField, ShowIf(nameof(canCapture))] 
    private Color     captureColor = Color.white;
    [SerializeField, ShowIf(nameof(canCapture))] 
    private float     immunityDuration = 2.0f;
    [SerializeField] 
    private Color     hostileColor = Color.red;
    [SerializeField] 
    private LayerMask wallMask;
    [SerializeField] 
    private LayerMask playerBeamMask;
    
    Vector2         velocity;
    SpriteRenderer  spriteRenderer;
    float           immunityTimer;
    float           angle;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        velocity = maxSpeed * Random.insideUnitCircle.normalized;
        angle = Random.Range(0.0f, 360.0f);
    }

    void Update()
    {
        if (canCapture)
        {
            spriteRenderer.color = captureColor;

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
            }
        }
        else
        {
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
                }
            }

            spriteRenderer.color = hostileColor;
        }

        var delta = velocity.xy0() * Time.deltaTime;

        transform.position += delta;
        var hit = LineCollisionDetector.IsColliding(transform.position, radius, wallMask);
        if (hit != null)
        {
            transform.position -= delta;

            // Reflect velocity along the collision normal
            var n = hit.normal;                  // already normalized
            velocity = Vector2.Reflect(velocity, n);

            // (optional) small positional correction to avoid immediate re-collision
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
