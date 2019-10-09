using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public struct TouchInfo
{
    
    public float StartTime;
    public Vector2 StartPos;

}

public class InputController : MonoBehaviour
{
    [SerializeField] private Text _debugText;

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

    private const float _delayBeforeWalk = 0.025f;
    // Start is called before the first frame update
    private void Start()
    {
        _currentInputs = new Dictionary<int, TouchInfo>();
    }

    // Update is called once per frame
    private void Update()
    {
        _timer += Time.deltaTime;

      

        if (_dash && (_timer - _dashStart < _dashTime))
        {
            transform.Translate(Vector3.right * _dashDir * _dashSpeed * Time.deltaTime);
        }




        foreach (Touch touch in Input.touches)
        {
            // add new touch
            if (_currentInputs.Count == 0 || !_currentInputs.ContainsKey(touch.fingerId))
            {
                TouchInfo info = new TouchInfo
                {
                    
                    StartPos = touch.position,
                    StartTime = _timer
                };
                _currentInputs.Add(touch.fingerId,info);
            }

            // walking touch
            if (touch.phase == TouchPhase.Stationary && _timer - _currentInputs[touch.fingerId].StartTime >_delayBeforeWalk)
            {
                if (touch.position.x < Screen.width / 2)
                {
                    // walk left
                    Walk(-1);
                }
                else
                {
                    // walk right
                    Walk(1);
                }
            }

            if (touch.phase == TouchPhase.Ended)
            {
                
                // dashing
                if (_timer - _currentInputs[touch.fingerId].StartTime < _countTapTime && NotMoved(touch))
                {
                    
                    if (_firstTap)
                    {
                        _debugPhaseEnded = "has been in first tap :" + _debugAmount++ + "/" + (_timer - _currentInputs[touch.fingerId].StartTime);
                        _firstTap = false;
                        if (touch.position.x < Screen.width / 2)
                        {
                            _debugPhaseEnded = _debugPhaseEnded + "\ndash left";
                            //dash left
                           Dash(-1);
                        }
                        else
                        {
                            _debugPhaseEnded = _debugPhaseEnded + "\ndash right";
                            //dash right
                            Dash(1);
                            
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
                    Vector2 slashdirection = touch.position - _currentInputs[touch.fingerId].StartPos;

                    if (Mathf.Abs(slashdirection.x) > Mathf.Abs(slashdirection.y))
                    {
                        if (slashdirection.x > 0)
                        {
                            _debugPhaseEnded = "swipe stab attack";
                            //stab attack
                        }
                        else
                        {
                            _debugPhaseEnded = "swipe stab block";
                            //stab Block
                        }
                    }
                    else
                    {
                        if (slashdirection.y < 0)
                        {
                            _debugPhaseEnded = "swipe overhead attack";
                            //overhead attack
                        }
                        else
                        {
                            _debugPhaseEnded = "swipe overhead block";
                            //overhead block
                        }
                    }
                }

                _currentInputs.Remove(touch.fingerId);
            }
        }

        if (_timer - _doubleTapTime > _countDoubleTapTime)
        {
            _firstTap = false;
        }

        _debugText.text = _debugPhaseEnded;
    }




    private bool NotMoved(Touch t)
    {
        return Vector2.Distance(_currentInputs[t.fingerId].StartPos, t.position) < _tapMaxMoveDistance;
    }

    private void Walk(int dir)
    {
        transform.Translate(Vector3.right * dir * _walkSpeed * Time.deltaTime);
    }

    private void Dash(int dir)
    {
        _dash = true;
        _dashDir = dir;
        _dashStart = _timer;
    }
}
