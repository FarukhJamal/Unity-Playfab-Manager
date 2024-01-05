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
        public Button backBtn,enterBtn,loginBtn,registerBtn, guestBtn, googleSignIn;
        public TMP_InputField userName, emailID, password,confirmPassword;
        public RectTransform Signup, InputPanel;
        private void Start()
        {
            AddListerners();
        }

        private void AddListerners()
        {
           backBtn.onClick.AddListener(RemoveListerners);
           loginBtn.onClick.AddListener(LoginClicked);
           registerBtn.onClick.AddListener(SignUpwithEmailClicked);
            guestBtn.onClick.AddListener(SignUpAsGuestClicked);
            googleSignIn.onClick.AddListener(GoogleSignIn);
        } 

        private void RemoveListerners()
        {
            enterBtn.onClick.RemoveAllListeners();
        }

        void SignUpWithEmail()
        {
            PlayfabManager.Instance.PlayfabSignUpRequest(userName.text, emailID.text, password.text,confirmPassword.text);
        }

        void SignUpWithGuestID()
        {
            PlayfabManager.Instance.LoginWithGuestUser();
        }


        void GoogleSignIn()
        {
            PlayfabManager.Instance.LoginWithGoogle();
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

        void SignUpwithEmailClicked()
        {
            confirmPassword.gameObject.SetActive(true);
            userName.gameObject.SetActive(true);
            enterBtn.onClick.AddListener(SignUpWithEmail);
        }

        void SignUpAsGuestClicked()
        {
            SignUpWithGuestID();
        }
 
    }
    
}