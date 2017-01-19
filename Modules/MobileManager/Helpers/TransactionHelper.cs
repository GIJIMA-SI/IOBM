using System;
using System.Configuration;
using System.Transactions;

namespace Gijima.IOBM.MobileManager.Common
{
    public class TransactionHelper
    {
        private TransactionHelper() { }

        /// <summary>
        /// Create a transaction scope with a default timeout, as configured. 
        /// </summary>
        /// <returns> The transaction scope. </returns>
        public static TransactionScope CreateTransactionScope()
        {
            int transactionTimeOut = Convert.ToInt32(ConfigurationManager.AppSettings["TransactionScopeTimeOutInMinute"]);

            TransactionOptions transactionOptions = new TransactionOptions();

            if (Transaction.Current != null)
                transactionOptions.IsolationLevel = Transaction.Current.IsolationLevel;
            else
                transactionOptions.IsolationLevel = IsolationLevel.ReadCommitted;

            transactionOptions.Timeout = new TimeSpan(0, transactionTimeOut, 0);
            return new TransactionScope(TransactionScopeOption.Required, transactionOptions);
        }

        /// <summary>
        /// Create a transaction scope with a given timeout. 
        /// </summary>
        /// <param name="timeout"> The timeout in minutes. </param>
        /// <returns> The transaction scope. </returns>
        public static TransactionScope CreateTransactionScope(int timeout)
        {
            TransactionOptions transactionOptions = new TransactionOptions();
            if (Transaction.Current != null)
                transactionOptions.IsolationLevel = Transaction.Current.IsolationLevel;
            else
                transactionOptions.IsolationLevel = IsolationLevel.ReadCommitted;

            transactionOptions.Timeout = new TimeSpan(0, timeout, 0);
            return new TransactionScope(TransactionScopeOption.Required, transactionOptions);
        }

        /// <summary>
        /// Create a transaction scope with a given timeout. 
        /// </summary>
        /// <param name="timeout"> The timeout in minutes. </param>
        /// <param name="level"> The isolation level. </param>
        /// <returns> The transaction scope. </returns>
        public static TransactionScope CreateTransactionScope(int timeout, IsolationLevel level)
        {
            TransactionOptions transactionOptions = new TransactionOptions();
            if (Transaction.Current != null)
                transactionOptions.IsolationLevel = Transaction.Current.IsolationLevel;
            else
                transactionOptions.IsolationLevel = level;

            transactionOptions.Timeout = new TimeSpan(0, timeout, 0);
            return new TransactionScope(TransactionScopeOption.Required, transactionOptions);
        }
    }
}
