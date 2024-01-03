using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.PackageManager;

namespace General
{
    public  class Validations
    {
        #region Register

        public  bool ValidateRegisteration(string name,string email,string password,string confirmPassword, out string errorMsg)
        {
            if (!IsEmailValid(email))
            {
                errorMsg = "Wrong Email format.";
                return false;
            }
            if (!IsPasswordValid(password, out errorMsg))
            {
                return false;
            }
            if (!IsPasswordMatch(password, confirmPassword))
            {
                errorMsg = "Passwords do not match. Retype your password.";
                return false;
            }

            errorMsg = String.Empty;
            return true;
        }
        #endregion

        #region Login

        public  bool ValidateLogin(string username, string password,out string errorMsg)
        {
            string error = "";
            if (!IsValidUsername(username))
            {
                errorMsg = "Username's first letter should be Capital and length should be in between 3 and 32 with alphanumeric values in it.";
                return false;
            }

            if (!IsPasswordValid(password, out error))
            {
                errorMsg = error;
                return false;
            }
            errorMsg = String.Empty;
            return true;
        }

        #endregion

        #region Name Validations
        
        public static bool IsValidUsername(string username)
        {
            return Rules.All(rule => rule(username));
        }
        private static bool IsNotNull(string username)
        {
            return username != null;
        }

        private static bool IsInLengthRange(string username)
        {
            var length = username.Length;
            return length >= 3 && length <= 32;
        }

        private static bool IsFirstLetterCapital(string username)
        {
            return IsLowerAlphanumeric(username[0]);
        }
        private static bool IsLowerAlpha(char c)
        {
            return c >= 'a' && c <= 'z';
        }

        private static bool IsLowerAlphanumeric(char c)
        {
            return IsLowerAlpha(c) || (c >= '0' && c <= '9');
        }
        
        private static readonly Predicate<string>[] Rules = new Predicate<string>[]
        {
            IsNotNull,
            IsInLengthRange
        };
        #endregion

        #region Email Validation

        public static bool IsEmailValid(string email)
        {
            return Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
        }
        #endregion

        #region Password Validation

        public static bool IsPasswordValid(string password, out string ErrorMessage)
        {
            var input = password;
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                throw new Exception("Password should not be empty");
            }

            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasMiniMaxChars = new Regex(@".{8,15}");
            var hasLowerChar = new Regex(@"[a-z]+");
            var hasSymbols = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");

            if (!hasLowerChar.IsMatch(input))
            {
                ErrorMessage = "Password should contain at least one lower case letter.";
                return false;
            }
            else if (!hasUpperChar.IsMatch(input))
            {
                ErrorMessage = "Password should contain at least one upper case letter.";
                return false;
            }
            else if (!hasMiniMaxChars.IsMatch(input))
            {
                ErrorMessage = "Password should not be lesser than 8 or greater than 15 characters.";
                return false;
            }
            else if (!hasNumber.IsMatch(input))
            {
                ErrorMessage = "Password should contain at least one numeric value.";
                return false;
            }

            else if (!hasSymbols.IsMatch(input))
            {
                ErrorMessage = "Password should contain at least one special case character.";
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool IsPasswordMatch(string password, string confirmPassword)
        {
            if (password.Length != confirmPassword.Length && !confirmPassword.Equals(password))
                return false;
            return true;
        }

        #endregion
    }
}