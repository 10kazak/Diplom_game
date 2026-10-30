using UnityEngine;

namespace AIEnemyLibrary.Core
{
    /// <summary>
    /// Базовый абстрактный класс для любого поведения ИИ.
    /// От него наследуются Idle, Chase, Attack, Patrol и другие поведения.
    /// AIController выбирает активное поведение по приоритету.
    /// </summary>
    public abstract class AIBehaviour : MonoBehaviour
    {
        [Header("СЛУЖЕБНЫЕ НАСТРОЙКИ ПОВЕДЕНИЯ")]
        [Tooltip("Приоритет поведения. Чем больше число, тем раньше AIController выберет это поведение.")]
        [SerializeField] private int priority = 0;

        [Tooltip("Включено ли это поведение. Если выключено, AIController его игнорирует.")]
        [SerializeField] private bool behaviourEnabled = true;

        protected AIController controller;

        public int Priority => priority;
        public bool BehaviourEnabled => behaviourEnabled;

        protected virtual void Awake()
        {
            controller = GetComponent<AIController>();
        }

        /// <summary>
        /// Проверка: можно ли сейчас запускать это поведение.
        /// </summary>
        public abstract bool CanRun();

        /// <summary>
        /// Вызывается один раз при входе в поведение.
        /// </summary>
        public virtual void OnEnterState() { }

        /// <summary>
        /// Вызывается каждый кадр, пока поведение активно.
        /// </summary>
        public virtual void OnUpdateState() { }

        /// <summary>
        /// Вызывается один раз при выходе из поведения.
        /// </summary>
        public virtual void OnExitState() { }
    }
}