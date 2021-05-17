using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Geotab.Checkmate;
using Geotab.Checkmate.ObjectModel;

namespace Geotab.CustomerOnboardngStarterKit.Utilities
{
    /// <summary>
    /// Contains methods to assist in working with the MyGeotab SDK.
    /// </summary>
    public static class GeotabSdkUtility
    {
        public const string CompanyGroupName = "**Org**";

        #region " Enums "

        /// <summary>
        /// MyGeotab group types.
        /// </summary>
        enum GroupType
        {
            Company, Security
        }

        /// <summary>
        /// MyGeotab security group names.
        /// </summary>
        public enum SecurityGroupName
        {
            Administrator, Supervisor, ViewOnly, Nothing
        }

        #endregion

        /// <summary>
        /// Adds a <see cref="User"/> to the database associated with the credemtials of the authenticated myGeotabApi (<see cref="API"/>) object.
        /// </summary>
        /// <param name="myGeotabApi">An authenticated MyGeotab <see cref="API"/> object.</param>
        /// <param name="user">The <see cref="User"/> to be added.</param>
        public static async Task AddUserAsync(API myGeotabApi, User user)
        {
            await myGeotabApi.CallAsync<Id>("Add", typeof(User), new { entity = user });
        }

        /// <summary>
        /// Adds a <see cref="Device"/> to the database associated with the credemtials of the authenticated myGeotabApi (<see cref="API"/>) object and returns the new device.
        /// </summary>
        /// <param name="myGeotabApi">An authenticated MyGeotab <see cref="API"/> object.</param>
        /// <param name="device">The <see cref="Device"/> to be added.</param>
        /// <returns></returns>
        public static async Task<Device> AddDeviceAsync(API myGeotabApi, Device device)
        {
            var newDeviceId = await myGeotabApi.CallAsync<Id>("Add", typeof(Device), new { entity = device });
            IList<Device> returnedDevices = await myGeotabApi.CallAsync<IList<Device>>("Get", typeof(Device), new { search = new DeviceSearch(newDeviceId) });
            if (returnedDevices.Count == 0)
            {
                return null;
            }
            return returnedDevices.First();
        }

        /// <summary>
        /// Requests user's MyGeotab credentials, authenticates and returns the authenticated MyGeotab <see cref="API"/> object required for subsequent API calls.
        /// </summary>
        /// <param name="server">The MyGeotab server to authenticate against.</param>
        /// <param name="database">The MyGeotab database to authenticate against.</param>
        /// <returns>An authenticated MyGeotab <see cref="API"/> object.</returns>
        public static async Task<API> AuthenticateMyGeotabApiAsync(string server, string database) 
        {
            string userName = ConsoleUtility.GetUserInput($"MyGeotab username for '{database}' database");
            string password = ConsoleUtility.GetUserInputMasked($"MyGeotab password for '{database}' database");
            API myGeotabApi = new(userName, password, null, database, server);
            ConsoleUtility.LogInfoStart($"Authenticating MyGeotab API (User: '{myGeotabApi.UserName}', Database: '{myGeotabApi.Database}', Server: '{myGeotabApi.Server}')...");
            await myGeotabApi.AuthenticateAsync();
            ConsoleUtility.LogComplete();
            return myGeotabApi;
        }

        /// <summary>
        /// Authenticates and returns the authenticated MyGeotab <see cref="API"/> object required for subsequent API calls.
        /// </summary>
        /// <param name="server">The MyGeotab server to authenticate against.</param>
        /// <param name="database">The MyGeotab database to authenticate against.</param>
        /// <param name="userName">The MyGeotab username.</param>   
        /// <param name="password">The MyGeotab password.</param>
        /// <returns>>An authenticated MyGeotab <see cref="API"/> object.</returns>
        public static async Task<API> AuthenticateMyGeotabApiAsync(string server, string database, string userName, string password) 
        {
            API myGeotabApi = new(userName, password, null, database, server);
            ConsoleUtility.LogInfoStart($"Authenticating MyGeotab API (User: '{myGeotabApi.UserName}', Database: '{myGeotabApi.Database}', Server: '{myGeotabApi.Server}')...");
            await  myGeotabApi.AuthenticateAsync();
            ConsoleUtility.LogComplete();
            return myGeotabApi;
        }

