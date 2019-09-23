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
    /// Executes a workflow that, at a high level:
    /// <list type="bullet">
    /// <item>Creates a new MyGeotab database</item>
    /// <item>Adds an administrative user to the new database</item>
    /// <item>Uploads a list of devices from a CSV file into the new database</item>
    /// </list>
    /// </summary>
    class Processor_CreateDatabaseAndLoadDevices
    {
        const string UtilityName = "Create Database & Load Devices Utility";
        const string GeotabServer = "my.geotab.com";  
        IList<ConfigItem> configItems;  
        WebServerInvoker myAdminApi;
        ApiUser myAdminApiUser;
        string myAdminApiUsername;
        string myAdminApiPassword;
        API myGeotabApi;

        const string ArgNameResellerName = "ResellerName";
        const string ArgNameResellerErpAccountId = "ResellerErpAccountId";
        const string ArgNameCustomerCompanyName = "CustomerCompanyName";
        const string ArgNameCustomerAccountAdminFirstName = "CustomerAccountAdminFirstName";
        const string ArgNameCustomerAccountAdminLastName = "CustomerAccountAdminLastName";
        const string ArgNameCustomerAccountAdminEmail = "CustomerAccountAdminEmail";
        const string ArgNameCustomerPhoneNumber = "CustomerPhoneNumber";
        const string ArgNameCustomerFleetSize = "CustomerFleetSize";
        const string ArgNameCustomerSignUpForNews = "CustomerSignUpForNews";
        const string ArgNameCustomerDesiredDatabaseName = "CustomerDesiredDatabaseName";
        const string ArgNameCustomerTimeZoneId = "CustomerTimeZoneId";
        const string ArgNameCustomerDeviceListPath = "CustomerDeviceListPath";

        /// <summary>
        /// Constructor is private.  Use Create() method to instantiate. Example: <c>var processor_CreateDatabaseAndLoadDevices = Processor_CreateDatabaseAndLoadDevices.Create();</c>.  This is to facilitate use of MyGeotab async methods, since the 'await' operator can only be used within an async method.
        /// </summary>
        private Processor_CreateDatabaseAndLoadDevices()
        {

        }

        /// <summary>
        /// Creates a <see cref="Processor_CreateDatabaseAndLoadDevices"/> instance and calls its <c>Execute()</c> method.
        /// </summary>
        /// <returns>The <see cref="Processor_CreateDatabaseAndLoadDevices"/> instance once execution has completed.</returns>
        public static async Task<Processor_CreateDatabaseAndLoadDevices> CreateAsync()
        {
            var processor_CreateDatabaseAndLoadDevices = new Processor_CreateDatabaseAndLoadDevices();
            Task.Run(async() => { await processor_CreateDatabaseAndLoadDevices.Execute(); }).Wait();
            return processor_CreateDatabaseAndLoadDevices;
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

            string resellerName, resellerErpAccountId, customerCompanyName, customerAccountAdminFirstName,  customerAccountAdminLastName, customerAccountAdminEmail, customerPhoneNumber, customerFleetSize, customerSignUpForNews, customerDesiredDatabaseName, customerTimeZoneId, customerDeviceListPath;

            resellerName = configItems.Where(configItem => configItem.Key == ArgNameResellerName).FirstOrDefault().Value;
            resellerErpAccountId = configItems.Where(configItem => configItem.Key == ArgNameResellerErpAccountId).FirstOrDefault().Value;
            customerCompanyName = configItems.Where(configItem => configItem.Key == ArgNameCustomerCompanyName).FirstOrDefault().Value;
            customerAccountAdminFirstName = configItems.Where(configItem => configItem.Key == ArgNameCustomerAccountAdminFirstName).FirstOrDefault().Value;
            customerAccountAdminLastName = configItems.Where(configItem => configItem.Key == ArgNameCustomerAccountAdminLastName).FirstOrDefault().Value;
            customerAccountAdminEmail =configItems.Where(configItem => configItem.Key == ArgNameCustomerAccountAdminEmail).FirstOrDefault().Value;
            customerPhoneNumber = configItems.Where(configItem => configItem.Key == ArgNameCustomerPhoneNumber).FirstOrDefault().Value;
            customerFleetSize = configItems.Where(configItem => configItem.Key == ArgNameCustomerFleetSize).FirstOrDefault().Value;
            customerSignUpForNews = configItems.Where(configItem => configItem.Key == ArgNameCustomerSignUpForNews).FirstOrDefault().Value;
            customerDesiredDatabaseName = configItems.Where(configItem => configItem.Key == ArgNameCustomerDesiredDatabaseName).FirstOrDefault().Value;
            customerTimeZoneId = configItems.Where(configItem => configItem.Key == ArgNameCustomerTimeZoneId).FirstOrDefault().Value;
            customerDeviceListPath = configItems.Where(configItem => configItem.Key == ArgNameCustomerDeviceListPath).FirstOrDefault().Value;

            // Validate input values.
            ConsoleUtility.LogInfoStart("Validating input parameter values...");
            // OPTIONAL: Validate email address if enforcing the use of email addresses for usernames.
            // customerAccountAdminEmail = ValidationUtility.ValidateEmailAddress(customerAccountAdminEmail);
            customerFleetSize = ValidationUtility.ValidateStringIsInt32(customerFleetSize,"customer fleet size");
            customerSignUpForNews = ValidationUtility.ValidateStringIsBoolean(customerSignUpForNews, "customer's desire to sign-up for news");
            resellerName = ValidationUtility.ValidateStringLength(resellerName, 3, ArgNameResellerName);
            resellerErpAccountId = ValidationUtility.ValidateStringLength(resellerErpAccountId, 6, ArgNameResellerErpAccountId);
            customerCompanyName = ValidationUtility.ValidateStringLength(customerCompanyName, 3, ArgNameCustomerCompanyName);
            customerAccountAdminFirstName = ValidationUtility.ValidateStringLength(customerAccountAdminFirstName, 1, ArgNameCustomerAccountAdminFirstName);
            customerAccountAdminLastName = ValidationUtility.ValidateStringLength(customerAccountAdminLastName, 1, ArgNameCustomerAccountAdminLastName);
            customerPhoneNumber = ValidationUtility.ValidateStringLength(customerPhoneNumber, 10, ArgNameCustomerPhoneNumber);
            customerDesiredDatabaseName = ValidationUtility.ValidateStringLength(customerDesiredDatabaseName, 3, ArgNameCustomerDesiredDatabaseName);
            customerTimeZoneId = ValidationUtility.ValidateStringLength(customerTimeZoneId, 3, ArgNameCustomerTimeZoneId);
            customerDeviceListPath = ValidationUtility.ValidateStringLength(customerDeviceListPath, 3, ArgNameCustomerDeviceListPath);
            ConsoleUtility.LogComplete();

            // Ensure customer device list file exists.
            ConsoleUtility.LogInfoStart("Checking if customer device list file exists...");
            if (!File.Exists(customerDeviceListPath))
            {
                ConsoleUtility.LogInfo($"The file path '{customerDeviceListPath}' entered for '{ArgNameCustomerDeviceListPath}' is not valid.");
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
            myGeotabApi = await GeotabSdkUtility.AuthenticateMyGeotabApiAsync(GeotabServer, "", myAdminApiUsername, myAdminApiPassword);

            // Check whether customer desired database name is already used.  If so, prompt user for
            // alternates until an unused database name is found.
            ConsoleUtility.LogInfoStart($"Checking whether database name '{customerDesiredDatabaseName}' is available...");
            if (await GeotabSdkUtility.DatabaseExistsAsync(myGeotabApi, customerDesiredDatabaseName)) 
            {
                bool tryAnotherDatabaseName = true;
                string proposedDatabaseName = string.Empty;
                while (tryAnotherDatabaseName)
                {
                    customerDesiredDatabaseName = ConsoleUtility.GetUserInput($"a different database name ('{customerDesiredDatabaseName}' is already used)");
                    tryAnotherDatabaseName = await GeotabSdkUtility.DatabaseExistsAsync(myGeotabApi, customerDesiredDatabaseName);
                }                
            }
            else
            {
                ConsoleUtility.LogComplete();
            }  

            // Get the password to be used for the customer's administrative user account which will be created in the customer dataabse.
            string customerAccountAdminPassword = ConsoleUtility.GetVerifiedUserInputMasked($"Desired MyGeotab password for '{customerAccountAdminEmail}' to access '{customerDesiredDatabaseName}' database", $"[RE-ENTER] desired MyGeotab password for '{customerAccountAdminEmail}' to access '{customerDesiredDatabaseName}' database");
            customerAccountAdminPassword = ValidationUtility.ValidatePassword(customerAccountAdminPassword);            

            // Validate input parameters that require API access.
            customerTimeZoneId = await GeotabSdkUtility.ValidateTimeZoneIdAsync(myGeotabApi, customerTimeZoneId);

            // Create customer database.
            string createDatabaseResult = await GeotabSdkUtility.CreateDatabaseAsync(myGeotabApi, customerDesiredDatabaseName, myAdminApiUsername
            , myAdminApiPassword, customerCompanyName, customerAccountAdminFirstName
            , customerAccountAdminLastName, customerPhoneNumber, resellerName, Int32.Parse(customerFleetSize)
            , bool.Parse(customerSignUpForNews), customerTimeZoneId);

            // Get the server and database information for the new database.
            string[] serverAndDatabase = (createDatabaseResult).Split('/');
            string customerDatabaseServer = serverAndDatabase.First();
            string customerDatabase = serverAndDatabase.Last();

            // Authenticate MyGeotab API against the newly-created database:
            myGeotabApi = await GeotabSdkUtility.AuthenticateMyGeotabApiAsync(customerDatabaseServer, customerDatabase, myAdminApiUsername, myAdminApiPassword);

            // Create administrative user in customer database for customer administrator to use.
            IList<User> existingUsers = new List<User>();
            IList<Group> companyGroups = await GeotabSdkUtility.GetCompanyGroupsAsync(myGeotabApi);
            IList<Group> securityGroups = await GeotabSdkUtility.GetSecurityGroupsAsync(myGeotabApi);
            IList<Group> adminSecurityGroup = GeotabSdkUtility.GetSecurityGroupAsList(GeotabSdkUtility.SecurityGroupName.Administrator, securityGroups);
            IList<Group> companyGroup = GeotabSdkUtility.GetGroupAsList(GeotabSdkUtility.CompanyGroupName, companyGroups);

            User user = User.CreateBasicUser(null, customerAccountAdminEmail, customerAccountAdminFirstName, customerAccountAdminLastName, customerAccountAdminPassword, null, null, null, DateTime.MinValue, DateTime.MaxValue, companyGroup, null , adminSecurityGroup, null);
            user.ChangePassword = true;
            if (GeotabSdkUtility.ValidateUser(user, existingUsers))
            {
                try
                {
                    ConsoleUtility.LogInfoStart($"Adding user '{user.Name}' to database '{myGeotabApi.Database}'...");
                    await GeotabSdkUtility.AddUserAsync(myGeotabApi, user);
                    ConsoleUtility.LogComplete();
                    existingUsers.Add(user);
                }
                catch (Exception exception)
                {
                    ConsoleUtility.LogError($"Error adding user '{user.Name}' to database '{myGeotabApi.Database}'\n{exception.Message}");
                }
            }
            else
            {
                return;
            }

            // Load the list of devices to be imported from the CSV file.
            ConsoleUtility.LogInfoStart($"Loading the list of devices to be imported into the '{customerDesiredDatabaseName}' database from file '{customerDeviceListPath}'...");
            IList<DeviceDetails> deviceCandidates = null;
            using (FileStream devicesToImportFile = File.OpenRead(customerDeviceListPath))
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
            IList<Device> existingDevices = await myGeotabApi.CallAsync<IList<Device>>("Get", typeof(Device));
            IList<ApiDeviceDatabaseExtended> currentDeviceDatabases = AdminSdkUtility.GetCurrentDeviceDatabases(ref myAdminApi, ref myAdminApiUser, resellerErpAccountId);
            ConsoleUtility.LogComplete();

            // Add devices into the MyGeotab database.
            ConsoleUtility.LogInfo($"Processing {deviceCandidates.Count()} device(s) - attempting to add to MyGeotab database '{customerDesiredDatabaseName}'."); 
            foreach (DeviceDetails deviceCandidate in deviceCandidates)
            {
                IList<Group> deviceGroups = new List<Group>();

                // Use the serial number for the description since it won't likely be known at this point which vehicle the device will be installed into.
                deviceCandidate.Name = deviceCandidate.SerialNumber;
                string deviceCandidateSerialNumber = deviceCandidate.SerialNumber.Replace("-", "");

                // Check if the device is already in any database.
                if (currentDeviceDatabases.Where(database => database.SerialNumber == deviceCandidateSerialNumber).Any())
                {
                    IList<ApiDeviceDatabaseExtended> existingDatabases = currentDeviceDatabases.Where(database => database.SerialNumber == deviceCandidateSerialNumber).ToList();

                    StringBuilder existingDatabaseNames = new StringBuilder();
                    foreach (ApiDeviceDatabaseExtended database in existingDatabases)
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
                    ConsoleUtility.LogListItemWithResult($"{deviceCandidate.Name}", $"NOT ADDED: Device already exists in MyGeotab database(s) {existingDatabaseNames.ToString()}.", ConsoleColor.Red);
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
                    await myGeotabApi.CallAsync<Id>("Add", typeof(Device), new { entity = newDevice });
                    ConsoleUtility.LogListItemWithResult($"{deviceCandidate.Name}", $"ADDED", ConsoleColor.Green);                          
                }
                catch (Exception ex)
                {
                    ConsoleUtility.LogListItemWithResult($"{deviceCandidate.Name}", $"NOT ADDED: ERROR adding device: {ex.Message}\n{ex.StackTrace}", ConsoleColor.Red);    
                }
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Completed processing.");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}