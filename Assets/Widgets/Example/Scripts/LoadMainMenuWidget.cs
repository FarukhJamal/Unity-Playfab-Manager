using UnityEngine;
using System.Collections;
using eeGames.Widget;
using TMPro;
using PlayFab.ClientModels;
using PlayFab;
using Managers;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using static System.Net.Mime.MediaTypeNames;

public class LoadMainMenuWidget : Widget
{
    public TMP_Text userID, userName;
    public TMP_InputField score;
    public Button StartGame_btn, SendLeaderboardData_btn, GetLeaderboardData_btn, Close_btn;

    private LeaderboardHandler LeaderboardHandler;
    
    // Use this for initialization
	void Start () 
    {
        LeaderboardHandler = gameObject.GetComponent<LeaderboardHandler>();
        UI_SetUserData();

        AddListeners();
    }
    void AddListeners()
    {
        StartGame_btn.onClick.AddListener(ChangeScene);
        SendLeaderboardData_btn.onClick.AddListener(SendLBoardData);
        GetLeaderboardData_btn.onClick.AddListener(GetLBoardData);
        Close_btn.onClick.AddListener(CleanLeaderboard);
    }

    void UI_SetUserData()
    {
       StartCoroutine(SetData());
    }
    IEnumerator SetData()
    {
        PlayfabManager.Instance.GetAccountInfo();
        yield return new WaitUntil(()=>PlayfabManager.Instance.IsAccountData);
        userID.text = PlayfabManager.Instance.MyPlayfabID;
        userName.text = PlayfabManager.Instance.MyUserName;
    }
    
    void ChangeScene()
    {
        SceneManager.LoadScene("Gameplay");
    }

    void SendLBoardData()
    {
        if(!score.text.Equals(""))
        {
            string playerscore = score.text;

            var Score = Convert.ToInt32(playerscore);
            
            PlayfabManager.Instance.SendLeaderboard(Score);
        }
    }

    void GetLBoardData()
    {
        PlayfabManager.Instance.GetLeaderboard();
    }
    public void LeaderBoardDataPlacment(int srNo, string name, int statValue)
    {
        LeaderboardHandler.Leaderboard_Gameobject.SetActive(true);
        var tempGameObject = Instantiate(LeaderboardHandler.User, LeaderboardHandler.DataPlacment.transform);
        TMP_Text[] textFields = tempGameObject.GetComponentsInChildren<TMP_Text>();
        textFields[0].text = srNo.ToString();
        textFields[1].text = name;
        textFields[2].text = statValue.ToString();
    }

    void CleanLeaderboard()
    {
        UnityEngine.UI.Image[] spawned_users = LeaderboardHandler.DataPlacment.GetComponentsInChildren<UnityEngine.UI.Image>(true);
        for(int i = 1; i < spawned_users.Length; i++)
        {
            Destroy(spawned_users[i].gameObject);
        }
    }
}
