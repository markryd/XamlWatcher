using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using NUnit.Framework;
using FluentAssertions;
using XamlWatcher.WPF;

namespace XamlWatcher.Tests
{
    class Tests
    {
        private string _sourceXaml =
        @"<Window x:Class=""WPF.MainWindow""
            xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
            xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
            xmlns:wpf=""clr-namespace:WPF""
            Title=""MainWindow"" Height=""350"" Width=""525"">
            <TextBlock Text=""Yay""/>
            <!--Comment-->
            <Window.Resources>
                <Style TargetType=""Button"" x:Key=""First""/>
                <Style TargetType=""Button"" x:Key=""Second""/>
            </Window.Resources>
        </Window>";

        private Holder _holder;
        private XDocument _source;

        [SetUp]
        public void SetUp()
        {
            _source = XDocument.Parse(_sourceXaml);
            _holder = new Holder(_source);
        }

        [Test]
        public void HolderDocumentIsAContentControlWithEmptyResourcesAndNamespaces()
        {
            _holder.Document.ToString(); //make sure this doesn't throw
            _holder.Document.Root.Name.LocalName.Should().Be("ContentControl");
            _holder.Document.Root.Elements().Single().Name.LocalName.Should().Be("ContentControl.Resources");

            //check namespaces
            _holder.Document.Root.Attributes().Count().Should().Be(3);
            _holder.Document.Root.Attributes().All(x => x.IsNamespaceDeclaration).Should().BeTrue();
        }

        [Test]
        public void AddResourcesFromShouldCopyAllResources()
        {
            _holder.AddResourcesFrom(_source);

            _holder.Document.ToString(); //make sure this doesn't throw
            var holderResources = _holder.Document.Root.Elements().Single(z => z.Name.LocalName == "ContentControl.Resources");
            holderResources.Elements().Count().Should().Be(2);
        }

        [Test]
        public void AddContentFromShouldAddTheContent()
        {
            _holder.AddContentsFrom(_source);
            _holder.Document.Root.Elements().FirstOrDefault(x => x.Name.LocalName == "TextBlock").Should().NotBeNull();
        }
    }
}
