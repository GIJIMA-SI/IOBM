using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class ValidationRulesDataModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public ValidationRulesDataModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new validation rule data entity in the database
        /// </summary>
        /// <param name="validationRulesData">The validation rule data entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateValidationRulesGroup(ValidationRulesData validationRulesData)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.ValidationRulesDatas.Any(p => p.enValidationRuleGroup == validationRulesData.enValidationRuleGroup &&
                                                          p.ValidationDataName == validationRulesData.ValidationDataName))
                    {
                        db.ValidationRulesDatas.Add(validationRulesData);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        _eventAggregator.GetEvent<MessageEvent>().Publish(string.Format("The validation rule data {0} already exist.", validationRulesData.ValidationDataName));
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
        /// Read all or active only validation rules data from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <param name="excludeDefault">Flag to include or exclude the default entity.</param>
        /// <returns>Collection of ValidationRulesData</returns>
        public ObservableCollection<ValidationRulesData> ReadValidationRulesData(bool activeOnly, bool excludeDefault = false)
        {
            try
            {
                IEnumerable<ValidationRulesData> validationRulesData = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    validationRulesData = ((DbQuery<ValidationRulesData>)(from validationRuleData in db.ValidationRulesDatas
                                                                          where activeOnly ? validationRuleData.IsActive : true &&
                                                                                excludeDefault ? validationRuleData.pkValidationRulesDataID > 0 : true
                                                                          select validationRuleData)).OrderBy(p => p.ValidationDataName).ToList();

                    return new ObservableCollection<ValidationRulesData>(validationRulesData);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return null;
            }
        }

        /// <summary>
        /// Update an existing validation rule data entity in the database
        /// </summary>
        /// <param name="validationRulesData">The validation rule data entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateValidationRulesData(ValidationRulesData validationRulesData)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    ValidationRulesData existingValidationRulesData = db.ValidationRulesDatas.Where(p => p.enValidationRuleGroup == validationRulesData.enValidationRuleGroup &&
                                                                                                         p.ValidationDataName == validationRulesData.ValidationDataName).FirstOrDefault();

                    // Check to see if the validation rule data name already exist for another entity 
                    if (existingValidationRulesData != null && existingValidationRulesData.pkValidationRulesDataID != validationRulesData.pkValidationRulesDataID)
                    {
                        _eventAggregator.GetEvent<MessageEvent>().Publish(string.Format("The validation rule data {0} already exist.", validationRulesData.ValidationDataName));
                        return false;
                    }
                    else
                    {
                        if (existingValidationRulesData != null)
                        {
                            existingValidationRulesData.enValidationRuleGroup = validationRulesData.enValidationRuleGroup;
                            existingValidationRulesData.ValidationDataName = validationRulesData.ValidationDataName;
                            existingValidationRulesData.ModifiedBy = validationRulesData.ModifiedBy;
                            existingValidationRulesData.ModifiedDate = validationRulesData.ModifiedDate;
                            db.SaveChanges();
                            return true;
                        }

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
    }
}
