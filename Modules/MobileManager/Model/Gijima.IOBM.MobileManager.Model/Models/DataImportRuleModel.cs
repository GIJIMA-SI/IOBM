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
    public class DataImportRuleModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public DataImportRuleModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new data import rule entity in the database
        /// </summary>
        /// <param name="dataImportRule">The data import rule entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateDataImportRule(DataImportRule dataImportRule)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.DataImportRules.Any(p => p.enDataBaseEntity == dataImportRule.enDataBaseEntity))
                    {
                        db.DataImportRules.Add(dataImportRule);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        //_eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(string.Format("The data import rule source {0} and destination {1} already exist.", 
                        //                                                               ((DataUpdateColumn)importRuleData.enDataUpdateColumn).ToString(),
                        //                                                               ((DataUpdateEntity)importRuleData.enDataUpdateEntity).ToString()));
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
        /// Read all or active only data import rules from the database
        /// </summary>
        /// <returns>Collection of DataImportRules</returns>
        public ObservableCollection<DataImportRule> ReadDataImportRules()
        {
            try
            {
                IEnumerable<DataImportRule> importRulesData = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    importRulesData = ((DbQuery<DataImportRule>)(from ruleData in db.DataImportRules
                                                                 select ruleData)).ToList();

                    return new ObservableCollection<DataImportRule>(importRulesData);
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
