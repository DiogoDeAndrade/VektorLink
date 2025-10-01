using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineCollisionDetector : MonoBehaviour
{
    public class LineCollision
    {
        public LineCollisionDetector    detector; // The collider hit
        public Vector2                  position; // Point on the line where the circle touches (closest point)
        public Vector2                  normal;   // Outward normal (CW winding => left-perpendicular of segment)
    }

    private LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    private void OnEnable()
    {
        allCollisionDetectors.Add(this);
    }

    private void OnDisable()
    {
        allCollisionDetectors.Remove(this);
    }

    public LineCollision IsColliding(Vector2 pos, float radius)
    {
        if (lr == null) lr = GetComponent<LineRenderer>();
        int count = lr.positionCount;
        if (count < 2 || radius < 0f) return null;

        float r2 = radius * radius;
        int last = lr.loop ? count : count - 1;

        for (int i = 0; i < last; i++)
        {
            Vector3 a3 = lr.GetPosition(i);
            Vector3 b3 = lr.GetPosition((i + 1) % count);

            if (!lr.useWorldSpace)
            {
                a3 = transform.TransformPoint(a3);
                b3 = transform.TransformPoint(b3);
            }

            Vector2 a = new Vector2(a3.x, a3.y);
            Vector2 b = new Vector2(b3.x, b3.y);

            // Closest point on segment to circle center
            Vector2 closest = ClosestPointOnSegment(pos, a, b);
            Vector2 diff = pos - closest;

            if (diff.sqrMagnitude <= r2)
            {
                // CW winding => outward normal = left-perpendicular of segment direction
                Vector2 seg = b - a;
                Vector2 n;

                if (seg.sqrMagnitude > Mathf.Epsilon)
                {
                    // left-perp = (-y, x)
                    n = new Vector2(-seg.y, seg.x).normalized;
                }
                else
                {
                    // Degenerate segment: use direction from point to circle center
                    n = (diff.sqrMagnitude > Mathf.Epsilon) ? diff.normalized : Vector2.up;
                }

                return new LineCollision
                {
                    detector = this,
                    position = closest,
                    normal = n
                };
            }
        }

        return null;
    }

    private static Vector2 ClosestPointOnSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float lenSq = ab.sqrMagnitude;
        if (lenSq <= Mathf.Epsilon) return a;

        float t = Vector2.Dot(p - a, ab) / lenSq;
        t = Mathf.Clamp01(t);
        return a + t * ab;
    }

    static readonly List<LineCollisionDetector> allCollisionDetectors = new();

    public static LineCollision IsColliding(Vector2 pos, float radius, LayerMask mask)
    {
        foreach (var det in allCollisionDetectors)
        {
            if (((1 << det.gameObject.layer) & mask.value) == 0)
                continue; // not in mask

            var hit = det.IsColliding(pos, radius);
            if (hit != null)
                return hit;
        }
        return null;
    }
}
