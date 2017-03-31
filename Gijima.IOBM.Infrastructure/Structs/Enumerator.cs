using System;
using System.ComponentModel;

namespace Gijima.IOBM.Infrastructure.Structs
{
    /// <summary>
    /// The <see cref="SolutionApplication"/> enumeration lists of
    /// applications in the IOBM solution.
    /// </summary>
    public enum SolutionApplication
    {
        BillingEngine = 1,
        MobileManager = 2
    }

    /// <summary>
    /// The <see cref="IOBMSecurityRole"/> enumeration lists of
    /// application security roles.
    /// </summary>
    public enum IOBMSecurityRole
    {
        Administrator = 1,
        Supervisor = 2,
        User = 3
    }

    /// <summary>
    /// The <see cref="DataTypeName"/> enumeration a list of 
    /// data types to validate on.
    /// </summary>
    public enum DataTypeName
    {
        None = 0,
        String = 1,
        Short = 2,
        Integer = 3,
        Decimal = 4,
        DateTime = 5,
        Bool = 6,
        Long = 7,
        Float = 8
    }

    /// <summary>
    /// The <see cref="OperatorType"/> enumeration a list of 
    /// operrator types to validate on.
    /// </summary>
    public enum OperatorType
    {
        [Description("None")]
        None = 0,
        [Description("String")]
        StringOperator = 1,
        [Description("Numeric")]
        NumericOperator = 2,
        [Description("Date")]
        DateOperator = 3,
        [Description("Boolean")]
        BooleanOperator = 4,
        [Description("Rule")]
        RuleOperator = 5
    }

    /// <summary>
    /// The <see cref="StringOperator"/> enumeration a list of 
    /// string operator options to validate on.
    /// </summary>
    public enum StringOperator
    {
        None = 0,
        PreFix = 1,
        PostFix,
        Contains,
        Equal,
        Format,
        LengthEqual,
        LengthGreater,
        LenghtSmaller
    }

    /// <summary>
    /// The <see cref="NumericOperator"/> enumeration a list of 
    /// numeric operator options to validate on.
    /// </summary>
    public enum NumericOperator
    {
        None = 0,
        Equal = 1,
        Greater,
        Smaller,
        GreaterEqual,
        SmallerEqual
    }

    /// <summary>
    /// The <see cref="DateOperator"/> enumeration a list of 
    /// date comapre values to validate on.
    /// </summary>
    public enum DateOperator
    {
        None = 0,
        Equal = 1,
        Current,
        Min,
        Max,
        MonthStart,
        MonthEnd
    }

    /// <summary>
    /// The <see cref="MathOperator"/> enumeration a list of 
    /// math operators to validate on.
    /// </summary>
    public enum MathOperator
    {
        None = 0,
        Multiply = 1,
        Devide,
        Add,
        Subtract
    }

    /// <summary>
    /// The <see cref="BooleanOperator"/> enumeration a list of 
    /// boolean operator to validate on.
    /// </summary>
    public enum BooleanOperator
    {
        None = 0,
        True = 1,
        False
    }

    /// <summary>
    /// The <see cref="BooleanOperator"/> enumeration a list of 
    /// boolean operator to validate on.
    /// </summary>
    public enum RuleOperator
    {
        None = 0,
        Duplicate = 1,
        Range,
        Sum
    }

    /// <summary>
    /// The below extension class can be used for all Enum types defined herein.
    /// Please not that you'll have to define each extension method per Enum type
    /// to handle the correct return type.
    /// </summary>
    public static class EnumExtensions
    {
        public static short Value(this SolutionApplication type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this IOBMSecurityRole type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this OperatorType type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this StringOperator type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this NumericOperator type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this DateOperator type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this MathOperator type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this BooleanOperator type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this DataTypeName type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this RuleOperator type)
        {
            return Convert.ToInt16(type);
        }
    }
}
