using UnityEngine;

namespace AIEnemyLibrary.Core
{
    /// <summary>
    /// Projectile - простой снаряд для дальних атак.
    /// Летит вперёд, наносит урон при попадании и уничтожается.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile - настройки снаряда")]

        [Tooltip("Скорость полёта снаряда.")]
        [SerializeField] private float speed = 20f;

        [Tooltip("Время жизни снаряда в секундах.")]
        [SerializeField] private float lifetime = 5f;

        [Tooltip("Слои, которым снаряд может наносить урон.")]
        [SerializeField] private LayerMask hitMask;

        [Tooltip("Уничтожать ли снаряд при любом столкновении, даже если урон не был нанесён.")]
        [SerializeField] private bool destroyOnAnyHit = true;

        [Tooltip("Показывать ли отладочные сообщения в Console.")]
        [SerializeField] private bool debugLogs = true;

        private int damage = 10;
        private Transform owner;
        private bool hasHit = false;

        /// <summary>
        /// Инициализация снаряда после создания.
        /// </summary>
        public void Initialize(int projectileDamage, Transform projectileOwner)
        {
            damage = projectileDamage;
            owner = projectileOwner;
        }

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleHit(other);
        }

        private void OnCollisionEnter(Collision collision)
        {
            HandleHit(collision.collider);
        }

        private void HandleHit(Collider other)
        {
            if (hasHit || other == null)
                return;

            // Игнорируем самого владельца
            if (owner != null)
            {
                if (other.transform == owner || other.transform.root == owner.root)
                    return;
            }

            bool layerMatched = (hitMask.value & (1 << other.gameObject.layer)) != 0;

            if (!layerMatched)
            {
                if (debugLogs)
                    Debug.Log($"Projectile hit {other.name}, but layer is not in HitMask.");

                if (destroyOnAnyHit)
                {
                    hasHit = true;
                    Destroy(gameObject);
                }

                return;
            }

            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable == null)
                damageable = other.GetComponentInParent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(damage);

                if (debugLogs)
                    Debug.Log($"Projectile dealt {damage} damage to {other.name}");
            }
            else
            {
                if (debugLogs)
                    Debug.LogWarning($"Projectile hit {other.name}, but no IDamageable was found.");
            }

            hasHit = true;
            Destroy(gameObject);
        }
    }
}