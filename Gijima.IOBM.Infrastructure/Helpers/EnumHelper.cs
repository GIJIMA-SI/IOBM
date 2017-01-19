using Gijima.IOBM.Infrastructure.Structs;
using System;
using System.ComponentModel;
using System.Reflection;

namespace Gijima.IOBM.Infrastructure.Helpers
{
    public static class EnumHelper
    {
        /// <summary>
        /// Retrieve the description on the enum, e.g.
        /// [Description("Bright Pink")]
        /// BrightPink = 2,
        /// Then when you pass in the enum, it will retrieve the description
        /// </summary>
        /// <param name="en">The Enumeration</param>
        /// <returns>A string representing the friendly name</returns>
        public static string GetDescriptionFromEnum(Enum en)
        {
            Type type = en.GetType();

            MemberInfo[] memInfo = type.GetMember(en.ToString());

            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs != null && attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            return en.ToString();
        }

        /// <summary>
        /// Retrieve the enum from the description on the enum, e.g.
        /// [Description("Bright Pink")]
        /// BrightPink = 2,
        /// Then when you pass in the decription, it will retrieve the enum
        /// </summary>
        /// <param name="en">The Enumeration</param>
        /// <returns>A string representing t
        public static T GetEnumFromDescription<T>(string description)
        {
            var type = typeof(T);

            if (!type.IsEnum) throw new InvalidOperationException();

            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

                if (attribute != null)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }

            throw new ArgumentException("Not found.", "description");
        }

        /// <summary>
        /// Retrieve the name on the relevant operator, e.g.
        /// string, date, numeric, bool, integer
        /// </summary>
        /// <param name="enDataType">The data type Enumerator</param>
        /// <param name="operatorEnumValue">The operator enum value.</param>
        /// <returns>A string representing the friendly name</returns>
        public static string GetOperatorFromDataTypeEnum(DataTypeName enDataType, short operatorEnumValue)
        {
            switch (enDataType)
            {
                case DataTypeName.None:
                    return "Equal";
                case DataTypeName.String:
                    return ((StringOperator)operatorEnumValue).ToString();
                case DataTypeName.DateTime:
                    return ((DateOperator)operatorEnumValue).ToString();
                default:
                    return ((NumericOperator)operatorEnumValue).ToString();
            }
        }

        /// <summary>
        /// Retrieve the name on the relevant operator, e.g.
        /// string, date, numeric, bool, integer
        /// </summary>
        /// <param name="enOperatorType">The operator type Enumerator</param>
        /// <param name="operatorEnumValue">The operator enum value.</param>
        /// <returns>A string representing the friendly name</returns>
        public static string GetOperatorFromOperatorTypeEnum(OperatorType enOperatorType, short operatorEnumValue)
        {
            switch (enOperatorType)
            {
                case OperatorType.StringOperator:
                    return ((StringOperator)operatorEnumValue).ToString();
                case OperatorType.NumericOperator:
                    return ((NumericOperator)operatorEnumValue).ToString();
                case OperatorType.DateOperator:
                    return ((DateOperator)operatorEnumValue).ToString();
                case OperatorType.BooleanOperator:
                    return ((BooleanOperator)operatorEnumValue).ToString();
                default:
                    return StringOperator.Equal.ToString();
            }
        }
    }
}
