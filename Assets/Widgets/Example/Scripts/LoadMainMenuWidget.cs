using UnityEngine;
using System.Collections;
using eeGames.Widget;
using TMPro;
using PlayFab.ClientModels;
using PlayFab;
using Managers;

public class LoadMainMenuWidget : Widget
{
    public TMP_Text userID, userName;

	// Use this for initialization
	void Start () 
    {

        UI_SetUserData();
	}

    void UI_SetUserData()
    {
        PlayfabManager.Instance.GetAccountInfo();
        userID.text = PlayfabManager.Instance.MyPlayfabID;
        userName.text = PlayfabManager.Instance.MyUserName;
    }
    
}
