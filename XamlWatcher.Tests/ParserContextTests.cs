using System.Xml.Linq;
using FluentAssertions;
using NUnit.Framework;
using XamlWatcher.WPF;

namespace XamlWatcher.Tests
{
    class ParserContextTests
    {
        [Test]
        public void BuilderMustBeAbleToParseLocalTypes()
        {
            var source = XDocument.Parse(Data.SourceXaml);
            var context = ParserContextBuilder.Build(source, GetType()); //normally is the name of the view so we can grab it's assembly

            var type = context.XamlTypeMapper.GetType("clr-namespace:XamlWatcher.Tests", "LocalButton");
            type.Should().Be(typeof (LocalButton));
        }
    }
}