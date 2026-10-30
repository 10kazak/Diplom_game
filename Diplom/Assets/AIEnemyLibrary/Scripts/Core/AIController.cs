using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using AIEnemyLibrary.Sensors;
using AIEnemyLibrary.Animation;

namespace AIEnemyLibrary.Core
{
    public class AIController : MonoBehaviour
    {
        [Header("AIController - основные ссылки")]

        [Tooltip("NavMeshAgent врага.")]
        [SerializeField] private NavMeshAgent agent;

        [Tooltip("Компонент зрения.")]
        [SerializeField] private VisionSensor visionSensor;

        [Tooltip("Компонент слуха.")]
        [SerializeField] private HearingSensor hearingSensor;

        [Tooltip("Память о цели.")]
        [SerializeField] private TargetMemory targetMemory;

        [Tooltip("Мост к Animator.")]
        [SerializeField] private AnimationControllerBridge animationBridge;

        private AIBehaviour[] behaviours;
        private AIBehaviour currentBehaviour;

        public NavMeshAgent Agent => agent;
        public VisionSensor VisionSensor => visionSensor;
        public HearingSensor HearingSensor => hearingSensor;
        public TargetMemory TargetMemory => targetMemory;
        public AnimationControllerBridge AnimationBridge => animationBridge;
        public AIBehaviour CurrentBehaviour => currentBehaviour;

        private void Awake()
        {
            if (agent == null)
                agent = GetComponent<NavMeshAgent>();

            if (visionSensor == null)
                visionSensor = GetComponent<VisionSensor>();

            if (hearingSensor == null)
                hearingSensor = GetComponent<HearingSensor>();

            if (targetMemory == null)
                targetMemory = GetComponent<TargetMemory>();

            if (animationBridge == null)
                animationBridge = GetComponent<AnimationControllerBridge>();

            behaviours = GetComponents<AIBehaviour>()
                .OrderByDescending(b => b.Priority)
                .ToArray();
        }

        private void Update()
        {
            UpdateTargetMemory();
            SelectBehaviour();
            currentBehaviour?.OnUpdateState();
        }

        private void UpdateTargetMemory()
        {
            if (visionSensor != null && targetMemory != null)
            {
                Transform visibleTarget = visionSensor.GetVisibleTarget();

                if (visibleTarget != null)
                {
                    targetMemory.RememberTarget(visibleTarget);
                    animationBridge?.SetAlerted(true);

                    if (hearingSensor != null)
                        hearingSensor.ClearHeardNoise();

                    return;
                }
            }

            if (targetMemory != null && !targetMemory.HasFreshMemory)
            {
                targetMemory.ForgetTarget();
                animationBridge?.SetAlerted(false);
            }
        }

        private void SelectBehaviour()
        {
            AIBehaviour nextBehaviour = null;

            foreach (AIBehaviour behaviour in behaviours)
            {
                if (!behaviour.BehaviourEnabled)
                    continue;

                if (behaviour.CanRun())
                {
                    nextBehaviour = behaviour;
                    break;
                }
            }

            if (nextBehaviour == currentBehaviour)
                return;

            currentBehaviour?.OnExitState();
            currentBehaviour = nextBehaviour;
            currentBehaviour?.OnEnterState();
        }
    }
}