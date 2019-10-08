using System.Collections;
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
    [SerializeField] InputField m_nameinputfield;
    [SerializeField] Text m_namehelpertext;

    int m_numberofwins;
    int m_finalscore;
    bool m_isscoresubmitted;

    void Start()
    {
        StartCoroutine(GameStart());
    }

    //UPDATE DEBUG USES!!! -- Delete if building
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) //Kill Player
        {
            m_ai1.Die();
        }
        else if (Input.GetKeyDown(KeyCode.I)) //Kill AI
        {
            m_ai2.Die();
        }
    }

    IEnumerator GameStart()
    {
        StopCoroutine(SubmitScore());
        m_isscoresubmitted = false;
        m_nameinputfield.gameObject.SetActive(false);
        m_namehelpertext.gameObject.SetActive(false);
        m_scoresubmitter.SetActive(false);

        m_numberofwins = 0;
        m_finalscore = 0;
        m_nameinputfield.text = "";
        m_ai2.AIDifficulty = IAIDifficulty.Easy;

        m_ai1.SetPosistion(m_ai1startpos.position);
        m_ai2.SetPosistion(m_ai2startpos.position);
        m_ai1.IsDead = false;
        m_ai2.IsDead = false;
        m_ai1.ChangeAnimationState(IEntityAnimationState.Idle);
        m_ai2.ChangeAnimationState(IEntityAnimationState.Idle);

        while (!Input.GetKey(KeyCode.P))
        {
            yield return null;
        }

        m_countdown.SetActive(true);
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

        if (m_numberofwins > 100) { delay = 0.1f; }
        else if (m_numberofwins > 75) { delay = 0.2f; }
        else if (m_numberofwins > 25) { delay = 0.3f; }
        else if (m_numberofwins > 10) { delay = 0.5f; }
        else { delay = 0.65f; }

        m_scoresubmitter.SetActive(true);
        
        while (m_finalscore < m_numberofwins)
        {
            m_scoretext.text = "FINAL SCORE" + "\n" + (m_finalscore += 1).ToString();
            yield return new WaitForSeconds(delay);
        }

        //m_isscoresubmitted = false;
        yield return SubmitScore();
    }

    IEnumerator SubmitScore()
    {
        Debug.Log("SUBMIT SCORE");

        if (m_isscoresubmitted == false)
        {
            while (m_nameinputfield.text == "")
            {
                m_namehelpertext.text = "Please enter your name";
                m_nameinputfield.gameObject.SetActive(true);
                m_namehelpertext.gameObject.SetActive(true);
                yield return null;
            }

            m_namehelpertext.text = "Press enter to submit";

            while (!Input.GetKey(KeyCode.Return))
            {
                yield return null;
            }

            ObjectsToBeSaved objtojson = new ObjectsToBeSaved
            {
                m_username = m_nameinputfield.text,
                m_score = m_finalscore
            };

            string json = JsonUtility.ToJson(objtojson);
            File.WriteAllText(Application.dataPath + "/ScoreFrom_" + m_nameinputfield.text + ".txt", json);

            m_isscoresubmitted = true;
            m_nameinputfield.text = "";

            yield return StartCoroutine(GameStart());
        }
    }

    class ObjectsToBeSaved
    {
        public string m_username;
        public int m_score;
    }
}
