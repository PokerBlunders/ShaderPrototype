using UnityEngine;

namespace VaporwaveBallShowcase
{
    // Visual animation only. Rigidbody and BallImpact remain responsible for motion and collisions.
    public sealed class BallEffects : MonoBehaviour
    {
        public LineRenderer[] arcs;
        public LineRenderer wake;
        public Transform center;
        [Min(0.02f)] public float refreshInterval = 0.065f;
        [Min(0.01f)] public float radius = 0.35f;
        [Min(0.1f)] public float wakeLifetime = 0.55f;
        readonly Vector3[] history = new Vector3[24];
        readonly float[] times = new float[24];
        int count;
        float nextRefresh;
        uint randomState;

        float Next()
        {
            randomState ^= randomState << 13;
            randomState ^= randomState >> 17;
            randomState ^= randomState << 5;
            return (randomState & 65535) / 65535f;
        }

        void OnEnable()
        {
            randomState = unchecked((uint)GetInstanceID()) ^ 0x9e3779b9u;
            if (randomState == 0) randomState = 1;
            count = 0;
            nextRefresh = 0;
            if (wake != null) wake.positionCount = 0;
        }

        void LateUpdate()
        {
            if (center == null || Time.time < nextRefresh) return;
            nextRefresh = Time.time + refreshInterval;
            if (arcs != null)
            {
                for (int a = 0; a < arcs.Length; a++)
                {
                    var line = arcs[a];
                    if (line == null) continue;
                    line.enabled = Next() > 0.12f;
                    const int points = 17;
                    line.positionCount = points;
                    float phase = Next() * Mathf.PI * 2;
                    float spread = 1.9f + Next() * 2f;
                    for (int i = 0; i < points; i++)
                    {
                        float angle = phase + spread * i / (points - 1);
                        float jitter = (Next() - 0.5f) * 0.13f;
                        var p = new Vector3(Mathf.Cos(angle), jitter, Mathf.Sin(angle)) * radius;
                        // Snapped corners make the arcs feel digital instead of smooth ribbons.
                        p = new Vector3(Mathf.Round(p.x * 45) / 45, Mathf.Round(p.y * 45) / 45, Mathf.Round(p.z * 45) / 45);
                        line.SetPosition(i, p);
                    }
                }
            }
            if (wake == null) return;
            while (count > 0 && Time.time - times[count - 1] > wakeLifetime) count--;
            if (count == 0 || Vector3.Distance(history[0], center.position) > 0.025f)
            {
                count = Mathf.Min(count + 1, history.Length);
                for (int i = count - 1; i > 0; i--) { history[i] = history[i - 1]; times[i] = times[i - 1]; }
                history[0] = center.position;
                times[0] = Time.time;
            }
            wake.positionCount = count < 2 ? 0 : count * 2 - 1;
            for (int i = 0; i < count && count >= 2; i++)
            {
                wake.SetPosition(i * 2, history[i]);
                if (i == count - 1) continue;
                Vector3 direction = history[i + 1] - history[i];
                Vector3 side = Vector3.Cross(direction.normalized, Vector3.forward);
                if (side.sqrMagnitude < 0.01f) side = Vector3.right;
                wake.SetPosition(i * 2 + 1, (history[i] + history[i + 1]) * 0.5f + side.normalized * (i % 2 == 0 ? 0.07f : -0.07f));
            }
        }
    }
}
