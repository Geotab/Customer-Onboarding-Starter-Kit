using System.ComponentModel.DataAnnotations;
using Geotab.Checkmate.ObjectModel;

namespace Geotab.CustomerOnboardngStarterKit
{
    class UserDetails
    {
        /// <summary>
        /// A <c>|</c>-separated list of company <see cref="Group"/> names to which the <see cref="User"/> belongs.
        /// </summary>
        public readonly string CompanyGroupNames;

        /// <summary>
        /// The name of the security <see cref="Group"/> to which the <see cref="User"/> belongs.
        /// </summary>
        public readonly string SecurityGroupName;

        /// <summary>
        /// The <see cref="User"/>.
        /// </summary>
        public readonly User User;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserDetails"/> class.
        /// </summary>
        /// <param name="user">The <see cref="User"/>.</param>
        /// <param name="companyGroupNames">A <c>|</c>-separated list of company <see cref="Group"/> names to which the <see cref="User"/> belongs.</param>
        /// <param name="securityGroupName">The name of the security <see cref="Group"/> to which the <see cref="User"/> belongs.</param>
        public UserDetails(User user, string companyGroupNames, string securityGroupName)
        {
            User = user;
            CompanyGroupNames = companyGroupNames;
            SecurityGroupName = securityGroupName;
        }        
    }
}