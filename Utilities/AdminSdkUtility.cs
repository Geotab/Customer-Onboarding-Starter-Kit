using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyAdminApiLib.Geotab.MyAdmin.MyAdminApi;
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
        /// Requests user's MyAdmin credentials and authenticates. Returns:
        /// <list type="bullet">
        /// <item>
        /// <term>myAdminApi</term>
        /// <description>>The authenticated MyAdmin API (<see cref="MyAdminInvoker"/>) object.</description>
        /// </item>
        /// <item>
        /// <term>myAdminApiUser</term>
        /// <description>The <see cref="ApiUser"/> object containing the API key and Session ID required for subsequent API calls.</description>
        /// </item>
        /// <item>
        /// <term>myAdminUsername</term>
        /// <description>The user-entered username.</description>
        /// </item>
        /// <item>
        /// <term>myAdminPassword</term>
        /// <description>The user-entered password.</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <returns></returns>
        public static async Task<(MyAdminInvoker myAdminApi, ApiUser myAdminApiUser, string myAdminUsername, string myAdminPassword)> AuthenticateMyAdminApi()
        {
			var myAdminUsername = ConsoleUtility.GetUserInput("MyAdmin Username");
			var myAdminPassword = ConsoleUtility.GetUserInputMasked("MyAdmin Password");
            var myAdminApi = new MyAdminInvoker("https://myadminapi.geotab.com/v2/MyAdminApi.ashx", 60000);
            Dictionary<string, object> parameters = new()
			{
				{ "username", myAdminUsername },
				{ "password", myAdminPassword }
			};
			ConsoleUtility.LogInfoStart($"Authenticating MyAdmin API (User: '{myAdminUsername}')...");
			var myAdminApiUser = await myAdminApi.InvokeAsync<ApiUser>("Authenticate", parameters);
            ConsoleUtility.LogComplete();

            return (myAdminApi, myAdminApiUser, myAdminUsername, myAdminPassword);
        }

        /// <summary>
        /// Retrieves the full list of MyGeotab databases (<see cref="ApiDeviceDatabaseExtended"/> objects) associated with the specified ERP account Id. 
        /// </summary>
        /// <param name="myAdminApi">A reference to a MyAdmin API (<see cref="MyAdminInvoker"/>) object.</param>
        /// <param name="myAdminApiUser">A reference to an authenticated MyAdmin <see cref="ApiUser"/> object.</param>
        /// <param name="forAccount">The ERP account Id for which to retrieve the list of associated databases.</param>
        public static async Task<IList<ApiDeviceDatabaseExtended>> GetCurrentDeviceDatabases(MyAdminInvoker myAdminApi,  ApiUser myAdminApiUser,  string forAccount)
        {
            // Create a new list to store all results from one or more batches (since each result set is limited to 1000 records).  Repeat GetCurrentDeviceDatabases() calls until all records have been received and then return the full list.
            List<ApiDeviceDatabaseExtended> allCurrentDeviceDatabases = new();

            bool allRecordsReceived = false;
            double nextId = 0;

            while (!allRecordsReceived)
            {
                Dictionary<string, object> parameters = new()
                {
                    {"apiKey", myAdminApiUser.UserId},
                    {"sessionId", myAdminApiUser.SessionId},
                    {"forAccount", forAccount},
                    {"nextId", nextId}
                };
                var currentDeviceDatabasesBatch = await myAdminApi.InvokeAsync<IList<ApiDeviceDatabaseExtended>>("GetCurrentDeviceDatabases", parameters);

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

		/// <summary>
		/// Retrieves the list of MyGeotab databases (<see cref="ApiDeviceDatabaseOwnerShared"/> objects), if available, associated with the device serial number. 
		/// </summary>
		/// <param name="myAdminApi">A reference to a MyAdmin API (<see cref="MyAdminInvoker"/>) object.</param>
		/// <param name="myAdminApiUser">A reference to an authenticated MyAdmin <see cref="ApiUser"/> object.</param>
		/// <param name="serialNumbers">A list of device serial numbers for which to retrieve the list of associated owner/shared databases.</param>
		/// <returns></returns>
		public static async Task<IList<ApiDeviceDatabaseOwnerShared>> GetDeviceDatabaseNamesAsync(MyAdminInvoker myAdminApi, ApiUser myAdminApiUser, IList<string> serialNumbers)
		{
			Dictionary<string, object> parametersNew = new()
			{
				{"apiKey", myAdminApiUser.UserId},
				{"sessionId", myAdminApiUser.SessionId},
				{"serialNumbers", serialNumbers},
			};

			var currentDeviceDatabaseNames = await myAdminApi.InvokeAsync<IList<ApiDeviceDatabaseOwnerShared>> ("GetDeviceDatabaseNamesAsync", parametersNew);
            return currentDeviceDatabaseNames;
		}
	}
}
