using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class AdvancedSearchModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        public AdvancedSearchModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Returns the data from the given query
        /// </summary>
        /// <param name="sqlQuery"></param>
        /// <returns></returns>
        public DataTable ReadProvidedQuery(string sqlQuery)
        {
            try
            {
                string connectionString = MobileManagerEntities.GetContext().Database.Connection.ConnectionString;
                
                DataTable queryData = new DataTable();

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(sqlQuery, con);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(queryData);
                    }
                    con.Close();
                }

                List<DataColumn> columnsToRemove = new List<DataColumn>();
                //List all unwanted columns
                foreach (DataColumn column in queryData.Columns)
                {
                    if (column.ColumnName.StartsWith("fk") || column.ColumnName.StartsWith("pk") ||
                        column.ColumnName.StartsWith("en") || column.ColumnName == "IsActive" ||
                        column.ColumnName == "ModifiedBy" || column.ColumnName == "ModifiedDate" || 
                        column.ColumnName == "IsActive1" || column.ColumnName == "ModifiedBy1" || 
                        column.ColumnName == "ModifiedDate1")
                    { columnsToRemove.Add(column); }
                }
                //Remove all unwanted columns
                foreach (DataColumn column in columnsToRemove)
                {
                    queryData.Columns.Remove(column);
                }

                return queryData;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                    .Publish(new ApplicationMessage(this.GetType().Name,
                                             string.Format("Error! {0}, {1}.",
                                             ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                             MethodBase.GetCurrentMethod().Name,
                                             ApplicationMessage.MessageTypes.SystemError));
                return new DataTable();
            }
        }
    }
}
