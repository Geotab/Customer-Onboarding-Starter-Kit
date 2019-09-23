using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyAdminApiLib.Geotab.MyAdmin.MyAdminApi.ObjectModel;
using Geotab.Checkmate;
using Geotab.Checkmate.ObjectModel;
using Geotab.CustomerOnboardngStarterKit.Utilities;

namespace Geotab.CustomerOnboardngStarterKit
{
    /// <summary>
    /// Executes a workflow that updates devices in an existing MyGeotab database using information loaded from a CSV file.
    /// </summary>
    class Processor_UpdateDevices
    {
        const string UtilityName = "Update Devices Utility";  
        const string GeotabServer = "my.geotab.com";
        IList<ConfigItem> configItems; 
        WebServerInvoker myAdminApi;
        ApiUser myAdminApiUser;
        string myAdminApiUsername;
        string myAdminApiPassword; 
        API myGeotabApi;

        const string ArgNameResellerErpAccountId = "ResellerErpAccountId";
        const string ArgNameDatabaseName = "DatabaseName";
        const string ArgNameDevicesToUpdateFilePath = "DevicesToUpdateFilePath";

        /// <summary>
        /// Constructor is private.  Use Create() method to instantiate. Example: <c>var Processor_UpdateDevices = Processor_UpdateDevices.Create();</c>.  This is to facilitate use of MyGeotab async methods, since the 'await' operator can only be used within an async method.
        /// </summary>
        private Processor_UpdateDevices()
        {

        }

        /// <summary>
        /// Creates a <see cref="Processor_UpdateDevices"/> instance and calls its <c>Execute()</c> method.
        /// </summary>
        /// <returns>The <see cref="Processor_UpdateDevices"/> instance once execution has completed.</returns>
        public static async Task<Processor_UpdateDevices> CreateAsync()
        {
            var processor_UpdateDevices = new Processor_UpdateDevices();
            Task.Run(async() => { await processor_UpdateDevices.Execute(); }).Wait();
            return processor_UpdateDevices;
        }

