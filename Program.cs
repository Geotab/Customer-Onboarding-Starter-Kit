using System;
using Geotab.CustomerOnboardngStarterKit.Utilities;
namespace Geotab.CustomerOnboardngStarterKit
{
    class Program
    {
        public static void Main()
        {
            try
            {
                Console.Title = "Geotab Customer Onboarding Starter Kit";
                ConsoleUtility.LogUtilityStartup("Customer Onboarding Starter Kit");

                ConsoleUtility.LogInfo("Available Utilities:");
                ConsoleUtility.LogListItem("1", ": Create Database & Load Devices", ConsoleColor.Green);
                ConsoleUtility.LogListItem("2", ": Update Devices", ConsoleColor.Green);

                bool utilitySelected = false;
                while (!utilitySelected)
                {
                    utilitySelected = true;
                    string input = ConsoleUtility.GetUserInput("number of the utility to launch (from the above list)");
                    if (int.TryParse(input, out int selection))
                    {
                        switch (selection)
                        {
                            case 1:
                                var processor_CreateDatabaseAndLoadDevices = Processor_CreateDatabaseAndLoadDevices.Create();
                                break;
                            case 2:
                                var processor_UpdateDevices = Processor_UpdateDevices.Create();
                                break;
                            default:
                                utilitySelected = false;
                                ConsoleUtility.LogError($"The value '{input}' is not valid.");
                                break;
                        }
                    }
                    else
                    {
                        utilitySelected = false;
                        ConsoleUtility.LogError($"The value '{input}' is not valid.");
                    }
                }
            }
            catch (Exception e)
            {
                ConsoleUtility.LogError(e);
            }
            finally
            {
                Console.WriteLine("======================================================================");
                Console.ForegroundColor = ConsoleColor.Yellow;
                ConsoleUtility.LogInfo("Customer Onboarding Starter Kit finshed.  Press any key to exit.");
                Console.ReadKey();
            }
        }
    }
}
