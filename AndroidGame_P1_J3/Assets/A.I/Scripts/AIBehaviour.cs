using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum IAIFightBehaviourState
{
    Nearby,
    MediumRange,
    FarAway,
    Anywhere
};

public enum IEntityAnimationState
{
    Idle,
    Walking,
    StabAttack,
    SlashAttack,
    Dashing,
    Jumping,
    SlashBlock,
    StabBlock
    //JumpAttack,
    //Running,
};

public enum IAIFightingMoves
{
    WalkTowards,
    WalkAway,
    DashTowards,
    DashAway,
    Slash,
    Stab,
    SlashBlock,
    StabBlock,
    Jump
    //JumpAttack,
    //RunTowards,
    //RunAway,
};

public enum IAIDifficulty
{
    Easy,
    Medium,
    Hard,
    Expert
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
    [SerializeField] float m_jumpspeed;

    bool m_inbattlemode;
    bool m_isdead;
    bool m_onground;

    [SerializeField] float m_fightdelay;
    float m_fightcounter;

    bool m_canbehitbystab;
    bool m_canbehitbyslash;

    Animator m_myanimator;

    [SerializeField] IAIDifficulty m_difficulty;
    IAIFightBehaviourState m_fightbehaviour;
    IAIFightingMoves m_fightingmovetouse;

    void Awake()
    {
        m_myrgigibody = GetComponent<Rigidbody2D>();
        m_myanimator = GetComponent<Animator>();
        //m_onground = true;
        //m_animationstate = IEntityAnimationState.Idle;
    }

    void Update()
    {
        if (m_fightingmovetouse != IAIFightingMoves.SlashBlock)
        {
            m_canbehitbyslash = true;
        }
        else { m_canbehitbyslash = false; }

        if (m_fightingmovetouse != IAIFightingMoves.StabBlock)
        {
            m_canbehitbystab = true;
        }
        else { m_canbehitbystab = false; }

        
        m_fightbehaviour = CalculateFightBehaviour(m_nearbynode, m_mediumnode, m_farnode, m_anywherenode);

        m_fightcounter += Time.deltaTime;
        if (m_onground == true)
        {
            if (m_fightcounter > m_fightdelay)
            {
                m_fightingmovetouse = CalculateFightMove(m_fightbehaviour);

                //Debug.Log("Distance till player: " + DistanceCheck(m_player));
                Debug.Log("AI Fight distance: " + m_fightbehaviour);
                Debug.Log("AI Fighting move: " + m_fightingmovetouse);

                m_fightcounter = 0;
            }
        }

        switch (m_fightingmovetouse)
        {
            case IAIFightingMoves.Slash:
                StopMoving();
                ChangeAnimationState(IEntityAnimationState.SlashAttack);
                break;

            case IAIFightingMoves.Stab:
                StopMoving();
                ChangeAnimationState(IEntityAnimationState.StabAttack);
                break;

            case IAIFightingMoves.SlashBlock:
                StopMoving();
                ChangeAnimationState(IEntityAnimationState.SlashBlock);
                break;

            case IAIFightingMoves.StabBlock:
                StopMoving();
                ChangeAnimationState(IEntityAnimationState.StabBlock);
                break;

            case IAIFightingMoves.DashTowards:
                StopMoving();
                DashTowards(m_fastestmovespeed);
                ChangeAnimationState(IEntityAnimationState.Dashing);
                break;

            case IAIFightingMoves.DashAway:
                StopMoving();
                DashAway(m_fastestmovespeed);
                ChangeAnimationState(IEntityAnimationState.Dashing);
                break;

            case IAIFightingMoves.Jump:
                //StopMoving();
                if (m_onground != false)
                {
                    Jump(m_jumpspeed);
                    m_onground = false;
                }
                ChangeAnimationState(IEntityAnimationState.Jumping);
                break;

            //case IAIFightingMoves.JumpAttack:
            //StopMoving();
            //ChangeAnimationState(IEntityAnimationState.JumpAttack);
            //break;

            case IAIFightingMoves.WalkTowards:
                WalkTowardsTarget(m_slowestmovespeed);
                ChangeAnimationState(IEntityAnimationState.Walking);
                break;

            //case IAIFightingMoves.RunTowards:
            //RunTowardsTarget(m_fastestmovespeed);
            //ChangeAnimationState(IEntityAnimationState.Running);
            //break;

            case IAIFightingMoves.WalkAway:
                WalkAwayFromTarget(m_slowestmovespeed);
                ChangeAnimationState(IEntityAnimationState.Walking);
                break;

                //case IAIFightingMoves.RunAway:
                //RunAwayFromTarget(m_fastestmovespeed);
                //ChangeAnimationState(IEntityAnimationState.Running);
                //break;
        }
    }

