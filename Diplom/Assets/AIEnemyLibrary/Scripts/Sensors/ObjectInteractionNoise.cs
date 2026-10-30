using UnityEngine;

namespace AIEnemyLibrary.Sensors
{
    /// <summary>
    /// Универсальный шумовой компонент для предметов.
    /// Может издавать шум при поднятии, броске и столкновениях.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class ObjectInteractionNoise : MonoBehaviour
    {
        [Header("ObjectInteractionNoise - шумы взаимодействия с предметом")]

        [Tooltip("AudioSource предмета.")]
        [SerializeField] private AudioSource audioSource;

        [Tooltip("Показывать ли debug-логи.")]
        [SerializeField] private bool debugLogs = false;

        [Header("Шум при поднятии в руки")]
        [SerializeField]
        private NoiseActionSettings pickupNoise = new NoiseActionSettings
        {
            enabled = true,
            playAudio = true,
            emitNoiseEvent = true,
            volume = 1f,
            noiseRadius = 4f,
            noiseTag = "Pickup"
        };

        [Header("Шум при броске / отпускании")]
        [SerializeField]
        private NoiseActionSettings dropNoise = new NoiseActionSettings
        {
            enabled = true,
            playAudio = true,
            emitNoiseEvent = true,
            volume = 1f,
            noiseRadius = 7f,
            noiseTag = "Drop"
        };

        [Header("Шум при толчке / столкновении")]
        [SerializeField]
        private NoiseActionSettings pushNoise = new NoiseActionSettings
        {
            enabled = true,
            playAudio = true,
            emitNoiseEvent = true,
            volume = 1f,
            noiseRadius = 5f,
            noiseTag = "Push"
        };

        [Tooltip("Создавать ли шум автоматически при столкновении.")]
        [SerializeField] private bool emitOnCollision = true;

        [Tooltip("Минимальная сила столкновения, чтобы создать шум.")]
        [SerializeField] private float collisionImpulseThreshold = 1.5f;

        [Tooltip("Минимальная пауза между шумами столкновения.")]
        [SerializeField] private float collisionCooldown = 0.2f;

        private float lastCollisionNoiseTime = -999f;

        private void Awake()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();

            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
            audioSource.loop = false;
        }
        public void NotifyPickedUp()
        {
            Emit(pickupNoise, transform.position);
        }

        public void NotifyDropped()
        {
            Emit(dropNoise, transform.position);
        }

        public void NotifyPushed()
        {
            Emit(pushNoise, transform.position);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!emitOnCollision)
                return;

            if (Time.time < lastCollisionNoiseTime + collisionCooldown)
                return;

            if (collision.impulse.magnitude < collisionImpulseThreshold)
                return;

            lastCollisionNoiseTime = Time.time;

            Vector3 position = collision.contacts.Length > 0
                ? collision.contacts[0].point
                : transform.position;

            Emit(pushNoise, position);

            if (debugLogs)
                Debug.Log($"{name} emitted collision noise. Impulse={collision.impulse.magnitude:F2}");
        }

        private void Emit(NoiseActionSettings settings, Vector3 position)
        {
            if (settings == null || !settings.enabled)
                return;

            if (settings.playAudio && settings.clip != null && audioSource != null)
                audioSource.PlayOneShot(settings.clip, settings.volume);

            if (settings.emitNoiseEvent)
                NoiseSystem.MakeNoise(position, settings.noiseRadius, gameObject, settings.noiseTag);

            if (debugLogs)
                Debug.Log($"{name} emitted object interaction noise: {settings.noiseTag}");
        }
    }
}