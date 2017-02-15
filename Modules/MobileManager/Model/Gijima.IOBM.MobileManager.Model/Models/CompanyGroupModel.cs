using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class CompanyGroupModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public CompanyGroupModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new company group entity in the database
        /// </summary>
        /// <param name="group">The company group entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateCompanyGroup(CompanyGroup group)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.CompanyGroups.Any(p => p.GroupName.ToUpper() == group.GroupName))
                    {
                        db.CompanyGroups.Add(group);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                        .Publish(new ApplicationMessage("CompanyGroupModel",
                                                                        string.Format("The {0} group already exist.", group.GroupName),
                                                                        "CreateCompanyGroup",
                                                                        ApplicationMessage.MessageTypes.Information));
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("CompanyGroupModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "CreateCompanyGroup",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Read all or active only company groups from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <param name="excludeDefault">Flag to include or exclude the default entity.</param>
        /// <returns>Collection of CompanyGroups</returns>
        public ObservableCollection<CompanyGroup> ReadCompanyGroups(bool activeOnly, bool excludeDefault = false)
        {
            try
            {
                IEnumerable<CompanyGroup> groups = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    groups = ((DbQuery<CompanyGroup>)(from companyGroup in db.CompanyGroups
                                                      where activeOnly ? companyGroup.IsActive : true &&
                                                            excludeDefault ? companyGroup.pkCompanyGroupID > 0 : true
                                                      select companyGroup)).OrderBy(p => p.GroupName).ToList();

                    return new ObservableCollection<CompanyGroup>(groups);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("CompanyGroupModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadCompanyGroups",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return null;
            }
        }

        /// <summary>
        /// Update an existing company group entity in the database
        /// </summary>
        /// <param name="group">The company group entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateCompanyGroup(CompanyGroup group)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    CompanyGroup existingCompanyGroup = db.CompanyGroups.Where(p => p.GroupName == group.GroupName).FirstOrDefault();

                    // Check to see if the group description already exist for another entity 
                    if (existingCompanyGroup != null && existingCompanyGroup.pkCompanyGroupID != group.pkCompanyGroupID)
                    {
                        _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                        .Publish(new ApplicationMessage("CompanyGroupModel",
                                                                        string.Format("The {0} group already exist.", group.GroupName),
                                                                        "UpdateCompanyGroup",
                                                                        ApplicationMessage.MessageTypes.Information));
                        return false;
                    }
                    else
                    {
                        // Prevent primary key confilcts when using attach property
                        if (existingCompanyGroup != null)
                            db.Entry(existingCompanyGroup).State = System.Data.Entity.EntityState.Detached;

                        db.CompanyGroups.Attach(group);
                        db.Entry(group).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("CompanyGroupModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "UpdateCompanyGroup",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }
    }
}
