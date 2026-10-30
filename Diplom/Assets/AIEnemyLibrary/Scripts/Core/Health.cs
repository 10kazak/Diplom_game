using UnityEngine;

namespace AIEnemyLibrary.Core
{
    public class Health : MonoBehaviour, IDamageable
    {
        [Header("Health - хранение и обработка здоровья объекта")]

        [Tooltip("Максимальное здоровье объекта.")]
        [SerializeField] private int maxHealth = 100;

        [Tooltip("Текущее здоровье. Видно в Inspector для отладки.")]
        [SerializeField] private int currentHealth;

        [Tooltip("Уничтожать ли объект при смерти.")]
        [SerializeField] private bool destroyOnDeath = false;

        [Tooltip("Показывать ли сообщения в Console.")]
        [SerializeField] private bool debugLogs = true;

        private bool isDead = false;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsDead => isDead;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(int damage)
        {
            if (isDead)
                return;

            currentHealth -= damage;
            currentHealth = Mathf.Max(currentHealth, 0);

            if (debugLogs)
                Debug.Log($"{name} получил урон: {damage}. Осталось здоровья: {currentHealth}/{maxHealth}");

            if (currentHealth <= 0)
                Die();
        }

        private void Die()
        {
            if (isDead)
                return;

            isDead = true;

            if (debugLogs)
                Debug.Log($"{name} погиб.");

            if (destroyOnDeath)
                Destroy(gameObject);
        }
    }
}