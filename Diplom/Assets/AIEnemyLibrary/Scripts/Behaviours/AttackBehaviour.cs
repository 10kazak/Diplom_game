using System.Collections;
using UnityEngine;
using AIEnemyLibrary.Core;
using AIEnemyLibrary.Sensors;

namespace AIEnemyLibrary.Behaviours
{
    public class AttackBehaviour : AIBehaviour
    {
        public enum AttackType
        {
            Melee = 0,
            Ranged = 1
        }

        [Header("AttackBehaviour - атака цели в радиусе поражения")]

        [Tooltip("Тип атаки: ближняя (Melee) или дальняя (Ranged).")]
        [SerializeField] private AttackType attackType = AttackType.Melee;

        [Tooltip("Максимальная дистанция, на которой враг может начать атаку.")]
        [SerializeField] private float attackDistance = 2f;

        [Tooltip("Пауза после завершения анимации атаки перед следующей атакой.")]
        [SerializeField] private float attackCooldown = 1f;

        [Tooltip("Количество урона.")]
        [SerializeField] private int damage = 10;

        [Tooltip("Должен ли враг поворачиваться к цели во время атаки.")]
        [SerializeField] private bool rotateToTarget = true;

        [Tooltip("Скорость поворота к цели.")]
        [SerializeField] private float rotationSpeed = 8f;

        [Header("Animation Timing Settings")]

        [Tooltip("Во сколько от длины melee-анимации наносить урон.")]
        [Range(0.05f, 0.95f)]
        [SerializeField] private float meleeDamageNormalizedTime = 0.45f;

        [Tooltip("Во сколько от длины ranged-анимации выпускать снаряд.")]
        [Range(0.05f, 0.95f)]
        [SerializeField] private float rangedShotNormalizedTime = 0.35f;

        [Tooltip("Запасное время анимации атаки, если не удалось автоматически получить длину клипа.")]
        [SerializeField] private float fallbackAttackAnimationDuration = 1f;

        [Tooltip("Небольшая пауза после Trigger, чтобы Animator успел войти в состояние атаки.")]
        [SerializeField] private float animatorTransitionDelay = 0.05f;

        [Header("Melee Attack Settings")]
        [Tooltip("Радиус поражения ближней атаки.")]
        [SerializeField] private float meleeHitRadius = 1.9f;

        [Tooltip("Слои, которым можно нанести урон ближней атакой.")]
        [SerializeField] private LayerMask meleeTargetMask;

        [Header("Ranged Attack Settings")]
        [Tooltip("Точка выстрела.")]
        [SerializeField] private Transform muzzlePoint;

        [Tooltip("Префаб снаряда.")]
        [SerializeField] private GameObject projectilePrefab;

        [Header("Шумы атаки")]
        [Tooltip("Эмиттер шумов атаки. Можно не назначать.")]
        [SerializeField] private ActionNoiseEmitter actionNoiseEmitter;

        private bool isAttackInProgress = false;
        private float nextAttackAllowedTime = 0f;
        private float lastAttackStartTime = -999f;
        private Coroutine attackRoutine;

        public override bool CanRun()
        {
            if (isAttackInProgress)
                return true;

            if (controller.TargetMemory == null || !controller.TargetMemory.HasTarget)
                return false;

            Transform target = controller.TargetMemory.CurrentTarget;
            if (target == null)
                return false;

            float distance = Vector3.Distance(transform.position, target.position);
            return distance <= attackDistance;
        }

        protected override void Awake()
        {
            base.Awake();

            if (actionNoiseEmitter == null)
                actionNoiseEmitter = GetComponent<ActionNoiseEmitter>();
        }

        public override void OnEnterState()
        {
            if (controller.Agent != null)
            {
                controller.Agent.isStopped = true;
                controller.Agent.ResetPath();
            }
        }

        public override void OnUpdateState()
        {
            if (controller.TargetMemory == null || !controller.TargetMemory.HasTarget)
                return;

            Transform target = controller.TargetMemory.CurrentTarget;
            if (target == null)
                return;

            if (rotateToTarget)
                RotateTowardsTarget(target);

            if (isAttackInProgress)
                return;

            if (Time.time < nextAttackAllowedTime)
                return;

            StartAttack();
        }

        public override void OnExitState()
        {
            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
                attackRoutine = null;
            }

            isAttackInProgress = false;
        }

        private void StartAttack()
        {
            if (Time.time - lastAttackStartTime < 0.2f)
                return;

            lastAttackStartTime = Time.time;
            isAttackInProgress = true;

            if (attackType == AttackType.Melee)
                controller.AnimationBridge?.SetAttackMode(0);
            else
                controller.AnimationBridge?.SetAttackMode(1);

            controller.AnimationBridge?.TriggerAttack();

            if (actionNoiseEmitter != null)
                actionNoiseEmitter.EmitAttackNoise();

            attackRoutine = StartCoroutine(AttackRoutine());
        }