        /// <summary>
        /// Executes the application workflow.
        /// </summary>
        private async Task Execute() 
        {
            Console.ForegroundColor = ConsoleColor.White;
            ConsoleUtility.LogUtilityStartup(UtilityName);

            // Get configurtaion information.
            configItems = GeotabSdkUtility.GetConfigItems("configuration");

            string resellerErpAccountId, databaseName, devicesToUpdateFilePath;

            resellerErpAccountId = configItems.Where(configItem => configItem.Key == ArgNameResellerErpAccountId).FirstOrDefault().Value;
            databaseName = configItems.Where(configItem => configItem.Key == ArgNameDatabaseName).FirstOrDefault().Value;
            devicesToUpdateFilePath = configItems.Where(configItem => configItem.Key == ArgNameDevicesToUpdateFilePath).FirstOrDefault().Value;

            // Validate input values.
            ConsoleUtility.LogInfoStart("Validating input parameter values...");
            resellerErpAccountId = ValidationUtility.ValidateStringLength(resellerErpAccountId, 6, ArgNameResellerErpAccountId);
            databaseName = ValidationUtility.ValidateStringLength(databaseName, 3, ArgNameDatabaseName);
            devicesToUpdateFilePath = ValidationUtility.ValidateStringLength(devicesToUpdateFilePath, 3, ArgNameDevicesToUpdateFilePath);
            ConsoleUtility.LogComplete();

            // Ensure devices to update file exists.
            ConsoleUtility.LogInfoStart("Checking if devices to update file exists...");
            if (!File.Exists(devicesToUpdateFilePath))
            {
                ConsoleUtility.LogInfo($"The file path '{devicesToUpdateFilePath}' entered for '{ArgNameDevicesToUpdateFilePath}' is not valid.");
                return;
            }
            ConsoleUtility.LogComplete();

            // Authenticate MyAdmin API.
            try
            {
                AdminSdkUtility.AuthenticateMyAdminApi(ref myAdminApi, ref myAdminApiUser, ref myAdminApiUsername, ref myAdminApiPassword);
            }
            catch (Exception e)
            {
                ConsoleUtility.LogError(e);
                // Provide user with a second authentication attempt.
                AdminSdkUtility.AuthenticateMyAdminApi(ref myAdminApi, ref myAdminApiUser, ref myAdminApiUsername, ref myAdminApiPassword);
            } 
            
            // Authenticate MyGeotab API.
            myGeotabApi = await GeotabSdkUtility.AuthenticateMyGeotabApiAsync(GeotabServer, databaseName, myAdminApiUsername, myAdminApiPassword);

            // Load the devices to update from the CSV file.
            ConsoleUtility.LogInfoStart($"Loading the device information that will be used to update devices in the '{databaseName}' database from file '{devicesToUpdateFilePath}'...");
            IList<DeviceDetails> deviceCandidates = null;
            using (FileStream devicesToImportFile = File.OpenRead(devicesToUpdateFilePath))
            {
                deviceCandidates =  devicesToImportFile.CsvToList<DeviceDetails>(); 
            }
            ConsoleUtility.LogComplete();
            if (!deviceCandidates.Any())
            {
                ConsoleUtility.LogInfo($"No devices were loaded from the CSV file.");
                return;
            }

            // Get existing devices.
            ConsoleUtility.LogInfoStart($"Retrieving device and device database lists...");
            IList<Device> existingDevices = await myGeotabApi.CallAsync<IList<Device>>("Get", typeof(Device)) ?? new List<Device>();
            IList<ApiDeviceDatabaseExtended> currentDeviceDatabases = AdminSdkUtility.GetCurrentDeviceDatabases(ref myAdminApi, ref myAdminApiUser, resellerErpAccountId);
            ConsoleUtility.LogComplete();          

            // Process device list, adding new devices to the database and updating existing devices.
            ConsoleUtility.LogInfo($"Processing {deviceCandidates.Count()} device(s) - attempting to update devices in MyGeotab database '{databaseName}'.");             
            foreach (DeviceDetails deviceCandidate in deviceCandidates)
            {
                // Update device if it already exists in the current database.
                if (GeotabSdkUtility.DeviceExists(deviceCandidate.SerialNumber, existingDevices))
                {
                    try
                    {
                        Device deviceToUpdate = existingDevices.Where(device => device.SerialNumber == deviceCandidate.SerialNumber).First();
                        await GeotabSdkUtility.UpdateDeviceAsync(myGeotabApi, deviceToUpdate, deviceCandidate.Name, deviceCandidate.EnableDeviceBeeping, deviceCandidate.EnableDriverIdentificationReminder, deviceCandidate.DriverIdentificationReminderImmobilizeSeconds, deviceCandidate.EnableBeepOnEngineRpm, deviceCandidate.EngineRpmBeepValue, deviceCandidate.EnableBeepOnIdle, deviceCandidate.IdleMinutesBeepValue, deviceCandidate.EnableBeepOnSpeeding, deviceCandidate.SpeedingStartBeepingSpeed, deviceCandidate.SpeedingStopBeepingSpeed, deviceCandidate.EnableBeepBrieflyWhenApprocahingWarningSpeed, deviceCandidate.EnableBeepOnDangerousDriving, deviceCandidate.AccelerationWarningThreshold, deviceCandidate.BrakingWarningThreshold, deviceCandidate.CorneringWarningThreshold, deviceCandidate.EnableBeepWhenSeatbeltNotUsed, deviceCandidate.SeatbeltNotUsedWarningSpeed, deviceCandidate.EnableBeepWhenPassengerSeatbeltNotUsed, deviceCandidate.BeepWhenReversing);
                        ConsoleUtility.LogListItemWithResult($"{deviceCandidate.SerialNumber} ({deviceCandidate.Name})", $"UPDATED", ConsoleColor.Green);                     
                    }
                    catch (Exception ex)
                    {
                        ConsoleUtility.LogListItemWithResult($"{deviceCandidate.Name}", $"NOT UPDATED: ERROR updating device: {ex.Message}\n{ex.StackTrace}", ConsoleColor.Red);    
                    }
                    continue;
                }

                // Device does not exist in the current database.  Instead of updating, try to add the device to the database instead.
                IList<Group> deviceGroups = new List<Group>();

                // Use the serial number for the name if a name is not provided.
                if (string.IsNullOrEmpty(deviceCandidate.Name))
                {
                    deviceCandidate.Name = deviceCandidate.SerialNumber;
                }
                
                string deviceCandidateSerialNumber = deviceCandidate.SerialNumber.Replace("-", "");

                // Check if the device is already in any database.
                if (currentDeviceDatabases.Where(database => database.SerialNumber == deviceCandidateSerialNumber).Any())
                {
                    IList<ApiDeviceDatabaseExtended> exitstingDatabases = currentDeviceDatabases.Where(database => database.SerialNumber == deviceCandidateSerialNumber).ToList();

                    StringBuilder existingDatabaseNames = new StringBuilder();
                    foreach (ApiDeviceDatabaseExtended database in exitstingDatabases)
                    {
                        if (existingDatabaseNames.Length > 0)
                        {
                            existingDatabaseNames.Append($", '{database.DatabaseName}'");
                        }
                        else
                        {
                           existingDatabaseNames.Append($"'{database.DatabaseName}'"); 
                        }
                    }
                    ConsoleUtility.LogListItemWithResult($"{deviceCandidate.Name}", $"NOT UPDATED OR ADDED: Device does not exist in '{databaseName}' database, but already exists in MyGeotab database(s) {existingDatabaseNames.ToString()}.", ConsoleColor.Red);
                    continue;                    
                }

                // Assign the device to the Company group.
                deviceGroups.Add(new CompanyGroup());

                // Add the device to the MyGeotab database.
                try
                {
                    // Create the device object.
                    Device newDevice = Device.FromSerialNumber(deviceCandidate.SerialNumber);
                    newDevice.PopulateDefaults();
                    newDevice.Name = deviceCandidate.Name;
                    newDevice.Groups = deviceGroups;
                    newDevice.WorkTime = new WorkTimeStandardHours();

                    // Add the device.
                    var addedDevice = await GeotabSdkUtility.AddDeviceAsync(myGeotabApi, newDevice);
                    ConsoleUtility.LogListItemWithResult($"{deviceCandidate.SerialNumber}", $"ADDED: Device did not previously exist in '{databaseName}' database", ConsoleColor.Green);

                    // Update the device properties.
                    await GeotabSdkUtility.UpdateDeviceAsync(myGeotabApi, addedDevice, deviceCandidate.Name, deviceCandidate.EnableDeviceBeeping, deviceCandidate.EnableDriverIdentificationReminder, deviceCandidate.DriverIdentificationReminderImmobilizeSeconds, deviceCandidate.EnableBeepOnEngineRpm, deviceCandidate.EngineRpmBeepValue, deviceCandidate.EnableBeepOnIdle, deviceCandidate.IdleMinutesBeepValue, deviceCandidate.EnableBeepOnSpeeding, deviceCandidate.SpeedingStartBeepingSpeed, deviceCandidate.SpeedingStopBeepingSpeed, deviceCandidate.EnableBeepBrieflyWhenApprocahingWarningSpeed, deviceCandidate.EnableBeepOnDangerousDriving, deviceCandidate.AccelerationWarningThreshold, deviceCandidate.BrakingWarningThreshold, deviceCandidate.CorneringWarningThreshold, deviceCandidate.EnableBeepWhenSeatbeltNotUsed, deviceCandidate.SeatbeltNotUsedWarningSpeed, deviceCandidate.EnableBeepWhenPassengerSeatbeltNotUsed, deviceCandidate.BeepWhenReversing);
                    ConsoleUtility.LogListItemWithResult($"{deviceCandidate.SerialNumber} ({deviceCandidate.Name})", $"UPDATED", ConsoleColor.Green);
                }
                catch (Exception ex)
                {
                    ConsoleUtility.LogListItemWithResult($"{deviceCandidate.Name}", $"NOT UPDATED OR ADDED: Device did not previously exist in '{databaseName}' database, but an error was encountered while attempting to add the device. ERROR adding device: {ex.Message}\n{ex.StackTrace}", ConsoleColor.Red);    
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Completed processing.");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}