        /// <summary>
        /// Creates a new MyGeotab database.
        /// </summary>
        /// <param name="myGeotabApi">An authenticated MyGeotab <see cref="API"/> object.</param>
        /// <param name="database">The database name (short company name). Spaces and non alphanumeric characters will be converted to the underscore character. Maximum 58 characters.</param>
        /// <param name="userName">The database administrator email address.</param>
        /// <param name="password">The database administrator password.</param>
        /// <param name="companyName">The company name.</param>
        /// <param name="firstName">The account administrator's first name.</param>
        /// <param name="lastName">The account administrator's last name.</param>
        /// <param name="phoneNumber">The company phone number.</param>
        /// <param name="resellerName">The reseller name.</param>
        /// <param name="fleetSize">The number of vehicles in the company fleet.</param>
        /// <param name="signUpForNews">A value indicating whether sign-up to receive news about new telematics products, events and promotions.</param>
        /// <param name="timeZoneId">The IANA time zone Id of the device used to determine local work times. This is typically the "home location" of the admin user.  Retrieve a list of valid timezone Id's by querying "GetTimeZones".</param>
        /// <returns>A string with the direct server the database was created on and database name. Ex. "my0.geotab.com/abc_company".</returns>
        public static async Task<string> CreateDatabaseAsync(API myGeotabApi, string database, string userName, string password
        , string companyName, string firstName, string lastName, string phoneNumber
        , string resellerName, int fleetSize, bool signUpForNews, string timeZoneId) 
        {
            ConsoleUtility.LogInfoStartMultiPart($"Creating customer database named '{database}'.", $"THIS MAY TAKE UP TO 10 MINUTES...", ConsoleColor.Magenta);

            // Set timeout to 10 minutes.
            myGeotabApi.Timeout = 600000;

            string result = await myGeotabApi.CallAsync<string>("CreateDatabase", new 
            {
                database,
                userName,
                password,
                companyDetails = new {
                    companyName,
                    firstName,
                    lastName,
                    phoneNumber,
                    resellerName,
                    fleetSize,
                    comments = "",
                    signUpForNews,
                    timeZoneId
                }
            });

            ConsoleUtility.LogComplete();
            ConsoleUtility.LogInfo($"Created database: {result}");
            return result;
        }

        /// <summary>
        /// Checks if a database with the specified name exists in MyGeotab.
        /// </summary>
        /// <param name="myGeotabApi">An authenticated MyGeotab <see cref="API"/> object.</param>
        /// <param name="databaseName">The database name.</param>
        /// <returns>A boolean indicating whether the specified database exists in MyGeotab.</returns>
        public static async Task<bool> DatabaseExistsAsync(API myGeotabApi, string databaseName) 
        {
            bool databaseExists = await myGeotabApi.CallAsync<bool>("DatabaseExists", new {database = databaseName});
            return databaseExists;
        }

        /// <summary>
        /// Checks if a device serial number exists in a collection of devices.
        /// </summary>
        /// <param name="deviceSerialNo">The device serial number.</param>
        /// <param name="devices">The collection of devices to search in.</param>
        /// <returns>A boolean indicating whether the specified device is found.</returns>
        public static bool DeviceExists(string deviceSerialNo, IList<Device> devices)
        {
            deviceSerialNo = deviceSerialNo.Replace("-", "");
            return devices.Where(device => device.SerialNumber == deviceSerialNo).Any();
        }

        /// <summary>
        /// Retrieves a list of company groups from the database associated with the credemtials of the authenticated myGeotabApi (<see cref="API"/>) object.
        /// </summary>
        /// <param name="myGeotabApi">An authenticated MyGeotab <see cref="API"/> object.</param>
        /// <returns>The retrieved list of groups or an empty list of <see cref="Group"/> if not found.</returns>
        public static async Task<IList<Group>> GetCompanyGroupsAsync(API myGeotabApi)
        {
            IList<Group> groups = await GetGroupsAsync(myGeotabApi, GroupType.Company);
            return groups; 
        }

