using System;
using System.ComponentModel;
using System.Reflection;

namespace RPGTest.Helpers
{
    public static class EnumExtensions
    {
        public static string GetName(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());

            NameAttribute attribute
                    = Attribute.GetCustomAttribute(field, typeof(NameAttribute))
                        as NameAttribute;

            return attribute != null ? attribute.Name : value.ToString();
        }

        public static string GetShortName(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());

            ShortNameAttribute attribute
                    = Attribute.GetCustomAttribute(field, typeof(ShortNameAttribute))
                        as ShortNameAttribute;

            if (attribute == null)
            {
                return GetName(value);
            }

            return attribute.ShortName;
        }

        public static string GetDescription(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());

            DescriptionAttribute attribute
                    = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute))
                        as DescriptionAttribute;

            return attribute != null ? attribute.Description : value.ToString();
        }
    }
}
