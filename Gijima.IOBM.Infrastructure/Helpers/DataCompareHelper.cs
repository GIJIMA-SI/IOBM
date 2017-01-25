using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Structs;
using Prism.Events;
using System;
using System.Globalization;

namespace Gijima.IOBM.Infrastructure.Helpers
{
    public class DataCompareHelper
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public DataCompareHelper(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Compare string values based on the specified operator
        /// </summary>
        /// <param name="stringOperator">The string operator to use in the validation.</param>
        /// <param name="valueToCompare">The value to compare.</param>
        /// <param name="valueToCompareTo">The value to compare to.</param>
        /// <returns>True if successfull</returns>
        public bool CompareStringValues(StringOperator stringOperator, string valueToCompare, string valueToCompareTo)
        {
            try
            {
                string[] compareToValues = valueToCompareTo.Split(';');

                foreach (string value in compareToValues)
                {
                    switch (stringOperator)
                    {
                        case StringOperator.Contains:
                            if (valueToCompare.ToUpper().Trim().Contains(value.ToUpper().Trim()))
                                return true;
                            break;
                        case StringOperator.Equal:
                            if (string.Equals(valueToCompare.ToUpper().Trim(), value.ToUpper().Trim()))
                                return true;
                            break;
                       case StringOperator.Format:
                            return false;
                        case StringOperator.LenghtSmaller:
                            if (valueToCompare.Trim().Length < Convert.ToInt32(value))
                                return true;
                            break;
                        case StringOperator.LengthEqual:
                            if (valueToCompare.Trim().Length == Convert.ToInt32(value))
                                return true;
                            break;
                        case StringOperator.LengthGreater:
                            if (valueToCompare.Trim().Length > Convert.ToInt32(value))
                                return true;
                            break;
                        case StringOperator.PostFix:
                            if (valueToCompare.Trim().EndsWith(value))
                                return true;
                            break;
                        case StringOperator.PreFix:
                            if (valueToCompare.Trim().StartsWith(value))
                                return true;
                            break;
                        default:
                            return false;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return false;
            }
        }

        /// <summary>
        /// Compare numeric values based on the specified operator
        /// </summary>
        /// <param name="numericOperator">The numeric operator to use in the validation.</param>
        /// <param name="valueToCompare">The value to compare.</param>
        /// <param name="valueToCompareTo">The value to compare to.</param>
        /// <returns>True if successfull</returns>
        public bool CompareNumericValues(NumericOperator numericOperator, string valueToCompare, string valueToCompareTo)
        {
            try
            {
                dynamic parsedValueToCompare = 0;
                dynamic parsedValueToCompareTo = 0;
                short shortValueToCompare = 0;
                short shortValueToCompareTo = 0;
                int intValueToCompare = 0;
                int intValueToCompareTo = 0;
                long longValueToCompare = 0;
                long longValueToCompareTo = 0;
                decimal decimalValueToCompare = 0;
                decimal decimalValueToCompareTo = 0;
                float floatValueToCompare = 0;
                float floatValueToCompareTo = 0 ;
                double doubleValueToCompare = 0;
                double doubleValueToCompareTo = 0;
                string[] compareToValues = valueToCompareTo.Split(';');

                // Convert the decimal symbol in the string to the computers
                // configured decimal symbol to ensure the correct parsing
                char decimalSymbol = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                if (decimalSymbol == ',')
                    valueToCompare = valueToCompare.Replace('.', decimalSymbol);
                else
                    valueToCompare = valueToCompare.Replace(',', decimalSymbol);

                foreach (string value in compareToValues)
                {
                    if (short.TryParse(valueToCompare, out shortValueToCompare) && short.TryParse(value, out shortValueToCompareTo))
                    {
                        parsedValueToCompare = shortValueToCompare;
                        parsedValueToCompareTo = shortValueToCompareTo;
                    }
                    else if (int.TryParse(valueToCompare, out intValueToCompare) && int.TryParse(value, out intValueToCompareTo))
                    {
                        parsedValueToCompare = intValueToCompare;
                        parsedValueToCompareTo = intValueToCompareTo;
                    }
                    else if (long.TryParse(valueToCompare, out longValueToCompare) && long.TryParse(value, out longValueToCompareTo))
                    {
                        parsedValueToCompare = longValueToCompare;
                        parsedValueToCompareTo = longValueToCompareTo;
                    }
                    else if (decimal.TryParse(valueToCompare, out decimalValueToCompare) && decimal.TryParse(value, out decimalValueToCompareTo))
                    {
                        parsedValueToCompare = decimalValueToCompare;
                        parsedValueToCompareTo = decimalValueToCompareTo;
                    }
                    else if (float.TryParse(valueToCompare, out floatValueToCompare) && float.TryParse(value, out floatValueToCompareTo))
                    {
                        parsedValueToCompare = floatValueToCompare;
                        parsedValueToCompareTo = floatValueToCompareTo;
                    }
                    else if (double.TryParse(valueToCompare, out doubleValueToCompare) && double.TryParse(value, out doubleValueToCompareTo))
                    {
                        parsedValueToCompare = floatValueToCompare;
                        parsedValueToCompareTo = floatValueToCompareTo;
                    }

                    switch (numericOperator)
                    {
                        case NumericOperator.Equal:
                            if (parsedValueToCompare == parsedValueToCompareTo)
                                return true;
                            break;
                        case NumericOperator.Greater:
                            if (parsedValueToCompare > parsedValueToCompareTo)
                                return true;
                            break;
                        case NumericOperator.GreaterEqual:
                            if (parsedValueToCompare >= parsedValueToCompareTo)
                                return true;
                            break;
                        case NumericOperator.Smaller:
                            if (parsedValueToCompare < parsedValueToCompareTo)
                                return true;
                            break;
                        case NumericOperator.SmallerEqual:
                            if (parsedValueToCompare <= parsedValueToCompareTo)
                                return true;
                            break;
                        default:
                            return false;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return false;
            }
        }

        /// <summary>
        /// Compare values based on the specified boolean operator
        /// </summary>
        /// <param name="booleanOperator">The boolean operator to use in the validation.</param>
        /// <param name="valueToCompare">The value to compare.</param>
        /// <returns>True if successfull</returns>
        public bool CompareBooleanValues(BooleanOperator booleanOperator, string valueToCompare)
        {
            try
            {
                bool parsedValueToCompare;

                if (!bool.TryParse(valueToCompare, out parsedValueToCompare))
                    return false;

                switch (booleanOperator)
                {
                    case BooleanOperator.True:
                        return parsedValueToCompare == true ? true : false;
                    case BooleanOperator.False:
                        return parsedValueToCompare == true ? true : false;
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return false;
            }
        }
    }
}
