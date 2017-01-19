using System;
using System.ComponentModel;
using System.Globalization;

namespace Gijima.IOBM.Infrastructure.Helpers
{
    public static class UnitConvertionHelper
    {
        public enum UnitTypes
        {
            Cm,
            In,
            Mm,
            Px
        }

        [TypeConverter(typeof(UnitConverter))]
        public struct Unit
        {
            public Unit(double value, UnitTypes type)
            {
                this._value = value;

                this._type = type;
            }

            private double _value;
            public double Value
            {
                get { return _value; }
                set { _value = value; }
            }

            private UnitTypes _type;
            public UnitTypes Type
            {
                get { return _type; }
                set
                {
                    this.Value = this.To(value).Value;

                    _type = value;
                }
            }

            public double GetPixelPer(UnitTypes unitType)
            {
                switch (unitType)
                {
                    case UnitTypes.Cm:

                        return this.GetPixelPer(UnitTypes.In) / 2.54F;
                    case UnitTypes.In:

                        return 96;
                    case UnitTypes.Mm:

                        return this.GetPixelPer(UnitTypes.Cm) / 10;
                    default:

                        return 1;
                }
            }

            public Unit To(UnitTypes unitType)
            {
                return new Unit((this.Value * this.GetPixelPer(this.Type)) / this.GetPixelPer(unitType), unitType);
            }

            public static Unit operator +(Unit a, Unit b)
            {
                return new Unit(a.To(UnitTypes.Px) + b.To(UnitTypes.Px), UnitTypes.Px);
            }

            public static Unit operator -(Unit a, Unit b)
            {
                return new Unit(a.To(UnitTypes.Px) - b.To(UnitTypes.Px), UnitTypes.Px);
            }

            public static Unit operator *(Unit a, Unit b)
            {
                return new Unit(a.To(UnitTypes.Px) * b.To(UnitTypes.Px), UnitTypes.Px);
            }

            public static Unit operator /(Unit a, Unit b)
            {
                return new Unit(a.To(UnitTypes.Px) / b.To(UnitTypes.Px), UnitTypes.Px);
            }

            public static implicit operator double(Unit u)
            {
                return u.To(UnitTypes.Px);
            }

            public override string ToString()
            {
                return string.Format("{0} {1}", this.Value.ToString(), this.Type.ToString());
            }
        }

        public class UnitConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string))

                    return true;

                return CanConvertFrom(context, sourceType);
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (destinationType == typeof(string))

                    return true;

                return CanConvertTo(context, destinationType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value is string)
                {
                    string unitString = (value as string);

                    double unitValue = -1;

                    if (double.TryParse(unitString.Remove(unitString.Length - 2, 2), out unitValue))

                        return new Unit(unitValue, (UnitTypes)Enum.Parse(typeof(UnitTypes), unitString.Substring(unitString.Length - 2, 2), true));

                    else throw new Exception("Property value is not correct. ( { ±5.0 × 10−324 to ±1.7 × 10308 } { Cm | Px | In | Mm } )");
                }

                return ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))

                    return value.ToString();

                else return ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}