        private IEnumerator AttackRoutine()
        {
            yield return new WaitForSeconds(animatorTransitionDelay);

            float attackAnimationDuration = GetCurrentAttackClipLength();
            if (attackAnimationDuration <= 0f)
                attackAnimationDuration = fallbackAttackAnimationDuration;

            float actionTime = attackType == AttackType.Melee
                ? attackAnimationDuration * meleeDamageNormalizedTime
                : attackAnimationDuration * rangedShotNormalizedTime;

            yield return new WaitForSeconds(actionTime);

            if (attackType == AttackType.Melee)
                PerformMeleeAttack();
            else
                PerformRangedAttack();

            float remainingTime = Mathf.Max(0f, attackAnimationDuration - actionTime);
            yield return new WaitForSeconds(remainingTime);

            isAttackInProgress = false;
            nextAttackAllowedTime = Time.time + attackCooldown;
            attackRoutine = null;
        }

        private float GetCurrentAttackClipLength()
        {
            if (controller.AnimationBridge == null)
                return fallbackAttackAnimationDuration;

            Animator animator = controller.AnimationBridge.GetAnimator();
            if (animator == null)
                return fallbackAttackAnimationDuration;

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            if (!stateInfo.IsName("Attack_Melee") && !stateInfo.IsName("Attack_Ranged"))
            {
                AnimatorStateInfo nextStateInfo = animator.GetNextAnimatorStateInfo(0);

                if (nextStateInfo.IsName("Attack_Melee") || nextStateInfo.IsName("Attack_Ranged"))
                    stateInfo = nextStateInfo;
            }

            return stateInfo.length > 0f ? stateInfo.length : fallbackAttackAnimationDuration;
        }

        private void RotateTowardsTarget(Transform target)
        {
            Vector3 targetPoint = target.position;

            Collider targetCollider = target.GetComponent<Collider>();
            if (targetCollider == null)
                targetCollider = target.GetComponentInParent<Collider>();

            if (targetCollider != null)
                targetPoint = targetCollider.bounds.center;

            Vector3 lookDirection = targetPoint - transform.position;
            lookDirection.y = 0f;

            // Если цель слишком близко, не пытаемся крутиться
            if (lookDirection.sqrMagnitude < 0.25f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        private void PerformMeleeAttack()
        {
            Transform target = controller.TargetMemory != null ? controller.TargetMemory.CurrentTarget : null;
            if (target == null)
                return;

            Collider[] hits = Physics.OverlapSphere(
                transform.position,
                meleeHitRadius,
                meleeTargetMask
            );

            foreach (Collider hit in hits)
            {
                if (hit.transform != target && hit.transform.root != target.root)
                    continue;

                IDamageable damageable = hit.GetComponent<IDamageable>();
                if (damageable == null)
                    damageable = hit.GetComponentInParent<IDamageable>();

                if (damageable != null)
                {
                    damageable.TakeDamage(damage);
                    Debug.Log($"Melee hit dealt {damage} damage to {hit.name}");
                    return;
                }
            }
        }

        private void PerformRangedAttack()
        {
            if (projectilePrefab == null || muzzlePoint == null)
                return;

            Transform target = controller.TargetMemory != null
                ? controller.TargetMemory.CurrentTarget
                : null;

            Quaternion spawnRotation = muzzlePoint.rotation;

            if (target != null)
            {
                Vector3 targetPoint = target.position;

                Collider targetCollider = target.GetComponent<Collider>();
                if (targetCollider == null)
                    targetCollider = target.GetComponentInParent<Collider>();

                if (targetCollider != null)
                    targetPoint = targetCollider.bounds.center;

                Vector3 directionToTarget = targetPoint - muzzlePoint.position;

                if (directionToTarget.sqrMagnitude > 0.001f)
                    spawnRotation = Quaternion.LookRotation(directionToTarget.normalized);
            }

            GameObject projectileObject = Instantiate(
                projectilePrefab,
                muzzlePoint.position,
                spawnRotation
            );

            Projectile projectile = projectileObject.GetComponent<Projectile>();
            if (projectile != null)
                projectile.Initialize(damage, transform);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackDistance);

            if (attackType == AttackType.Melee)
            {
                Gizmos.color = new Color(1f, 0.4f, 0.4f, 1f);
                Gizmos.DrawWireSphere(transform.position, meleeHitRadius);
            }
        }
    }
}