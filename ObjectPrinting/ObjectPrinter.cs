using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting;

public class ObjectPrinter
{
    private readonly PrintingConfig config;
    private readonly HashSet<object> processed = [];

    public static PrintingConfigurator<T> For<T>()
    {
        return new PrintingConfigurator<T>();
    }

    public ObjectPrinter(PrintingConfig config)
    {
        this.config = config;
    }

    public string PrintToString<TOwner>(TOwner obj)
    {
        return PrintToString(obj, 0);
    }

    private string? TryFinishProperty(object obj)
    {
        if (obj == null)
        {
            return "null" + Environment.NewLine;
        }

        if (processed.Contains(obj))
        {
            return "cyclic reference" + Environment.NewLine;
        }

        if (obj.GetType().IsSimple())
        {
            return obj + Environment.NewLine;
        }

        return null;
    }

    private string? ProcessProperty(PropertyInfo propertyInfo, object obj, int nestingLevel)
    {
        foreach (var action in config.Actions.Where(a => a.CanHandle(propertyInfo)))
        {
            var result = action.Handle(propertyInfo.GetValue(obj));
            return result.IsSkipped ? null : result.Value + Environment.NewLine;
        }

        return PrintToString(propertyInfo.GetValue(obj), nestingLevel + 1);
    }

    private string PrintToString(object obj, int nestingLevel)
    {
        CultureInfo.CurrentCulture = config.Culture;

        var possibleResult = TryFinishProperty(obj);
        if (possibleResult != null) return possibleResult;

        processed.Add(obj);

        var tabsCount = new string('\t', nestingLevel + 1);
        var sb = new StringBuilder();
        var type = obj.GetType();
        sb.AppendLine(type.Name);
        foreach (var propertyInfo in type.GetProperties())
        {
            var propertyResult = ProcessProperty(propertyInfo, obj, nestingLevel);
            if (propertyResult is null) continue;
            sb.Append(tabsCount + propertyInfo.Name + " = " + propertyResult);
        }

        return sb.ToString();
    }
}