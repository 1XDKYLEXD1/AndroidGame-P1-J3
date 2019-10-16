﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class GameManager : MonoBehaviour
{
    //[SerializeField] List<AIBehaviour> m_theai; THIS COMES LATER
    
    [SerializeField] AIBehaviour m_ai1; //Change this to player
    [SerializeField] AIBehaviour m_ai2;

    [SerializeField] Transform m_ai1startpos;
    [SerializeField] Transform m_ai2startpos;

    [SerializeField] GameObject m_countdown;
    [SerializeField] Text m_scoretext;

    [SerializeField] GameObject m_scoresubmitter;
    [SerializeField] Text m_namehelpertext;

    [SerializeField] GameObject m_pressplaybutton;
    [SerializeField] GameObject[] m_answerbuttons;

    int m_numberofwins;
    int m_finalscore;
    
    bool m_pressplayed;

    string m_submitscoreanswer;

    void Start()
    {
        StartCoroutine(GameStart());
    }

    //UPDATE DEBUG USES!!! -- Delete if building
    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.K)) //Kill Player
    //    {
    //        m_ai1.Die();
    //    }
    //    else if (Input.GetKeyDown(KeyCode.I)) //Kill AI
    //    {
    //        m_ai2.Die();
    //    }
    //}

    IEnumerator GameStart()
    {
        StopCoroutine(GameOver());
        StopCoroutine(SubmitScore());
        m_namehelpertext.gameObject.SetActive(false);
        m_scoresubmitter.SetActive(false);

        m_pressplaybutton.SetActive(true);
        m_pressplayed = false;

        m_numberofwins = 0;
        m_finalscore = 0;
        m_submitscoreanswer = "";
        m_ai2.AIDifficulty = IAIDifficulty.Easy;

        m_ai1.SetPosistion(m_ai1startpos.position);
        m_ai2.SetPosistion(m_ai2startpos.position);
        m_ai1.IsDead = false;
        m_ai2.IsDead = false;
        m_ai1.ChangeAnimationState(IEntityAnimationState.Idle);
        m_ai2.ChangeAnimationState(IEntityAnimationState.Idle);

        //m_pressptoplaytext.text = "Press 'P' to Start";

        while (!m_pressplayed)
        {
            yield return null;
        }

        m_countdown.SetActive(true);
        m_pressplaybutton.SetActive(false);
        //m_pressptoplaytext.text = "";
        yield return new WaitForSeconds(m_countdown.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.length - 0.1f);
        m_countdown.SetActive(false);

        StopCoroutine(Battling());
        StartCoroutine(Battling());
    }

    IEnumerator BattleReset()
    {
        m_ai1.SetPosistion(m_ai1startpos.position);
        m_ai2.SetPosistion(m_ai2startpos.position);
        m_ai1.IsDead = false;
        m_ai2.IsDead = false;

        yield return StartCoroutine(Battling());
    }

    IEnumerator Battling()
    {
        StopCoroutine(GameStart());

        while (m_ai1.IsDead != true && m_ai2.IsDead != true)
        {
            m_ai1.InBattleMode = true;
            m_ai2.InBattleMode = true;
            yield return null;
        }

        Debug.Log("Someone died...");
        m_ai1.InBattleMode = false;
        m_ai2.InBattleMode = false;

        if (m_ai1.IsDead) //This will be changed player 1
        {
            Debug.Log("AI1 is dead! GAMEOVER!");
            StartCoroutine(GameOver());
        }
        else if (m_ai2.IsDead)
        {
            StartCoroutine(BattleWin());
        }
    }

    IEnumerator BattleWin()
    {
        m_numberofwins += 1;

        Debug.Log("BATTLE WON");

        switch (m_numberofwins)
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

        switch (m_ai2.AIDifficulty)
        {
            case IAIDifficulty.Medium:
                m_ai2.AttackDelay -= 0.01f;
                break;

            case IAIDifficulty.Hard:
                m_ai2.AttackDelay -= 0.025f;
                break;

            case IAIDifficulty.Expert:
                m_ai2.AttackDelay -= 0.05f;
                if (m_ai2.AttackDelay < 0.5f) { m_ai2.AttackDelay = 0.5f; }
                break;
        }

        yield return StartCoroutine(BattleReset());
    }

    IEnumerator GameOver()
    {
        StopCoroutine(Battling());

        float delay;

        if (m_numberofwins > 30) { delay = 0.05f; }
        else if (m_numberofwins > 20) { delay = 0.1f; }
        else if (m_numberofwins > 10) { delay = 0.2f; }
        else if (m_numberofwins > 5) { delay = 0.5f; }
        else { delay = 0.7f; }

        m_scoresubmitter.SetActive(true);
        
        while (m_finalscore < m_numberofwins)
        {
            m_scoretext.text = "FINAL SCORE" + "\n" + (m_finalscore += 1).ToString();
            yield return new WaitForSeconds(delay);
        }

        for (int i = 0; i < m_answerbuttons.Length; i++)
        {
            m_answerbuttons[i].SetActive(true);
        }

        m_namehelpertext.text = "Submit Score?";
        while (m_submitscoreanswer == "")
        {
            yield return null;
        }

        if (m_submitscoreanswer == "yes")
        {
            yield return StartCoroutine(SubmitScore());
        }
        else if (m_submitscoreanswer == "no")
        {
            yield return StartCoroutine(GameStart());
        }
        else { Debug.LogError("Something is wrong here..."); }
    }

    IEnumerator SubmitScore()
    {
        Debug.Log("SUBMITTING SCORE");
        
        GooglePlayManager.AddScoreToLeaderboard(SuddenDeathResources.leaderboard_sudden_death_leaderboard, m_finalscore);

        yield return StartCoroutine(GameStart());
        //ObjectsToBeSaved objtojson = new ObjectsToBeSaved
        //{
        //    m_username = m_nameinputfield.text,
        //    m_score = m_finalscore
        //};

        //string json = JsonUtility.ToJson(objtojson);
        //File.WriteAllText(Application.dataPath + "/ScoreFrom_" + m_nameinputfield.text + ".txt", json);
    }

    public void SubmitScoreAnswer(string answer)
    {
        m_submitscoreanswer = answer;
    }

    public void PressPlay()
    {
        m_pressplayed = true;
    }
        
    //class ObjectsToBeSaved
    //{
    //    public string m_username;
    //    public int m_score;
    //}
}
