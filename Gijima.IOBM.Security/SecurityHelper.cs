using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Model.Data;
using Gijima.IOBM.Model.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gijima.IOBM.Security
{
    public class SecurityHelper
    {
        #region Properties and Attributes

        #region Active Directory

        private const string ADLogin = "cn";
        private const string ADAccountName = "SAMAccountName";
        private const string ADConnection = "LDAP_Connection";
        private const string ADFirstName = "givenname";
        private const string ADSurname = "sn";
        private const string ADDepartment = "department";
        private const string ADContactNumber = "telephonenumber";
        private const string ADMobileNumber = "mobile";
        private const string ADSearchFilter = "(&(objectClass=user)(objectCategory=Person)(&(givenname={0}*)(sn={1}*)))";

        #endregion

        private IEventAggregator _eventAggregator;
        private IOBMSecurityModel _model = null;

        #region Properties

        public static string LoggedInDomainName { get; set; }
        public static string LoggedInFullName { get; set; }
        public static IEnumerable<UserInRole> UserRoles { get; set; }
        public static IEnumerable<UserInApplication> UserApplications { get; set; }

        #endregion

        #endregion

        #region Public Methods

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public SecurityHelper(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _model = new IOBMSecurityModel(_eventAggregator);
        }

        /// <summary>
        /// Validate if the user is a valid application user
        /// </summary>
        /// <param name="userName">The user domain name to validate.</param>
        /// <returns>True if the user exist</returns>
        public bool IsUserAuthenticated(string userName)
        {
            try
            {
                User loggedInUser = _model.ReadApplicationUser(userName);

                if (loggedInUser == null)
                    return false;

                LoggedInDomainName = loggedInUser.UserName;
                LoggedInFullName = loggedInUser.UserFullName != null ? loggedInUser.UserFullName : loggedInUser.UserName;
                UserRoles = loggedInUser.UserInRoles;
                UserApplications = loggedInUser.UserInApplications;
                return true;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return false;
            }
        }

        /// <summary>
        /// Check if a user has the specified security role
        /// </summary>
        /// <param name="roleID">The security rolle to ckeck.</param>
        /// <returns>True if user has role</returns>
        public static bool IsUserInRole(int roleID)
        { 
            return UserRoles.Any(p => p.fkRoleID == roleID);
        }

        /// <summary>
        /// Check if a user has access to the pecified application
        /// </summary>
        /// <param name="applicationID">The security application to ckeck.</param>
        /// <returns>True if user has the application</returns>
        public static bool IsUserInApplication(int applicationID)
        {
            return UserApplications.Any(p => p.fkApplicationID == applicationID);
        }

        #endregion
    }
}
