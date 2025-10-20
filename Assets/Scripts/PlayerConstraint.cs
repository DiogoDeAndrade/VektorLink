using System;
using System.Collections;
using TMPro;
using UC;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;
using static PlayerConstraint;
using Random = UnityEngine.Random;

public class PlayerConstraint : MonoBehaviour
{
    public delegate void OnHurt(Enemy enemy);
    public event OnHurt onHurt;

    public delegate void OnChangeScore(int score);
    public event OnChangeScore onChangeScore;

    public delegate void OnChangeMultiplier(int score);
    public event OnChangeMultiplier onChangeMultiplier;

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
    private UC.InputControl continueButton;
    [SerializeField, InputPlayer(nameof(playerInput)), InputButton]
    private UC.InputControl swapButton;
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
    [SerializeField]
    private CanvasGroup     gameOverCanvas;
    [SerializeField]
    private SoundDef        gameOverSnd;
    [SerializeField]
    private CanvasGroup     scoreEntryCanvas;
    [SerializeField]
    private CanvasGroup     leaderboardCanvas;

    private float lastCaptureTime = float.NegativeInfinity;
    private int multiplier = 1;
    private float _lifetime;
    private bool isSwapping = false;
    private int score = 0;
    private bool highscoreHandling = false;
    private VibrationManager    vibrationManager;

    public float lifetime => _lifetime;
    public bool isDead => (_lifetime <= 0);

    void Start()
    {
        _lifetime = initialTime;

        foreach (var ctrl in ctrlPoints)
        {
            ctrl.moveCtrl.playerInput = playerInput;
        }
        continueButton.playerInput = playerInput;
        swapButton.playerInput = playerInput;

        vibrationManager = GetComponent<VibrationManager>();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (isSwapping) return;

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
        if (highscoreHandling) return;
        if (isDead)
        {
            if (continueButton.IsDown())
            {
                if (score > 0)
                {
                    highscoreHandling = true;
                    var nameEntry = scoreEntryCanvas.GetComponentInChildren<NameEntry>();
                    if (nameEntry)
                    {
                        nameEntry.onNameEntryComplete += NameEntry_onNameEntryComplete;
                        gameOverCanvas.FadeOut(0.25f);
                        scoreEntryCanvas.FadeIn(0.25f);

                    }
                    else
                    {
                        Restart();
                    }
                }
                else
                {
                    Restart();
                }
            }
            return;
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Q))
        {
            score = 25;
            onChangeScore?.Invoke(score);
            ChangeLifetime(-10000);
            return;
        }
#endif

        if (swapButton.IsDown())
        {
            var lineUpdate = GetComponentInChildren<LineBetweenPoints>();
            if (lineUpdate)
            {
                isSwapping = true;
                lineUpdate.enabled = false;
                StartCoroutine(SwappingCR());
            }
        }

        if (!isSwapping)
        {
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
        }

        if (!isDead)
        {
            if (_lifetime > 0.0f)
            {
                ChangeLifetime(-Time.deltaTime);
            }
        }
    }

    private void NameEntry_onNameEntryComplete(string name)
    {
        if (name == "")
        {
            Restart();
            return;
        }
        scoreEntryCanvas.FadeOut(0.25f);
        LeaderboardManager.UploadScore(name, score, OnUploadDone, true);
    }

    private void OnUploadDone(bool b)
    {
        if (!b) Restart();

        LeaderboardManager.GetLocalVicinity((ourRank, localVicinity) =>
        {
            leaderboardCanvas.FadeIn(0.5f);
            var leaderboardDisplay = leaderboardCanvas.GetComponentInChildren<LeaderboardDisplay>();
            leaderboardDisplay.RefreshData(localVicinity, ourRank);
            leaderboardDisplay.onBackPressed += BackToMenu;
        });
    }

    void Restart()
    {
        FullscreenFader.FadeOut(0.5f, Color.black, () =>
        {
            GameManager.Instance.ResetGame();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        });
    }

    void BackToMenu()
    {
        FullscreenFader.FadeOut(0.5f, Color.black, () =>
        {
            GameManager.Instance.ResetGame();
            SceneManager.LoadScene("Title");
        });
    }

    IEnumerator SwappingCR()
    {
        Vector3 t1 = ctrlPoints[0].transform.position;
        Vector3 t2 = ctrlPoints[1].transform.position;

        float totalSwapTime = 0.25f;
        float elapsedTime = 0.0f;
        float p = 0.5f;
        
        while (elapsedTime < totalSwapTime)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / totalSwapTime;
            ctrlPoints[0].transform.position = Vector3.Lerp(t1, t2, Mathf.Pow(t, p));
            ctrlPoints[1].transform.position = Vector3.Lerp(t2, t1, Mathf.Pow(t, p));

            yield return null;
        }

        ctrlPoints[0].transform.position = t2;
        ctrlPoints[1].transform.position = t1;

        isSwapping = false;

        var lineUpdate = GetComponentInChildren<LineBetweenPoints>();
        lineUpdate.enabled = true;
    }

    public void Capture(Enemy enemy)
    {
        if (isDead) return;

        string text = "";

        if ((Time.time - lastCaptureTime) > multiplierTime)
        {
            ChangeMultiplier(1);
        }
        else
        {
            ChangeMultiplier(multiplier + 1);
        }

        captureSnd?.Play(1.0f, 0.95f + 0.05f * multiplier);

        lastCaptureTime = Time.time;
        minMaxDistance.y += baseLengthIncrement + multiplier * baseLengthIncrementByMuliplier;

        int deltaLifetime = Mathf.FloorToInt(baseTimeIncrement + multiplier * baseTimeIncrementByMuliplier);
        ChangeLifetime(deltaLifetime);

        text = $"+{deltaLifetime}<size=70%>s";

        if (text != "")
        {
            var textObj = Instantiate(popupText, enemy.transform.position + Vector3.up * 15.0f, Quaternion.identity);
            textObj.color = Color.green;
            textObj.text = text;
        }

        vibrationManager.Vibrate(0.2f, 0.6f, 0.15f);
    }

    public void Hurt(Enemy enemy, Transform damageTarget = null)
    {
        if (isDead) return;

        minMaxDistance.y = Mathf.Max(minMaxDistance.x, minMaxDistance.y - baseLengthDecrement);
        CameraShake2d.Shake(10.0f, 0.2f);
        vibrationManager.Vibrate(0.6f, 0.2f, 0.15f);

        hurtSnd?.Play();

        if (enemy)
        {
            ChangeLifetime(-baseTimeDecrement);

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

        ChangeMultiplier(1);
        lastCaptureTime = float.NegativeInfinity;

        onHurt?.Invoke(enemy);
    }

    void ChangeLifetime(float lifeDelta)
    {
        _lifetime = Mathf.Max(0, _lifetime + lifeDelta);
        if (lifeDelta < 0)
        {
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

                gameOverCanvas.FadeIn(0.25f);
                gameOverSnd?.Play();
            }
        }
    }

    public Transform GetTransform(int idx)
    {
        return ctrlPoints[idx].transform;
    }

    public void ChangeScore(int deltaScore, bool useLengthModifier)
    {
        float len = Vector3.Distance(ctrlPoints[0].transform.position, ctrlPoints[1].transform.position) / minMaxDistance.y;
        score += Mathf.CeilToInt(deltaScore * len * multiplier);
        onChangeScore?.Invoke(score);
    }

    public void ChangeMultiplier(int multiplier)
    {
        this.multiplier = multiplier;
        onChangeMultiplier?.Invoke(multiplier);
    }    
}
