using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrinterTests
{
    private const bool GenerateExpectedResult = false;

    private static string GetExpectedFilePath() =>
        $"{TestContext.CurrentContext.TestDirectory}/../../../Expected/{TestContext.CurrentContext.Test.MethodName}.txt";

    private static string GetExpectedResult() => File.ReadAllText(GetExpectedFilePath());

    private void AssertOrGenerateExpectedResult(string actual)
    {
        var filePath = GetExpectedFilePath();

        if (GenerateExpectedResult)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            File.WriteAllText(filePath, actual);
            Assert.Inconclusive($"Generated expected result: {filePath}");
        }

        else
        {
            var expected = GetExpectedResult();

            TestContext.WriteLine("Expected: ");
            TestContext.WriteLine(expected);
            TestContext.WriteLine("Actual: ");
            TestContext.WriteLine(actual);

            Assert.That(actual, Is.EqualTo(expected));
        }
    }

    [Test]
    public void ObjectPrinter_ExcludeType()
    {
        var person = new Person
        {
            Name = "Alex",
            Age = 19
        };

        var printer = ObjectPrinter.For<Person>()
            .Exclude<Guid>()
            .Build();

        var actual = printer.PrintToString(person);

        AssertOrGenerateExpectedResult(actual);
    }

    [Test]
    public void ObjectPrinter_ExcludeProperty()
    {
        var person = new Person
        {
            Name = "Alex",
            Age = 19
        };

        var printer = ObjectPrinter.For<Person>()
            .Exclude(p => p.Age)
            .Build();

        var actual = printer.PrintToString(person);

        AssertOrGenerateExpectedResult(actual);
    }

    [Test]
    public void ObjectPrinter_CustomTypeSerializer()
    {
        var person = new Person
        {
            Name = "Alex",
            Surname = "Brown",
            Age = 19,
            Height = 2005
        };

        var printer = ObjectPrinter.For<Person>()
            .AddSerializer<string>(s => $"{s} - string")
            .Build();

        var actual = printer.PrintToString(person);

        AssertOrGenerateExpectedResult(actual);
    }

    [Test]
    public void ObjectPrinter_CustomPropertySerializer()
    {
        var person = new Person
        {
            Name = "Alex",
            Age = 19,
        };

        var printer = ObjectPrinter.For<Person>()
            .AddSerializer(p => p.Age, a => $"{a} years old")
            .Build();

        var actual = printer.PrintToString(person);

        AssertOrGenerateExpectedResult(actual);
    }

    [Test]
    public void ObjectPrinter_TrimProperty()
    {
        var person = new Person
        {
            Name = "Alex",
            Surname = "VeryLongSurnameWeDontWantToSee",
            Age = 19,
        };

        var printer = ObjectPrinter.For<Person>()
            .Trim(p => p.Surname, 10)
            .Build();

        var actual = printer.PrintToString(person);

        AssertOrGenerateExpectedResult(actual);
    }

    [Test]
    public void ObjectPrinter_NoStackOverflow()
    {
        var father = new Person()
        {
            Name = "John",
            Age = 42,
        };
        var person = new Person
        {
            Name = "Alex",
            Age = 19,
            Father = father
        };
        father.Child = person;

        var printer = ObjectPrinter.For<Person>()
            .Build();

        var actual = printer.PrintToString(person);

        AssertOrGenerateExpectedResult(actual);
    }

    [Test]
    public void ObjectPrinter_ICollection()
    {
        var person = new Person
        {
            Name = "Alex",
            Age = 19,
            Tags = ["tag1", "tag2"],
        };

        var printer = ObjectPrinter.For<Person>()
            .Build();

        var actual = printer.PrintToString(person);

        AssertOrGenerateExpectedResult(actual);
    }

    [Test]
    public void ObjectPrinter_IDictionary()
    {
        var person = new Person
        {
            Name = "Alex",
            Age = 19,
            CustomProperties = new Dictionary<string, string>()
                { { "PhoneNumber", "88005553535" }, { "HomeAddress", "Novokoltsovo" } }
        };

        var printer = ObjectPrinter.For<Person>()
            .Build();

        var actual = printer.PrintToString(person);

        AssertOrGenerateExpectedResult(actual);
    }
}