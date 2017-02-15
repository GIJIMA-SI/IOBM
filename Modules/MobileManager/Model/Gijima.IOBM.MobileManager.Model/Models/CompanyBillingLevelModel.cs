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
    public class CompanyBillingLevelModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public CompanyBillingLevelModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new company billing level entity in the database
        /// </summary>
        /// <param name="companyID">The companyID the billing level is linked to.</param>
        /// <param name="companyBillingLevel">The company billing level entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateCompanyBillingLevel(int companyID, CompanyBillingLevel companyBillingLevel)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (db.CompanyBillingLevels.Any(p => p.fkCompanyGroupID == companyBillingLevel.fkCompanyGroupID &&
                                                         p.fkBillingLevelID == companyBillingLevel.fkBillingLevelID &&
                                                         p.enBillingLevelType == companyBillingLevel.enBillingLevelType))
                    {
                        _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                        .Publish(new ApplicationMessage("CompanyBillingLevelModel",
                                                                        "The company billing level already exist.",
                                                                        "CreateCompanyBillingLevel",
                                                                        ApplicationMessage.MessageTypes.SystemError));
                        return false;
                    }

                    db.CompanyBillingLevels.Add(companyBillingLevel);
                    db.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("CompanyBillingLevelModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "CreateCompanyBillingLevel",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Read all the company billing levels from the database
        /// </summary>
        /// <param name="companyGroupID">The company group linked to the billing levels.</param>
        /// <param name="billingLevelType">The billing level type based on the package type.</param>
        /// <param name="excludeDefault">Flag to include or exclude the default entity.</param>
        /// <returns>Collection of CompanyBillingLevels</returns>
        public ObservableCollection<CompanyBillingLevel> ReadCompanyBillingLevels(int companyGroupID, short? billingLevelType = null, bool excludeDefault = false)
        {
            try
            {
                IEnumerable<CompanyBillingLevel> companyBillingLevels = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    companyBillingLevels = ((DbQuery<CompanyBillingLevel>)(from companyLevel in db.CompanyBillingLevels
                                                                           join billingLevel in db.BillingLevels on companyLevel.fkBillingLevelID equals billingLevel.pkBillingLevelID
                                                                           where companyLevel.fkCompanyGroupID == 0 ||
                                                                                 companyLevel.fkCompanyGroupID == companyGroupID
                                                                           select companyLevel)).Include("BillingLevel")
                                                                                                .Include("CompanyGroup")
                                                                                                .OrderBy(p => p.BillingLevel.LevelDescription).ToList();

                    if (billingLevelType != null)
                        companyBillingLevels = companyBillingLevels.Where(p => p.enBillingLevelType == billingLevelType || p.enBillingLevelType == 0);

                    if (excludeDefault)
                        companyBillingLevels = companyBillingLevels.Where(p => p.pkCompanyBillingLevelID > 0);

                    return new ObservableCollection<CompanyBillingLevel>(companyBillingLevels);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("CompanyBillingLevelModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadCompanyBillingLevels",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return null;
            }
        }

        /// <summary>
        /// Update an existing company billing level entity in the database
        /// </summary>
        /// <param name="companyID">The companyID the billing level is linked to.</param>
        /// <param name="companyBillingLevel">The company billing level entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateCompanyBillingLevel(int companyID, CompanyBillingLevel companyBillingLevel)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    CompanyBillingLevel existingBillingLevel = db.CompanyBillingLevels.Where(p => p.pkCompanyBillingLevelID == companyBillingLevel.pkCompanyBillingLevelID).FirstOrDefault();

                    // Check to see if the existing billing level description already exist for another entity 
                    if (existingBillingLevel != null && existingBillingLevel.pkCompanyBillingLevelID != companyBillingLevel.pkCompanyBillingLevelID)
                    {
                        _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                        .Publish(new ApplicationMessage("CompanyBillingLevelModel",
                                                                        string.Format("The {0} company billing level already exist.", companyBillingLevel.BillingLevel.LevelDescription),
                                                                        "UpdateCompanyBillingLevel",
                                                                        ApplicationMessage.MessageTypes.SystemError));
                        return false;
                    }

                    existingBillingLevel.fkCompanyGroupID = companyBillingLevel.fkCompanyGroupID;
                    existingBillingLevel.fkBillingLevelID = companyBillingLevel.fkBillingLevelID;
                    existingBillingLevel.enBillingLevelType = companyBillingLevel.enBillingLevelType;
                    existingBillingLevel.Amount = companyBillingLevel.Amount;
                    existingBillingLevel.ModifiedBy = Security.SecurityHelper.LoggedInFullName;
                    existingBillingLevel.ModifiedDate = DateTime.Now;
                    db.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("CompanyBillingLevelModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "UpdateCompanyBillingLevel",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Delete an existing company billing level entity from the database
        /// </summary>
        /// <param name="companyBillingLevelID">The ID of the company billing level entity to delete.</param>
        /// <returns>True if successfull</returns>
        public bool DeleteCompanyBillingLevel(int companyBillingLevelID)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    CompanyBillingLevel billingLevelToDelete = db.CompanyBillingLevels.Where(p => p.pkCompanyBillingLevelID == companyBillingLevelID).FirstOrDefault();

                    if (billingLevelToDelete != null)
                    {
                        db.CompanyBillingLevels.Remove(billingLevelToDelete);
                        db.SaveChanges();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("CompanyBillingLevelModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "DeleteCompanyBillingLevel",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }
    }
}
