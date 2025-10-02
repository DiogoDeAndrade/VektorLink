using System;
using TMPro;
using UC;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Processors;
using UnityEngine.SceneManagement;

public class PlayerConstraint : MonoBehaviour
{
    [Serializable]
    private record CtrlPoint
    {
        public Transform        transform;
        [SerializeField, InputPlayer(nameof(playerInput))]
        public UC.InputControl  moveCtrl;
        [HideInInspector]
        public Vector3          moveDir;
        [HideInInspector]
        public Vector3          velocity = Vector3.zero;
    }
    [SerializeField, Header("Movement")]
    private float           maxMoveSpeed = 200.0f;
    [SerializeField]
    private float           acceleration = 1600.0f;
    [SerializeField]
    private float           drag = 4.0f;
    [SerializeField] 
    private Vector2         minMaxDistance = new Vector2(20.0f, 200.0f);
    [SerializeField]
    private PlayerInput     playerInput;
    [SerializeField]
    private CtrlPoint[]     ctrlPoints;
    [SerializeField, InputPlayer(nameof(playerInput)), InputButton]
    private UC.InputControl restartButton;
    [SerializeField, Header("Collision")]
    private float           radius = 10.0f;
    [SerializeField]
    private LayerMask       wallMask;
    [SerializeField, Header("FX")]
    private GameObject      wallHurtFX;
    [SerializeField]
    private GameObject      hurtFX;
    [SerializeField]
    private SoundDef        hurtSnd;
    [SerializeField, Header("Capture")]
    private SoundDef        captureSnd;
    [SerializeField]
    private float           multiplierTime;
    [SerializeField]
    private TextMeshPro     popupText;
    [SerializeField, Header("Length")]
    private float           baseLengthIncrement = 10f;
    [SerializeField]
    private float           baseLengthIncrementByMuliplier = 10f;
    [SerializeField]
    private float           baseLengthDecrement = 40f;
    [SerializeField, Header("Lifetime")]
    private float           initialTime = 10.0f;
    [SerializeField]
    private float           baseTimeIncrement = 1f;
    [SerializeField]
    private float           baseTimeIncrementByMuliplier = 1f;
    [SerializeField]
    private float           baseTimeDecrement = 2f;

    private float lastCaptureTime = float.NegativeInfinity;
    private int multiplier = 1;
    private float _lifetime;

    public float lifetime => _lifetime;
    public bool isDead => (_lifetime <= 0);

    void Start()
    {
        _lifetime = initialTime;

        foreach (var ctrl in ctrlPoints)
        {
            ctrl.moveCtrl.playerInput = playerInput;
        }
        restartButton.playerInput = playerInput;
    }

    void FixedUpdate()
    {
        if (isDead) return;

        for (int i = 0; i < ctrlPoints.Length; i++)
        {
            var ctrl = ctrlPoints[i];

            // Apply drag
            ctrl.velocity = ctrl.velocity - ctrl.velocity.normalized * ctrl.velocity.magnitude * drag * Time.fixedDeltaTime;

            // Apply acceleration
            ctrl.velocity = ctrl.velocity + ctrl.moveDir * acceleration * Time.fixedDeltaTime;

            // Clamp velocity
            ctrl.velocity = ctrl.velocity.normalized * Mathf.Min(ctrl.velocity.magnitude, maxMoveSpeed);

            ctrl.transform.position = ctrl.transform.position + ctrl.velocity * Time.fixedDeltaTime;
        }

        // Enforce symmetric distance constraint between the two points
        var a = ctrlPoints[0];
        var b = ctrlPoints[1];

        Vector3 delta = b.transform.position - a.transform.position;
        float dist = delta.magnitude;

        // Handle near-coincident points
        if (dist < 1e-5f)
        {
            // Pick an arbitrary separation direction
            Vector3 dirFallback = Vector3.right;
            float half = minMaxDistance.x * 0.5f;
            a.transform.position -= dirFallback * half;
            b.transform.position += dirFallback * half;
            // wipe relative-velocity along that direction
            Vector3 vRel = b.velocity - a.velocity;
            float along = Vector3.Dot(vRel, dirFallback);
            Vector3 vRelAlong = along * dirFallback;
            a.velocity += 0.5f * vRelAlong;
            b.velocity -= 0.5f * vRelAlong;
            return;
        }

        Vector3 dir = delta / dist;
        float minD = minMaxDistance.x;
        float maxD = minMaxDistance.y;

        // Helper to apply symmetric positional & velocity corrections
        void ApplyCorrection(float targetDist, bool stopIfApproaching)
        {
            float correction = targetDist - dist; // positive -> need to increase separation
            Vector3 corrVec = dir * (correction * 0.5f);

            // Symmetric positional projection
            a.transform.position -= corrVec;
            b.transform.position += corrVec;

            // Remove the violating relative velocity component along the constraint axis
            Vector3 vRel = b.velocity - a.velocity;
            float along = Vector3.Dot(vRel, dir); // +: separating, -: approaching

            bool violating =
                (stopIfApproaching && along < 0f) ||     // too close: stop approaching
                (!stopIfApproaching && along > 0f);      // too far: stop further separating

            if (violating)
            {
                Vector3 vRelAlong = along * dir;
                // Distribute equally so no one is favored
                a.velocity += 0.5f * vRelAlong;
                b.velocity -= 0.5f * vRelAlong;
            }
        }

        if (dist < minD)
        {
            // Push apart to min distance; stop approaching motion
            ApplyCorrection(minD, stopIfApproaching: true);
        }
        else if (dist > maxD)
        {
            // Pull together to max distance; stop separating motion
            ApplyCorrection(maxD, stopIfApproaching: false);
        }
    }

