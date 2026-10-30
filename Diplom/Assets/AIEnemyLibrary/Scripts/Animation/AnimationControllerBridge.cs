using UnityEngine;
using UnityEngine.AI;

namespace AIEnemyLibrary.Animation
{
    public class AnimationControllerBridge : MonoBehaviour
    {
        [Header("AnimationControllerBridge - передача параметров в Animator")]

        [SerializeField] private Animator animator;
        [SerializeField] private NavMeshAgent agent;

        [Header("Animator Parameters")]
        [SerializeField] private string speedParam = "Speed";
        [SerializeField] private string isMovingParam = "IsMoving";
        [SerializeField] private string attackTriggerParam = "AttackTrigger";
        [SerializeField] private string alertedParam = "Alerted";
        [SerializeField] private string attackModeParam = "AttackMode";

        public Animator GetAnimator() => animator;

        private void Awake()
        {
            if (agent == null)
                agent = GetComponent<NavMeshAgent>();

            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            if (animator == null || agent == null)
                return;

            float speed = agent.velocity.magnitude;
            animator.SetFloat(speedParam, speed);
            animator.SetBool(isMovingParam, speed > 0.1f);
        }

        public void SetAttackMode(int mode)
        {
            if (animator != null)
                animator.SetInteger(attackModeParam, mode);
        }

        public void TriggerAttack()
        {
            if (animator != null)
                animator.SetTrigger(attackTriggerParam);
        }

        public void SetAlerted(bool value)
        {
            if (animator != null)
                animator.SetBool(alertedParam, value);
        }
    }
}