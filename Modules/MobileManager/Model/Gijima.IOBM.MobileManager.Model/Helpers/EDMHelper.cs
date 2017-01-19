using Gijima.IOBM.MobileManager.Model.Data;
using System;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;

namespace Gijima.IOBM.MobileManager.Model.Helpers
{
    public class EDMHelper
    {
        /// <summary>
        /// Get the base sql structure of the entity.
        /// </summary>
        /// <typeparam name="T">The Entity</typeparam>
        /// <returns>Entity Properties</returns>
        public static PropertyDescriptor[] GetEntityStructure<T>()
        {
            // Get the entity structure
            Type entityType = typeof(T);
            PropertyDescriptorCollection edmxProperties = TypeDescriptor.GetProperties(typeof(T));
            PropertyDescriptor[] sqlProperties = null;
            int idx = 0;

            // Get the sql table structure
            using (var db = MobileManagerEntities.GetContext())
            {
                var objectContext = ((IObjectContextAdapter)db).ObjectContext;
                var storageMetadata = ((EntityConnection)objectContext.Connection).GetMetadataWorkspace().GetItems(DataSpace.SSpace);
                var entityProperties = (from s in storageMetadata where s.BuiltInTypeKind == BuiltInTypeKind.EntityType select s as EntityType);
                var tableProperties = (from m in entityProperties where m.Name == entityType.Name select m).Single();
                sqlProperties = new PropertyDescriptor[tableProperties.Properties.Count];

                // Add only the properties found in the sql table structure
                foreach (System.ComponentModel.PropertyDescriptor prop in edmxProperties)
                {
                    if (tableProperties.Properties.Contains(prop.Name))
                    {
                        sqlProperties[idx] = prop;
                        idx++;
                    }
                }

                return sqlProperties;
            }
        }

