using System;

namespace Gijima.IOBM.Infrastructure.Helpers
{
    public class UIHelper
    {
        /// <summary>
        /// Validate if the input string is numeric
        /// </summary>
        /// <param name="value"></param>
        /// <returns>True if numeric else return false</returns>
        public static Boolean IsNumeric(string value)
        {
            int result;
            return int.TryParse(value, out result);
        }

        /// <summary>
        /// Validate if the input string is decimal
        /// </summary>
        /// <param name="value"></param>
        /// <returns>True if numeric else return false</returns>
        public static Boolean IsDecimal(string value)
        {
            decimal result;
            return decimal.TryParse(value, out result);
        }

        /// <summary>
        /// Validate if the input string is float
        /// </summary>
        /// <param name="value"></param>
        /// <returns>True if float else return false</returns>
        public static Boolean IsFloat(string value)
        {
            float result;
            return float.TryParse(value, out result);
        }

        /// <summary>
        /// Validate if the input string is DateTime
        /// </summary>
        /// <param name="value"></param>
        /// <returns>True if DateTime else return false</returns>
        public static Boolean IsDateTime(string value)
        {
            DateTime result;
            return DateTime.TryParse(value, out result);
        }
    }
}
