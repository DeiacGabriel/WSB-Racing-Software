using System.ComponentModel;
using System.Globalization;
using WSB_Management.Models;

namespace WSB_Management.Converter
{
    public sealed class BrandTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? ctx, Type srcType)
            => srcType == typeof(string) || base.CanConvertFrom(ctx, srcType);

        public override bool CanConvertTo(ITypeDescriptorContext? ctx, Type? destType)
            => destType == typeof(string) || base.CanConvertTo(ctx, destType);

        public override object? ConvertFrom(ITypeDescriptorContext? ctx, CultureInfo? cul, object value)
        {
            if (value is null) return null;
            if (value is string s)
            {
                s = s.Trim();
                if (s.Length == 0) return null;

                if (long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
                    return new Brand { Id = id };

                throw new FormatException($"'{s}' ist keine gültige Brand-Id.");
            }
            return base.ConvertFrom(ctx, cul, value);
        }

        public override object? ConvertTo(ITypeDescriptorContext? ctx, CultureInfo? cul, object? value, Type destType)
        {
            if (destType == typeof(string))
            {
                if (value is null) return string.Empty;
                if (value is Brand b) return b.Id.ToString(CultureInfo.InvariantCulture);
            }
            return base.ConvertTo(ctx, cul, value, destType);
        }
    }
}
