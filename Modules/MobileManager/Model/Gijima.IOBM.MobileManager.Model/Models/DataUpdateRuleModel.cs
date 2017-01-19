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
    public class DataUpdateRuleModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public DataUpdateRuleModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new data update rule entity in the database
        /// </summary>
        /// <param name="dataUpdateRule">The data update rule entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateDataUpdateRule(DataUpdateRule dataUpdateRule)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.DataUpdateRules.Any(p => p.enDataUpdateColumn == dataUpdateRule.enDataUpdateColumn &&
                                                     p.enDataUpdateEntity == dataUpdateRule.enDataUpdateEntity))
                    {
                        db.DataUpdateRules.Add(dataUpdateRule);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        //_eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(string.Format("The data update rule source {0} and destination {1} already exist.", 
                        //                                                               ((DataUpdateColumn)updateRuleData.enDataUpdateColumn).ToString(),
                        //                                                               ((DataUpdateEntity)updateRuleData.enDataUpdateEntity).ToString()));
                        return false;
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
        /// Read all or active only data update rule data from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <returns>Collection of DataUpdateRules</returns>
        public ObservableCollection<DataUpdateRule> ReadDataUpdateRules(bool activeOnly)
        {
            try
            {
                IEnumerable<DataUpdateRule> updateRuleData = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    updateRuleData = ((DbQuery<DataUpdateRule>)(from ruleData in db.DataUpdateRules
                                                                where activeOnly ? ruleData.IsActive : true
                                                                select ruleData)).ToList();

                    return new ObservableCollection<DataUpdateRule>(updateRuleData);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return null;
            }
        }

        /// <summary>
        /// Update an existing validation rule data entity in the database
        /// </summary>
        /// <param name="validationRulesData">The validation rule data entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateDataValidationOption(DataValidationProperty validationRulesData)
        {
            try
            {
                //using (var db = MobileManagerEntities.GetContext())
                //{
                //    DataValidationOption existingDataValidationOption = db.DataValidationOptions.Where(p => p.enDataValidationEntity == validationRulesData.enDataValidationEntity &&
                //                                                                                         p.ValidationDataName == validationRulesData.ValidationDataName).FirstOrDefault();

                //    // Check to see if the validation rule data name already exist for another entity 
                //    if (existingDataValidationOption != null && existingDataValidationOption.pkDataValidationOptionID != validationRulesData.pkDataValidationOptionID)
                //    {
                //        _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(string.Format("The validation rule data {0} already exist.", validationRulesData.ValidationDataName));
                //        return false;
                //    }
                //    else
                //    {
                //        if (existingDataValidationOption != null)
                //        {
                //            existingDataValidationOption.enDataValidationEntity = validationRulesData.enDataValidationEntity;
                //            existingDataValidationOption.ValidationDataName = validationRulesData.ValidationDataName;
                //            existingDataValidationOption.ModifiedBy = validationRulesData.ModifiedBy;
                //            existingDataValidationOption.ModifiedDate = validationRulesData.ModifiedDate;
                //            db.SaveChanges();
                //            return true;
                //        }

                //        return false;
                //    }
                //}

                return true;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return false;
            }
        }
    }
}
