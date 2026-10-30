using System;
using UnityEngine;

namespace AIEnemyLibrary.Sensors
{
    [Serializable]
    public class NoiseActionSettings
    {
        [Tooltip("Включено ли это действие.")]
        public bool enabled = true;

        [Tooltip("Проигрывать ли AudioClip при этом действии.")]
        public bool playAudio = true;

        [Tooltip("Отправлять ли шум в систему слуха AI.")]
        public bool emitNoiseEvent = true;

        [Tooltip("Звук для этого действия. Можно оставить None.")]
        public AudioClip clip;

        [Tooltip("Громкость звука.")]
        [Range(0f, 1f)]
        public float volume = 1f;

        [Tooltip("Радиус шума для AI.")]
        public float noiseRadius = 8f;

        [Tooltip("Тег шума.")]
        public string noiseTag = "Default";
    }

    [RequireComponent(typeof(AudioSource))]
    public class ActionNoiseEmitter : MonoBehaviour
    {
        [Header("ActionNoiseEmitter - универсальные шумы действий")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private bool debugLogs = false;

        [Header("Gizmos")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private Color gizmoColor = new Color(0f, 0.6f, 1f, 0.35f);
        [SerializeField] private bool showNoisePulse = true;
        [SerializeField] private float pulseDuration = 0.5f;

        [Header("Ходьба")]
        [SerializeField]
        private NoiseActionSettings walkNoise = new NoiseActionSettings
        {
            enabled = true,
            playAudio = false,
            emitNoiseEvent = true,
            volume = 1f,
            noiseRadius = 5f,
            noiseTag = "Walk"
        };

        [Header("Бег")]
        [SerializeField]
        private NoiseActionSettings runNoise = new NoiseActionSettings
        {
            enabled = true,
            playAudio = false,
            emitNoiseEvent = true,
            volume = 1f,
            noiseRadius = 8f,
            noiseTag = "Run"
        };

        [Header("Прыжок")]
        [SerializeField]
        private NoiseActionSettings jumpNoise = new NoiseActionSettings
        {
            enabled = true,
            playAudio = false,
            emitNoiseEvent = true,
            volume = 1f,
            noiseRadius = 7f,
            noiseTag = "Jump"
        };

        [Header("Атака")]
        [SerializeField]
        private NoiseActionSettings attackNoise = new NoiseActionSettings
        {
            enabled = true,
            playAudio = false,
            emitNoiseEvent = true,
            volume = 1f,
            noiseRadius = 10f,
            noiseTag = "Attack"
        };

        [Header("Взаимодействие")]
        [SerializeField]
        private NoiseActionSettings interactNoise = new NoiseActionSettings
        {
            enabled = true,
            playAudio = false,
            emitNoiseEvent = true,
            volume = 1f,
            noiseRadius = 6f,
            noiseTag = "Interact"
        };

        [Header("Пользовательский шум")]
        [SerializeField]
        private NoiseActionSettings customNoise = new NoiseActionSettings
        {
            enabled = true,
            playAudio = false,
            emitNoiseEvent = true,
            volume = 1f,
            noiseRadius = 8f,
            noiseTag = "Custom"
        };

        private float lastCustomNoiseTime = -999f;

        public NoiseActionSettings WalkNoise => walkNoise;
        public NoiseActionSettings RunNoise => runNoise;
        public NoiseActionSettings JumpNoise => jumpNoise;
        public NoiseActionSettings AttackNoise => attackNoise;
        public NoiseActionSettings InteractNoise => interactNoise;
        public NoiseActionSettings CustomNoise => customNoise;

        private void Awake()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();

            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
            audioSource.loop = false;
        }

        public void EmitWalkNoise() => EmitOneShot(walkNoise, transform.position);
        public void EmitRunNoise() => EmitOneShot(runNoise, transform.position);
        public void EmitJumpNoise() => EmitOneShot(jumpNoise, transform.position);
        public void EmitAttackNoise() => EmitOneShot(attackNoise, transform.position);
        public void EmitInteractNoise() => EmitOneShot(interactNoise, transform.position);
        public void EmitCustomNoise() => EmitOneShot(customNoise, transform.position, true);

        public void StartLoopedMovementAudio(bool running)
        {
            NoiseActionSettings settings = running ? runNoise : walkNoise;

            if (settings == null || !settings.enabled || !settings.playAudio || settings.clip == null || audioSource == null)
                return;

            if (audioSource.isPlaying && audioSource.clip == settings.clip)
                return;

            audioSource.Stop();
            audioSource.loop = true;
            audioSource.clip = settings.clip;
            audioSource.volume = settings.volume;
            audioSource.Play();
        }

        public void StopLoopedMovementAudio()
        {
            if (audioSource == null)
                return;

            if (audioSource.isPlaying)
                audioSource.Stop();

            audioSource.loop = false;
            audioSource.clip = null;
        }

        public void EmitMovementNoiseEvent(bool running)
        {
            NoiseActionSettings settings = running ? runNoise : walkNoise;

            if (settings == null || !settings.enabled || !settings.emitNoiseEvent)
                return;

            NoiseSystem.MakeNoise(transform.position, settings.noiseRadius, gameObject, settings.noiseTag);

            if (debugLogs)
                Debug.Log($"{name} emitted movement noise: {settings.noiseTag}, radius={settings.noiseRadius}");
        }

        public void EmitCustomNoiseAt(Vector3 position, float radius, string tag = "Custom")
        {
            if (!customNoise.enabled)
                return;

            if (customNoise.playAudio && customNoise.clip != null && audioSource != null)
                audioSource.PlayOneShot(customNoise.clip, customNoise.volume);

            if (customNoise.emitNoiseEvent)
                NoiseSystem.MakeNoise(position, radius, gameObject, tag);

            lastCustomNoiseTime = Time.time;

            if (debugLogs)
                Debug.Log($"{name} emitted custom noise at {position}, radius={radius}, tag={tag}");
        }

        private void EmitOneShot(NoiseActionSettings settings, Vector3 position, bool isCustom = false)
        {
            if (settings == null || !settings.enabled)
                return;

            if (settings.playAudio && settings.clip != null && audioSource != null)
                audioSource.PlayOneShot(settings.clip, settings.volume);

            if (settings.emitNoiseEvent)
                NoiseSystem.MakeNoise(position, settings.noiseRadius, gameObject, settings.noiseTag);

            if (isCustom)
                lastCustomNoiseTime = Time.time;

            if (debugLogs)
                Debug.Log($"{name} emitted one-shot noise: {settings.noiseTag}, radius={settings.noiseRadius}");
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos)
                return;

            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, customNoise.noiseRadius);
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos || !showNoisePulse || !Application.isPlaying)
                return;

            float t = Time.time - lastCustomNoiseTime;
            if (t < pulseDuration)
            {
                float normalized = Mathf.Clamp01(t / pulseDuration);
                float radius = Mathf.Lerp(0f, customNoise.noiseRadius, normalized);

                Gizmos.color = new Color(0f, 1f, 1f, 0.9f);
                Gizmos.DrawWireSphere(transform.position, radius);
            }
        }
    }
}