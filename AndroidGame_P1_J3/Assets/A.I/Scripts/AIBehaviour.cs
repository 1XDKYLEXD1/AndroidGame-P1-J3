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

    //Variables to calculate what moveset to use;
    [SerializeField] float m_nearbynode;
    [SerializeField] float m_mediumnode;
    [SerializeField] float m_farnode;
    [SerializeField] float m_anywherenode;

    [SerializeField] float m_fastestmovespeed;
    [SerializeField] float m_slowestmovespeed;

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
        m_fightbehaviour = CalculateFightBehaviour(m_nearbynode, m_mediumnode, m_farnode, m_anywherenode);

        switch(m_fightbehaviour)
        {
            case AIFightBehaviourState.Nearby:
                //RunAwayFromTarget();
                break;

            case AIFightBehaviourState.MediumRange:
                //WalkAwayFromTarget();
                break;

            case AIFightBehaviourState.FarAway:
                RunTowardsTarget();
                break;

            case AIFightBehaviourState.Anywhere:
                break;
        }

        Debug.Log("Distance: " + DistanceCheck(m_player));
        Debug.Log("AIFightBehaviour: " + m_fightbehaviour);
    }

    float DistanceCheck(Transform target)
    {
        float distance = Vector2.Distance(transform.position, target.transform.position);

        return distance;
    }

    void RunTowardsTarget()
    {
        m_myrgigibody.velocity = new Vector2(m_fastestmovespeed, 0);
    }
    void WalkTowardsTarget()
    {
        m_myrgigibody.velocity = new Vector2(m_slowestmovespeed, 0);
    }

    void RunAwayFromTarget()
    {
        m_myrgigibody.velocity = new Vector2(-m_fastestmovespeed, 0);
    }
    void WalkAwayFromTarget()
    {
        m_myrgigibody.velocity = new Vector2(-m_slowestmovespeed, 0);
    }
    

    AIFightBehaviourState CalculateFightBehaviour(float nearnode, float mednode, float farnode, float anywnode)
    {
        float neardistance = nearnode - DistanceCheck(m_player);
        float meddistance = mednode - DistanceCheck(m_player);
        float fardistance = farnode - DistanceCheck(m_player);
        float anywdistance = anywnode - DistanceCheck(m_player);

        if(neardistance < 0) { neardistance = neardistance * (-1); }
        if (meddistance < 0) { meddistance = meddistance * (-1); }
        if (fardistance < 0) { fardistance = fardistance * (-1); }
        if (anywdistance < 0) { anywdistance = anywdistance * (-1); }

        Mathf.Abs(neardistance);
        Mathf.Abs(meddistance);
        Mathf.Abs(fardistance);
        Mathf.Abs(anywdistance);

        if (neardistance < meddistance && neardistance < fardistance && neardistance < anywdistance)
        {
            return AIFightBehaviourState.Nearby;
        }
        else if (meddistance < neardistance && meddistance < fardistance && meddistance < anywdistance)
        {
            return AIFightBehaviourState.MediumRange;
        }
        else if (fardistance < neardistance && fardistance < meddistance && fardistance < anywdistance)
        {
            return AIFightBehaviourState.FarAway;
        }
        else if (anywdistance < neardistance && anywdistance < meddistance && anywdistance < fardistance)
        {
            return AIFightBehaviourState.Anywhere;
        }
        else
        {
            Debug.LogError("Something is wrong in the AI calculations or every number is the same");
            return AIFightBehaviourState.Anywhere;
        }
    }
}
