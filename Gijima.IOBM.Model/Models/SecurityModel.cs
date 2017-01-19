using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Gijima.IOBM.Model.Models
{
    public class IOBMSecurityModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public IOBMSecurityModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        #region Create Methods

        /// <summary>
        /// Create a new user entity in the database
        /// </summary>
        /// <param name="applicationUser">The user entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateApplicationUser(User applicationUser)
        {
            try
            {
                using (var db = IOBMEntities.GetContext())
                {
                    if (db.Users.Any(p => p.UserName == applicationUser.UserName))
                    {
                        //_eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(string.Format("User {0} already exists!", applicationUser.UserName));
                        return false;
                    }
                    else
                    {
                        applicationUser.LastActivityDate = DateTime.Now;
                        db.Users.Add(applicationUser);
                        db.SaveChanges();

                        // Save the user roles
                        CreateApplicationUserRoles(applicationUser.pkUserID, applicationUser.UserInRoles);

                        // Save the user companies
                        CreateApplicationUserApplications(applicationUser.pkUserID, applicationUser.UserInApplications);

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return false;
            }
        }

        /// <summary>
        /// Save the application user role data
        /// </summary>
        /// <param name="applicationUserID">The application userID.</param>
        /// <param name="userRoles">The roles linked to the user.</param>
        /// <returns>True if successfull</returns>
        private bool CreateApplicationUserRoles(int applicationUserID, IEnumerable<UserInRole> userRoles)
        {
            try
            {
                using (var db = IOBMEntities.GetContext())
                {
                    IEnumerable<UserInRole> exitingRoles = db.UserInRoles.Where(p => p.fkUserID == applicationUserID);

                    foreach (UserInRole userRole in exitingRoles)
                    {
                        db.UserInRoles.Remove(userRole);
                    }

                    db.SaveChanges();

                    foreach (UserInRole userRole in userRoles)
                    {
                        userRole.fkUserID = applicationUserID;
                        db.UserInRoles.Add(userRole);
                    }

                    db.SaveChanges();
                }

                return true;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return false;
            }
        }

        /// <summary>
        /// Save the application user application data
        /// </summary>
        /// <param name="applicationUserID">The application userID.</param>
        /// <param name="userApplications">The applications linked to the user.</param>
        /// <returns>True if successfull</returns>
        private bool CreateApplicationUserApplications(int applicationUserID, IEnumerable<UserInApplication> userApplications)
        {
            try
            {
                using (var db = IOBMEntities.GetContext())
                {
                    IEnumerable<UserInApplication> exitingApplications = db.UserInApplications.Where(p => p.fkUserID == applicationUserID);

                    foreach (UserInApplication userApplication in exitingApplications)
                    {
                        db.UserInApplications.Remove(userApplication);
                    }

                    db.SaveChanges();

                    foreach (UserInApplication userApplication in userApplications)
                    {
                        userApplication.fkUserID = applicationUserID;
                        db.UserInApplications.Add(userApplication);
                    }

                    db.SaveChanges();
                }

                return true;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return false;
            }
        }

        #endregion

        #region Read Methods

        /// <summary>
        /// Read the application user entityt and roles by user name.
        /// </summary>
        /// <param name="userName"> The user domain name to authenticate.</param>
        /// <returns> The user entity.</returns>
        public User ReadApplicationUser(string userName)
        {
            try
            {
                using (var db = IOBMEntities.GetContext())
                {
                    IQueryable<User> query = from user in db.Users
                                             where string.Compare(user.UserName, userName, true) == 0 && user.IsActive
                                             select user;

                    return ((DbQuery<User>)query).Include("UserInRoles")
                                                 .Include("UserInApplications").FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return null;
            }
        }

        /// <summary>
        /// Read all or active only users from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <returns>Collection of Users</returns>
        public ObservableCollection<User> ReadApplicationUsers(bool activeOnly)
        {
            try
            {
                IEnumerable<User> users = null;

                using (var db = IOBMEntities.GetContext())
                {
                    users = ((DbQuery<User>)(from user in db.Users
                                             where activeOnly ? user.IsActive : true
                                             select user)).OrderBy(p => p.UserFullName).ToList();

                    return new ObservableCollection<User>(users);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return null;
            }
        }

        /// <summary>
        /// Read all the application roles
        /// </summary>
        /// <returns>List of role entities</returns>
        public List<Role> ReadApplicationRoles()
        {
            try
            {
                using (var db = IOBMEntities.GetContext())
                {
                    return db.Roles.ToList();
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return null;
            }
        }

        /// <summary>
        /// Read all roles linked to the application user
        /// </summary>
        /// <param name="userID">The application userID to get roles for.</param>
        /// <returns>List of application user role entities</returns>
        public ObservableCollection<Role> ReadApplicationUserRoles(int userID)
        {
            try
            {
                using (var db = IOBMEntities.GetContext())
                {
                    return new ObservableCollection<Role>(from role in db.Roles
                                                          join userRole in db.UserInRoles
                                                          on role.pkRoleID equals userRole.fkRoleID
                                                          where userRole.fkUserID == userID
                                                          select role);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return null;
            }
        }

        /// <summary>
        /// Read all applications linked to the application user
        /// </summary>
        /// <param name="userID">The application userID to get applications for.</param>
        /// <returns>List of application user application entities</returns>
        public ObservableCollection<Application> ReadApplicationUserApplications(int userID)
        {
            try
            {
                using (var db = IOBMEntities.GetContext())
                {
                    return new ObservableCollection<Application>(from app in db.Applications
                                                                 join userApp in db.UserInApplications
                                                                 on app.pkApplicationID equals userApp.fkApplicationID
                                                                 where userApp.fkUserID == userID
                                                                 select app);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return null;
            }
        }

        #endregion

        #region Update Methods

        /// <summary>
        /// Update the specified application user data.
        /// </summary>
        /// <param name="applicationUser">The application user entity to save.</param>
        /// <returns>True is successfull</returns>
        public bool UpdateApplicationUser(User applicationUser)
        {
            try
            {
                using (var db = IOBMEntities.GetContext())
                {
                    User existingUser = db.Users.Where(p => p.pkUserID == applicationUser.pkUserID).FirstOrDefault();

                    // Check to see if the device description already exist for another entity 
                    if (existingUser != null)
                    {
                        existingUser.UserFullName = applicationUser.UserFullName;
                        existingUser.IsActive = applicationUser.IsActive;
                        db.SaveChanges();

                        // Save the user roles
                        CreateApplicationUserRoles(applicationUser.pkUserID, applicationUser.UserInRoles);

                        // Save the user companies
                        CreateApplicationUserApplications(applicationUser.pkUserID, applicationUser.UserInApplications);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return false;
            }
        }

        /// <summary>
        /// Sync the specified application user data with the its linked solution user.
        /// </summary>
        /// <param name="applicationUser">The application user entity to sync.</param>
        /// <returns>True is successfull</returns>
        public bool SyncSolutionUser(User applicationUser)
        {
            try
            {
                using (var db = IOBMEntities.GetContext())
                {
                    User existingUser = db.Users.Where(p => p.UserName == applicationUser.UserName).FirstOrDefault();

                    if (existingUser == null)
                    {
                        db.Users.Add(applicationUser);
                        db.SaveChanges();

                        // Add the user to the calling application
                        UserInApplication userApp = new UserInApplication();
                        userApp.fkApplicationID = SolutionApplication.MobileManager.Value();
                        userApp.fkUserID = applicationUser.pkUserID;
                        db.UserInApplications.Add(userApp);
                        db.SaveChanges();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return false;
            }
        }

        #endregion
    }
}
