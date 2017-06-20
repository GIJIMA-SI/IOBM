using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class LineManagerModel
    {
        #region Variables

        private IEventAggregator _eventAggregator;
        private string _defaultItem = "-- Please Select --";

        #endregion

        #region Methods

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="eventAggregator"></param>
        public LineManagerModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        /// <summary>
        /// Creates a new instance of a line manager
        /// </summary>
        /// <param name="lineManager"></param>
        /// <returns></returns>
        public bool CreateLineManager(LineManager lineManager)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    db.LineManagers.Add(lineManager);
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
        /// Updates the specified line manager
        /// </summary>
        /// <param name="lineManager"></param>
        /// <returns></returns>
        public bool UpdateLineManager(LineManager lineManager)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    LineManager existingLineManger = db.LineManagers.Where(x => x.pkLineManagerID == lineManager.pkLineManagerID).FirstOrDefault();

                    if (existingLineManger == null && existingLineManger.pkLineManagerID != lineManager.pkLineManagerID)
                        return false;
                    else
                    {
                        //Since this is an included property an exception is raised since it also get detached
                        //Clearing it removes the issue
                        lineManager.Department = null;

                        db.Entry(existingLineManger).State = System.Data.Entity.EntityState.Detached;
                        db.LineManagers.Attach(lineManager);
                        db.Entry(lineManager).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
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
        /// Returns a collection of the all the line managers
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<LineManager> ReadLineManagers(bool activeOnly = true, bool defaultItem = true)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    ObservableCollection<LineManager> tmpList;

                    if (activeOnly && defaultItem)
                    {
                        tmpList = new ObservableCollection<LineManager>(db.LineManagers.Where(x => x.IsActive).ToList());

                        //Insert Please Select
                        LineManager defaultLineManger = new LineManager();
                        defaultLineManger.fkDepartmentID = -1;
                        defaultLineManger.Name = _defaultItem;
                        defaultLineManger.Surname = string.Empty;

                        tmpList.Insert(0, defaultLineManger);
                    }
                    else
                    {
                        tmpList = new ObservableCollection<LineManager>(((DbQuery<LineManager>)(from linemanager in db.LineManagers
                                                                                                select linemanager)).Include("Department")
                                                                                                                  .OrderBy(p => p.Name).ToList());
                    }

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
        /// Read all the line manager emails for advanced search
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<string> ReadLineManagerEmails()
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    ObservableCollection<string> tmpListEmails = new ObservableCollection<string>();
                    ObservableCollection<LineManager> tmpListLineManagers;


                    tmpListLineManagers = new ObservableCollection<LineManager>(db.LineManagers.Where(x => x.IsActive).ToList());

                    foreach (LineManager linemanager in tmpListLineManagers)
                    {
                        tmpListEmails.Add(linemanager.LineManagerEmail);
                    }
                    
                    tmpListEmails.Insert(0, _defaultItem);
                    
                    return tmpListEmails;
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
        /// Returns a collection of the all the line managers for a company
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<LineManager> ReadDepartmentLineManagers(int departmentID)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    ObservableCollection<LineManager> tmpList;

                    tmpList = new ObservableCollection<LineManager>(db.LineManagers.Where(x => x.IsActive && x.fkDepartmentID == departmentID).ToList());

                    //Insert Please Select
                    LineManager defaultLineManger = new LineManager();
                    defaultLineManger.fkDepartmentID = -1;
                    defaultLineManger.Name = _defaultItem;
                    defaultLineManger.Surname = string.Empty;

                    tmpList.Insert(0, defaultLineManger);

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

        #endregion
    }
}
