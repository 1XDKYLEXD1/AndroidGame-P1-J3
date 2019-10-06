using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] AIBehaviour m_ai1; //Change this to player
    [SerializeField] AIBehaviour m_ai2;

    Transform m_ai1startpos;
    Transform m_ai2startpos;

    int m_numberofwins;

    void Start()
    {
        m_ai1startpos = m_ai1.gameObject.transform;
        m_ai2startpos = m_ai2.gameObject.transform;
        StartCoroutine(GameStart());
    }

    IEnumerator GameStart()
    {
        m_ai1.SetPosistion(m_ai1startpos.position);
        m_ai2.SetPosistion(m_ai2startpos.position);
        m_ai1.IsDead = false;
        m_ai2.IsDead = false;

        while (true)
        {
            yield return StartCoroutine(Battling());
        }
    }

    IEnumerator Battling()
    {
        while(m_ai1.IsDead != true || m_ai2.IsDead != true)
        {
            m_ai1.InBattleMode = true;
            m_ai2.InBattleMode = true;
            yield return null;
        }

        m_ai1.InBattleMode = false;
        m_ai2.InBattleMode = false;

        if (m_ai1.IsDead) //This will be changed player 1
        {
            StartCoroutine(GameOver());
        }
        else if(m_ai2.IsDead)
        {
            StartCoroutine(BattleWin());
        }
    }

    IEnumerator BattleWin()
    {     
        m_numberofwins += 1;

        switch(m_numberofwins)
        {
            case 5:
                m_ai2.AIDifficulty = IAIDifficulty.Medium;
                break;

            case 10:
                m_ai2.AIDifficulty = IAIDifficulty.Hard;
                break;

            case 20:
                m_ai2.AIDifficulty = IAIDifficulty.Expert;
                break;
        }

        switch(m_ai2.AIDifficulty)
        {
            case IAIDifficulty.Medium:
                m_ai2.AttackDelay -= 0.01f;
                break;

            case IAIDifficulty.Hard:
                m_ai2.AttackDelay -=  0.025f;
                break;

            case IAIDifficulty.Expert:
                m_ai2.AttackDelay -= 0.05f;
                if(m_ai2.AttackDelay < 0.5f) { m_ai2.AttackDelay = 0.5f; }
                break;
        }

        yield return StartCoroutine(GameStart());
    }

    IEnumerator GameOver()
    {
        yield return null;
    }
}
