using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using eeGames.Widget;
using General;
using GooglePlayGames;
using Models;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using UnityEngine;
using Widgets;
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

        #region Google Sign In 
        PFGoogleSignInUnity GoogleSignIn;
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
        public string MyPlayfabID;
        public string MyUserName;
        public bool IsAccountData = false;
        public static PlayfabManager Instance;
        public UIManager UI_Manager;
        public static string SessionTicket;
        public static string EntityID;
        public Currency GameCurrency;          // Virtual Currency if any
        #endregion

        #region Private-Variables
        private const string encryptionKey = "SecretKey";
        private string TicketID;
        private Coroutine pollTicketRoutine;
        private bool IsGuestLogin = false;
        private bool IsEmailLogin = false;
        private string Email = null, Password = null, GuestID = null, Guest = null;
        private LoadMainMenuWidget menuWidget;
        //private bool ISGoogleLogin = false;
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
            GoogleSignIn = new PFGoogleSignInUnity();
            
            //AutoLogin
            GetBoolDataFromPlayerPrefs();

            if(!IsEmailLogin && !IsGuestLogin)
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
            Debug.Log("Player Registeration request created");
            if (Validator.ValidateRegisteration(userName,emailId,password,confirmPassword, out string errorMsg))
            {
                Debug.Log("Player info validated before registeration");
                Email = emailId;
                Password = password;
                PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegisterSuccess, OnRegisterError);
            }
            else
                UIManager.Instance.ShowErrorMessage("Invalid Register! \n Error!Kindly check your credentials Again. \n", errorMsg);
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
            Debug.Log("Log in request created");
            if(Validator.ValidateLogin(emailId,password,out string errorMsg))
            {
                Email = emailId;
                Password = password;
                PlayFabClientAPI.LoginWithEmailAddress(loginRequest, EmailLoginSuccessResult, EmailLoginErrorResult);
                Debug.Log("Logged In");
            }
            else
                UIManager.Instance.ShowErrorMessage("Invalid Username!",errorMsg);

        }
        public void LoginWithGuestUser()
        {
            var request = new LoginWithCustomIDRequest
            {
                CustomId = SystemInfo.deviceUniqueIdentifier,
                CreateAccount = true,
            };
            PlayFabClientAPI.LoginWithCustomID(request, GuestLoginSuccessResult, GuestLoginErrorResult);
        }
        public void LoginWithGoogle()
        {
            PlayGamesPlatform.Instance.Authenticate(GoogleSignIn.ProcessAuthentication);
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
        public void GetAccountInfo()
        {
            GetAccountInfoRequest request = new GetAccountInfoRequest();
            PlayFabClientAPI.GetAccountInfo(request, AccountInfoSuccess, AcountInfoFail);
        }

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
            UIManager.Instance.ShowErrorMessage("Invalid Register! \n Error!Kindly check your credentials Again. \n", error.ToString());
            Email = null;
            Password = null;
            Debug.Log(error.GenerateErrorReport());
        }
        private void OnRegisterSuccess(RegisterPlayFabUserResult result)
        {
            Debug.Log("Player successfully registered");

            SessionTicket = result.SessionTicket;
            EntityID = result.EntityToken.Entity.Id;
            OnRegisterSuccessful?.Invoke(SessionTicket);
            
            IsEmailLogin = true;
            UserDataStoringInPlayerPrefs();
            UpdateDisplayName(result.Username);
            Widget widget = UIManager.Instance.GetUI(WidgetName.PlayfabLogin);
            LoginPanelWidget loginPanel = widget.GetComponent<LoginPanelWidget>();
            loginPanel.Signup.gameObject.SetActive(true);
            loginPanel.InputPanel.gameObject.SetActive(false);
            loginPanel.userName.text = null;
            loginPanel.emailID.text = null;
            loginPanel.password.text = null;
            loginPanel.confirmPassword.text = null;
        }
        private void EmailLoginErrorResult(PlayFabError error)
        {
            OnEmailLoginUnSuccessful?.Invoke();
            UIManager.Instance.ShowErrorMessage("No such account found\n", error.ToString());
            Email = null;
            Password = null;
        }
        private void EmailLoginSuccessResult(LoginResult result)
        {
            SessionTicket = result.SessionTicket;
            EntityID = result.EntityToken.Entity.Id;
            OnEmailLoginSuccessful?.Invoke(SessionTicket);
            IsEmailLogin = true;
            UserDataStoringInPlayerPrefs();
            UIManager.Instance.SpawnUI(WidgetName.PlayfabMainMenu);
            Widget widget = UIManager.Instance.GetUI(WidgetName.PlayfabMainMenu);
            menuWidget = widget.gameObject.GetComponent<LoadMainMenuWidget>();
        }
        private void GuestLoginErrorResult(PlayFabError error)
        {
            OnEmailLoginUnSuccessful?.Invoke();
            UIManager.Instance.ShowErrorMessage("Failed To Create Guest Account\n", error.ToString());
        }
        private void GuestLoginSuccessResult(LoginResult result)
        {
            SessionTicket = result.SessionTicket;
            EntityID= result.EntityToken.Entity.Id;
            OnEmailLoginSuccessful?.Invoke(SessionTicket);
            if(!IsGuestLogin)
            {
                Guest = GuestName();

                IsGuestLogin = true;

                UserDataStoringInPlayerPrefs(Guest);
            }
            else
            {
                Guest = GuestName();
                MyUserName = Guest;
                MyPlayfabID = GuestID;
            }

            UIManager.Instance.SpawnUI(WidgetName.PlayfabMainMenu);

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
        void AccountInfoSuccess(GetAccountInfoResult result)
        {
            Debug.Log("Account Info Success");
            MyPlayfabID = result.AccountInfo.PlayFabId;
            if(IsGuestLogin)
            {
                MyUserName = Guest;
            }
            else
            {
                MyUserName = result.AccountInfo.Username;
            }
            Debug.Log("PlayFab ID: " + MyPlayfabID + "\nUser Name: " + MyUserName);

            IsAccountData = true;
        }
        void AcountInfoFail(PlayFabError error)
        {
            Debug.LogError(error.GenerateErrorReport());
        }
        void OnLeaderBoardUpdate(UpdatePlayerStatisticsResult result)
        {
            Debug.Log("Successful Leaderboard Sent " + result.ToJson());
        }
        void OnLeaderboardError(PlayFabError error)
        {
            Debug.LogError(error.GenerateErrorReport());
        }
        void OnGetLeaderbaordSuccess(GetLeaderboardResult result)
        {
            foreach(var item in result.Leaderboard)
            {
                menuWidget.LeaderBoardDataPlacment(item.Position, item.DisplayName, item.StatValue);
            }
        }
        void OnGetLeaderboardFail(PlayFabError error)
        {
            Debug.Log(error.GenerateErrorReport());
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

        #region AutoLogin System
        void UserDataStoringInPlayerPrefs(string email = null, string password = null)
        {
            PlayerPrefs.SetInt("IsGuestLogin", IsGuestLogin ? 1 : 0);
            PlayerPrefs.SetInt("IsEmailLogin", IsEmailLogin ? 1 : 0);
            
            if(IsEmailLogin)
            {
                PlayerPrefs.SetString("Email", Encrypt(Email));
                PlayerPrefs.SetString("Password", Encrypt(Password));
            }
            else if(IsGuestLogin)
            {
                PlayerPrefs.SetString("GuestName", Encrypt(email));

                PlayerPrefs.SetString("CustomID", Encrypt(SystemInfo.deviceUniqueIdentifier));
            }
            PlayerPrefs.SetString("SessionTicket", SessionTicket);
        }

        void GetBoolDataFromPlayerPrefs()
        {
            int GuestLogin = PlayerPrefs.GetInt("IsGuestLogin");
            int EmailLogin = PlayerPrefs.GetInt("IsEmailLogin");

            if (GuestLogin == 1)
            {
                IsGuestLogin = true;
            }
            else
            {
                IsGuestLogin = false;
            }
            
            if (EmailLogin == 1)
            {
                IsEmailLogin = true;
            }
            else
            {
                IsEmailLogin = false;
            }

            AutoLogin(IsGuestLogin, IsEmailLogin);

        }

        void AutoLogin(bool IsGuestLogin, bool IsEmailLogin)
        {
            if(IsEmailLogin)
            {
                string encryptedemail = PlayerPrefs.GetString("Email");
                string encryptedpassword = PlayerPrefs.GetString("Password");

                Email = Decrypt(encryptedemail);
                Password = Decrypt(encryptedpassword);
                
                LoginWithEmail(Email, Password);
                Debug.Log("Auto Loggedin with Email" + Email);
            }
            else if(IsGuestLogin)
            {
                string encryptedCustomID = PlayerPrefs.GetString("CustomID");
                GuestID = Decrypt(encryptedCustomID);
                string encryptedGuestName = PlayerPrefs.GetString("GuestName");
                Guest = Decrypt(encryptedGuestName);
                
                if (SystemInfo.deviceUniqueIdentifier == GuestID)
                {
                    LoginWithGuestUser();
                    Debug.Log("Auto Loggedin with customID" + GuestID);
                }
            }
        }

        private string Encrypt(string plainText)
        {
            char[] keyChars = encryptionKey.ToCharArray();
            char[] plainTextChars = plainText.ToCharArray();
            for (int i = 0; i < plainTextChars.Length; i++)
            {
                plainTextChars[i] = (char)(plainTextChars[i] ^ keyChars[i % keyChars.Length]);
            }
            return BitConverter.ToString(System.Text.Encoding.UTF8.GetBytes(new string(plainTextChars))).Replace("-", "");
        }

        private string Decrypt(string hexText)
        {
            char[] keyChars = encryptionKey.ToCharArray();
            byte[] hexBytes = new byte[hexText.Length / 2];
            for (int i = 0; i < hexBytes.Length; i++)
            {
                hexBytes[i] = Convert.ToByte(hexText.Substring(i * 2, 2), 16);
            }
            char[] decryptedChars = System.Text.Encoding.UTF8.GetChars(hexBytes);
            for (int i = 0; i < decryptedChars.Length; i++)
            {
                decryptedChars[i] = (char)(decryptedChars[i] ^ keyChars[i % keyChars.Length]);
            }
            return new string(decryptedChars);
        }
        #endregion

        #region Assign Name to Guest
        string GuestName()
        {
            int randValues = UnityEngine.Random.Range(100000, 900000);
            string GuestName = "User" + randValues.ToString();
            UpdateDisplayName(GuestName);
            return GuestName;
        }
        #endregion

        #region Leaderboard
        public void SendLeaderboard(int score)
        {
            var request = new UpdatePlayerStatisticsRequest
            {
                Statistics = new List<StatisticUpdate> {
                    new StatisticUpdate {
                        StatisticName = "Match-3 Game",
                        Value = score
                    }
                }
            };
            PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderBoardUpdate, OnLeaderboardError);
        }

        public void GetLeaderboard()
        {
            var request = new GetLeaderboardRequest
            {
                StatisticName = "Match-3 Game",
                StartPosition = 0,
                MaxResultsCount = 10
            };
            Debug.Log("Sending Leaderboard Get Data Request");
            PlayFabClientAPI.GetLeaderboard(request, OnGetLeaderbaordSuccess, OnGetLeaderboardFail);
        }
        #endregion
        #endregion

    }
}