        /// <summary>
        /// Prompts the user to input the path to a config file and then loads the contents of the config file into a list of <see cref="ConfigItem"/> objects.
        /// </summary>
        /// <param name="configFilePathPromptMessage">A description of the file type being sought (e.g. '<c>config</c>').  For use in user prompts.</param>
        /// <returns>A list of <see cref="ConfigItem"/> objects.</returns>
        public static IList<ConfigItem> GetConfigItems(string configFilePathPromptMessage)
        {
            // Get the config file path.
            string configFilePath = ConsoleUtility.GetUserInputFilePath(configFilePathPromptMessage);

            // Load the config file contents.
            ConsoleUtility.LogInfoStart($"Loading configuration information from file '{configFilePath}'...");
            IList<ConfigItem> configItems = null;
            using (FileStream configFile = File.OpenRead(configFilePath))
            {
                configItems =  configFile.CsvToList<ConfigItem>(); 
            }
            if (!configItems.Any())
            {
                throw new Exception($"No configuration information was loaded from the CSV file '{configFilePath}'.");
            }
            ConsoleUtility.LogComplete();
            return configItems;
        }

        /// <summary>
        /// Searches for and returns a group from a flat list of groups.
        /// </summary>
        /// <param name="name">The group name to search for.</param>
        /// <param name="groups">The group collection to search in.</param>
        /// <returns>The found group or <c>null</c> if not found.</returns>
        public static Group GetGroup(string name, IList<Group> groups)
        {
            return groups.Where(group => group.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        }

        /// <summary>
        /// Searches for and returns (within a <see cref="IList"/>) a group from a flat list of groups.
        /// </summary>
        /// <param name="name">The group name to search for.</param>
        /// <param name="groups">The group collection to search in.</param>
        /// <returns>A <see cref="IList"/> containing the retrieved <see cref="Group"/> or an empty <see cref="IList"/> if not found.</returns>
        public static IList<Group> GetGroupAsList(string name, IList<Group> groups)
        {
            IList<Group> returnGroups = new List<Group>();
            for (int i = 0; i < groups.Count; i++)
            {
                Group group = groups[i];
                if (group.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    returnGroups.Add(group);
                    break;
                }
            }
            return returnGroups;
        }

        /// <summary>
        /// Retrieves a list of groups of the specified <see cref="GroupType"/> from the database associated with the credemtials of the authenticated myGeotabApi (<see cref="API"/>) object.
        /// </summary>
        /// <param name="myGeotabApi">An authenticated MyGeotab <see cref="API"/> object.</param>
        /// <param name="groupType">The <see cref="GroupType"/> of groups to be retrieved.</param>
        /// <returns>The retrieved list of groups or an empty list of <see cref="Group"/> if not found.</returns>
        static async Task<IList<Group>> GetGroupsAsync(API myGeotabApi, GroupType groupType)
        {
            IList<Group> groups = groupType switch
            {
                GroupType.Security => await myGeotabApi.CallAsync<IList<Group>>("Get", typeof(Group), new { search = new GroupSearch(new SecurityGroup().Id) }) ?? new List<Group>(),
                GroupType.Company => await myGeotabApi.CallAsync<IList<Group>>("Get", typeof(Group)) ?? new List<Group>(),
                _ => new List<Group>(),
            };
            return groups;
        }

        /// <summary>
        /// Searches for and returns (within a <see cref="IList"/>) the security <see cref="Group"/> associated with the specified <see cref="SecurityGroupName"/> from a list of security groups.
        /// </summary>
        /// <param name="securityGroupName">The user-friendly <see cref="SecurityGroupName"/> for which to find the associated security <see cref="Group"/>.</param>
        /// <param name="securityGroups">The list of security <see cref="Group"/> to search.</param>
        /// <returns>A <see cref="IList"/> containing the retrieved security <see cref="Group"/> or the <c>Nothing</c> security <see cref="Group"/> if not found.</returns>
        public static IList<Group> GetSecurityGroupAsList(SecurityGroupName securityGroupName, IList<Group> securityGroups)
        {
            IList<Group> groups = new List<Group>();
            string securityGroupFullName = securityGroupName switch
            {
                SecurityGroupName.Administrator => "**EverythingSecurity**",
                SecurityGroupName.Supervisor => "**SupervisorSecurity**",
                SecurityGroupName.ViewOnly => "**ViewOnlySecurity**",
                SecurityGroupName.Nothing => "**NothingSecurity**",
                _ => "**NothingSecurity**",
            };
            for (int i = 0; i < securityGroups.Count; i++)
            {
                Group group = securityGroups[i];
                if (group.Name.Equals(securityGroupFullName, StringComparison.OrdinalIgnoreCase))
                {
                    groups.Add(group);
                    break;
                }
            }
            return groups;
        }

        /// <summary>
        /// Retrieves a list of security groups from the database associated with the credemtials of the authenticated myGeotabApi (<see cref="API"/>) object.
        /// </summary>
        /// <param name="myGeotabApi">An authenticated myGeotabApi (<see cref="API"/>) object.</param>
        /// <returns>The retrieved list of groups or an empty list of <see cref="Group"/> if not found.</returns>
        public static async Task<IList<Group>> GetSecurityGroupsAsync(API myGeotabApi)
        {
            IList<Group> groups = await GetGroupsAsync(myGeotabApi, GroupType.Security);
            return groups; 
        }

        /// <summary>
        /// Searches for and returns a User from the database associated with the credemtials of the authenticated myGeotabApi (<see cref="API"/>) object.
        /// </summary>
        /// <param name="myGeotabApi">The authenticated MyGeotab <see cref="API"/> object.</param>
        /// <param name="userName">The name of the <see cref="User"/> to search for.</param>
        /// <returns>The <see cref="User"/> with the specified userName or <c>null</c> if not found.</returns>
        public static async Task<User> GetUserAsync(API myGeotabApi, string userName)
        {
            IList<User> users = await myGeotabApi.CallAsync<List<User>>("Get", typeof(User), new 
            {
                search = new UserSearch
                {
                    Name = userName
                }
            });

            if (users.Any())
            {
                return users.First();
            }
            return null;
        }

        /// <summary>
        /// Updates the supplied <see cref="Device"/> using the values of the supplied parameters.  
        /// </summary>
        /// <param name="myGeotabApi">An authenticated MyGeotab <see cref="API"/> object.</param>
        /// <param name="device">The <see cref="Device"/> to be updated.</param>
        /// <param name="name">The name of this entity that uniquely identifies it and is used when displaying this entity. Maximum length [50].</param>
        /// <param name="enableDeviceBeeping">Master toggle to enable the device buzzer. When set to <c>false</c>, the device will not provide driver feedback of any kind. Default <c>false</c>.</param>
        /// <param name="enableDriverIdentificationReminder">A value mainly used for enable or disable driver identification reminder. If it is used in conjunction with vehicle relay circuits, it can force the driver to swipe the driver key before starting the vehicle. Default <c>false</c>.</param>
        /// <param name="driverIdentificationReminderImmobilizeSeconds">With enableDriverIdentificationReminder being true, it is used to define the delay before the driver identification reminder is sent out if the driver key has not been not swiped. The maximum value of this property is 255. When it is less or equal to 180, it indicates the number of seconds of the delay. When it is greater than 180, the delay increases 30 seconds for every increment of one of this property. For example, 180 indicates 180 seconds, 181 indicates 210 seconds, and 182 indicates 240 seconds. Maximum [255] Default <c>30</c>.</param>
        /// <param name="enableBeepOnEngineRpm">Toggle to enable beeping when the vehicle's RPM exceeds the 'engineRpmBeepValue'. Default <c>false</c>.</param>
        /// <param name="engineRpmBeepValue">The RPM value that when exceeded triggers device beeping. Default <c>3500</c>.</param>
        /// <param name="enableBeepOnIdle">Toggle to enable beeping when the vehicle idles for more than idleMinutesBeepValue. Default <c>false</c>.</param>
        /// <param name="idleMinutesBeepValue">The number of minutes of allowed idling before device beeping starts. enableBeepOnIdle must be enabled. Default <c>3</c>.</param>
        /// <param name="enableBeepOnSpeeding">A toggle to beep constantly when the vehicle reaches the speed set in 'speedingStartBeepingSpeed', and do not stop until the vehicle slows below the 'speedingStopBeepingSpeed' speed. To only beep briefly (instead of continuously), enable 'enableBeepBrieflyWhenApprocahingWarningSpeed'. Default <c>false</c>.</param>
        /// <param name="speedingStartBeepingSpeed">The speeding on value in km/h. When 'enableBeepOnSpeeding' is enabled, the device will start beeping when the vehicle exceeds this speed. Default <c>100</c>.</param>
        /// <param name="speedingStopBeepingSpeed">The speeding off value in km/h. When 'enableBeepOnSpeeding' is enabled, once beeping starts, the vehicle must slow down to this speed for the beeping to stop. Default <c>90</c>.</param>
        /// <param name="enableBeepBrieflyWhenApprocahingWarningSpeed">Toggle to enable speed warning value for the vehicle. When enabled [true], only beep briefly (instead of continuously), when 'speedingStopBeepingSpeed' value is exceeded. 'speedingStartBeepingSpeed' must also be enabled. Default <c>false</c>.</param>
        /// <param name="enableBeepOnDangerousDriving">Toggle to enable beeping when any of the acceleration thresholds are exceeded by device accelerometer readings. Default <c>false</c>.</param>
        /// <param name="accelerationWarningThreshold">The acceleration warning accelerometer threshold (y axis) value for the vehicle. A positive value that when exceeded will trigger device beeping. Threshold value to mS2 conversion (threshold * 18 = milli-g / 1000 = g / 1.0197162 = mS2). Default <c>22</c>.</param>
        /// <param name="brakingWarningThreshold">The braking warning accelerometer threshold (y axis) value for the vehicle. A negative value that when exceeded will trigger device beeping. Threshold value to mS2 conversion (threshold * 18 = milli-g / 1000 = g / 1.0197162 = mS2). Default <c>-34</c>.</param>
        /// <param name="corneringWarningThreshold">The cornering warning threshold (x axis) value for the vehicle. A positive value that when exceeded will trigger device beeping (the additive inverse is automatically applied: 26/-26). Threshold value to mS2 conversion (threshold * 18 = milli-g / 1000 = g / 1.0197162 = mS2). Default <c>26</c>.</param>
        /// <param name="enableBeepWhenSeatbeltNotUsed">Value which toggles beeping if an unbuckled seat belt is detected. This will only work if the device is able to obtain seat belt information from the vehicle. Default <c>false</c>.</param>
        /// <param name="seatbeltNotUsedWarningSpeed">The value in km/h that below will not trigger 'enableBeepWhenSeatbeltNotUsed'. Default <c>10</c>.</param>
        /// <param name="enableBeepWhenPassengerSeatbeltNotUsed">Value which toggles monitoring both passenger and driver unbuckled seat belt, otherwise only the driver is monitored. Default <c>false</c>.</param>
        /// <param name="beepWhenReversing">Value which toggles device beeping when the vehicle is reversing. Default <c>false</c>.</param>
        /// <returns></returns>
        public static async Task UpdateDeviceAsync(API myGeotabApi, Device device, string name, bool enableDeviceBeeping = false, bool enableDriverIdentificationReminder = false, int driverIdentificationReminderImmobilizeSeconds = 30, bool enableBeepOnEngineRpm = false, int engineRpmBeepValue = 3500, bool enableBeepOnIdle = false, int idleMinutesBeepValue = 3, bool enableBeepOnSpeeding = false, int speedingStartBeepingSpeed = 100, int speedingStopBeepingSpeed = 90, bool enableBeepBrieflyWhenApprocahingWarningSpeed = false, bool enableBeepOnDangerousDriving = false, int accelerationWarningThreshold = 22, int brakingWarningThreshold = -34, int corneringWarningThreshold = 26, bool enableBeepWhenSeatbeltNotUsed = false, int seatbeltNotUsedWarningSpeed = 10, bool enableBeepWhenPassengerSeatbeltNotUsed = false, bool beepWhenReversing = false)
        {
            // Update device name.
            device.Name = name;
            // Update properties available to all GO devices.
            if (device is GoDevice)
            {
                GoDevice goDevice = device as GoDevice;
                goDevice.DisableBuzzer = !enableDeviceBeeping;
                goDevice.EnableBeepOnIdle = enableBeepOnIdle;
                goDevice.IdleMinutes = idleMinutesBeepValue;
                goDevice.IsSpeedIndicator = enableBeepOnSpeeding;
                goDevice.SpeedingOn = speedingStartBeepingSpeed;
                goDevice.SpeedingOff = speedingStopBeepingSpeed;
                goDevice.EnableSpeedWarning = enableBeepBrieflyWhenApprocahingWarningSpeed;
                // Update properties by specific GO device type.
                if (goDevice is Go4v3)
                {
                    Go4v3 go4v3 = goDevice as Go4v3;
                    go4v3.ImmobilizeUnit = enableDriverIdentificationReminder;
                    go4v3.ImmobilizeArming = driverIdentificationReminderImmobilizeSeconds;
                    go4v3.EnableBeepOnRpm = enableBeepOnEngineRpm;
                    go4v3.RpmValue = engineRpmBeepValue;
                    go4v3.EnableBeepOnDangerousDriving = enableBeepOnDangerousDriving;
                    go4v3.AccelerationWarningThreshold = accelerationWarningThreshold;
                    go4v3.AccelerometerThresholdWarningFactor = 0;
                    go4v3.BrakingWarningThreshold = brakingWarningThreshold;
                    go4v3.CorneringWarningThreshold = corneringWarningThreshold;
                    go4v3.IsDriverSeatbeltWarningOn = enableBeepWhenSeatbeltNotUsed;
                    go4v3.SeatbeltWarningSpeed = seatbeltNotUsedWarningSpeed;
                    go4v3.IsPassengerSeatbeltWarningOn = enableBeepWhenPassengerSeatbeltNotUsed;
                    go4v3.IsReverseDetectOn = beepWhenReversing;

                    await myGeotabApi.CallAsync<object>("Set", typeof(Device), new { entity = go4v3 });
                }                
                else if (goDevice is Go5)
                {
                    Go5 go5 = goDevice as Go5;
                    go5.EnableBeepOnRpm = enableBeepOnEngineRpm;
                    go5.RpmValue = engineRpmBeepValue;
                    go5.EnableBeepOnDangerousDriving = enableBeepOnDangerousDriving;
                    go5.AccelerationWarningThreshold = accelerationWarningThreshold;
                    go5.AccelerometerThresholdWarningFactor = 0;
                    go5.BrakingWarningThreshold = brakingWarningThreshold;
                    go5.CorneringWarningThreshold = corneringWarningThreshold;
                    go5.IsDriverSeatbeltWarningOn = enableBeepWhenSeatbeltNotUsed;
                    go5.SeatbeltWarningSpeed = seatbeltNotUsedWarningSpeed;
                    go5.IsPassengerSeatbeltWarningOn = enableBeepWhenPassengerSeatbeltNotUsed;
                    go5.IsReverseDetectOn = beepWhenReversing;

                    await myGeotabApi.CallAsync<object>("Set", typeof(Device), new { entity = go5 });
                }
                else if (goDevice is Go6)
                {
                    Go6 go6 = goDevice as Go6;
                    go6.ImmobilizeUnit = enableDriverIdentificationReminder;
                    go6.ImmobilizeArming = driverIdentificationReminderImmobilizeSeconds;
                    go6.EnableBeepOnRpm = enableBeepOnEngineRpm;
                    go6.RpmValue = engineRpmBeepValue;
                    go6.EnableBeepOnDangerousDriving = enableBeepOnDangerousDriving;
                    go6.AccelerationWarningThreshold = accelerationWarningThreshold;
                    go6.AccelerometerThresholdWarningFactor = 0;
                    go6.BrakingWarningThreshold = brakingWarningThreshold;
                    go6.CorneringWarningThreshold = corneringWarningThreshold;
                    go6.IsDriverSeatbeltWarningOn = enableBeepWhenSeatbeltNotUsed;
                    go6.SeatbeltWarningSpeed = seatbeltNotUsedWarningSpeed;
                    go6.IsPassengerSeatbeltWarningOn = enableBeepWhenPassengerSeatbeltNotUsed;
                    go6.IsReverseDetectOn = beepWhenReversing;

                    await myGeotabApi.CallAsync<object>("Set", typeof(Device), new { entity = go6 });
                }
                else if (goDevice is Go7)
                {
                    Go7 go7 = goDevice as Go7;
                    go7.ImmobilizeUnit = enableDriverIdentificationReminder;
                    go7.ImmobilizeArming = driverIdentificationReminderImmobilizeSeconds;
                    go7.EnableBeepOnRpm = enableBeepOnEngineRpm;
                    go7.RpmValue = engineRpmBeepValue;
                    go7.EnableBeepOnDangerousDriving = enableBeepOnDangerousDriving;
                    go7.AccelerationWarningThreshold = accelerationWarningThreshold;
                    go7.AccelerometerThresholdWarningFactor = 0;
                    go7.BrakingWarningThreshold = brakingWarningThreshold;
                    go7.CorneringWarningThreshold = corneringWarningThreshold;
                    go7.IsDriverSeatbeltWarningOn = enableBeepWhenSeatbeltNotUsed;
                    go7.SeatbeltWarningSpeed = seatbeltNotUsedWarningSpeed;
                    go7.IsPassengerSeatbeltWarningOn = enableBeepWhenPassengerSeatbeltNotUsed;
                    go7.IsReverseDetectOn = beepWhenReversing;

                    await myGeotabApi.CallAsync<object>("Set", typeof(Device), new { entity = go7 });
                }
                else if (goDevice is Go8)
                {
                    Go8 go8 = goDevice as Go8;
                    go8.ImmobilizeUnit = enableDriverIdentificationReminder;
                    go8.ImmobilizeArming = driverIdentificationReminderImmobilizeSeconds;
                    go8.EnableBeepOnRpm = enableBeepOnEngineRpm;
                    go8.RpmValue = engineRpmBeepValue;
                    go8.EnableBeepOnDangerousDriving = enableBeepOnDangerousDriving;
                    go8.AccelerationWarningThreshold = accelerationWarningThreshold;
                    go8.AccelerometerThresholdWarningFactor = 0;
                    go8.BrakingWarningThreshold = brakingWarningThreshold;
                    go8.CorneringWarningThreshold = corneringWarningThreshold;
                    go8.IsDriverSeatbeltWarningOn = enableBeepWhenSeatbeltNotUsed;
                    go8.SeatbeltWarningSpeed = seatbeltNotUsedWarningSpeed;
                    go8.IsPassengerSeatbeltWarningOn = enableBeepWhenPassengerSeatbeltNotUsed;
                    go8.IsReverseDetectOn = beepWhenReversing;

                    await myGeotabApi.CallAsync<object>("Set", typeof(Device), new { entity = go8 });
                }
                else if (goDevice is Go9)
                {
                    Go9 go9 = goDevice as Go9;
                    go9.ImmobilizeUnit = enableDriverIdentificationReminder;
                    go9.ImmobilizeArming = driverIdentificationReminderImmobilizeSeconds;
                    go9.EnableBeepOnRpm = enableBeepOnEngineRpm;
                    go9.RpmValue = engineRpmBeepValue;
                    go9.EnableBeepOnDangerousDriving = enableBeepOnDangerousDriving;
                    go9.AccelerationWarningThreshold = accelerationWarningThreshold;
                    go9.AccelerometerThresholdWarningFactor = 0;
                    go9.BrakingWarningThreshold = brakingWarningThreshold;
                    go9.CorneringWarningThreshold = corneringWarningThreshold;
                    go9.IsDriverSeatbeltWarningOn = enableBeepWhenSeatbeltNotUsed;
                    go9.SeatbeltWarningSpeed = seatbeltNotUsedWarningSpeed;
                    go9.IsPassengerSeatbeltWarningOn = enableBeepWhenPassengerSeatbeltNotUsed;
                    go9.IsReverseDetectOn = beepWhenReversing;

                    await myGeotabApi.CallAsync<object>("Set", typeof(Device), new { entity = go9 });
                }
                else
                {
                    await myGeotabApi.CallAsync<object>("Set", typeof(Device), new { entity = goDevice });
                }
            }
        } 

        /// <summary>
        /// Checks if a user exists by searching for the user name in a <see cref="IList"/> of existing users.
        /// </summary>
        /// <param name="name">The user name to search for.</param>
        /// <param name="existingUsers">The <see cref="IList"/> of existing users.</param>
        /// <returns><c>true</c> if user with the specified name is found, otherwise <c>false</c>.</returns>
        static bool UserExists(string name, IList<User> existingUsers)
        {
            for (int i = 0; i < existingUsers.Count; i++)
            {
                User user = existingUsers[i];
                if (user.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

         /// <summary>
         /// Gets a list of supported time zones and, if the supplied timeZoneId is not in the list, prompts the user to input a supported time zone until one from the list is entered.  Returns the valid time zone ID.
         /// </summary>
         /// <param name="myGeotabApi">An authenticated myGeotabApi (<see cref="API"/>) object.</param>
         /// <param name="timeZoneId">The time zone ID to be validated.</param>
         /// <returns>A valid time zone ID.</returns>
        public static async Task<string> ValidateTimeZoneIdAsync(API myGeotabApi, string timeZoneId)
        {
            ConsoleUtility.LogInfoStart($"Validating input time zone Id ('{timeZoneId}')...");
            List<Geotab.Checkmate.ObjectModel.TimeZoneInfo> supportedTimeZones = await myGeotabApi.CallAsync<List<Geotab.Checkmate.ObjectModel.TimeZoneInfo>>("GetTimeZones"); 
            if (supportedTimeZones.Where(supportedTimeZone => timeZoneId.Equals(supportedTimeZone.Id)).Any())
            {
                ConsoleUtility.LogComplete();
                return timeZoneId;
            }
            
            ConsoleUtility.LogError($"The specified time zone Id '{timeZoneId}' is not supported.  Following is a list of supported time zone IDs:");
            foreach (Geotab.Checkmate.ObjectModel.TimeZoneInfo timeZone in supportedTimeZones)
            {
                ConsoleUtility.LogListItem(timeZone.Id);
            }

            bool tryAgain = true;
            string proposedTimeZoneId = timeZoneId;
            while (tryAgain)
            {
                ConsoleUtility.LogError($"The specified time zone Id '{proposedTimeZoneId}' is not supported.");
                proposedTimeZoneId = ConsoleUtility.GetUserInput($"a supported time zone Id");
                tryAgain = !supportedTimeZones.Where(supportedTimeZone => proposedTimeZoneId.Equals(supportedTimeZone.Id)).Any();
            }  
            return proposedTimeZoneId;
        }   

        /// <summary>
        /// Validate a user has groups assigned and does not exist.
        /// </summary>
        /// <param name="user">The user to validate.</param>
        /// <param name="existingUsers">IList of existing users.</param>
        /// <returns>True if user is valid otherwise False.</returns>
        public static bool ValidateUser(User user, IList<User> existingUsers)
        {
            ConsoleUtility.LogInfoStart($"Validating user '{user.Name}'...");
            if (user.CompanyGroups == null || user.CompanyGroups.Count == 0)
            {
                ConsoleUtility.LogWarning($"Invalid user: {user.Name}. Must have organization groups.");
                return false;
            }
            if (user.SecurityGroups == null || user.SecurityGroups.Count == 0)
            {
               ConsoleUtility.LogWarning($"Invalid user: {user.Name}. Must have security groups.");
                return false;
            }
            if (UserExists(user.Name, existingUsers))
            {
                ConsoleUtility.LogWarning($"Invalid user: {user.Name}. User already exists.");
                return false;
            }
            ConsoleUtility.LogComplete();
            return true;
        }
    }
}
