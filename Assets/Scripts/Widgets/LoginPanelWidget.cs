using System;
using eeGames.Widget;
using General;
using Managers;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Widgets
{
    public class LoginPanelWidget:Widget
    {
        public Button backBtn,enterBtn,loginBtn,registerBtn;
        public TMP_InputField userName, emailID, password,confirmPassword;
        private void Start()
        {
            AddListerners();
        }

        private void AddListerners()
        {
           backBtn.onClick.AddListener(RemoveListerners);
           loginBtn.onClick.AddListener(LoginClicked);
           registerBtn.onClick.AddListener(SignUpClicked);
        }

        private void RemoveListerners()
        {
            enterBtn.onClick.RemoveAllListeners();
        }

        void SignUp()
        {
            PlayfabManager.Instance.PlayfabSignUpRequest(userName.text, emailID.text, password.text,confirmPassword.text);
        }

        void Login()
        {
            
            PlayfabManager.Instance.LoginWithEmail(emailID.text,password.text);
        }

        void LoginClicked()
        {
            confirmPassword.gameObject.SetActive(false);
            userName.gameObject.SetActive(false);
            enterBtn.onClick.AddListener(Login);
        }
        void SignUpClicked()
        {
            confirmPassword.gameObject.SetActive(true);
            userName.gameObject.SetActive(true);
            enterBtn.onClick.AddListener(SignUp);
        }
 
    }
    
}