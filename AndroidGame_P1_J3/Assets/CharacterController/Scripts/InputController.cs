using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;



public struct TouchInfo
{
    
    public float StartTime;
    public Vector2 StartPos;
    public bool IsWalk;

}

public class InputController : MonoBehaviour
{
    //[SerializeField] private Text _debugText;

    private string _debugPhaseEnded;
    private int _debugAmount;

    private float _timer;
    private Dictionary<int, TouchInfo> _currentInputs;

    private bool _firstTap;
    private float _doubleTapTime;

    [SerializeField] private float _walkSpeed;

    [SerializeField] private float _dashSpeed;
    [SerializeField] private float _dashTime;
    private float _dashStart;
    private bool _dash = false;
    private int _dashDir;
    [SerializeField] private float _countTapTime,_countDoubleTapTime;

    [SerializeField] private int _tapMaxMoveDistance;

    [SerializeField] private int _minSlashDistance;

    private IEntityAnimationState _state = IEntityAnimationState.Idle;
    private IFightingMoves _currentFightingMove;
    [SerializeField] Animator m_myanimator;

    private bool m_inbattlemode;
    private bool m_isdead;

    bool m_canbehitbystab;
    bool m_canbehitbyslash;

    [SerializeField] BoxCollider2D m_slashattackhitbox;
    [SerializeField] BoxCollider2D m_stabattackhitbox;