    private void Update()
    {
        if (isDead)
        {
            if (restartButton.IsDown())
            {
                FullscreenFader.FadeOut(0.5f, Color.black, () =>
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                });
            }
        }
        foreach (var ctrl in ctrlPoints)
        {
            ctrl.moveDir = ctrl.moveCtrl.GetAxis2().xy0();

            var hit = LineCollisionDetector.IsColliding(ctrl.transform.position, radius, wallMask);
            if (hit != null)
            {
                Hurt(null);
                ctrl.transform.position = hit.position - hit.normal * radius * 1.1f;
                ctrl.velocity = -hit.normal * maxMoveSpeed;

                var psObj = Instantiate(wallHurtFX, hit.position, Quaternion.LookRotation(Vector3.forward, hit.normal));
                var ps = psObj.GetComponent<ParticleSystem>();
                ps?.Play();
            }
        }

        if (!isDead)
        {
            if (_lifetime > 0.0f)
            {
                _lifetime -= Time.deltaTime;

                if (isDead)
                {
                    foreach (var ctrl in ctrlPoints)
                    {
                        var sr = ctrl.transform.GetComponent<SpriteRenderer>();

                        var psObj = Instantiate(hurtFX, ctrl.transform.position, Quaternion.identity);
                        var ps = psObj.GetComponent<ParticleSystem>();
                        ps?.SetColor(sr.color);
                        ps?.Play();
                        sr.enabled = false;
                        Destroy(GetComponentInChildren<LineRenderer>().gameObject);
                    }
                }
            }
        }
    }

    public void Capture(Enemy enemy)
    {
        if (isDead) return;

        string text = "";

        if ((Time.time - lastCaptureTime) > multiplierTime)
        {
            multiplier = 1;
        }
        else
        {
            multiplier++;
        }

        captureSnd?.Play(1.0f, 0.95f + 0.05f * multiplier);

        lastCaptureTime = Time.time;
        minMaxDistance.y += baseLengthIncrement + multiplier * baseLengthIncrementByMuliplier;

        int deltaLifetime = Mathf.FloorToInt(baseTimeIncrement + multiplier * baseTimeIncrementByMuliplier);
        _lifetime += deltaLifetime;

        text = $"+{deltaLifetime}<size=70%>s";

        if (text != "")
        {
            var textObj = Instantiate(popupText, enemy.transform.position + Vector3.up * 15.0f, Quaternion.identity);
            textObj.color = Color.green;
            textObj.text = text;
        }
    }

    public void Hurt(Enemy enemy, Transform damageTarget = null)
    {
        if (isDead) return;

        minMaxDistance.y = Mathf.Max(minMaxDistance.x, minMaxDistance.y - baseLengthDecrement);
        CameraShake2d.Shake(10.0f, 0.2f);

        hurtSnd?.Play();

        if (enemy)
        {
            _lifetime = Mathf.Max(0, _lifetime - baseTimeDecrement);

            var textObj = Instantiate(popupText, enemy.transform.position + Vector3.up * 15.0f, Quaternion.identity);
            textObj.color = Color.red;
            textObj.text = $"-{baseTimeDecrement}<size=70%>s";
        }

        if (damageTarget)
        {
            var psObj = Instantiate(hurtFX, damageTarget.position, Quaternion.identity);
            var ps = psObj.GetComponent<ParticleSystem>();
            ps?.SetColor(damageTarget.GetComponent<SpriteRenderer>().color);
            ps?.Play();
        }

        lastCaptureTime = float.NegativeInfinity;
    }
}
