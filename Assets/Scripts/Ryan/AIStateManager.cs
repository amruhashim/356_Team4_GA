using UnityEngine;

public class AIStateManager : MonoBehaviour
{
    // Define the AIState enum within the AIStateManager class
    public enum AIState
    {
        Patrolling,
        Chasing,
        ReturningToLastKnownPosition
    }

    // Public variable to hold the current state
    public AIState currentState = AIState.Patrolling;

    public PatrolAgent patrolAgent;
    public Chase chaseScript;

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
                break;
        }
    }

    public void ChangeState(AIState newState)
    {
        currentState = newState;
        Update();  // Ensure that the state change is reflected immediately
    }
}
