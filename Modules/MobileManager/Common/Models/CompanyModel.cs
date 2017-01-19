using Gijima.IOBM.Common.Infrastructure;
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
    public class CompanyModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public CompanyModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new company entity in the database
        /// </summary>
        /// <param name="company">The company entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateCompany(Company company)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.Companies.Any(p => p.CompanyName.ToUpper() == company.CompanyName))
                    {
                        db.Companies.Add(company);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        _eventAggregator.GetEvent<MessageEvent>().Publish(string.Format("The {0} company already exist.", company.CompanyName));
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return false;
            }
        }

        /// <summary>
        /// Read all or active only companies from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <param name="excludeDefault">Flag to include or exclude the default entity.</param>
        /// <returns>Collection of Companies</returns>
        public ObservableCollection<Company> ReadCompanies(bool activeOnly, bool excludeDefault = false)
        {
            try
            {
                IEnumerable<Company> companies = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    companies = ((DbQuery<Company>)(from company in db.Companies
                                                    where activeOnly ? company.IsActive : true &&
                                                          excludeDefault ? company.pkCompanyID > 0 : true
                                                    select company)).Include("CompanyBillingLevels")
                                                                    .Include("CompanyBillingLevels.BillingLevel")
                                                                    .OrderBy(p => p.CompanyName).ToList();

                    return new ObservableCollection<Company>(companies);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return null;
            }
        }

        /// <summary>
        /// Update an existing company entity in the database
        /// </summary>
        /// <param name="company">The company entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateCompany(Company company)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    Company existingCompany = db.Companies.Where(p => p.CompanyName == company.CompanyName).FirstOrDefault();

                    // Check to see if the company name already exist for another entity 
                    if (existingCompany != null && existingCompany.pkCompanyID != company.pkCompanyID)
                    {
                        _eventAggregator.GetEvent<MessageEvent>().Publish(string.Format("The {0} company already exist.", company.CompanyName));
                        return false;
                    }
                    else
                    {
                        // Prevent primary key confilcts when using attach property
                        if (existingCompany != null)
                            db.Entry(existingCompany).State = System.Data.Entity.EntityState.Detached;

                        db.Companies.Attach(company);
                        db.Entry(company).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return false;
            }
        }
    }
}
