using UnityEngine;

namespace AIEnemyLibrary.Sensors
{
    public class NoiseEmitter : MonoBehaviour
    {
        [Header("NoiseEmitter - генерация шума")]

        [Tooltip("Радиус слышимости шума.")]
        [SerializeField] private float noiseRadius = 8f;

        [Tooltip("Тег шума.")]
        [SerializeField] private string noiseTag = "PlayerNoise";

        [Tooltip("Клавиша для теста шума.")]
        [SerializeField] private KeyCode testNoiseKey = KeyCode.N;

        [Tooltip("Разрешить тестовый шум по кнопке.")]
        [SerializeField] private bool allowManualTestNoise = true;

        [Tooltip("Показывать радиус шума в Scene.")]
        [SerializeField] private bool drawGizmos = true;

        [Tooltip("Цвет радиуса шума.")]
        [SerializeField] private Color gizmoColor = new Color(0f, 0.6f, 1f, 0.3f);

        [Tooltip("Показывать вспышку шума при создании.")]
        [SerializeField] private bool showNoisePulse = true;

        [Tooltip("Длительность отображения вспышки шума.")]
        [SerializeField] private float pulseDuration = 0.5f;

        private float lastNoiseTime = -999f;

        public void EmitNoise()
        {
            NoiseSystem.MakeNoise(transform.position, noiseRadius, gameObject, noiseTag);

            lastNoiseTime = Time.time;

            Debug.Log($"{name} created noise. Radius: {noiseRadius}");
        }

        private void Update()
        {
            if (allowManualTestNoise && Input.GetKeyDown(testNoiseKey))
            {
                EmitNoise();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos)
                return;

            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, noiseRadius);
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos)
                return;

            // Вспышка при шуме
            if (showNoisePulse && Application.isPlaying)
            {
                float t = Time.time - lastNoiseTime;

                if (t < pulseDuration)
                {
                    float normalized = t / pulseDuration;
                    float radius = Mathf.Lerp(0f, noiseRadius, normalized);

                    Gizmos.color = new Color(0f, 1f, 1f, 0.8f);
                    Gizmos.DrawWireSphere(transform.position, radius);
                }
            }
        }
    }
}