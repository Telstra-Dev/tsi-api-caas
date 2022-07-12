using System;
using System.Reflection;

namespace Telstra.Common
{
    public static class ReflectionExtensions
    {
        public static bool IsBool(this PropertyInfo prop)
        {
            return prop.PropertyType.Equals(typeof(bool)) || prop.PropertyType.Equals(typeof(bool?));
        }

        public static bool IsString(this PropertyInfo prop)
        {
            return prop.PropertyType.Equals(typeof(string));
        }

        public static bool IsDecimal(this PropertyInfo prop)
        {
            return prop.PropertyType.Equals(typeof(decimal)) || prop.PropertyType.Equals(typeof(decimal?));
        }

        public static bool IsInt(this PropertyInfo prop)
        {
            return prop.PropertyType.Equals(typeof(int)) || prop.PropertyType.Equals(typeof(int?));
        }

        public static bool IsDouble(this PropertyInfo prop)
        {
            return prop.PropertyType.Equals(typeof(double)) || prop.PropertyType.Equals(typeof(double?));
        }
    }
}
