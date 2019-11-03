using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class GameManager : MonoBehaviour
{
    //[SerializeField] List<AIBehaviour> m_theai; THIS COMES LATER
    
    [SerializeField] InputController m_playercontroller;
    [SerializeField] AIBehaviour m_ai;

    [SerializeField] Transform m_playerstartpos;
    [SerializeField] Transform m_aistartpos;

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
        StopCoroutine(BattleReset());
        m_namehelpertext.gameObject.SetActive(false);
        m_scoresubmitter.SetActive(false);

        for (int b = 0; b < m_answerbuttons.Length; b++)
        {
            m_answerbuttons[b].SetActive(false);
        }

        m_pressplaybutton.SetActive(true);
        m_pressplayed = false;

        m_numberofwins = 0;
        m_finalscore = 0;
        m_submitscoreanswer = "";
        m_ai.AIDifficulty = IAIDifficulty.Easy;

        m_playercontroller.SetPosistion(m_playerstartpos.position);
        m_ai.SetPosistion(m_aistartpos.position);
        m_playercontroller.IsDead = false;
        m_ai.IsDead = false;
        m_playercontroller.ChangeAnimationState(IEntityAnimationState.Idle);
        m_ai.ChangeAnimationState(IEntityAnimationState.Idle);

        while (!m_pressplayed)
        {
            yield return null;
        }

        m_countdown.SetActive(true);
        m_pressplaybutton.SetActive(false);
        yield return new WaitForSeconds(m_countdown.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.length - 0.1f);
        m_countdown.SetActive(false);

        StopCoroutine(Battling());
        StartCoroutine(Battling());
    }

    IEnumerator BattleReset()
    {
        m_playercontroller.SetPosistion(m_playerstartpos.position);
        m_ai.SetPosistion(m_aistartpos.position);
        m_playercontroller.IsDead = false;
        m_ai.IsDead = false;

        yield return StartCoroutine(Battling());
    }

    IEnumerator Battling()
    {
        StopCoroutine(GameStart());

        while (m_playercontroller.IsDead != true && m_ai.IsDead != true)
        {
            m_playercontroller.InBattleMode = true;
            m_ai.InBattleMode = true;
            yield return null;
        }

        Debug.Log("Someone died...");
        m_playercontroller.InBattleMode = false;
        m_ai.InBattleMode = false;

        if (m_playercontroller.IsDead) //This will be changed player 1
        {
            Debug.Log("AI1 is dead! GAMEOVER!");
            StartCoroutine(GameOver());
        }
        else if (m_ai.IsDead)
        {
            StartCoroutine(BattleWin());
        }
    }

    IEnumerator BattleWin()
    {
        m_numberofwins += 1;

        if(m_numberofwins == 1)
        {
            GooglePlayManager.UnlockAchievement("CgkI0K_Ww44WEAIQAQ");
        }

        Debug.Log("BATTLE WON");

        switch (m_numberofwins)
        {
            case 5:
                m_ai.AIDifficulty = IAIDifficulty.Medium;
                break;

            case 10:
                m_ai.AIDifficulty = IAIDifficulty.Hard;
                break;

            case 20:
                m_ai.AIDifficulty = IAIDifficulty.Expert;
                break;
        }

        switch (m_ai.AIDifficulty)
        {
            case IAIDifficulty.Medium:
                m_ai.AttackDelay -= 0.01f;
                break;

            case IAIDifficulty.Hard:
                m_ai.AttackDelay -= 0.025f;
                break;

            case IAIDifficulty.Expert:
                m_ai.AttackDelay -= 0.05f;
                if (m_ai.AttackDelay < 0.5f) { m_ai.AttackDelay = 0.5f; }
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

        if (m_numberofwins == 0) { m_scoretext.text = m_numberofwins.ToString(); }
        else
        {
            while (m_finalscore < m_numberofwins)
            {
                m_scoretext.text = "FINAL SCORE" + "\n" + (m_finalscore += 1).ToString();
                yield return new WaitForSeconds(delay);
            }
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

        StartCoroutine(BattleReset());

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

    public void ResetGame()
    {
        m_playercontroller.SetPosistion(m_playerstartpos.position);
        m_ai.SetPosistion(m_aistartpos.position);
        m_playercontroller.IsDead = false;
        m_ai.IsDead = false;
    }

    public void ShowAchievements()
    {
        GooglePlayManager.ShowAchievementsUI();
    }

    public void ShowLeaderboard()
    {
        GooglePlayManager.ShowLeaderboard();
    }

    //class ObjectsToBeSaved
    //{
    //    public string m_username;
    //    public int m_score;
    //}
}
