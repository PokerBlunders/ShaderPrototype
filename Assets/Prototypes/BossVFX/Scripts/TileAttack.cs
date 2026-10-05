using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace VaporwaveBossVFX
{
    public sealed class TileAttack : MonoBehaviour
    {
        [Header("Preview controls")]
        public bool keyboardPreview = true;
#if ENABLE_INPUT_SYSTEM
        public Key previewKey = Key.G;
#else
        public KeyCode previewKey = KeyCode.G;
#endif
        [Header("Attack timing")]
        [Min(0.05f)] public float warningDuration = 1.1f;
        [Min(0.05f)] public float attackDuration = 1.2f;
        [Min(0f)] public float recoveryDuration = 0.7f;

        [Header("Effect references")]
        public GameObject warningVisuals;
        public Renderer[] warningLines;
        public Renderer hazardFill;
        public ParticleSystem warningSparks;
        public ParticleSystem[] attackJets;
        public ParticleSystem shockwave;
        public ParticleSystem detonation;

        public bool IsBusy { get; private set; }
        public bool IsDamaging { get; private set; }
        public int ActivationCount { get; private set; }
        public string Phase { get; private set; } = "Idle";
        Coroutine sequence;
        MaterialPropertyBlock block;
        static readonly int ColorId = Shader.PropertyToID("_BaseColor");
        readonly Color pink = new Color(1f, 0.025f, 0.22f);

        void Awake() { block = new MaterialPropertyBlock(); ResetEffects(); }

        void Update()
        {
            if (!keyboardPreview) return;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current[previewKey].wasPressedThisFrame) TriggerAttack();
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(previewKey)) TriggerAttack();
#endif
        }

        public void TriggerAttack()
        {
            if (!isActiveAndEnabled || IsBusy) return;
            sequence = StartCoroutine(AttackSequence());
        }

        IEnumerator AttackSequence()
        {
            IsBusy = true; ActivationCount++; Phase = "Warning";
            warningVisuals.SetActive(true);
            warningSparks.Play(true);
            float elapsed = 0;
            while (elapsed < warningDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / warningDuration);
                float pulse = 0.5f + 0.5f * Mathf.Sin(elapsed * Mathf.Lerp(14, 30, progress));
                SetWarningColor(pink * Mathf.Lerp(1.4f, 4f, pulse), Mathf.Lerp(0.07f, 0.23f, pulse));
                yield return null;
            }
            warningSparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            Phase = "Attack"; IsDamaging = true;
            SetWarningColor(new Color(1, 0.03f, 0.04f) * 5, 0.3f);
            foreach (var jet in attackJets) if (jet != null) jet.Play(true);
            shockwave.Play(true); shockwave.Emit(1);
            detonation.Play(true); detonation.Emit(38);
            yield return new WaitForSeconds(attackDuration);

            IsDamaging = false; Phase = "Recovery";
            foreach (var jet in attackJets) if (jet != null) jet.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            elapsed = 0;
            while (elapsed < recoveryDuration)
            {
                elapsed += Time.deltaTime;
                float fade = 1 - Mathf.Clamp01(elapsed / Mathf.Max(0.01f, recoveryDuration));
                SetWarningColor(pink * (fade * 2), fade * 0.15f);
                yield return null;
            }
            ResetEffects(); sequence = null;
        }

        void SetWarningColor(Color color, float alpha)
        {
            if (block == null) block = new MaterialPropertyBlock();
            color.a = 1;
            foreach (var renderer in warningLines)
            {
                if (renderer == null) continue;
                renderer.GetPropertyBlock(block); block.SetColor(ColorId, color); renderer.SetPropertyBlock(block);
            }
            if (hazardFill != null)
            {
                hazardFill.GetPropertyBlock(block); color.a = alpha;
                block.SetColor(ColorId, color); hazardFill.SetPropertyBlock(block);
            }
        }

        void ResetEffects()
        {
            IsBusy = false; IsDamaging = false; Phase = "Idle";
            if (warningVisuals != null) warningVisuals.SetActive(false);
            foreach (var ps in GetComponentsInChildren<ParticleSystem>(true))
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        void OnDisable()
        {
            if (sequence != null) StopCoroutine(sequence);
            sequence = null;
            ResetEffects();
        }
    }
}
