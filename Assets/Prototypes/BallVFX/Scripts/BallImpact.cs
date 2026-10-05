using UnityEngine;

namespace VaporwaveBallShowcase
{
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public sealed class BallImpact : MonoBehaviour
    {
        [Header("Collision particles")]
        public ParticleSystem impactTemplate;
        [Min(1)] public int particleCount = 24;
        [Min(0f)] public float minimumImpactSpeed = 0.6f;
        [Min(0f)] public float impactCooldown = 0.08f;

        [Header("Optional cosmetic spiral trails")]
        public Transform[] spiralEmitters;
        public float spiralSpeed = 250f;
        public float spiralRadius = 0.36f;

        public int ImpactCount { get; private set; }
        public Vector3 LastImpactPoint { get; private set; }
        float lastImpactTime = float.NegativeInfinity;
        float orbitAngle;

        void OnEnable()
        {
            ImpactCount = 0;
            lastImpactTime = float.NegativeInfinity;
            foreach (var trail in GetComponentsInChildren<TrailRenderer>()) trail.Clear();
            if (impactTemplate != null)
                impactTemplate.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        void Update()
        {
            // Only the VFX emitter positions rotate; Rigidbody physics moves the ball.
            if (spiralEmitters == null) return;
            orbitAngle += spiralSpeed * Time.deltaTime;
            for (int i = 0; i < spiralEmitters.Length; i++)
            {
                if (spiralEmitters[i] == null) continue;
                float angle = (orbitAngle + i * 180f) * Mathf.Deg2Rad;
                spiralEmitters[i].localPosition = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * spiralRadius;
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (impactTemplate == null || collision.contactCount == 0 || Time.time - lastImpactTime < impactCooldown) return;
            var contact = collision.GetContact(0);
            float speed = Mathf.Abs(Vector3.Dot(collision.relativeVelocity, contact.normal));
            if (speed < minimumImpactSpeed) return;
            lastImpactTime = Time.time;
            ImpactCount++;
            LastImpactPoint = contact.point;

            // A detached world-space burst stays at the hit surface as the ball rebounds.
            var burst = Instantiate(impactTemplate, contact.point + contact.normal * 0.012f,
                Quaternion.FromToRotation(Vector3.up, contact.normal));
            burst.name = "Ball Impact Burst";
            burst.transform.localScale = impactTemplate.transform.lossyScale;
            burst.gameObject.SetActive(true);
            burst.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = burst.main;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            burst.Play(true);
            burst.Emit(particleCount);
            burst.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            Destroy(burst.gameObject, main.startLifetime.constantMax / Mathf.Max(0.01f, main.simulationSpeed) + 0.2f);
        }
    }
}
