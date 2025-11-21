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
        private List<Type> excludedTypes = [];
        private List<PropertyInfo> excludedProperties = [];

        private Dictionary<Type, Func<IReflect, string>> typeSerializers = new();
        private Dictionary<PropertyInfo, Func<IReflect, string>> propertySerializers = new();

        private Dictionary<PropertyInfo, int> propertyMaxLengths = new();

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

        public PrintingConfig<TOwner> AddSerializer<T>(Func<IReflect, string> serializer)
        {
            typeSerializers.Add(typeof(T), serializer);
            return this;
        }

        public PrintingConfig<TOwner> AddSerializer<TProperty>(Expression<Func<TOwner, TProperty>> expression,
            Func<IReflect, string> serializer)
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

        private string PrintToString(object obj, int nestingLevel)
        {
            //TODO apply configurations
            if (obj == null)
                return "null" + Environment.NewLine;

            var finalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float), typeof(string),
                typeof(DateTime), typeof(TimeSpan), typeof(Guid)
            };
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedTypes.Contains(propertyInfo.PropertyType) ||
                    excludedProperties.Contains(propertyInfo))
                    continue;
                sb.Append(identation + propertyInfo.Name + " = " +
                          PrintToString(propertyInfo.GetValue(obj),
                              nestingLevel + 1));
            }

            return sb.ToString();
        }
    }
}