    float DistanceCheck(Transform target)
    {
        float distance = Vector2.Distance(transform.position, target.transform.position);

        return distance;
    }

    void DashTowards(float speed)
    {
        m_myrgigibody.velocity = new Vector2(speed - 2, 0);
    }
    void DashAway(float speed)
    {
        m_myrgigibody.velocity = new Vector2(-speed + 2, 0);
    }

    //void RunTowardsTarget(float speed)
    //{
    //    m_myrgigibody.velocity = new Vector2(speed, 0);
    //}
    void WalkTowardsTarget(float speed)
    {
        m_myrgigibody.velocity = new Vector2(speed, 0);
        Debug.Log("AI Velocity: " + m_myrgigibody.velocity);
    }

    void StopMoving()
    {
        m_myrgigibody.velocity = Vector2.zero;
    }

    //void RunAwayFromTarget(float speed)
    //{
    //    m_myrgigibody.velocity = new Vector2(-speed, 0);
    //}
    void WalkAwayFromTarget(float speed)
    {
        m_myrgigibody.velocity = new Vector2(-speed, 0);
    }

    void Jump(float speed)
    {
        m_myrgigibody.velocity += new Vector2(0, speed);
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            m_onground = true;
        }
    }

    void ChangeAnimationState(IEntityAnimationState state)
    {
        m_myanimator.SetInteger("AnimationState", (int)state);
    }

    IAIFightBehaviourState CalculateFightBehaviour(float nearnode, float mednode, float farnode, float anywnode)
    {
        float neardistance = nearnode - DistanceCheck(m_player);
        float meddistance = mednode - DistanceCheck(m_player);
        float fardistance = farnode - DistanceCheck(m_player);
        float anywdistance = anywnode - DistanceCheck(m_player);

        if (neardistance < 0) { neardistance = neardistance * (-1); }
        if (meddistance < 0) { meddistance = meddistance * (-1); }
        if (fardistance < 0) { fardistance = fardistance * (-1); }
        if (anywdistance < 0) { anywdistance = anywdistance * (-1); }

        Mathf.Abs(neardistance);
        Mathf.Abs(meddistance);
        Mathf.Abs(fardistance);
        Mathf.Abs(anywdistance);

        if (neardistance < meddistance && neardistance < fardistance && neardistance < anywdistance)
        {
            return IAIFightBehaviourState.Nearby;
        }
        else if (meddistance < neardistance && meddistance < fardistance && meddistance < anywdistance)
        {
            return IAIFightBehaviourState.MediumRange;
        }
        else if (fardistance < neardistance && fardistance < meddistance && fardistance < anywdistance)
        {
            return IAIFightBehaviourState.FarAway;
        }
        else if (anywdistance < neardistance && anywdistance < meddistance && anywdistance < fardistance)
        {
            return IAIFightBehaviourState.Anywhere;
        }
        else
        {
            Debug.LogError("Something is wrong in the AI calculations or every number is the same");
            return IAIFightBehaviourState.Anywhere;
        }
    }

    IAIFightingMoves CalculateFightMove(IAIFightBehaviourState currentstate)
    {
        switch (currentstate) // Calculates the moves that the AI is going to use based on his difficulty and distance.
        {
            case IAIFightBehaviourState.Anywhere:

                switch (m_difficulty)
                {
                    case IAIDifficulty.Easy:
                        int easyrandomoutput = RandomNumberReturner(0, 3);

                        if (easyrandomoutput == 0) { return IAIFightingMoves.SlashBlock; }
                        else if (easyrandomoutput == 1) { return IAIFightingMoves.Stab; }
                        else if (easyrandomoutput == 2) { return IAIFightingMoves.WalkTowards; }
                        else { return IAIFightingMoves.StabBlock; }

                    case IAIDifficulty.Medium:
                        int mediumrandomoutput = RandomNumberReturner(0, 6);

                        if (mediumrandomoutput == 0) { return IAIFightingMoves.Slash; }
                        else if (mediumrandomoutput == 1) { return IAIFightingMoves.Stab; }
                        else if (mediumrandomoutput == 2) { return IAIFightingMoves.SlashBlock; }
                        else if (mediumrandomoutput == 3) { return IAIFightingMoves.WalkTowards; }
                        else if (mediumrandomoutput == 4) { return IAIFightingMoves.WalkAway; }
                        else if (mediumrandomoutput == 5) { return IAIFightingMoves.StabBlock; }
                        else { return IAIFightingMoves.Jump; }

                    case IAIDifficulty.Hard:
                        int hardrandomoutput = RandomNumberReturner(0, 5);

                        if (hardrandomoutput == 0) { return IAIFightingMoves.Slash; }
                        else if (hardrandomoutput == 1) { return IAIFightingMoves.Stab; }
                        else if (hardrandomoutput == 2) { return IAIFightingMoves.SlashBlock; }
                        else if (hardrandomoutput == 3) { return IAIFightingMoves.Jump; }
                        else if (hardrandomoutput == 4) { return IAIFightingMoves.StabBlock; }
                        else { return IAIFightingMoves.DashTowards; }

                    case IAIDifficulty.Expert:
                        int expertrandomoutput = RandomNumberReturner(0, 7);

                        if (expertrandomoutput == 0) { return IAIFightingMoves.Slash; }
                        else if (expertrandomoutput == 1) { return IAIFightingMoves.Stab; }
                        else if (expertrandomoutput == 2) { return IAIFightingMoves.SlashBlock; }
                        else if (expertrandomoutput == 3) { return IAIFightingMoves.Jump; }
                        else if (expertrandomoutput == 4) { return IAIFightingMoves.StabBlock; }
                        else if (expertrandomoutput == 5) { return IAIFightingMoves.WalkAway; }
                        else if (expertrandomoutput == 6) { return IAIFightingMoves.DashTowards; }
                        else { return IAIFightingMoves.DashAway; }
                }
                break;

            case IAIFightBehaviourState.FarAway:

                switch (m_difficulty)
                {
                    case IAIDifficulty.Easy:
                        return IAIFightingMoves.WalkTowards;

                    case IAIDifficulty.Medium:
                        return IAIFightingMoves.WalkTowards;

                    case IAIDifficulty.Hard:
                        return IAIFightingMoves.WalkTowards;

                    case IAIDifficulty.Expert:
                        int expertrandomoutput = RandomNumberReturner(0, 2);

                        if (expertrandomoutput == 0) { return IAIFightingMoves.WalkTowards; }
                        else { return IAIFightingMoves.DashTowards; }
                }
                break;

            case IAIFightBehaviourState.MediumRange:

                switch (m_difficulty)
                {
                    case IAIDifficulty.Easy:
                        int easyrandomoutput = RandomNumberReturner(0, 3);

                        if (easyrandomoutput == 0) { return IAIFightingMoves.WalkTowards; }
                        else if (easyrandomoutput == 1) { return IAIFightingMoves.SlashBlock; }
                        else if (easyrandomoutput == 2) { return IAIFightingMoves.StabBlock; }
                        else { return IAIFightingMoves.Jump; }

                    case IAIDifficulty.Medium:
                        int mediumrandomoutput = RandomNumberReturner(0, 3);

                        if (mediumrandomoutput == 0) { return IAIFightingMoves.WalkTowards; }
                        else if (mediumrandomoutput == 1) { return IAIFightingMoves.WalkAway; }
                        else if (mediumrandomoutput == 2) { return IAIFightingMoves.StabBlock; }
                        else { return IAIFightingMoves.SlashBlock; }

                    case IAIDifficulty.Hard:
                        int hardrandomoutput = RandomNumberReturner(0, 5);

                        if (hardrandomoutput == 0) { return IAIFightingMoves.DashTowards; }
                        else if (hardrandomoutput == 1) { return IAIFightingMoves.WalkAway; }
                        else if (hardrandomoutput == 2) { return IAIFightingMoves.WalkTowards; }
                        else if (hardrandomoutput == 3) { return IAIFightingMoves.Stab; }
                        else if (hardrandomoutput == 4) { return IAIFightingMoves.StabBlock; }
                        else { return IAIFightingMoves.SlashBlock; }

                    case IAIDifficulty.Expert:
                        int expertrandomoutput = RandomNumberReturner(0, 4);

                        if (expertrandomoutput == 1) { return IAIFightingMoves.DashAway; }
                        else if (expertrandomoutput == 2) { return IAIFightingMoves.WalkAway; }
                        else if (expertrandomoutput == 3) { return IAIFightingMoves.Slash; }
                        else { return IAIFightingMoves.SlashBlock; }

                }
                break;

            case IAIFightBehaviourState.Nearby:

                switch (m_difficulty)
                {
                    case IAIDifficulty.Easy:
                        int easyrandomoutput = RandomNumberReturner(0, 4);

                        if (easyrandomoutput == 0) { return IAIFightingMoves.WalkAway; }
                        if (easyrandomoutput == 1) { return IAIFightingMoves.Stab; }
                        if (easyrandomoutput == 2) { return IAIFightingMoves.SlashBlock; }
                        if (easyrandomoutput == 3) { return IAIFightingMoves.Jump; }
                        else return IAIFightingMoves.StabBlock;

                    case IAIDifficulty.Medium:
                        int mediumrandomoutput = RandomNumberReturner(0, 7);

                        if (mediumrandomoutput == 0) { return IAIFightingMoves.WalkAway; }
                        else if (mediumrandomoutput == 1) { return IAIFightingMoves.Slash; }
                        else if (mediumrandomoutput == 2 || mediumrandomoutput == 3 || mediumrandomoutput == 4) { return IAIFightingMoves.Stab; }
                        else if (mediumrandomoutput == 5) { return IAIFightingMoves.StabBlock; }
                        else if (mediumrandomoutput == 6) { return IAIFightingMoves.SlashBlock; }
                        else { return IAIFightingMoves.Jump; }

                    case IAIDifficulty.Hard:
                        int hardrandomoutput = RandomNumberReturner(0, 5);

                        if (hardrandomoutput == 0) { return IAIFightingMoves.WalkAway; }
                        else if (hardrandomoutput == 1) { return IAIFightingMoves.SlashBlock; }
                        else if (hardrandomoutput == 2 || hardrandomoutput == 3) { return IAIFightingMoves.DashAway; }
                        else if (hardrandomoutput == 4) { return IAIFightingMoves.Stab; }
                        else if (hardrandomoutput == 1) { return IAIFightingMoves.StabBlock; }
                        else { return IAIFightingMoves.Slash; }


                    case IAIDifficulty.Expert:
                        int expertrandomoutput = RandomNumberReturner(0, 12);

                        if (expertrandomoutput == 0) { return IAIFightingMoves.WalkAway; }
                        else if (expertrandomoutput == 1 || expertrandomoutput == 2 || expertrandomoutput == 3) { return IAIFightingMoves.Stab; }
                        else if (expertrandomoutput == 4 || expertrandomoutput == 5 || expertrandomoutput == 6) { return IAIFightingMoves.Slash; }
                        else if (expertrandomoutput == 7 || expertrandomoutput == 8) { return IAIFightingMoves.SlashBlock; } //Make it based on what the player does
                        else if (expertrandomoutput == 9 || expertrandomoutput == 10) { return IAIFightingMoves.StabBlock; } //Make it based on what the player does
                        else if (expertrandomoutput == 11 || expertrandomoutput == 12) { return IAIFightingMoves.DashAway; }
                        else { return IAIFightingMoves.DashAway; }
                }
                break;

        }
        return IAIFightingMoves.WalkAway;
    }

    int RandomNumberReturner(int min, int max)
    {
        int randomnumber = Random.Range(min, max);
        return randomnumber;
    }

    public void Die()
    {
        m_isdead = true;
    }

    public bool CanBeHitBySlash { get { return m_canbehitbyslash; } }
    public bool CanBeHitByStab { get { return m_canbehitbystab; } }

    public void IncreaseSlowSpeed(float amounttoincrease)
    {
        m_slowestmovespeed += amounttoincrease;
    }
    public void IncreaseMaxSpeed(float amounttoincrease)
    {
        m_fastestmovespeed += amounttoincrease;
    }

    //public void Hit()

    public bool IsDead { get { return m_isdead; } set { m_isdead = value; } }

    public void SetPosistion(Vector2 pos)
    {
        transform.position = pos;
    }

    public IAIDifficulty AIDifficulty 
    {
        get { return m_difficulty; }
        set { m_difficulty = value; }
    }

    public float AttackDelay
    {
        get { return m_fightdelay; }
        set { m_fightdelay = value; }
    }

    public bool InBattleMode
    {
        get { return m_inbattlemode; }
        set { m_inbattlemode = value; }
    }
}