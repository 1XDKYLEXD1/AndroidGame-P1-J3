using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;

public class GooglePlayManager : MonoBehaviour
{
    void Start()
    {
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder().Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.Activate();

        SignIn();
    }

    void SignIn()
    {
        Social.localUser.Authenticate(success => { });
    }

    #region Achievements
    
    public static void UnlockAchievement(string achievementID)
    {
        Social.ReportProgress(achievementID, 100, success => { });
    }

    public static void IncrementAchievement(string achievementID, int stepstoincrement)
    {
        PlayGamesPlatform.Instance.IncrementAchievement(achievementID, stepstoincrement, success => { });
    }

    public static void ShowAchievementsUI()
    {
        Social.ShowAchievementsUI();
    }

    #endregion

    #region Leaderboards

    public static void AddScoreToLeaderboard(string leaderboardID, long score)
    {
        Social.ReportScore(score, leaderboardID, success => { });
    }

    public static void ShowLeaderboard()
    {
        Social.ShowLeaderboardUI();
    }

    #endregion
}
