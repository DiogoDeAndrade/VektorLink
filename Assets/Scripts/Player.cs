using System;
using UC;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
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
    private float           springConstant = 200f;
    [SerializeField] 
    private float           restLength = 200f;
    [SerializeField]
    private float           minDist = 20.0f;
    [SerializeField]
    private PlayerInput     playerInput;
    [SerializeField]
    private CtrlPoint[]     ctrlPoints;

    void Start()
    {
        foreach (var ctrl in ctrlPoints)
        {
            ctrl.moveCtrl.playerInput = playerInput;
        }
    }

    void FixedUpdate()
    {
        // Run spring system
        var a = ctrlPoints[0];
        var b = ctrlPoints[1];

        Vector3 delta = b.transform.position - a.transform.position;
        float   dist = delta.magnitude;

        if (dist >= minDist)
        {
            Vector3 dir = delta / dist;
            float displacement = dist - Mathf.Max(minDist, restLength);

            // Hooke's law: F = -k * displacement
            Vector3 force = dir * (springConstant * displacement);

            // Apply equal and opposite force
            a.velocity += force * Time.fixedDeltaTime;
            b.velocity -= force * Time.fixedDeltaTime;
        }

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
    }

    private void Update()
    {
        foreach (var ctrl in ctrlPoints)
        {
            ctrl.moveDir = ctrl.moveCtrl.GetAxis2().xy0();
        }
        
    }
}
