using UnityEngine;

namespace VaporwaveBallShowcase
{
    /// <summary>Looping presentation motion for comparing ball effects in Play mode.</summary>
    public sealed class BallBounce : MonoBehaviour
    {
        [Header("Ball")]
        public Transform ball;
        public Transform visual;
        public ParticleSystem impactParticles;
        public Transform[] spiralEmitters;

        [Header("Bounce preview")]
        [Min(0.1f)] public float bounceHeight = 2.8f;
        [Min(0.2f)] public float bouncePeriod = 1.8f;
        [Range(0f, 1f)] public float phaseOffset;
        [Min(0f)] public float sidewaysMotion = 0.35f;
        public float spinSpeed = 110f;
        public float spiralSpeed = 250f;
        [Min(0)] public int impactParticleCount = 24;

        Vector3 restPosition;
        Quaternion restRotation;
        float elapsed;
        int previousCycle;

        void Awake()
        {
            if (ball == null) { enabled = false; return; }
            restPosition = ball.localPosition;
            if (visual != null) restRotation = visual.localRotation;
        }

        void OnEnable()
        {
            elapsed = 0f;
            previousCycle = 0;
            foreach (var trail in GetComponentsInChildren<TrailRenderer>()) trail.Clear();
        }

        void Update()
        {
            elapsed += Time.deltaTime;
            float cycles = elapsed / Mathf.Max(0.2f, bouncePeriod) + phaseOffset;
            int cycle = Mathf.FloorToInt(cycles);
            float t = cycles - cycle;
            ball.localPosition = restPosition + new Vector3(
                Mathf.Sin(t * Mathf.PI * 2f) * sidewaysMotion,
                4f * bounceHeight * t * (1f - t), 0f);
            if (visual != null)
                visual.localRotation = restRotation * Quaternion.Euler(elapsed * spinSpeed, elapsed * spinSpeed * 0.7f, 0f);
            if (spiralEmitters != null)
            {
                for (int i = 0; i < spiralEmitters.Length; i++)
                {
                    if (spiralEmitters[i] == null) continue;
                    float angle = (elapsed * spiralSpeed + i * 180f) * Mathf.Deg2Rad;
                    spiralEmitters[i].localPosition = new Vector3(Mathf.Cos(angle) * 0.36f, 0f, Mathf.Sin(angle) * 0.36f);
                }
            }
            if (cycle != previousCycle && impactParticles != null)
                impactParticles.Emit(impactParticleCount);
            previousCycle = cycle;
        }

        void OnDisable()
        {
            if (ball != null) ball.localPosition = restPosition;
            if (visual != null) visual.localRotation = restRotation;
            foreach (var trail in GetComponentsInChildren<TrailRenderer>()) trail.Clear();
        }
    }
}
