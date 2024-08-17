using UnityEngine;

public class AIStateManager : MonoBehaviour
{
    public enum AIState
    {
        Patrolling,
        Chasing
    }

    public enum TargetType
    {
        Player,
        Drone
    }

    public AIState currentState = AIState.Patrolling;

    [Tooltip("Define the target tags here.")]
    public string[] targetTags = { "Player", "Drone" };

    public PatrolAgent patrolAgent;
    public Chase chaseScript;

    private TargetType detectedTargetType;

    private void Start()
    {
        patrolAgent = GetComponent<PatrolAgent>();
        chaseScript = GetComponent<Chase>();
    }

    private void Update()
    {
        switch (currentState)
        {
            case AIState.Patrolling:
                patrolAgent.enabled = true;
                chaseScript.enabled = false;
                break;

            case AIState.Chasing:
                patrolAgent.enabled = false;
                chaseScript.enabled = true;
                chaseScript.SetTarget(patrolAgent.GetDetectedTarget(), detectedTargetType);
                break;
        }
    }

    public void ChangeState(AIState newState, TargetType targetType)
    {
        detectedTargetType = targetType;
        currentState = newState;
        Update();
    }

    public void ChangeState(AIState newState)
    {
        currentState = newState;
        Update();
    }

    public string GetTagForTarget(TargetType targetType)
    {
        return targetTags[(int)targetType];
    }
}
