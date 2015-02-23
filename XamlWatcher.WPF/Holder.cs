using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace XamlWatcher.WPF
{
    public static class HolderBuilder
    {
        public static XDocument Build(XDocument xml)
        {
            Logger.Log("Building holder");

            var document = BuildBaseDocument(xml);
            document.AddResourcesFrom(xml);
            document.AddContentsFrom(xml);

            return document;
        }

        private static XDocument BuildBaseDocument(XDocument xml)
        {
            var sb = new StringBuilder();
            sb.Append("<ContentControl ");

            foreach (var att in xml.Root.Attributes().Where(s => s.IsNamespaceDeclaration))
            {
                sb.Append(" ");
                sb.Append(att);
            }

            sb.Append(" ><ContentControl.Resources></ContentControl.Resources></ContentControl>");

            return XDocument.Parse(sb.ToString());
        }

        private static void AddResourcesFrom(this XDocument holder, XDocument xml)
        {
            var resource = xml.Root.Elements().SingleOrDefault(z => z.Name.LocalName.EndsWith(".Resources"));
            if (resource != null)
            {
                var targetResource = holder.Root.Elements().Single(z => z.Name.LocalName == "ContentControl.Resources");
                foreach (var element in resource.Elements())
                {
                    targetResource.Add(element);
                }
            }
        }

        private static void AddContentsFrom(this XDocument holder, XDocument xml)
        {
            var content = xml.Root.Elements().FirstOrDefault(x => x.Name.LocalName.EndsWith(".Resources") == false);
            if (content != null)
            {
                holder.Root.Add(content);
            }
        }
    }
}
