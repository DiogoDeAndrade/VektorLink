using System;
using UC;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField]
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

    private float lastCaptureTime;

    void Start()
    {
        foreach (var ctrl in ctrlPoints)
        {
            ctrl.moveCtrl.playerInput = playerInput;
        }
    }

    void FixedUpdate()
    {
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
        foreach (var ctrl in ctrlPoints)
        {
            ctrl.moveDir = ctrl.moveCtrl.GetAxis2().xy0();
        }
    }

    public void Capture(Enemy enemy)
    {
        lastCaptureTime = Time.time;
        minMaxDistance.y += 20.0f;
    }

    public void Hurt(Enemy enemy)
    {
        minMaxDistance.y = Mathf.Max(minMaxDistance.x, minMaxDistance.y - 40.0f);
        CameraShake2d.Shake(10.0f, 0.2f);
    }
}
