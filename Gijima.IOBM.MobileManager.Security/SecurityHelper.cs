using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Gijima.IOBM.MobileManager.Model.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gijima.IOBM.MobileManager.Security
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
        private SecurityModel _model = null;

        #region Properties

        public static string LoggedInDomainName { get; set; }
        public static string LoggedInUserFullName { get; set; }
        public static string LoggedInUserRoleName { get; set; }
        public static int LoggedInUserRoleID { get; set; }
        public static IEnumerable<UserInRole> UserRoles { get; set; }
        public static IEnumerable<UserInCompany> UserCompanies { get; set; }

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
            _model = new SecurityModel(_eventAggregator);
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

                // Determine the user's highest role 
                if (loggedInUser.UserInRoles.Count > 0)
                {
                    foreach (UserInRole userRole in loggedInUser.UserInRoles)
                    {
                        if (userRole.Role.RoleName.ToUpper() == SecurityRole.Administrator.ToString().ToUpper())
                        {
                            LoggedInUserRoleName = userRole.Role.RoleName.ToUpper();
                            LoggedInUserRoleID = userRole.Role.pkRoleID;
                            break;
                        }
                        else if (userRole.Role.RoleName.ToUpper() == SecurityRole.SystemUser.ToString().ToUpper())
                        {
                            LoggedInUserRoleName = userRole.Role.RoleName.ToUpper();
                            LoggedInUserRoleID = userRole.Role.pkRoleID;
                            break;
                        }
                        else
                        {
                            LoggedInUserRoleName = SecurityRole.ReadOnly.ToString().ToUpper();
                            LoggedInUserRoleID = userRole.Role.pkRoleID;
                        }
                    }
                }
                else
                {
                    LoggedInUserRoleName = "NONE";
                    LoggedInUserRoleID = 0;
                }

                LoggedInDomainName = loggedInUser.UserName;
                LoggedInUserFullName = loggedInUser.UserFullName;
                UserRoles = loggedInUser.UserInRoles;
                UserCompanies = loggedInUser.UserInCompanies;
                return true;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return false;
            }
        }

        /// <summary>
        /// Read the database server name and database name
        /// </summary>
        /// <param name="serverName">OUT - The server the user authenticate against.</param>
        /// <param name="databaseName">OUT - The databse the user authenticate against.</param>  
        public void ReadConnectionInfo(out string serverName, out string databaseName)
        {
            serverName = string.Empty;
            databaseName = string.Empty;

            try
            {
                _model.ReadConnectionInfo(out serverName, out databaseName);

            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
            }
        }

        /// <summary>
        /// Check if a user has the specified security role
        /// </summary>
        /// <param name="roleID">The security rolle to ckeck.</param>
        /// <returns>True if user has role</returns>
        public bool IsUserInRole(int roleID)
        {
            if (UserRoles != null)
                return UserRoles.Any(p => p.fkRoleID == roleID);
            else
                return false;
        }

        /// <summary>
        /// Check if a user has access to the pecified company
        /// </summary>
        /// <param name="companyID">The security company to ckeck.</param>
        /// <returns>True if user has company</returns>
        public bool IsUserInCompany(int companyID)
        {
            return UserCompanies != null && UserCompanies.Any(p => p.fkCompanyID == companyID);
        }

        #endregion
    }
}
