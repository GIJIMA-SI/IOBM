using System;
using System.Configuration;
using System.Data.Entity.Infrastructure;

namespace Gijima.IOBM.MobileManager.Model.Data
{
    public partial class MobileManagerEntities
    {
        #region Attributes and Properties

        #endregion

        #region Construction

        private MobileManagerEntities(string connection)
            : base(connection)
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// Get a database context. This coustom method applies the Mobile Manager configuration schema.
        /// </summary>
        /// <param name="timeout"> The timeout in secods to apply to commands. Default is 30 seconds. </param>
        /// <returns> The database context. </returns>
        public static MobileManagerEntities GetContext(int timeout = 30)
        {
            string connectionString = string.Empty;

            if (string.IsNullOrEmpty(connectionString))
            {
                // First, try to get the database connection string from the local config (which is handy for extraction servers)                
                connectionString = ConfigurationManager.ConnectionStrings["MobileManagerEntities"].ToString();
            }

            // By now we should have a connection string, if not, complain.
            if (string.IsNullOrEmpty(connectionString))
                throw new Exception("Unable to obtain database connection string from local/global settings.");

            // Get the entity framework DB context.
            MobileManagerEntities sqlEntities = new MobileManagerEntities(connectionString);

            // Apply the timeout setting
            IObjectContextAdapter adapter = sqlEntities as IObjectContextAdapter;
            System.Data.Entity.Core.Objects.ObjectContext objectContext = adapter.ObjectContext;
            objectContext.CommandTimeout = timeout;
            sqlEntities.Configuration.ProxyCreationEnabled = false;

            return sqlEntities;
        }

        #endregion
    }
}
