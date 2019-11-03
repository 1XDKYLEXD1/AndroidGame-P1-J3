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

public enum IFightingMoves
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

    [SerializeField] BoxCollider2D m_slashattackhitbox;
    [SerializeField] BoxCollider2D m_stabattackhitbox;

    [SerializeField] IAIDifficulty m_difficulty;
    IAIFightBehaviourState m_fightbehaviour;
    IFightingMoves m_fightingmovetouse;

    void Awake()
    {
        m_myrgigibody = GetComponent<Rigidbody2D>();
        m_myanimator = GetComponent<Animator>();
        m_slashattackhitbox.enabled = false;
        m_stabattackhitbox.enabled = false;
    }

    void Update()
    {
        if (m_inbattlemode == true)
        {
            if (m_fightingmovetouse != IFightingMoves.SlashBlock)
            {
                m_canbehitbyslash = true;
            }
            else { m_canbehitbyslash = false; }

            if (m_fightingmovetouse != IFightingMoves.StabBlock)
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
                    //Debug.Log("AI Fight distance: " + m_fightbehaviour);
                    //Debug.Log("AI Fighting move: " + m_fightingmovetouse);

                    m_fightcounter = 0;
                }
            }

            switch (m_fightingmovetouse)
            {
                case IFightingMoves.Slash:
                    StopMoving();
                    ChangeAnimationState(IEntityAnimationState.SlashAttack);
                    break;

                case IFightingMoves.Stab:
                    StopMoving();
                    ChangeAnimationState(IEntityAnimationState.StabAttack);
                    break;

                case IFightingMoves.SlashBlock:
                    StopMoving();
                    ChangeAnimationState(IEntityAnimationState.SlashBlock);
                    break;

                case IFightingMoves.StabBlock:
                    StopMoving();
                    ChangeAnimationState(IEntityAnimationState.StabBlock);
                    break;

                case IFightingMoves.DashTowards:
                    StopMoving();
                    DashTowards(m_fastestmovespeed);
                    ChangeAnimationState(IEntityAnimationState.Dashing);
                    break;

                case IFightingMoves.DashAway:
                    StopMoving();
                    DashAway(m_fastestmovespeed);
                    ChangeAnimationState(IEntityAnimationState.Dashing);
                    break;

                case IFightingMoves.Jump:
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

                case IFightingMoves.WalkTowards:
                    WalkTowardsTarget(m_slowestmovespeed);
                    ChangeAnimationState(IEntityAnimationState.Walking);
                    break;

                //case IAIFightingMoves.RunTowards:
                    //RunTowardsTarget(m_fastestmovespeed);
                    //ChangeAnimationState(IEntityAnimationState.Running);
                    //break;

                case IFightingMoves.WalkAway:
                    WalkAwayFromTarget(m_slowestmovespeed);
                    ChangeAnimationState(IEntityAnimationState.Walking);
                    break;

                //case IAIFightingMoves.RunAway:
                    //RunAwayFromTarget(m_fastestmovespeed);
                    //ChangeAnimationState(IEntityAnimationState.Running);
                    //break;
            }
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
        //Debug.Log("AI Velocity: " + m_myrgigibody.velocity);
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

    public void ChangeAnimationState(IEntityAnimationState state)
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

    IFightingMoves CalculateFightMove(IAIFightBehaviourState currentstate)
    {
        switch (currentstate) // Calculates the moves that the AI is going to use based on his difficulty and distance.
        {
            case IAIFightBehaviourState.Anywhere:

                switch (m_difficulty)
                {
                    case IAIDifficulty.Easy:
                        int easyrandomoutput = RandomNumberReturner(0, 3);

                        if (easyrandomoutput == 0) { return IFightingMoves.SlashBlock; }
                        else if (easyrandomoutput == 1) { return IFightingMoves.Stab; }
                        else if (easyrandomoutput == 2) { return IFightingMoves.WalkTowards; }
                        else { return IFightingMoves.StabBlock; }

                    case IAIDifficulty.Medium:
                        int mediumrandomoutput = RandomNumberReturner(0, 6);

                        if (mediumrandomoutput == 0) { return IFightingMoves.Slash; }
                        else if (mediumrandomoutput == 1) { return IFightingMoves.Stab; }
                        else if (mediumrandomoutput == 2) { return IFightingMoves.SlashBlock; }
                        else if (mediumrandomoutput == 3) { return IFightingMoves.WalkTowards; }
                        else if (mediumrandomoutput == 4) { return IFightingMoves.WalkAway; }
                        else if (mediumrandomoutput == 5) { return IFightingMoves.StabBlock; }
                        else { return IFightingMoves.Jump; }

                    case IAIDifficulty.Hard:
                        int hardrandomoutput = RandomNumberReturner(0, 5);

                        if (hardrandomoutput == 0) { return IFightingMoves.Slash; }
                        else if (hardrandomoutput == 1) { return IFightingMoves.Stab; }
                        else if (hardrandomoutput == 2) { return IFightingMoves.SlashBlock; }
                        else if (hardrandomoutput == 3) { return IFightingMoves.Jump; }
                        else if (hardrandomoutput == 4) { return IFightingMoves.StabBlock; }
                        else { return IFightingMoves.DashTowards; }

                    case IAIDifficulty.Expert:
                        int expertrandomoutput = RandomNumberReturner(0, 7);

                        if (expertrandomoutput == 0) { return IFightingMoves.Slash; }
                        else if (expertrandomoutput == 1) { return IFightingMoves.Stab; }
                        else if (expertrandomoutput == 2) { return IFightingMoves.SlashBlock; }
                        else if (expertrandomoutput == 3) { return IFightingMoves.Jump; }
                        else if (expertrandomoutput == 4) { return IFightingMoves.StabBlock; }
                        else if (expertrandomoutput == 5) { return IFightingMoves.WalkAway; }
                        else if (expertrandomoutput == 6) { return IFightingMoves.DashTowards; }
                        else { return IFightingMoves.DashAway; }
                }
                break;

            case IAIFightBehaviourState.FarAway:

                switch (m_difficulty)
                {
                    case IAIDifficulty.Easy:
                        return IFightingMoves.WalkTowards;

                    case IAIDifficulty.Medium:
                        return IFightingMoves.WalkTowards;

                    case IAIDifficulty.Hard:
                        return IFightingMoves.WalkTowards;

                    case IAIDifficulty.Expert:
                        int expertrandomoutput = RandomNumberReturner(0, 3);

                        if (expertrandomoutput == 0) { return IFightingMoves.WalkTowards; }
                        else { Debug.Log("DASH FORWARD"); return IFightingMoves.DashTowards; }
                }
                break;

            case IAIFightBehaviourState.MediumRange:

                switch (m_difficulty)
                {
                    case IAIDifficulty.Easy:
                        int easyrandomoutput = RandomNumberReturner(0, 3);

                        if (easyrandomoutput == 0) { return IFightingMoves.WalkTowards; }
                        else if (easyrandomoutput == 1) { return IFightingMoves.SlashBlock; }
                        else if (easyrandomoutput == 2) { return IFightingMoves.StabBlock; }
                        else { return IFightingMoves.Jump; }

                    case IAIDifficulty.Medium:
                        int mediumrandomoutput = RandomNumberReturner(0, 3);

                        if (mediumrandomoutput == 0) { return IFightingMoves.WalkTowards; }
                        else if (mediumrandomoutput == 1) { return IFightingMoves.WalkAway; }
                        else if (mediumrandomoutput == 2) { return IFightingMoves.StabBlock; }
                        else { return IFightingMoves.SlashBlock; }

                    case IAIDifficulty.Hard:
                        int hardrandomoutput = RandomNumberReturner(0, 5);

                        if (hardrandomoutput == 0) { return IFightingMoves.DashTowards; }
                        else if (hardrandomoutput == 1) { return IFightingMoves.WalkAway; }
                        else if (hardrandomoutput == 2) { return IFightingMoves.WalkTowards; }
                        else if (hardrandomoutput == 3) { return IFightingMoves.Stab; }
                        else if (hardrandomoutput == 4) { return IFightingMoves.StabBlock; }
                        else { return IFightingMoves.SlashBlock; }

                    case IAIDifficulty.Expert:
                        int expertrandomoutput = RandomNumberReturner(0, 4);

                        if (expertrandomoutput == 1) { return IFightingMoves.DashAway; }
                        else if (expertrandomoutput == 2) { return IFightingMoves.WalkAway; }
                        else if (expertrandomoutput == 3) { return IFightingMoves.Slash; }
                        else { return IFightingMoves.SlashBlock; }

                }
                break;

            case IAIFightBehaviourState.Nearby:

                switch (m_difficulty)
                {
                    case IAIDifficulty.Easy:
                        int easyrandomoutput = RandomNumberReturner(0, 4);

                        if (easyrandomoutput == 0) { return IFightingMoves.WalkAway; }
                        if (easyrandomoutput == 1) { return IFightingMoves.Stab; }
                        if (easyrandomoutput == 2) { return IFightingMoves.SlashBlock; }
                        if (easyrandomoutput == 3) { return IFightingMoves.Jump; }
                        else return IFightingMoves.StabBlock;

                    case IAIDifficulty.Medium:
                        int mediumrandomoutput = RandomNumberReturner(0, 7);

                        if (mediumrandomoutput == 0) { return IFightingMoves.WalkAway; }
                        else if (mediumrandomoutput == 1) { return IFightingMoves.Slash; }
                        else if (mediumrandomoutput == 2 || mediumrandomoutput == 3 || mediumrandomoutput == 4) { return IFightingMoves.Stab; }
                        else if (mediumrandomoutput == 5) { return IFightingMoves.StabBlock; }
                        else if (mediumrandomoutput == 6) { return IFightingMoves.SlashBlock; }
                        else { return IFightingMoves.Jump; }

                    case IAIDifficulty.Hard:
                        int hardrandomoutput = RandomNumberReturner(0, 5);

                        if (hardrandomoutput == 0) { return IFightingMoves.WalkAway; }
                        else if (hardrandomoutput == 1) { return IFightingMoves.SlashBlock; }
                        else if (hardrandomoutput == 2 || hardrandomoutput == 3) { return IFightingMoves.DashAway; }
                        else if (hardrandomoutput == 4) { return IFightingMoves.Stab; }
                        else if (hardrandomoutput == 1) { return IFightingMoves.StabBlock; }
                        else { return IFightingMoves.Slash; }


                    case IAIDifficulty.Expert:
                        int expertrandomoutput = RandomNumberReturner(0, 12);

                        if (expertrandomoutput == 0) { return IFightingMoves.WalkAway; }
                        else if (expertrandomoutput == 1 || expertrandomoutput == 2 || expertrandomoutput == 3) { return IFightingMoves.Stab; }
                        else if (expertrandomoutput == 4 || expertrandomoutput == 5 || expertrandomoutput == 6) { return IFightingMoves.Slash; }
                        else if (expertrandomoutput == 7 || expertrandomoutput == 8) { return IFightingMoves.SlashBlock; } //Make it based on what the player does
                        else if (expertrandomoutput == 9 || expertrandomoutput == 10) { return IFightingMoves.StabBlock; } //Make it based on what the player does
                        else if (expertrandomoutput == 11 || expertrandomoutput == 12) { return IFightingMoves.DashAway; }
                        else { return IFightingMoves.DashAway; }
                }
                break;

        }
        return IFightingMoves.WalkAway;
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

    public void IncreaseSlowSpeed(float amounttoincrease)
    {
        m_slowestmovespeed += amounttoincrease;
    }
    public void IncreaseMaxSpeed(float amounttoincrease)
    {
        m_fastestmovespeed += amounttoincrease;
    }


    public void Hit(IFightingMoves attack)
    {
        Debug.Log(this.gameObject.name + " is HIT");

        if (attack == IFightingMoves.Slash && m_canbehitbyslash != false)
        {
            Debug.Log(this.gameObject.name + " dead by Slash");
            Die();
        }
        else if (attack == IFightingMoves.Stab && m_canbehitbystab != false)
        {
            Debug.Log(this.gameObject.name + " dead by Stab");
            Die();
        }
        else { Debug.Log(attack + " < CURRENT STATE!\nCan be hit by slash & stab > " + m_canbehitbyslash + " & " + m_canbehitbystab); Debug.LogError("NOT REGISTERED ATTACK! Pls look in code."); }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<InputController>() != null)
        {
            collision.GetComponent<InputController>().Hit(m_fightingmovetouse);
        }
    }

    public void SetPosistion(Vector2 pos)
    {
        transform.position = pos;
    }
    
    public void SwitchEnableSlashHitbox() //If enabled then turn disable.If disabled then enable.
    {
        if(m_slashattackhitbox.enabled == false)
        {
            m_slashattackhitbox.enabled = true;
        }
        else { m_slashattackhitbox.enabled = false; }
    }
    public void SwitchEnableStabHitbox() //If enabled then turn disable. If disabled then enable.
    {
        if (m_stabattackhitbox.enabled == false)
        {
            m_stabattackhitbox.enabled = true;
        }
        else { m_stabattackhitbox.enabled = false; }
    }

    public BoxCollider2D SlashHitbox { get { return m_slashattackhitbox; } }
    public BoxCollider2D StabHitbox { get { return m_stabattackhitbox; } }

    public bool CanBeHitBySlash { get { return m_canbehitbyslash; } }
    public bool CanBeHitByStab { get { return m_canbehitbystab; } }

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

    public bool IsDead
    {
        get { return m_isdead; }
        set { m_isdead = value; }
    }

    public bool InBattleMode
    {
        get { return m_inbattlemode; }
        set { m_inbattlemode = value; }
    }
}