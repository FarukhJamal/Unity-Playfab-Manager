using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using eeGames.Widget;
using General;
using Models;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using UnityEngine;
using Currency = General.Currency;
using EntityKey = PlayFab.MultiplayerModels.EntityKey;

namespace Managers
{
    /// <summary>
    /// Playfab Manager
    /// Its a playfab starter class where you can do following:
    /// 1-Register/Login
    /// 2- Get Player Data
    /// 3- Get Virtual Currency
    /// 4- Matchmaking
    /// </summary>
    public class PlayfabManager:MonoBehaviour
    {

        #region Authentications
    
        protected Validations Validator;
        #endregion
        
        #region Delegate-Events
        
        #region Register Events
        public delegate void RegisterSuccess(string ticket);
        public static RegisterSuccess OnRegisterSuccessful;
        public delegate void RegisterError();
        public static RegisterError OnRegisterUnSuccessful;
        #endregion
        
        #region Login Events
        // login with email 
        public delegate void EmailLoginSuccess(string ticket);
        public static EmailLoginSuccess OnEmailLoginSuccessful;
        public delegate void EmailLoginError();
        public static EmailLoginError OnEmailLoginUnSuccessful;
        #endregion
        
        #region Player-Data-Events
        
        public delegate void PlayerDataSuccess();
        public static PlayerDataSuccess OnSuccessfulDataFetch;
        public delegate void PlayerDataError();
        public static PlayerDataError OnDataFetchingError;
        #endregion
        #endregion
        
        #region Variables
        #region Public-Variables
        
        public static PlayfabManager Instance;
        public UIManager UI_Manager;
        public static string SessionTicket;
        public static string EntityID;
        public Currency GameCurrency;          // Virtual Currency if any
        #endregion
        
        #region Private-Variables
        
        private string TicketID;
        private Coroutine pollTicketRoutine;
        #endregion
        #endregion
        
        #region Unity-Calls
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        private void Start()
        {
            Validator = new Validations();
            UI_Manager = new UIManager();
            UIManager.Instance.SpawnUI(WidgetName.PlayfabLogin);
        }
        #endregion
        
        #region Functions
        
        #region Register-PlayFab
        public void PlayfabSignUpRequest(string userName, string emailId, string password,string confirmPassword)
        {
            var registerRequest = new RegisterPlayFabUserRequest
            {
                Email = emailId,
                Username = userName,
                Password = password
            };
            
            UpdateDisplayName(userName);
            if(Validator.ValidateRegisteration(userName,emailId,password,confirmPassword))
                PlayFabClientAPI.RegisterPlayFabUser(registerRequest,OnRegisterSuccess,OnRegisterError);
            else
                UIManager.Instance.ShowErrorMessage("Invalid Register!","Error!Kindly check your credentials Again.");
        }
        #endregion
        
        #region Login-With-Playfab
        public void LoginWithEmail(string emailId,string password)
        {
            var loginRequest = new LoginWithEmailAddressRequest
            {
                Email = emailId,
                Password = password
            };
            
            if(Validator.ValidateLogin(emailId,password,out string errorMsg))
                PlayFabClientAPI.LoginWithEmailAddress(loginRequest,EmailLoginSuccessResult,EmailLoginErrorResult);
            else
                UIManager.Instance.ShowErrorMessage("Invalid Username!",errorMsg);

        }
        void UpdateDisplayName(string name)
        {
            var request = new UpdateUserTitleDisplayNameRequest
            {
                DisplayName = name
            };
            PlayFabClientAPI.UpdateUserTitleDisplayName(request,OnDisplayNameUpdateSuccess,OnDiplayNameUpdateFail);
        }
        
        #endregion
        
        #region Player-Data

        public void GetPlayerData()
        {
            PlayFabClientAPI.GetUserData(new GetUserDataRequest(), UserDataSuccessCallback, UserDataErrorCallback);
        }
        public void SetPlayerData()
        {
            var updatePlayerDataRequest = new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>
                {
                    { "PlayerData", JsonConvert.SerializeObject(DataManager.GetAllPlayers()) }
                }
            };
        }
        #endregion
        
        #region Callbacks
        private void OnRegisterError(PlayFabError error)
        {
            OnRegisterUnSuccessful?.Invoke();
            Debug.Log(error.GenerateErrorReport());
        }
        private void OnRegisterSuccess(RegisterPlayFabUserResult result)
        {
            SessionTicket = result.SessionTicket;
            EntityID = result.EntityToken.Entity.Id;
            OnRegisterSuccessful?.Invoke(SessionTicket);
        }
        private void EmailLoginErrorResult(PlayFabError error)
        {
            OnEmailLoginUnSuccessful?.Invoke();
        }
        private void EmailLoginSuccessResult(LoginResult result)
        {
            SessionTicket = result.SessionTicket;
            EntityID = result.EntityToken.Entity.Id;
            OnEmailLoginSuccessful?.Invoke(SessionTicket);
        }
        private void UserDataErrorCallback(PlayFabError error)
        {
            OnDataFetchingError?.Invoke();
        }
        private void UserDataSuccessCallback(GetUserDataResult result)
        {
            if (result.Data != null && result.Data.ContainsKey("PlayerData"))
            {
                DataManager.SetGameData(result.Data["PlayerData"].Value);
                // bind this event with Data Manager function
                OnSuccessfulDataFetch?.Invoke();
            }
        }
        private void OnDiplayNameUpdateFail(PlayFabError error)
        {
            Debug.Log(error.GenerateErrorReport());
        }
        private void OnDisplayNameUpdateSuccess(UpdateUserTitleDisplayNameResult result)
        {
            Debug.Log(result.DisplayName);
        }
        #endregion

