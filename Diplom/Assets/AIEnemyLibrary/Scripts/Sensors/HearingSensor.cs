using UnityEngine;

namespace AIEnemyLibrary.Sensors
{
    /// <summary>
    /// HearingSensor - компонент слуха врага.
    /// Реагирует на глобальные события шума в заданном радиусе.
    /// </summary>
    public class HearingSensor : MonoBehaviour
    {
        [Header("HearingSensor - восприятие шума")]

        [Tooltip("Максимальный радиус, в котором враг может услышать шум.")]
        [SerializeField] private float hearingRadius = 10f;

        [Tooltip("Слои источников шума, которые враг может слышать.")]
        [SerializeField] private LayerMask sourceMask;

        [Tooltip("Игнорировать ли шумы от самого себя.")]
        [SerializeField] private bool ignoreOwnNoise = true;

        [Tooltip("Сохранять ли информацию о шуме в памяти цели.")]
        [SerializeField] private bool writeNoiseToMemory = true;

        [Tooltip("Показывать ли debug-сообщения в Console.")]
        [SerializeField] private bool debugLogs = false;

        public bool HasHeardNoise { get; private set; }
        public Vector3 LastHeardPosition { get; private set; }
        public GameObject LastHeardSource { get; private set; }

        private TargetMemory targetMemory;

        private void Awake()
        {
            targetMemory = GetComponent<TargetMemory>();
        }

        private void OnEnable()
        {
            NoiseSystem.OnNoiseMade += HandleNoise;
        }

        private void OnDisable()
        {
            NoiseSystem.OnNoiseMade -= HandleNoise;
        }

        private void HandleNoise(NoiseSystem.NoiseEvent noiseEvent)
        {
            if (ignoreOwnNoise && noiseEvent.Source == gameObject)
                return;

            if (noiseEvent.Source != null)
            {
                bool sourceAllowed = (sourceMask.value & (1 << noiseEvent.Source.layer)) != 0;

                if (sourceMask.value != 0 && !sourceAllowed)
                    return;
            }

            float distance = Vector3.Distance(transform.position, noiseEvent.Position);
            float effectiveRadius = Mathf.Min(hearingRadius, noiseEvent.Radius);

            if (distance > effectiveRadius)
                return;

            HasHeardNoise = true;
            LastHeardPosition = noiseEvent.Position;
            LastHeardSource = noiseEvent.Source;

            if (writeNoiseToMemory && targetMemory != null)
                targetMemory.RememberHeardPosition(noiseEvent.Position);

            if (debugLogs)
            {
                string sourceName = noiseEvent.Source != null ? noiseEvent.Source.name : "Unknown";
                Debug.Log($"{name} heard noise from {sourceName} at {noiseEvent.Position}");
            }
        }

        public void ClearHeardNoise()
        {
            HasHeardNoise = false;
            LastHeardSource = null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, hearingRadius);

            if (HasHeardNoise)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(LastHeardPosition, 0.25f);
                Gizmos.DrawLine(transform.position, LastHeardPosition);
            }
        }
    }
}