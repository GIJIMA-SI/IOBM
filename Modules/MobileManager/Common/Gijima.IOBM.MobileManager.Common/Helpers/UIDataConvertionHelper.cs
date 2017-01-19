using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Structs;
using System;
using System.Windows.Data;
using System.Windows.Media;

namespace Gijima.IOBM.MobileManager.Common.Helpers
{
    public class UIDataConvertionHelper : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string val = "";
            String direction = parameter as string;

            if (value != null)
            {
                if (value.GetType() == typeof(System.Boolean))
                    val = bool.Parse(value.ToString()) == true ? "1" : "0";
                else
                    val = value.ToString();
            }

            if (direction == "State")
            {
                return val == "1" ? "Active" : "In-Active";
            }

            if (direction == "StatusLink")
            {
                switch ((StatusLink)Enum.Parse(typeof(StatusLink), val))
                {
                    case StatusLink.Contract:
                        return StatusLink.Contract.ToString();
                    case StatusLink.Device:
                        return StatusLink.Device.ToString();
                    case StatusLink.Sim:
                        return StatusLink.Sim.ToString();
                    case StatusLink.ContractDevice:
                        return StatusLink.ContractDevice.ToString();
                    case StatusLink.ContractSim:
                        return StatusLink.ContractSim.ToString();
                    case StatusLink.DeviceSim:
                        return StatusLink.DeviceSim.ToString();
                    default:
                        return StatusLink.All.ToString(); ;
                }
            }

            if (direction == "PackageType")
            {
                switch ((PackageType)Enum.Parse(typeof(PackageType), val))
                {
                    case PackageType.VOICE:
                        return PackageType.VOICE.ToString();
                    case PackageType.DATA:
                        return PackageType.DATA.ToString();
                    default:
                        return PackageType.DATA.ToString();
                }
            }

            if (direction == "StringCompareType")
            {
                return ((StringOperator)Enum.Parse(typeof(StringOperator), val)).ToString();
            }

            if (direction == "NumericCompareType")
            {
                return ((NumericOperator)Enum.Parse(typeof(NumericOperator), val)).ToString();
            }

            if (direction == "DateCompareType")
            {
                return ((DateOperator)Enum.Parse(typeof(DateOperator), val)).ToString();
            }
            
            if (direction == "BoolToYesNo")
            {
                return val == "1" ? "Yes" : "No";
            }

            if (direction == "Foreground")
            {
                return val == "1" ? Brushes.Green : Brushes.Black;
            }

            if (direction == "ProcessResult")
            {
                if (val.Length > 0)
                    return val == "1" ? "Passed" : "Failed";
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static DataTypeName ConvertStringToDataType(string dataString)
        {
            if (UIHelper.IsNumeric(dataString))
                return DataTypeName.Integer;
            else if (UIHelper.IsDecimal(dataString))
                return DataTypeName.Decimal;
            else if (UIHelper.IsFloat(dataString))
                return DataTypeName.Float;
            else if (UIHelper.IsDateTime(dataString))
                return DataTypeName.DateTime;
            else
                return DataTypeName.String;
        }

        public static string ConvertStringToCellNumber(string dataString)
        {
            // Test for starting with +27
            if (dataString.Length == 12 && dataString.Substring(3) == "+27")
                return dataString.Replace("+27", "0");
            // Test for starting with 27
            if (dataString.Length == 11 && dataString.Substring(2) == "27")
                return dataString.Replace("27", "0");
            // Valid number
            if (dataString.Length == 10 && dataString.Substring(1) == "0")
                return dataString;
            // Test for NO starting 0
            if (dataString.Length == 9)
                return string.Format("0{0}", dataString);

            return dataString;
        }
    }
}
