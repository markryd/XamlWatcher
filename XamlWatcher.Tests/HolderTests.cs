using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Linq;
using NUnit.Framework;
using FluentAssertions;
using XamlWatcher.WPF;

namespace XamlWatcher.Tests
{
    public class HolderTests
    {
        [Test]
        public void WeShouldBeAbleToHandleEverything()
        {
            var holder = HolderBuilder.Build(XDocument.Parse(Data.XamlWithEverything));

            holder.Document.ToString(); //make sure this doesn't throw
        }

        [Test]
        public void ResourcesShouldBeCopied()
        {
            var holder = HolderBuilder.Build(XDocument.Parse(Data.XamlWithResources));

            holder.Document.ToString(); //make sure this doesn't throw
            var holderResources = holder.Document.Root.Elements().Single(z => z.Name.LocalName == "ContentControl.Resources");
            holderResources.Elements().Count().Should().Be(2);
        }

        [Test]
        public void ContentShouldBeCopied()
        {
            var holder = HolderBuilder.Build(XDocument.Parse(Data.XamlWithContent));

            holder.Document.Root.Elements().FirstOrDefault(x => x.Name.LocalName == "Grid").Should().NotBeNull();
        }

        [Test]
        public void NamspacedContentShouldBeCopied()
        {
            var holder = HolderBuilder.Build(XDocument.Parse(Data.XamlWithNamespacedContent));

            holder.Document.Root.Elements().FirstOrDefault(x => x.Name.LocalName == "LocalButton").Should().NotBeNull();
        }

        [Test]
        public void NamespacesShouldBeCopied()
        {
            var holder = HolderBuilder.Build(XDocument.Parse(Data.XamlWithNamespaces));

            holder.Document.Root.Attributes().Count().Should().Be(3);
            holder.Document.Root.Attributes().All(x => x.IsNamespaceDeclaration).Should().BeTrue();
        }
    }
}
