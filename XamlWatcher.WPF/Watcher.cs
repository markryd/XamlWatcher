using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml.Linq;

namespace XamlWatcher.WPF
{
    public class Watcher
    {
        public Watcher(string path)
        {
            var watcher = new FileSystemWatcher(path)
            {
                Filter = Path.GetFileName("*.xaml"),
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };
            watcher.Changed += watcher_Changed;
        }

        public Action<Exception> OnError { get; set; }

        public Action<ContentControl> OnRefreshed { get; set; }

        public Action<string> OnLog { get; set; }

        private void Log(string message)
        {
            if (OnLog == null)
                return;

            OnLog(message);
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Log(String.Format("File changed {0}", e.FullPath));
            var d = Application.Current.Dispatcher;
            if (d.CheckAccess())
                try
                {
                    SetNewContent(e.FullPath);
                }
                catch (Exception ex)
                {
                    if (OnError != null)
                        OnError(ex);
                }
            else
                d.BeginInvoke((Action)(() =>
                {
                    try
                    {
                        SetNewContent(e.FullPath);
                    }
                    catch (Exception ex)
                    {
                        if (OnError != null)
                            OnError(ex);
                    }
                }));
        }


        private void SetNewContent(string path)
        {
            Thread.Sleep(100); //give things a little time to finish saving, although we'll wait in the read method

            Log(String.Format("Reading XAML from {0}", path));

            var xml = ReadXaml(path);

            Log("Getting view type");

            var type = GetViewType(xml);

            if (type != null)
            {
                Log(String.Format("Processing view type {0}", type.FullName));
                ProcessView(xml, type);
            }

            if (xml.Root.Name.LocalName == "ResourceDictionary")
            {
                Log("Processing Resource Dictionary");
                ProcessResourceDictionary(xml);
            }
        }

        private void ProcessResourceDictionary(XDocument xml)
        {
            var resourceDictionary = (ResourceDictionary)XamlReader.Parse(xml.ToString());
            Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
        }

        private void ProcessView(XDocument xml, Type type)
        {
            Log("Getting context");
            var context = GetContextWithTypeMappings(xml, type);

            Log("Building holder");
            var holder = BuildHolderDocumentWithNamespaces(xml);

            Log("Adding resources to holder");
            AddResourcesToHolder(xml, holder);

            Log("Adding view contents to holder");
            AddViewContentsToHolder(xml, holder);

            Log("Updating instances of view");
            UpdateLiveInstancesOfTheView(type, holder, context);
        }

        private static void AddViewContentsToHolder(XDocument xml, XDocument holder)
        {
            //.Last is pretty dodgy...
            holder.Root.Add(xml.Root.Elements().Last());
        }

        private void UpdateLiveInstancesOfTheView(Type type, XDocument holder, ParserContext context)
        {
            foreach (var tb in FindVisualChildren(type, Application.Current.MainWindow).OfType<ContentControl>())
            {
                Stream stream = new MemoryStream(); // Create a stream
                holder.Save(stream); // Save XDocument into the stream
                stream.Position = 0; // Rewind the stream ready to read from it elsewhere

                var content = XamlReader.Load(stream, context) as DependencyObject;
                tb.Content = content;

                Log("Updating instance of view");
                tb.UpdateLayout();

                if (OnRefreshed != null)
                    OnRefreshed(tb);
            }
        }

        private static void AddResourcesToHolder(XDocument xml, XDocument holder)
        {
            var resource = xml.Root.Elements().SingleOrDefault(z => z.Name.LocalName.EndsWith(".Resources"));
            if (resource != null)
            {
                var targetResource = holder.Root.Elements().Single(z => z.Name.LocalName.EndsWith(".Resources"));
                foreach (var element in resource.Elements())
                {
                    targetResource.Add(element);
                }
            }
        }

        private static XDocument BuildHolderDocumentWithNamespaces(XDocument xml)
        {
            var sb = new StringBuilder();
            sb.Append("<ContentControl ");

            foreach (var att in xml.Root.Attributes().Where(s => s.IsNamespaceDeclaration))
            {
                sb.Append(" ");
                sb.Append(att);
            }

            sb.Append(" ><ContentControl.Resources></ContentControl.Resources></ContentControl>");

            var holder = XDocument.Parse(sb.ToString());
            return holder;
        }

        private static ParserContext GetContextWithTypeMappings(XDocument xml, Type type)
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

        private Type GetViewType(XDocument xml)
        {
            XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
            var className = xml.Root.Attributes(x + "Class").SingleOrDefault();

            return className == null ? null : FindType(className.Value);
        }

        private static Type FindType(string fullName)
        {
            return
                AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic)
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.FullName.Equals(fullName));
        }

        private XDocument ReadXaml(string path)
        {
            var read = false;
            var xaml = "";
            var count = 0;
            while (!read)
                try
                {
                    xaml = File.ReadAllText(path);
                    read = true;
                }
                catch (Exception) //if the file is still saving it will be locked, so retry a few times
                {
                    count++;
                    Thread.Sleep(100);
                    if (count > 20)
                    {
                        throw;
                    }
                }

            return XDocument.Parse(xaml);
        }

        private static IEnumerable<DependencyObject> FindVisualChildren(Type type, DependencyObject depObj)
        {
            if (depObj == null) yield break;

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                if (child != null && child.GetType() == type)
                {
                    yield return child;
                }

                foreach (var childOfChild in FindVisualChildren(type, child))
                {
                    yield return childOfChild;
                }
            }
        }
    }
}
