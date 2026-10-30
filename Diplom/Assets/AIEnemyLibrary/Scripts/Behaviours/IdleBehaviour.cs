using AIEnemyLibrary.Core;

namespace AIEnemyLibrary.Behaviours
{
    /// <summary>
    /// ѕоведение бездействи€.
    /// »спользуетс€, когда врагу нечего делать:
    /// нет цели, нет активного патрул€, нет атаки.
    /// </summary>
    public class IdleBehaviour : AIBehaviour
    {
        public override bool CanRun()
        {
            return true;
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
        }

        public override void OnExitState()
        {
        }
    }
}