using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Helpers;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Gijima.IOBM.MobileManager.Model.Helpers;
using Gijima.IOBM.Security;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class ExternalBillingDataModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        public ExternalBillingDataModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new external data entity entry in the database
        /// </summary>
        /// <param name="externalData">The external data entiyyto add.</param>
        /// <returns>The external data primary key</returns>
        public bool CreateExternalData(string sqlTableName, ExternalBillingData externalDataEntry, DataTable externalData)
        {
            try
            {
                int externalDataID = 0;

                using (TransactionScope tc = TransactionHelper.CreateTransactionScope())
                {
                    using (var db = MobileManagerEntities.GetContext())
                    {
                        // Create the external data table entry
                        ExternalBillingData existingData = db.ExternalBillingDatas.Where(p => p.TableName == sqlTableName).FirstOrDefault();

                        if (existingData == null)
                        {
                            db.ExternalBillingDatas.Add(externalDataEntry);
                            db.SaveChanges();
                            externalDataID = externalDataEntry.pkExternalBillingDataID;
                        }
                        else
                        {
                            existingData.BillingPeriod = externalDataEntry.BillingPeriod;
                            existingData.DataFileName = externalDataEntry.DataFileName;
                            existingData.ModifiedBy = externalDataEntry.ModifiedBy;
                            existingData.DateModified = externalDataEntry.DateModified;
                            externalDataID = existingData.pkExternalBillingDataID;
                        }
                        db.SaveChanges();
                        
                        // Create properties in the data validation property table
                        // for all the columns in the external data table
                        DataValidationProperty dataProperty = null;
                        foreach (DataColumn col in externalData.Columns)
                        {
                            dataProperty = new DataValidationProperty();
                            dataProperty.enDataValidationGroupName = DataValidationGroupName.ExternalData.Value();
                            dataProperty.enDataValidationEntity = (short)externalDataID;
                            dataProperty.ExtDataValidationProperty = col.ColumnName;
                            dataProperty.enDataType = GetDataValidationDataType(col.DataType);
                            dataProperty.ModifiedBy = SecurityHelper.LoggedInFullName;
                            dataProperty.ModifiedDate = DateTime.Now;
                            dataProperty.IsActive = true;
                            new DataValidationPropertyModel(_eventAggregator).CreateDataValidationProperty(dataProperty);
                        }

                        // Write the external data to a sql table
                        EDMHelper.WriteDataTableToSQL(sqlTableName, externalData);
                    }

                    tc.Complete();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ExternalDataModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "CreateExternalData",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Read all the external data entries from the database
        /// </summary>
        /// <param name="billingPeriod">The current billing period.</param>
        /// <returns>Collection of ExternalData entries</returns>
        public ObservableCollection<ExternalBillingData> ReadExternalData(string billingPeriod = null, bool state = false)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    IEnumerable<ExternalBillingData> externalData = ((DbQuery<ExternalBillingData>)(from extData in db.ExternalBillingDatas
                                                                                                    select extData)).ToList();

                    if (billingPeriod != null)
                    {
                        externalData = externalData.Where(p => (p.pkExternalBillingDataID == 0 || p.BillingPeriod == billingPeriod) &&
                                                                p.ValidationPassed == state);
                    }

                    return new ObservableCollection<ExternalBillingData>(externalData);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ExternalDataModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadExternalData",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return null;
            }
        }

        /// <summary>
        /// Read all the imported external data for the specified provider
        /// </summary>
        /// <param name="sqlTableName">The SQL table name.</param>
        /// <returns>Data table</returns>
        public DataTable ReadExternalBillingData(string sqlTableName)
        {
            try
            {
                string connectionString = MobileManagerEntities.GetContext().Database.Connection.ConnectionString;
                string sql = "SELECT * FROM [" + sqlTableName + "]";
                DataTable billingData = new DataTable();

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(sql, con);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(billingData);
                    }
                    con.Close();
                }

                return billingData;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Read all the external data entries from the database
        /// </summary>
        /// <returns>Collection of ExternalData entries</returns>
        public short GetDataValidationDataType(Type dataType)
        {
            switch (dataType.ToString())
            {
                case "System.String":
                    return DataTypeName.String.Value();
                case "System.Decimal":
                    return DataTypeName.Decimal.Value();
                case "System.Double":
                case "System.Single":
                    return DataTypeName.Float.Value();
                case "System.Int64":
                    return DataTypeName.Long.Value();
                case "System.Int16":
                    return DataTypeName.Short.Value();
                case "System.Int32":
                    return DataTypeName.Integer.Value();
                case "System.DateTime":
                    return DataTypeName.DateTime.Value();
                case "System.Boolean":
                    return DataTypeName.Bool.Value();
                default:
                    throw new Exception(dataType.ToString() + " not implemented.");
            }
        }
    }
}
