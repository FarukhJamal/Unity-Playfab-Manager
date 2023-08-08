using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Managers;
using PlayFab.ClientModels;
using EntityTokenResponse = PlayFab.AuthenticationModels.EntityTokenResponse;

namespace General
{
    public static class Utils
    {
        public static string Encrypt(string text)
        {
            MD5CryptoServiceProvider ecp = new MD5CryptoServiceProvider();
            byte[] byteString = Encoding.UTF8.GetBytes(text);
            byteString = ecp.ComputeHash(byteString);
            StringBuilder str = new StringBuilder();
            foreach (byte data in byteString)
            {
                str.Append(data.ToString("x2").ToLower());
            }
            return str.ToString();
        }
// these are just temporary naive validations just for reference
        public static bool ValidateSignUp(string name,string email,string password,string confirmPassword)
        {
            if (name.Length is < 3 or > 13)
            {
                ShowError("Invalid Name", "Name should be greater than 3 letters and less than 13 characters");
                return false;
            }

            if (!CheckEmailRegex(email))
            {
                ShowError("Invalid Email", "Invalid Email! Check your email address again.");
                return false;
            }

            if (!CheckPassword(password, confirmPassword))
            {
                ShowError("Invalid Password","Password does not match.Try again!");
            }
            return true;
        }

        public static bool ValidateLogin(string email,string password)
        {
            if (!CheckEmailRegex(email))
            {
                ShowError("Invalid Email","Email is invalid. Try Again");
                return false;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Invalid Password","Password Field is Empty");
                return false;
            }
            return true;
        }
        public static void ShowError(string title, string message)
        {
            UIManager.Instance.ShowErrorMessage(title, message);
        }

        static bool CheckEmailRegex(string email)
        {
           bool isEmail= Regex.IsMatch(email,
                   @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z",
                   RegexOptions.IgnoreCase);
           return isEmail;
        }

        static bool CheckPassword(string password, string confirmPassword)
        {
            if (!confirmPassword.Equals(password))
                return false;
            return true;
        }
        // end validations
    }
}