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
    public class DepartmentModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;
        private string _defaultItem = "-- Please Select --";

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="eventAggregator"></param>
        public DepartmentModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        /// <summary>
        /// Read the list of Departments
        /// </summary>
        /// <param name="activeOnly"></param>
        /// <returns></returns>
        public ObservableCollection<Department> ReadDepartments(bool activeOnly = true, bool defaultItem = true)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    ObservableCollection<Department> tmpList;

                    if (activeOnly && defaultItem)
                    {
                        tmpList = new ObservableCollection<Department>(db.Departments.Where(x => x.IsActive).ToList());

                        //Insert Please Select
                        Department defaultDepartment = new Department();
                        defaultDepartment.fkCompanyGroupID = -1;
                        defaultDepartment.DepartmentDescription = _defaultItem;

                        tmpList.Insert(0, defaultDepartment);
                    }
                    else
                    {
                        tmpList = new ObservableCollection<Department>(((DbQuery<Department>)(from department in db.Departments
                                                                                              select department)).Include("CompanyGroup")
                                                                            .OrderBy(p => p.fkCompanyGroupID).ToList());
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
        /// Read all the department names
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<string> ReadDepartmentNames()
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    ObservableCollection<string> tmpListNames = new ObservableCollection<string>();
                    ObservableCollection<Department> tmpListDepartments;

                    tmpListDepartments = new ObservableCollection<Department>(db.Departments.ToList());

                    foreach (Department department in tmpListDepartments)
                    {
                        tmpListNames.Add(department.DepartmentDescription);
                    }

                    //Insert the default items
                    tmpListNames.Insert(0, _defaultItem);

                    return tmpListNames;
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
        /// Read the list of Departments for the specified company
        /// </summary>
        /// <param name="activeOnly"></param>
        /// <returns></returns>
        public ObservableCollection<Department> ReadCompanyDepartments(int companyGroupID)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    ObservableCollection<Department> tmpList = new ObservableCollection<Department>(((DbQuery<Department>)(from department in db.Departments
                                                                                                                           where department.IsActive &&
                                                                                                                           department.fkCompanyGroupID == companyGroupID
                                                                                                                           select department))
                                                                                                                           .OrderBy(p => p.DepartmentDescription).ToList());

                    //Insert Please Select
                    Department defaultDepartment = new Department();
                    defaultDepartment.fkCompanyGroupID = -1;
                    defaultDepartment.DepartmentDescription = _defaultItem;

                    tmpList.Insert(0, defaultDepartment);

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
        /// Create a new department
        /// </summary>
        /// <param name="department"></param>
        /// <returns></returns>
        public bool CreateDepartment(Department department)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    db.Departments.Add(department);
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
        /// Update the supplied Department
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public bool UpdateDepartment(Department department)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    Department existingDepartment = db.Departments.Where(x => x.pkDepartmentID == department.pkDepartmentID).FirstOrDefault();

                    if (existingDepartment == null && existingDepartment.pkDepartmentID != department.pkDepartmentID)
                        return false;
                    else
                    {
                        //Since this is an included property an exception is raised since it also get detached
                        //Clearing it removes the issue
                        department.CompanyGroup = null;

                        db.Entry(existingDepartment).State = System.Data.Entity.EntityState.Detached;
                        db.Departments.Attach(department);
                        db.Entry(department).State = System.Data.Entity.EntityState.Modified;
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
    }
}