        /// <summary>
        /// Write a data table schema and data to a sql table
        /// </summary>
        /// <param name="externalDataID">The external data entry ID.</param>
        /// <param name="sqlTableName">The SQL table name.</param>
        /// <param name="dataTable">The data table to write to sql.</param>
        /// <returns>True if successfull</returns>
        public static bool WriteDataTableToSQL(string sqlTableName, DataTable dataTable)
        {
            try
            {
                string connectionString = MobileManagerEntities.GetContext().Database.Connection.ConnectionString;

                // Create the sql table structure
                if (CreateTableFromDataTable(sqlTableName, dataTable))
                {
                    // Write the data to the sql table
                    using (var bulkCopy = new SqlBulkCopy(connectionString))
                    {
                        foreach (DataColumn col in dataTable.Columns)
                        {
                            bulkCopy.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                        }

                        bulkCopy.BulkCopyTimeout = 600;
                        bulkCopy.DestinationTableName = sqlTableName;
                        bulkCopy.WriteToServer(dataTable);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Create the sql table based on the data table schema
        /// </summary>
        /// <param name="sqlTableName">The SQL table name.</param>
        /// <param name="dataTable">The data table to write to sql.</param>
        /// <returns>True if successfull</returns>
        public static bool CreateTableFromDataTable(string sqlTableName, DataTable table)
        {
            try
            {
                string connectionString = MobileManagerEntities.GetContext().Database.Connection.ConnectionString;

                // Drop the external data table if exist
                if (!DropTable(sqlTableName))
                    return false;

                // Get the SQL command to create the external sql table
                string sql = GetCreateFromDataTableSQL(sqlTableName, table);

                // Create the external sql table
                SqlConnection sqlCon = new SqlConnection(connectionString);
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                sqlCon.Open();
                cmd.ExecuteNonQuery();
                sqlCon.Close();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Drop the specified sql table
        /// </summary>
        /// <param name="sqlTableName">The SQL table name.</param>
        /// <returns>True if successfull</returns>
        public static bool DropTable(string sqlTableName)
        {
            try
            {
                string connectionString = MobileManagerEntities.GetContext().Database.Connection.ConnectionString;

                string sql = "IF OBJECT_ID('" + sqlTableName + "', 'U') IS NOT NULL ";
                sql += "BEGIN ";
                sql += "DROP TABLE [" + sqlTableName + "]";
                sql += "END";

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(sql))
                    {
                        cmd.Connection = con;
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Create the sql script to create the sql table from the data table
        /// </summary>
        /// <param name="sqlTableName">The SQL table name.</param>
        /// <param name="table">The data table.</param>
        /// <returns>True if successfull</returns>
        public static string GetCreateFromDataTableSQL(string tableName, DataTable table)
        { 
            string sql = "CREATE TABLE [" + tableName + "] (\n";

            foreach (DataColumn column in table.Columns) 
            { 
                sql += "[" + column.ColumnName + "] " + GetSQLDataTypeForDataTableType(column) + ",\n"; 
            }
             
            sql = sql.TrimEnd(new char[] { ',', '\n' }) + ")\n"; 
 
            return sql; 
        }

        /// <summary>
        /// Get the data table column type
        /// </summary>
        /// <param name="column">Data table column.</param>
        /// <returns>Data Type Name</returns>
        public static string GetSQLDataTypeForDataTableType(DataColumn column)
        { 
            return GetSQLDataType(column.DataType, column.MaxLength, 10, 2);
        }

        /// <summary>
        /// Get the data table column type
        /// </summary>
        /// <param name="column">Data table column.</param>
        /// <returns>Data Type Name</returns>
        public static string GetSQLDataType(object type, int columnSize, int numericPrecision, int numericScale)
        { 
            switch (type.ToString()) 
            { 
                case "System.String": 
                    return "VARCHAR(" + ((columnSize == -1) ? 255 : columnSize) + ")"; 
                case "System.Decimal": 
                    if (numericScale > 0) 
                        return "REAL"; 
                else if (numericPrecision > 10) 
                    return "BIGINT"; 
                else 
                    return "INT"; 
                case "System.Double": 
                case "System.Single": 
                    return "REAL"; 
                case "System.Int64": 
                    return "BIGINT"; 
                case "System.Int16": 
                case "System.Int32": 
                    return "INT"; 
                case "System.DateTime": 
                    return "DATETIME";     
                default: 
                    throw new Exception(type.ToString() + " not implemented."); 
            } 
        }

        /// <summary>
        /// Get the CLR data type based on the SQL data type
        /// </summary>
        /// <param name="sqlType">The Sql data type</param>
        /// <returns></returns>
        public static Type GetClrTypeFromSQLType(SqlDbType sqlType)
        {
            switch (sqlType)
            {
                case SqlDbType.BigInt:
                    return typeof(long?);
                case SqlDbType.Binary:
                case SqlDbType.Image:
                case SqlDbType.Timestamp:
                case SqlDbType.VarBinary:
                    return typeof(byte[]);
                case SqlDbType.Bit:
                    return typeof(bool?);
                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                case SqlDbType.VarChar:
                case SqlDbType.Xml:
                    return typeof(string);
                case SqlDbType.DateTime:
                case SqlDbType.SmallDateTime:
                case SqlDbType.Date:
                case SqlDbType.Time:
                case SqlDbType.DateTime2:
                    return typeof(DateTime?);
                case SqlDbType.Decimal:
                case SqlDbType.Money:
                case SqlDbType.SmallMoney:
                    return typeof(decimal?);
                case SqlDbType.Float:
                    return typeof(double?);
                case SqlDbType.Int:
                    return typeof(int?);
                case SqlDbType.Real:
                    return typeof(float?);
                case SqlDbType.UniqueIdentifier:
                    return typeof(Guid?);
                case SqlDbType.SmallInt:
                    return typeof(short?);
                case SqlDbType.TinyInt:
                    return typeof(byte?);
                case SqlDbType.Variant:
                case SqlDbType.Udt:
                    return typeof(object);
                case SqlDbType.Structured:
                    return typeof(DataTable);
                case SqlDbType.DateTimeOffset:
                    return typeof(DateTimeOffset?);
                default:
                    throw new ArgumentOutOfRangeException("sqlType");
            }
        }
    }
}
