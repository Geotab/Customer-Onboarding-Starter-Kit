using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Geotab.CustomerOnboardngStarterKit.Utilities
{
    /// <summary>
    /// Contains methods to assist with validation.
    /// </summary>
    public static class ValidationUtility
    {
        /// <summary>
        /// Indicates whether the specified email address is valid (in terms of format).
        /// </summary>
        /// <param name="email">The email address to be validated.</param>
        /// <returns>A boolean indicating whether the specified email address is valid (in terms of format).</returns>
        static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain.
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                    RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                static string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    var domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        /// <summary>
        /// Indicates whether the supplied password is deemed valid and, if not, indicates the reason why in the errorMessage.
        /// </summary>
        /// <param name="password">The password to be evaluated.</param>
        /// <param name="errorMessage">The error message indicating why the password is invalid.</param>
        /// <returns><c>true</c> if the password is valid and <c>false</c> if the password is invalid.</returns>
        static bool IsValidPassword(string password, out string errorMessage)
        {
            errorMessage = "";

            if (string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "Password cannot be empty.";
            }

            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasMiniMaxChars = new Regex(@".{8,15}");
            var hasLowerChar = new Regex(@"[a-z]+");
            var hasSymbols = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");

            if (!hasLowerChar.IsMatch(password))
            {
                errorMessage = "Password must contain at least one lower-case letter.";
                return false;
            }
            else if (!hasUpperChar.IsMatch(password))
            {
                errorMessage = "Password must contain at least one upper-case letter.";
                return false;
            }
            else if (!hasMiniMaxChars.IsMatch(password))
            {
                errorMessage = "Password must not be less than 8 or greater than 15 characters in length.";
                return false;
            }
            else if (!hasNumber.IsMatch(password))
            {
                errorMessage = "Password must contain at least one numeric value.";
                return false;
            }
            else if (!hasSymbols.IsMatch(password))
            {
                errorMessage = "Password must contain at least one special character.";
                return false;
            }
            else
            {
                return true;
            }
        }          

        /// <summary>
        /// Checks whether the supplied email address is valid (in terms of format).  If it is not, prompts the user to input a valid email address until a valid one is entered.  Returns the valid email address.
        /// </summary>
        /// <param name="emailAddress">The email address to be validated.</param>
        /// <returns>The validated email address.</returns>
        public static string ValidateEmailAddress(string emailAddress)
        {
            if (IsValidEmail(emailAddress))
            {
                return emailAddress;
            }

            bool tryAgain = true;
            string proposedEmailAddress = emailAddress;
            while (tryAgain)
            {
                ConsoleUtility.LogInfo($"The specified email address '{proposedEmailAddress}' is not valid.");
                proposedEmailAddress = ConsoleUtility.GetUserInput($"a valid email address");
                tryAgain = !IsValidEmail(proposedEmailAddress);                
            }
            return proposedEmailAddress;
        }

        /// <summary>
        /// Checks whether the supplied password is valid.  If it is not, prompts the user to input a valid password until a valid one is entered.  Returns the valid password.
        /// </summary>
        /// <param name="password">The password to be validated.</param>
        /// <returns>The validated password.</returns>
        public static string ValidatePassword(string password)
        {
            if (IsValidPassword(password, out string errorMessage))
            {
                return password;
            }

            bool tryAgain = true;
            string proposedPassword = password;
            while (tryAgain)
            {
                ConsoleUtility.LogInfo(errorMessage);
                proposedPassword = ConsoleUtility.GetUserInputMasked($"a valid password");
                tryAgain = !IsValidPassword(proposedPassword, out errorMessage);                    
            }
            return proposedPassword;
        }        
        
        /// <summary>
        /// Checks whether the supplied inputString is a boolean.  If it is not, prompts the user to input a boolean using the entityTypeRepresented in the prompt message until a boolean is entered.  Returns the string representation of the valid boolean.
        /// </summary>
        /// <param name="inputString">The string to be evaluated.</param>
        /// <param name="entityTypeRepresented">The entity type being represented.static  For use in user feedback.</param>
        /// <returns>The string representation of the valid boolean</returns>
        public static string ValidateStringIsBoolean(string inputString, string entityTypeRepresented)
        {
            if (bool.TryParse(inputString, out _))
            {
                return inputString;
            }

            bool tryAgain = true;
            while (tryAgain)
            {
                ConsoleUtility.LogInfo($"The value '{inputString}' entered for '{entityTypeRepresented}' is not valid.");
                inputString = ConsoleUtility.GetUserInput($"a boolean (true or false) value for {entityTypeRepresented}");
                tryAgain = !bool.TryParse(inputString, out _);
            }
            return inputString;
        }

        /// <summary>
        /// Checks whether the supplied inputString is an integer.  If it is not, prompts the user to input an integer using the entityTypeRepresented in the prompt message until an integer is entered.  Returns the string representation of the valid integer.
        /// </summary>
        /// <param name="inputString">The string to be evaluated.</param>
        /// <param name="entityTypeRepresented">The entity type being represented.static  For use in user feedback.</param>
        /// <returns>The string representation of the valid integer.</returns>
        public static string ValidateStringIsInt32(string inputString, string entityTypeRepresented)
        {
            if (Int32.TryParse(inputString, out _))
            {
                return inputString;
            }

            bool tryAgain = true;
            while (tryAgain)
            {
                ConsoleUtility.LogInfo($"The value '{inputString}' entered for '{entityTypeRepresented}' is not valid.");
                inputString = ConsoleUtility.GetUserInput($"an integer value for {entityTypeRepresented}");
                tryAgain = !Int32.TryParse(inputString, out _);
            }
            return inputString;
        }

        /// <summary>
        /// Checks whether the supplied inputString is at least as long as the specified minimumLengthRequired.  If it is not, prompts the user to input a value at least as long as the specified minimumLengthRequired using the entityTypeRepresented in the prompt message until a valid entry is made.  Returns the valid string.
        /// </summary>
        /// <param name="inputString">The string to be evaluated.</param>
        /// <param name="minimumLengthRequired">The minimum length that the string must be in order to be considered valid.</param>
        /// <param name="entityTypeRepresented">The entity type being represented.static  For use in user feedback.</param>
        /// <returns>The valid string.</returns>
        public static string ValidateStringLength(string inputString, int minimumLengthRequired, string entityTypeRepresented)
        {
            if (inputString.Length >= minimumLengthRequired)
            {
                return inputString;
            }

            bool tryAgain = true;
            while (tryAgain)
            {
                ConsoleUtility.LogInfo($"The value '{inputString}' entered for '{entityTypeRepresented}' does not meet the minimum length of {minimumLengthRequired}.");
                inputString = ConsoleUtility.GetUserInput($"a value for {entityTypeRepresented} with a length of at least {minimumLengthRequired}");
                tryAgain = !(inputString.Length >= minimumLengthRequired);
            }
            return inputString;
        }        
    }
}
