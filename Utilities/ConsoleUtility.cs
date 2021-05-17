using System;
using System.IO;

namespace Geotab.CustomerOnboardngStarterKit.Utilities
{
    /// <summary>
    /// Contains methods to assist in working with the console.
    /// </summary>
    public static class ConsoleUtility
    {
        /// <summary>
        /// Prompts the user for input and returns that input.
        /// </summary>
        /// <param name="promptMessage">The message to prompt the user with.  Will be prefixed with <c>"> Enter "</c>.</param>
        /// <returns>The user input.</returns>
        public static string GetUserInput(string promptMessage) 
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("> Enter {0}:", promptMessage);
            Console.ForegroundColor = ConsoleColor.Cyan;
            string userInput = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.White;
            return userInput;
        }

        /// <summary>
        /// Prompts the user to input a file path followed by a file name until valid entries are made.  The validated full path is returned.
        /// </summary>
        /// <param name="fileTypeDescription">A description of the file type being sought (e.g. '<c>config</c>').  For use in user prompts.</param>
        /// <returns>The validated full path.</returns>
        public static string GetUserInputFilePath(string fileTypeDescription) 
        {
            string filePath = string.Empty;
            string fileFullPath = string.Empty;
            bool filePathIsValid = false;
            bool fileNameIsValid = false;

            // Get the user to enter a valid directory path.
            while(!filePathIsValid)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"> Enter {fileTypeDescription} folder (e.g. 'C:\\Temp'):");
                Console.ForegroundColor = ConsoleColor.Cyan;
                filePath = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.White;
                if (Directory.Exists(filePath))
                {
                    filePathIsValid = true;
                }
                else
                {
                    ConsoleUtility.LogError($"The folder entered does not exist.");
                }
            }

            // Get the use to enter a valid filename.
            while(!fileNameIsValid)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"> Enter {fileTypeDescription} file name (e.g. 'FileName.csv'):");
                Console.ForegroundColor = ConsoleColor.Cyan;
                string fileName = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.White;
                fileFullPath = Path.Combine(filePath, fileName);
                if (File.Exists(fileFullPath))
                {
                    fileNameIsValid = true;
                }
                else
                {
                    ConsoleUtility.LogError($"The file '{fileName}' does not exist in folder '{filePath}'.");
                }
            }
            return fileFullPath;
        }

        /// <summary>
        /// Prompts the user for input and returns that input.  The input is masked in the console as it is being entered.  For use with  passwords and other sensitive information that should not be displayed on the user's screen.
        /// </summary>
        /// <param name="promptMessage">The message to prompt the user with.  Will be prefixed with <c>"> Enter "</c>.</param>
        /// <returns>The user input.</returns>
        public static string GetUserInputMasked(string promptMessage) 
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("> Enter {0}:", promptMessage);
            Console.ForegroundColor = ConsoleColor.Cyan;

            string userInput = "";
            while (true) {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter) {
                    break;
                }
                else if (key.Key == ConsoleKey.Backspace) {
                    if (userInput.Length > 0) {
                        userInput = userInput.Remove(userInput.Length -1);
                        Console.Write("\b \b");
                    }
                }
                else if (key.KeyChar != '\u0000') {
                    // KeyChar == '\u0000' if the key pressed does not correspond 
                    // to a printable character (e.g. F1, PrtScr, etc.)
                    userInput += key.KeyChar;
                    Console.Write("*");
                }
            }
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.White;
            return userInput;
        }

        /// <summary>
        /// Prompts the user for input (will be prefixed with <c>"> Enter "</c>) twice and checks to ensure that both values match.  If both values do not match, user is prompted to re-enter values.  Repeats until a matching pair of values is entered and returns that input.  The input is masked in the console as it is being entered.  For use with  passwords and other sensitive information that should not be displayed on the user's screen.
        /// </summary>
        /// <param name="promptMessage">The message to prompt the user with.</param>
        /// <param name="verifyPromptMessage">The message to prompt the user with indicating that re-entry of the same value is required.</param>
        /// <returns>The verified user input.</returns>
        public static string GetVerifiedUserInputMasked(string promptMessage, string verifyPromptMessage) 
        {
            string input = GetUserInputMasked(promptMessage);
            string verifyInput = GetUserInputMasked(verifyPromptMessage);

            if (verifyInput != input)
            {
                LogError("Values entered do not match.  Try again.");
                input = GetVerifiedUserInputMasked(promptMessage, verifyPromptMessage);                
            }
            return input;
        }

        /// <summary>
        /// Adds <c>"COMPLETE."</c> to a log line.
        /// </summary>
        public static void LogComplete() 
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("COMPLETE.");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// Adds a log line that prefixes the supplied message with <c>"ERROR: "</c>.
        /// </summary>
        /// <param name="errorMessage">The message to be logged.</param>  
        public static void LogError(string errorMessage) 
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERROR: {errorMessage}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// Adds a log line that prefixes the supplied <see cref="Exception"/> with <c>"ERROR: "</c>.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> to be logged.</param>   
        public static void LogError(Exception exception) 
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERROR: {exception.Message}\n{exception.StackTrace}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// Adds a log line that prefixes the supplied message with <c>"INFO: "</c>.
        /// </summary>
        /// <param name="infoMessage">The message to be logged.</param>
        public static void LogInfo(string infoMessage) 
        {
            Console.WriteLine($"INFO: {infoMessage}");
        }

        /// <summary>
        /// Starts a log line that prefixes the supplied message with <c>"INFO: "</c>.
        /// </summary>
        /// <param name="infoMessage">The message to be logged.</param>
        public static void LogInfoStart(string infoMessage) 
        {
            Console.Write($"INFO: {infoMessage}");
        }

        /// <summary>
        /// Starts a log line that prefixes the supplied message with <c>"INFO: "</c>.  The infoMessagePart1 and infoMessagePart2 values are concatenated (with a single space between them) and the <see cref="ConsoleColor"/> specified by infoMessagePart2Color is applied to the infoMessagePart2 portion of the message. 
        /// </summary>
        /// <param name="infoMessagePart1"></param>
        /// <param name="infoMessagePart2"></param>
        /// <param name="infoMessagePart2Color"></param>
        public static void LogInfoStartMultiPart(string infoMessagePart1, string infoMessagePart2, ConsoleColor infoMessagePart2Color) 
        {
            Console.Write($"INFO: {infoMessagePart1} ");
            Console.ForegroundColor = infoMessagePart2Color;
            Console.Write($"{infoMessagePart2} ");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// Adds a log line that prefixes the supplied message with <c>"- "</c>.
        /// </summary>
        /// <param name="listItem">The message to be logged.</param>   
        public static void LogListItem(string listItem) 
        {
            Console.WriteLine($"- {listItem}");
        }

        /// <summary>
        /// Adds a log line that prefixes the supplied message with <c>"- "</c>.
        /// </summary>
        /// <param name="listItem">The message to be logged.</param>   
        
        
        /// <summary>
        /// Adds a log line that prefixes the supplied message with <c>"- "</c>.  The listItemId is then added in the colour specified by listItemIdColor, followed by a space and the listItem.
        /// </summary>
        /// <param name="listItemId">The Id of the item.</param>
        /// <param name="listItem">The name of the item.</param>
        /// <param name="listItemIdColor">The <see cref="ConsoleColor"/> to be applied to the listItemId.</param>
        public static void LogListItem(string listItemId, string listItem, ConsoleColor listItemIdColor) 
        {
            Console.Write($"- ");
            Console.ForegroundColor = listItemIdColor;
            Console.Write($"{listItemId} ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{listItem}");
        }        

        /// <summary>
        /// Adds a log line that prefixes the supplied message with <c>"- "</c>.  The listItem is then added, followed by <c>"..."</c> and the result.  The result is coloured based on the value of resultColor.
        /// </summary>
        /// <param name="listItem">The name of the item.</param>
        /// <param name="result">The result information.</param>
        /// <param name="resultColor">The <see cref="ConsoleColor"/> to be applied to the result.</param>
        public static void LogListItemWithResult(string listItem, string result, ConsoleColor resultColor) 
        {
            Console.Write($"- {listItem}...");
            Console.ForegroundColor = resultColor;
            Console.WriteLine(result);
            Console.ForegroundColor = ConsoleColor.White;
        }   

        /// <summary>
        /// Adds log lines indicating that the specified utility has started.
        /// </summary>
        /// <param name="utilityName">The name of the utility.</param>
        public static void LogUtilityStartup(string utilityName) 
        {   
            string spacer = "======================================================================";
            Console.WriteLine("");
            Console.WriteLine(spacer);
            Console.WriteLine($"{utilityName} started.");
            Console.WriteLine(spacer);
        }              

        /// <summary>
        /// Adds a log line that prefixes the supplied message with <c>"WARNING: "</c>.
        /// </summary>
        /// <param name="warningMessage">The message to be logged.</param>
        public static void LogWarning(string warningMessage) 
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"WARNING: {warningMessage}");
            Console.ForegroundColor = ConsoleColor.White;
        }   
    }
}
