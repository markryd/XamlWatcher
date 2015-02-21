using System;
using System.Linq;
using System.Windows.Markup;
using System.Xml.Linq;

namespace XamlWatcher.WPF
{
    public static class ParserContextBuilder
    {
        public static ParserContext Build(XDocument xml, Type type)
        {
            var context = new ParserContext { XamlTypeMapper = new XamlTypeMapper(new string[] { }) };
            foreach (var att in xml.Root.Attributes().Where(s => s.IsNamespaceDeclaration))
            {
                //any namespace with no assemebly (ie, they are in the view's assembly)
                //must be added to the XamlTypeMapper
                if (att.ToString().Contains("clr-namespace") && !att.ToString().Contains(";assembly="))
                {
                    //namespaces look like: xmlns:tools="clr-namespace:NextGen.Client.Features.Shared.Tools"
                    var s = att.ToString();
                    var nameSpace = s.Split(new[] { ':', '"' }, StringSplitOptions.RemoveEmptyEntries).Last();
                    var alias = s.Split(new[] { '"' }, StringSplitOptions.RemoveEmptyEntries).Last();
                    context.XamlTypeMapper.AddMappingProcessingInstruction(alias, nameSpace, type.Assembly.FullName);
                }
            }
            return context;
        }
    }
}