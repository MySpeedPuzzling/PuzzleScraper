using System.ComponentModel.DataAnnotations;

namespace Arctic.Puzzlers.Objects.Misc
{
    public static class EnumExtensions
    {
        public static T? GetAttributeOfType<T>(this Enum enumVal) where T : Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return attributes.Length > 0 ? (T)attributes[0] : null;
        }

        public static T GetValueFromShortName<T>(this string name) where T : Enum
        {
            var type = typeof(T);

            foreach (var field in type.GetFields())
            {
                if (Attribute.GetCustomAttribute(field, typeof(DisplayAttribute)) is DisplayAttribute attribute)
                {
                    if (attribute.ShortName == name)
                    {
                        return (T)field.GetValue(null);
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(name));
        }
        public static T GetEnumFromString<T>(this string name) where T : Enum
        {
            var type = typeof(T);
            if(Enum.TryParse(type,name,true, out object? enumResponse))
            {
                return (T)enumResponse;
            }
            foreach (var field in type.GetFields())
            {
                if (Attribute.GetCustomAttribute(field, typeof(DisplayAttribute)) is DisplayAttribute attribute)
                {
                    if (attribute.ShortName.ToLower() == name.ToLower())
                    {
                        return (T)field.GetValue(null);
                    }
                    if(attribute.Name.ToLower() == name.ToLower())
                    {
                        return (T)field.GetValue(null);
                    }
                }
            }

            return (T)Enum.Parse(type, "UNK");

        }
    }
}
