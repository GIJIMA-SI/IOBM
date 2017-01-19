using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class ValidationRuleModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public ValidationRuleModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new validation rule entity in the database
        /// </summary>
        /// <param name="validationRule">The validation rule entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateValidationRule(ValidationRule validationRule)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.ValidationRules.Any(p => p.fkValidationRulesDataID == validationRule.fkValidationRulesDataID &&
                                                     p.EntityID == validationRule.EntityID))
                    {
                        db.ValidationRules.Add(validationRule);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        _eventAggregator.GetEvent<MessageEvent>().Publish("The validation rule already exist.");
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
        /// Read all the validation rules for the specified group from the database
        /// </summary>
        /// <param name="validationGroup">The validation group linked to the rules.</param>
        /// <returns>Collection of ValidationRules</returns>
        public ObservableCollection<ValidationRule> ReadValidationRules(ValidationRuleGroup validationGroup)
        {
            try
            {
                List<ValidationRule> validationRules = new List<ValidationRule>();
                var rules = (dynamic)null;
                short groupID = validationGroup.Value();

                using (var db = MobileManagerEntities.GetContext())
                {
                    switch (validationGroup)
                    {
                        case ValidationRuleGroup.Client:
                            //EntityDisplayName = "PrimaryCellNumber";
                            //EntityCollection = new ObservableCollection<object>(new ClientModel(_eventAggregator).ReadClients(true));
                            break;
                        case ValidationRuleGroup.Company:
                            rules = (from validationRule in db.ValidationRules
                                     join validationRulesData in db.ValidationRulesDatas
                                     on validationRule.fkValidationRulesDataID equals validationRulesData.pkValidationRulesDataID
                                     join company in db.Companies
                                     on validationRule.EntityID equals company.pkCompanyID
                                     where validationRule.enValidationRuleGroup == groupID
                                     select new
                                     {
                                         pkValidationRuleID = validationRule.pkValidationRuleID,
                                         enValidationRuleGroup = validationRule.enValidationRuleGroup,
                                         fkValidationRulesDataID = validationRule.fkValidationRulesDataID,
                                         RuleDataName = validationRulesData.ValidationDataName,
                                         enRuleDataType = validationRulesData.enValidationDataType,
                                         EntityID = validationRule.EntityID,
                                         EntityName = company.CompanyName,
                                         ValidationDataValue = validationRule.ValidationDataValue,
                                         enStringCompareType = validationRule.enStringCompareType,
                                         enNumericCompareType = validationRule.enNumericCompareType,
                                         enDateCompareType = validationRule.enDateCompareType,
                                         ModifiedBy = validationRule.ModifiedBy,
                                         ModifiedDate = validationRule.ModifiedDate
                                     }).OrderBy(p => p.EntityName).ToList();

                            foreach (var rule in rules)
                            {
                                validationRules.Add(new ValidationRule
                                {
                                    pkValidationRuleID = rule.pkValidationRuleID,
                                    enValidationRuleGroup = rule.enValidationRuleGroup,
                                    fkValidationRulesDataID = rule.fkValidationRulesDataID,
                                    RuleDataName = rule.RuleDataName,
                                    RuleDataTypeName = ((ValidationDataType)rule.enRuleDataType).ToString(),
                                    EntityID = rule.EntityID,
                                    EntityName = rule.EntityName,
                                    ValidationDataValue = rule.ValidationDataValue,
                                    enStringCompareType = rule.enStringCompareType,
                                    enNumericCompareType = rule.enNumericCompareType,
                                    enDateCompareType = rule.enDateCompareType,
                                    ModifiedBy = rule.ModifiedBy,
                                    ModifiedDate = rule.ModifiedDate
                                });
                            }

                            break;
                    }

                    return new ObservableCollection<ValidationRule>(validationRules);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return null;
            }
        }

        /// <summary>
        /// Update an existing validation rule entity in the database
        /// </summary>
        /// <param name="validationRule">The validation rule entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateValidationRule(ValidationRule validationRule)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    ValidationRule existingValidationRule = db.ValidationRules.Where(p => p.pkValidationRuleID == validationRule.pkValidationRuleID).FirstOrDefault();

                    // Check to see if the validation rule name already exist for another entity 
                    if (existingValidationRule != null && existingValidationRule.pkValidationRuleID != validationRule.pkValidationRuleID)
                    {
                        _eventAggregator.GetEvent<MessageEvent>().Publish("The validationRule already exist.");
                        return false;
                    }
                    else
                    {
                        if (existingValidationRule != null)
                        {
                            existingValidationRule.fkValidationRulesDataID = validationRule.fkValidationRulesDataID;
                            existingValidationRule.EntityID = validationRule.EntityID;
                            existingValidationRule.ValidationDataValue = validationRule.ValidationDataValue;
                            existingValidationRule.enStringCompareType = validationRule.enStringCompareType;
                            existingValidationRule.enDateCompareType = validationRule.enDateCompareType;
                            existingValidationRule.enNumericCompareType = validationRule.enNumericCompareType;
                            existingValidationRule.ModifiedBy = validationRule.ModifiedBy;
                            existingValidationRule.ModifiedDate = validationRule.ModifiedDate;
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
