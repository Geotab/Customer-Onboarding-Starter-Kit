using System;
using System.Collections.Generic;
using System.Linq;
using MyAdminApiLib.Geotab.MyAdmin.MyAdminApi.ObjectModel;

namespace Geotab.CustomerOnboardngStarterKit.Utilities
{
    /// <summary>
    /// Contains methods to assist in working with the MyAdmin SDK.
    /// </summary>
    public static class AdminSdkUtility
    {
        // The MyAdmin API GetCurrentDeviceDatabases method has a result set limit of 1000 records.  If the result set contains 1000 records, call GetCurrentDeviceDatabases again passing the Id of the last record in the current set as the nextId parameter.
        const int ResultSetLimit_GetCurrentDeviceDatabases = 1000;

        /// <summary>
        /// Requests user's MyAdmin credentials, authenticates and updates the myAdminApi object as well as the myAdminApiUser object which will contain the API key and Session ID required for subsequent API calls.
        /// </summary>
        /// <param name="myAdminApi">A reference to a MyAdmin API (<see cref="WebServerInvoker"/>) object.</param>
        /// <param name="myAdminApiUser">A reference to a MyAdmin <see cref="ApiUser"/> object.</param>
        /// <param name="myAdminUsername">A reference to a string variable which will be populated with the user-entered username.</param>
        /// <param name="myAdminPassword">A reference to a string variable which will be populated with the user-entered password.</param>
        public static void AuthenticateMyAdminApi(ref WebServerInvoker myAdminApi, ref ApiUser myAdminApiUser, ref string myAdminUsername, ref string myAdminPassword) 
        {
            myAdminUsername = ConsoleUtility.GetUserInput("MyAdmin Username");
            myAdminPassword = ConsoleUtility.GetUserInputMasked("MyAdmin Password");
            myAdminApi = new WebServerInvoker(null, 60000) { Url = "https://myadminapi.geotab.com/v2/MyAdminApi.ashx" };
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"username", myAdminUsername},
                {"password", myAdminPassword}
            };
            ConsoleUtility.LogInfoStart($"Authenticating MyAdmin API (User: '{myAdminUsername}')...");
            myAdminApiUser = myAdminApi.Invoke("Authenticate", typeof(ApiUser), parameters) as ApiUser;
            ConsoleUtility.LogComplete();
        }  

        /// <summary>
        /// Retrieves the full list of MyGeotab databases (<see cref="ApiDeviceDatabaseExtended"/> objects) associated with the specified ERP account Id. 
        /// </summary>
        /// <param name="myAdminApi">A reference to a MyAdmin API (<see cref="WebServerInvoker"/>) object.</param>
        /// <param name="myAdminApiUser">A reference to an authenticated MyAdmin <see cref="ApiUser"/> object.</param>
        /// <param name="forAccount">The ERP account Id for which to retrieve the list of associated databases.</param>
        public static IList<ApiDeviceDatabaseExtended> GetCurrentDeviceDatabases(ref WebServerInvoker myAdminApi,  ref ApiUser myAdminApiUser,  string forAccount)
        {
            // Create a new list to store all results from one or more batches (since each result set is limited to 1000 records).  Repeat GetCurrentDeviceDatabases() calls until all records have been received and then return the full list.
            List<ApiDeviceDatabaseExtended> allCurrentDeviceDatabases = new List<ApiDeviceDatabaseExtended>();

            bool allRecordsReceived = false;
            int nextId = 0;

            while (!allRecordsReceived)
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    {"apiKey", myAdminApiUser.UserId},
                    {"sessionId", myAdminApiUser.SessionId},
                    {"forAccount", forAccount},
                    {"nextId", nextId}
                };
                IList<ApiDeviceDatabaseExtended> currentDeviceDatabasesBatch = myAdminApi.Invoke("GetCurrentDeviceDatabases", typeof(ApiDeviceDatabaseExtended[]),parameters) as IList<ApiDeviceDatabaseExtended>;

                if (currentDeviceDatabasesBatch.Any())
                {
                    allCurrentDeviceDatabases.AddRange(currentDeviceDatabasesBatch);  
                }

                if (currentDeviceDatabasesBatch.Count < ResultSetLimit_GetCurrentDeviceDatabases)
                {
                    allRecordsReceived = true;
                }
                else
                {
                    nextId = currentDeviceDatabasesBatch.Last().Id;
                }
            }
            return allCurrentDeviceDatabases;
        }    
    }
}