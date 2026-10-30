using System.Collections.Generic;
using UnityEngine;
using AIEnemyLibrary.Core;

namespace AIEnemyLibrary.Behaviours
{
    /// <summary>
    /// PatrolBehaviour - отвечает за патрулирование врага между заданными точками.
    /// »спользуетс€, когда враг не видит цель и не находитс€ в состо€нии атаки.
    /// </summary>
    public class PatrolBehaviour : AIBehaviour
    {
        public enum PatrolMode
        {
            Loop,
            PingPong,
            Random
        }

        [Header("PatrolBehaviour - патрулирование врага между точками")]

        [Tooltip("—писок точек патрулировани€. ¬раг будет последовательно перемещатьс€ между ними.")]
        [SerializeField] private List<Transform> patrolPoints = new List<Transform>();

        [Tooltip("–ежим обхода точек: Loop - по кругу, PingPong - туда и обратно, Random - случайна€ точка.")]
        [SerializeField] private PatrolMode patrolMode = PatrolMode.Loop;

        [Tooltip("—корость движени€ врага во врем€ патрулировани€.")]
        [SerializeField] private float patrolSpeed = 2f;

        [Tooltip("Ќа каком рассто€нии от точки считаетс€, что враг уже дошЄл до неЄ.")]
        [SerializeField] private float pointReachDistance = 0.5f;

        [Tooltip("ƒолжен ли враг останавливатьс€ на точке на некоторое врем€.")]
        [SerializeField] private bool waitOnPoint = true;

        [Tooltip("ћинимальное врем€ ожидани€ на точке.")]
        [SerializeField] private float minWaitTime = 1f;

        [Tooltip("ћаксимальное врем€ ожидани€ на точке.")]
        [SerializeField] private float maxWaitTime = 2.5f;

        private int currentPointIndex = 0;
        private bool movingForward = true;
        private bool isWaiting = false;
        private float waitTimer = 0f;

        public override bool CanRun()
        {
            if (patrolPoints == null || patrolPoints.Count == 0)
                return false;

            if (controller.TargetMemory != null)
            {
                if (controller.TargetMemory.HasTarget)
                    return false;

                if (controller.TargetMemory.HasFreshMemory)
                    return false;
            }

            return true;
        }

        public override void OnEnterState()
        {
            if (controller.Agent == null || patrolPoints.Count == 0)
                return;

            controller.Agent.speed = patrolSpeed;
            controller.Agent.isStopped = false;
            controller.Agent.stoppingDistance = 0f;

            MoveToCurrentPoint();
        }

        public override void OnUpdateState()
        {
            if (controller.Agent == null || patrolPoints.Count == 0)
                return;

            if (isWaiting)
            {
                waitTimer -= Time.deltaTime;

                if (waitTimer <= 0f)
                {
                    isWaiting = false;
                    SelectNextPoint();
                    MoveToCurrentPoint();
                }

                return;
            }

            float distanceToPoint = Vector3.Distance(
                transform.position,
                patrolPoints[currentPointIndex].position
            );

            if (distanceToPoint <= pointReachDistance)
            {
                if (waitOnPoint)
                {
                    isWaiting = true;
                    waitTimer = Random.Range(minWaitTime, maxWaitTime);

                    if (controller.Agent != null)
                    {
                        controller.Agent.isStopped = true;
                        controller.Agent.ResetPath();
                    }
                }
                else
                {
                    SelectNextPoint();
                    MoveToCurrentPoint();
                }
            }
        }

        public override void OnExitState()
        {
            if (controller.Agent != null)
            {
                controller.Agent.isStopped = false;
            }

            isWaiting = false;
        }

        private void MoveToCurrentPoint()
        {
            if (controller.Agent == null || patrolPoints.Count == 0)
                return;

            controller.Agent.isStopped = false;
            controller.Agent.SetDestination(patrolPoints[currentPointIndex].position);
        }

        private void SelectNextPoint()
        {
            if (patrolPoints.Count <= 1)
                return;

            switch (patrolMode)
            {
                case PatrolMode.Loop:
                    currentPointIndex = (currentPointIndex + 1) % patrolPoints.Count;
                    break;

                case PatrolMode.PingPong:
                    if (movingForward)
                    {
                        currentPointIndex++;

                        if (currentPointIndex >= patrolPoints.Count - 1)
                        {
                            currentPointIndex = patrolPoints.Count - 1;
                            movingForward = false;
                        }
                    }
                    else
                    {
                        currentPointIndex--;

                        if (currentPointIndex <= 0)
                        {
                            currentPointIndex = 0;
                            movingForward = true;
                        }
                    }
                    break;

                case PatrolMode.Random:
                    int nextIndex = currentPointIndex;

                    while (nextIndex == currentPointIndex && patrolPoints.Count > 1)
                    {
                        nextIndex = Random.Range(0, patrolPoints.Count);
                    }

                    currentPointIndex = nextIndex;
                    break;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (patrolPoints == null || patrolPoints.Count == 0)
                return;

            Gizmos.color = Color.green;

            for (int i = 0; i < patrolPoints.Count; i++)
            {
                if (patrolPoints[i] == null)
                    continue;

                Gizmos.DrawWireSphere(patrolPoints[i].position, 0.2f);

                if (i < patrolPoints.Count - 1 && patrolPoints[i + 1] != null)
                {
                    Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                }
            }

            if (patrolMode == PatrolMode.Loop && patrolPoints.Count > 1)
            {
                if (patrolPoints[0] != null && patrolPoints[patrolPoints.Count - 1] != null)
                {
                    Gizmos.DrawLine(
                        patrolPoints[patrolPoints.Count - 1].position,
                        patrolPoints[0].position
                    );
                }
            }
        }
    }
}