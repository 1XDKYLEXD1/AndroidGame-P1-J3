using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIFightBehaviourState
{
    Nearby,
    MediumRange,
    FarAway,
    Anywhere
};

public enum EntityAnimationState
{
    Idle,
    Walking,
    Running
};

public enum AIFightingMoves
{
    Slice,
    Stab,
    Block,
    DashTowards,
    DashAway,
    Jump,
    JumpAttack,
};

public class AIBehaviour : MonoBehaviour
{
    [SerializeField] Transform m_player;
    Rigidbody2D m_myrgigibody;

    AIFightBehaviourState m_fightbehaviour;
    AIFightingMoves m_fightingmoves;
    EntityAnimationState m_animationstate;

    void Awake()
    {
        m_myrgigibody = GetComponent<Rigidbody2D>();
        m_animationstate = EntityAnimationState.Idle;
    }

    void Update()
    {
        Debug.Log("Distance between P and AI: " + DistanceCheck());
    }

    float DistanceCheck()
    {
        float distance = Vector2.Distance(transform.position, m_player.transform.position);
        return distance;
    }
}
