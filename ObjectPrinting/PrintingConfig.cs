using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly Type[] finalTypes =
        [
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid)
        ];

        private readonly List<Type> excludedTypes = [];
        private readonly List<PropertyInfo> excludedProperties = [];

        private readonly Dictionary<Type, Func<object, string>> typeSerializers = new();
        private readonly Dictionary<PropertyInfo, Func<object, string>> propertySerializers = new();

        private readonly Dictionary<PropertyInfo, int> propertyMaxLengths = new();

        private CultureInfo Culture = CultureInfo.CurrentCulture;

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        public PrintingConfig<TOwner> Exclude<T>()
        {
            excludedTypes.Add(typeof(T));
            return this;
        }

        public PrintingConfig<TOwner> Exclude<TProperty>(Expression<Func<TOwner, TProperty>> expression)
        {
            var property = expression.Body as MemberExpression;
            var propertyInfo = property?.Member as PropertyInfo;
            excludedProperties.Add(propertyInfo);
            return this;
        }

        public PrintingConfig<TOwner> SetCulture(CultureInfo culture)
        {
            Culture = culture;
            return this;
        }

        public PrintingConfig<TOwner> AddSerializer<T>(Func<object, string> serializer)
        {
            typeSerializers.Add(typeof(T), serializer);
            return this;
        }

        public PrintingConfig<TOwner> AddSerializer<TProperty>(Expression<Func<TOwner, TProperty>> expression,
            Func<object, string> serializer)
        {
            var property = expression.Body as MemberExpression;
            var propertyInfo = property?.Member as PropertyInfo;
            propertySerializers.Add(propertyInfo, serializer);
            return this;
        }

        public PrintingConfig<TOwner> Trim(Expression<Func<TOwner, string>> expression, int maxLength)
        {
            var property = expression.Body as MemberExpression;
            var propertyInfo = property?.Member as PropertyInfo;
            propertyMaxLengths.Add(propertyInfo, maxLength);
            return this;
        }

        private string? ProcessProperty(PropertyInfo propertyInfo, object obj, int nestingLevel)
        {
            if (excludedTypes.Contains(propertyInfo.PropertyType) ||
                excludedProperties.Contains(propertyInfo))
            {
                return null;
            }

            if (propertySerializers.ContainsKey(propertyInfo))
            {
                return propertySerializers[propertyInfo](obj);
            }

            if (typeSerializers.ContainsKey(propertyInfo.PropertyType))
            {
                return typeSerializers[propertyInfo.PropertyType](obj);
            }

            if (propertyMaxLengths.ContainsKey(propertyInfo))
            {
                return propertyInfo.GetValue(obj).ToString()[..propertyMaxLengths[propertyInfo]] + Environment.NewLine;
            }

            return PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                var propertyResult = ProcessProperty(propertyInfo, obj, nestingLevel);
                if (propertyResult is null) continue;
                sb.Append(identation + propertyInfo.Name + " = " + propertyResult);
            }

            return sb.ToString();
        }
    }
}