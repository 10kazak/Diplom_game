using UnityEngine;
using AIEnemyLibrary.Core;

namespace AIEnemyLibrary.Behaviours
{
    public class ChaseBehaviour : AIBehaviour
    {
        [Header("ChaseBehaviour - преследование цели / проверка шума")]

        [Tooltip("—корость движени€ во врем€ преследовани€.")]
        [SerializeField] private float chaseSpeed = 3.5f;

        [Tooltip("ƒистанци€ остановки перед целью.")]
        [SerializeField] private float stopDistance = 2f;

        [Tooltip("ћаксимальное врем€ преследовани€ видимой цели.")]
        [SerializeField] private float maxChaseTime = 8f;

        [Tooltip("Ќа каком рассто€нии считать, что враг дошЄл до точки шума.")]
        [SerializeField] private float heardPointReachDistance = 1f;

        private float chaseStartTime;

        public override bool CanRun()
        {
            if (controller.TargetMemory == null)
                return false;

            // Chase только пока цель реально есть
            if (controller.TargetMemory.HasTarget && controller.TargetMemory.TimeSinceLastSeen() <= maxChaseTime)
                return true;

            // »дти к шуму тоже можно через Chase
            if (controller.TargetMemory.HasHeardMemory)
                return true;

            return false;
        }

        public override void OnEnterState()
        {
            chaseStartTime = Time.time;

            if (controller.Agent != null)
            {
                controller.Agent.speed = chaseSpeed;
                controller.Agent.isStopped = false;
                controller.Agent.stoppingDistance = stopDistance;
            }
        }

        public override void OnUpdateState()
        {
            if (controller.Agent == null || controller.TargetMemory == null)
                return;

            if (controller.TargetMemory.HasTarget)
            {
                if (Time.time - chaseStartTime > maxChaseTime)
                {
                    controller.TargetMemory.ForgetTarget();
                    return;
                }

                Vector3 targetPosition = controller.TargetMemory.CurrentTarget.position;
                controller.Agent.stoppingDistance = stopDistance;
                controller.Agent.isStopped = false;
                controller.Agent.SetDestination(targetPosition);
                return;
            }

            if (controller.TargetMemory.HasHeardMemory)
            {
                controller.Agent.stoppingDistance = 0f;
                controller.Agent.isStopped = false;
                controller.Agent.SetDestination(controller.TargetMemory.LastHeardPosition);

                float distance = Vector3.Distance(transform.position, controller.TargetMemory.LastHeardPosition);
                if (distance <= heardPointReachDistance)
                {
                    controller.TargetMemory.ClearHeardMemory();
                }
            }
        }

        public override void OnExitState()
        {
        }
    }
}