        #region Matchmaking
        
        public void StartMatchmaking()
        {
            // Do anything related UI and then call playfab matchmaking
            PlayFabMultiplayerAPI.CreateMatchmakingTicket(new CreateMatchmakingTicketRequest
            {
                Creator = new MatchmakingPlayer
                {
                    Entity = new PlayFab.MultiplayerModels.EntityKey
                    {
                        Id=EntityID,
                        Type="title_player_account"
                    },
                    Attributes = new MatchmakingPlayerAttributes
                    {
                        DataObject = new {}
                    }
                },
                GiveUpAfterSeconds = 120,
                QueueName = ServerConstants.SingleQueue
            },OnMatchMakingSuccess,OnMatchmakingError);
        }
        private void StartMatch(string matchID)
        {
            PlayFabMultiplayerAPI.GetMatch(new GetMatchRequest
            {
                MatchId = matchID,
                QueueName = ServerConstants.SingleQueue
            },OnGetMatch,OnMatchmakingError);
        }
        IEnumerator PollTicket()
        {
            while (true)
            {
                PlayFabMultiplayerAPI.GetMatchmakingTicket(new GetMatchmakingTicketRequest
                {
                    TicketId = TicketID,
                    QueueName = ServerConstants.SingleQueue
                }, OnGetTicketSuccess, OnMatchmakingError);
                yield return new WaitForSeconds(6);
            }
        }
        
        #region Matchmaking-Callbacks
        private void OnMatchMakingSuccess(CreateMatchmakingTicketResult matchmakingTicketResult)
        {
            TicketID = matchmakingTicketResult.TicketId;
            pollTicketRoutine = StartCoroutine(PollTicket());
        }
        private void OnGetTicketSuccess(GetMatchmakingTicketResult getTicketResult)
        {
            switch (getTicketResult.Status)
            {
                case "Matched":
                    StopCoroutine(pollTicketRoutine);
                    StartMatch(getTicketResult.MatchId);
                    break;
                case "Canceled":
                    break;
            }
        }
        private void OnGetMatch(GetMatchResult getMatchResult)
        {
          Debug.Log($"{getMatchResult.Members[0].Entity.Id} vs {getMatchResult.Members[1].Entity.Id}");
          // use the server details to connect with any server related SDK you use for your own game.
          ServerConstants.Port = getMatchResult.ServerDetails.Ports[0].Num;
          ServerConstants.IP_Address = getMatchResult.ServerDetails.IPV4Address;
        }
        private void OnMatchmakingError(PlayFabError error)
        {
          Debug.Log(error.GenerateErrorReport());
        }
        #endregion
        #endregion

        #region Virtual-Currencies
        public void GetVirtualCurrencies()
        {
            PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),OnGetUserInventorySuccess,OnGetInventoryError);
        }
        
        // to substract check fool true
        public void AddOrRemoveVirtualCurrency(CurrencyType currencyType,int amount,bool subtract=false)
        {
            if (subtract)
            {
                //subtracting virtual currency
                var subRequest = new SubtractUserVirtualCurrencyRequest
                {
                    VirtualCurrency = currencyType.ToString(),
                    Amount = amount
                };
                PlayFabClientAPI.SubtractUserVirtualCurrency(subRequest, OnChangeInVirtualCurrencySuccess, OnChangeInVirtualCurrencyFailed);
            }
            else
            {
                //adding virtual currency
                var addRequest = new AddUserVirtualCurrencyRequest
                {
                    VirtualCurrency = currencyType.ToString(),
                    Amount = amount
                };
                PlayFabClientAPI.AddUserVirtualCurrency(addRequest, OnChangeInVirtualCurrencySuccess, OnChangeInVirtualCurrencyFailed);
            }
        }
        
        #region VirtualCurrency-Callbacks
        private void OnGetUserInventorySuccess(GetUserInventoryResult result)
        {
            int shillings = result.VirtualCurrency["SH"];
            int jellies = result.VirtualCurrency["JL"];
            GameCurrency = new Currency(shillings, jellies);
            GameCurrency.UpdateCurrency();
        }    
        private void OnChangeInVirtualCurrencySuccess(ModifyUserVirtualCurrencyResult obj)
        {
            //On Success Do Anything you want to do in respective to your game and then Get Virtual Currency Again
            GetVirtualCurrencies();
        }
        private void OnGetInventoryError(PlayFabError error)
        {
            Debug.Log(error.GenerateErrorReport());
        }
        private void OnChangeInVirtualCurrencyFailed(PlayFabError error)
        {
            Debug.Log(error.GenerateErrorReport());
        }
        #endregion
        #endregion
        #endregion

    }
}