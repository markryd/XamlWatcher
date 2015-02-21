using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace XamlWatcher.WPF
{
    public class Holder
    {
        private readonly XDocument _document;

        public Holder(XDocument xml)
        {
            var sb = new StringBuilder();
            sb.Append("<ContentControl ");

            foreach (var att in xml.Root.Attributes().Where(s => s.IsNamespaceDeclaration))
            {
                sb.Append(" ");
                sb.Append(att);
            }

            sb.Append(" ><ContentControl.Resources></ContentControl.Resources></ContentControl>");

            _document = XDocument.Parse(sb.ToString());
        }

        public XDocument Document
        {
            get { return _document; }
        }

        public Holder AddResourcesFrom(XDocument xml)
        {
            var resource = xml.Root.Elements().SingleOrDefault(z => z.Name.LocalName.EndsWith(".Resources"));
            if (resource != null)
            {
                var targetResource = _document.Root.Elements().Single(z => z.Name.LocalName == "ContentControl.Resources");
                foreach (var element in resource.Elements())
                {
                    targetResource.Add(element);
                }
            }

            return this;
        }

        public Holder AddContentsFrom(XDocument xml)
        {
            var content = xml.Root.Elements().FirstOrDefault(x => x.Name.LocalName.EndsWith(".Resources") == false);
            if (content != null)
            {
                _document.Root.Add(content);
            }

            return this;
        }
    }
}
