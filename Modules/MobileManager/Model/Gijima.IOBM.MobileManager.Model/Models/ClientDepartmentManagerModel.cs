using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class ClientDepartmentManagerModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;
        private AuditLogModel _activityLogger = null;
        private string _defaultItem = "-- Please Select --";
        private DataActivityHelper _dataActivityHelper = null;

        #endregion

        #region Methods

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="eventAggregator"></param>
        public ClientDepartmentManagerModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _activityLogger = new AuditLogModel(_eventAggregator);
            _dataActivityHelper = new DataActivityHelper(_eventAggregator);
        }

        /// <summary>
        /// Provides the spesified clients department information
        /// </summary>
        public ClientDepartmentManager ReadClientDepartmentManager(int clientID)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    ClientDepartmentManager tmpList;

                    tmpList = db.ClientDepartmentManagers.Where(x => x.fkClientID == clientID).FirstOrDefault();

                    return tmpList;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                     .Publish(new ApplicationMessage(this.GetType().Name,
                                              string.Format("Error! {0}, {1}.",
                                              ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                              MethodBase.GetCurrentMethod().Name,
                                              ApplicationMessage.MessageTypes.SystemError));
                return null;
            }
        }

        /// <summary>
        /// Creates a new instance in the db of a client deparment manager
        /// </summary>
        /// <param name="clientDepartmentManager"></param>
        /// <returns></returns>
        public bool UpdateClientDepartmentManager(ClientDepartmentManager clientDepartmentManager)
        {
            try
            {
                ClientDepartmentManager existingClientDepartmentManager;

                using (var db = MobileManagerEntities.GetContext())
                {
                    existingClientDepartmentManager = db.ClientDepartmentManagers
                                                    .Where(x => x.pkClientDepartmentManagerID == clientDepartmentManager.pkClientDepartmentManagerID).FirstOrDefault();
                }

                using (var db = MobileManagerEntities.GetContext())
                {
                    //If the client has related info then update else remove entry
                    if (clientDepartmentManager.fkDepartmentID != 0)
                    {
                        if (existingClientDepartmentManager == null && existingClientDepartmentManager.pkClientDepartmentManagerID != existingClientDepartmentManager.pkClientDepartmentManagerID)
                            return false;
                        else
                        {
                            //Remove the link if none is selected
                            if (clientDepartmentManager.fkDepartmentID == null)
                            {
                                db.ClientDepartmentManagers.Remove(existingClientDepartmentManager);
                                db.SaveChanges();
                                return true;
                            }
                            else
                            {
                                db.Entry(existingClientDepartmentManager).State = System.Data.Entity.EntityState.Detached;
                                db.ClientDepartmentManagers.Attach(clientDepartmentManager);
                                db.Entry(clientDepartmentManager).State = System.Data.Entity.EntityState.Modified;

                                _activityLogger.CreateDataChangeAudits<ClientDepartmentManagerModel>(_dataActivityHelper.GetDataChangeActivities<ClientDepartmentManagerModel>(existingClientDepartmentManager, clientDepartmentManager, clientDepartmentManager.pkClientDepartmentManagerID, db));

                                db.SaveChanges();
                                return true;
                            }
                        }
                    }
                    else
                    {
                        if (clientDepartmentManager.pkClientDepartmentManagerID != 0)
                        {
                            ClientDepartmentManager cdm = db.ClientDepartmentManagers.Where(x => x.pkClientDepartmentManagerID == clientDepartmentManager.pkClientDepartmentManagerID).FirstOrDefault();
                            db.ClientDepartmentManagers.Remove(cdm);
                            db.SaveChanges();
                        }
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                     .Publish(new ApplicationMessage(this.GetType().Name,
                                              string.Format("Error! {0}, {1}.",
                                              ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                              MethodBase.GetCurrentMethod().Name,
                                              ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Creates a new instance of client database manager
        /// </summary>
        /// <param name="clientDepartmentManager"></param>
        /// <returns></returns>
        public bool CreateClientDepartmentManager(ClientDepartmentManager clientDepartmentManager)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    db.ClientDepartmentManagers.Add(clientDepartmentManager);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                     .Publish(new ApplicationMessage(this.GetType().Name,
                                              string.Format("Error! {0}, {1}.",
                                              ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                              MethodBase.GetCurrentMethod().Name,
                                              ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Determine if department should be loaded for the client
        /// </summary>
        /// <param name="clientID"></param>
        /// <returns></returns>
        public bool ClientHasDepartment(int clientID)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    return db.ClientDepartmentManagers.Any(x => x.fkClientID == clientID);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                     .Publish(new ApplicationMessage(this.GetType().Name,
                                              string.Format("Error! {0}, {1}.",
                                              ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                              MethodBase.GetCurrentMethod().Name,
                                              ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Determine if a line manager should be loaded for the client
        /// </summary>
        /// <param name="clientID"></param>
        /// <returns></returns>
        public bool ClientHasLineManager(int clientID)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    ClientDepartmentManager clientDepartmentManager = db.ClientDepartmentManagers.Where(x => x.fkClientID == clientID).FirstOrDefault();

                    if (clientDepartmentManager != null)
                    {
                        if (clientDepartmentManager.fkLineManagerID != null)
                            return true;
                        else
                            return false;
                    }
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                     .Publish(new ApplicationMessage(this.GetType().Name,
                                              string.Format("Error! {0}, {1}.",
                                              ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                              MethodBase.GetCurrentMethod().Name,
                                              ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        #endregion
    }
}