    private const float _delayBeforeWalk = 0.025f;
    // Start is called before the first frame update
    private void Start()
    {
        _currentInputs = new Dictionary<int, TouchInfo>();
        //m_myanimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    private void Update()
    {
        _timer += Time.deltaTime;

        if (m_inbattlemode)
        {



            if (_dash && (_timer - _dashStart < _dashTime))
            {
                transform.Translate(Vector3.right * _dashDir * _dashSpeed * Time.deltaTime);
            }



            if (_currentFightingMove != IFightingMoves.SlashBlock)
            {
                m_canbehitbyslash = true;
            }
            else { m_canbehitbyslash = false; }

            if (_currentFightingMove != IFightingMoves.StabBlock)
            {
                m_canbehitbystab = true;
            }
            else { m_canbehitbystab = false; }

#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.D))
            {
                Walk(1);
                _state = IEntityAnimationState.Walking;
                ChangeAnimationState(_state);

            }
            else if (Input.GetKey(KeyCode.A))
            {
                Walk(-1);
                _state = IEntityAnimationState.Walking;
                ChangeAnimationState(_state);
            }



            if (Input.GetKey(KeyCode.E))
            {
                Dash(1);
                _state = IEntityAnimationState.Dashing;
                ChangeAnimationState(_state);
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                Dash(-1);
                _state = IEntityAnimationState.Dashing;
                ChangeAnimationState(_state);
            }

            if (Input.GetKey(KeyCode.O))
            {
                SlashAttack();
                _state = IEntityAnimationState.SlashAttack;
                ChangeAnimationState(_state);
            }
            else if (Input.GetKey(KeyCode.J))
            {

                StabAttack();
                _state = IEntityAnimationState.StabAttack;
                ChangeAnimationState(_state);
            }

            if (Input.GetKey(KeyCode.Y))
            {
                SlashBlock();
                _state = IEntityAnimationState.SlashBlock;
                ChangeAnimationState(_state);
            }
            else if (Input.GetKey(KeyCode.H))
            {
                StabBlock();
                _state = IEntityAnimationState.StabBlock;
                ChangeAnimationState(_state);
            }


#endif

#if UNITY_ANDROID


        foreach (Touch touch in Input.touches)
        {
            // add new touch
            if (_currentInputs.Count == 0 || !_currentInputs.ContainsKey(touch.fingerId))
            {
                TouchInfo info = new TouchInfo
                {
                    
                    StartPos = touch.position,
                    StartTime = _timer,
                    IsWalk = false
                };
                _currentInputs.Add(touch.fingerId,info);
            }

            // walking touch
            if (touch.phase == TouchPhase.Stationary && _timer - _currentInputs[touch.fingerId].StartTime >_delayBeforeWalk)
            {
                TouchInfo i = _currentInputs[touch.fingerId];
                i.IsWalk = true;
                _currentInputs[touch.fingerId] = i;

                if (touch.position.x < Screen.width / 2)
                {
                    // walk left
                    Walk(-1);
                    _state = IEntityAnimationState.Walking;
                    ChangeAnimationState(_state);
                }
                else
                {
                    // walk right
                    Walk(1);
                    _state = IEntityAnimationState.Walking;
                    ChangeAnimationState(_state);
                }
            }

            if (touch.phase == TouchPhase.Ended)
            {
                

                
                // dashing
                if (_timer - _currentInputs[touch.fingerId].StartTime < _countTapTime && NotMoved(touch))
                {
                    TouchInfo i = _currentInputs[touch.fingerId];
                    i.IsWalk = false;
                    _currentInputs[touch.fingerId] = i;

                    if (_firstTap)
                    {
                        _debugPhaseEnded = "has been in first tap :" + _debugAmount++ + "/" + (_timer - _currentInputs[touch.fingerId].StartTime);
                        _firstTap = false;
                        if (touch.position.x < Screen.width / 2)
                        {
                            _debugPhaseEnded = _debugPhaseEnded + "\ndash left";
                            //dash left
                           Dash(-1);
                           _state = IEntityAnimationState.Dashing;
                           ChangeAnimationState(_state);
                        }
                        else
                        {
                            _debugPhaseEnded = _debugPhaseEnded + "\ndash right";
                            //dash right
                            Dash(1);
                            _state = IEntityAnimationState.Dashing;
                            ChangeAnimationState(_state);

                        }
                    }
                    else
                    {
                        _firstTap = true;
                        _doubleTapTime = _timer;
                    }
                    
                }

                // slashing
                if (Vector2.Distance(_currentInputs[touch.fingerId].StartPos, touch.position) > _minSlashDistance)
                {
                    TouchInfo i = _currentInputs[touch.fingerId];
                    i.IsWalk = false;
                    _currentInputs[touch.fingerId] = i;
                    Vector2 slashdirection = touch.position - _currentInputs[touch.fingerId].StartPos;

                    if (Mathf.Abs(slashdirection.x) > Mathf.Abs(slashdirection.y))
                    {
                        if (slashdirection.x > 0)
                        {
                            _debugPhaseEnded = "swipe stab attack";
                            //stab attack
                            _state = IEntityAnimationState.StabAttack;
                            ChangeAnimationState(_state);
                            StabAttack();
                        }
                        else
                        {
                            _debugPhaseEnded = "swipe stab block";
                            //stab Block
                            _state = IEntityAnimationState.StabBlock;
                            ChangeAnimationState(_state);
                            StabBlock();
                        }
                    }
                    else
                    {
                        if (slashdirection.y < 0)
                        {
                            _debugPhaseEnded = "swipe overhead attack";
                            //overhead attack
                            _state = IEntityAnimationState.SlashAttack;
                            ChangeAnimationState(_state);
                            SlashAttack();
                        }
                        else
                        {
                            _debugPhaseEnded = "swipe overhead block";
                            //overhead block
                            _state = IEntityAnimationState.SlashBlock;
                            ChangeAnimationState(_state);
                            SlashBlock();
                        }
                    }
                }

                if (_currentInputs[touch.fingerId].IsWalk)
                {
                    ChangeAnimationState(IEntityAnimationState.Idle);
                }
                

                _currentInputs.Remove(touch.fingerId);
            }
        }

        if (_timer - _doubleTapTime > _countDoubleTapTime)
        {
            _firstTap = false;
        }

        //_debugText.text = _debugPhaseEnded;


#endif
        }
    }




    private bool NotMoved(Touch t)
    {
        return Vector2.Distance(_currentInputs[t.fingerId].StartPos, t.position) < _tapMaxMoveDistance;
    }

    private void Walk(int dir)
    {
        transform.Translate(Vector3.right * dir * _walkSpeed * Time.deltaTime);
        _currentFightingMove = dir > 0 ? IFightingMoves.WalkTowards : IFightingMoves.WalkAway;
    }

    private void Dash(int dir)
    {

        _dash = true;
        _dashDir = dir;
        _dashStart = _timer;
        _currentFightingMove = dir > 0 ? IFightingMoves.DashTowards : IFightingMoves.DashAway;
    }

    private void SlashAttack()
    {
        _currentFightingMove = IFightingMoves.Slash;
    }

    private void SlashBlock()
    {
        _currentFightingMove = IFightingMoves.SlashBlock;
        
    }

    private void StabAttack()
    {
        _currentFightingMove = IFightingMoves.Stab;
    }

    private void StabBlock()
    {
        _currentFightingMove = IFightingMoves.StabBlock;
    }


    public void ChangeAnimationState(IEntityAnimationState state)
    {
        m_myanimator.SetInteger("AnimationState", (int)state);
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
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        //Change to Player
        if (collision.GetComponent<AIBehaviour>() != null)
        {
            collision.GetComponent<AIBehaviour>().Hit(_currentFightingMove);
        }
    }



    public void SwitchEnableSlashHitbox() //If enabled then turn disable.If disabled then enable.
    {
        if (m_slashattackhitbox.enabled == false)
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




    public void SetPosistion(Vector2 pos)
    {
        transform.position = pos;
    }

    public void Die()
    {
        m_isdead = true;
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
