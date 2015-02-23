using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace XamlWatcher.WPF
{
    static internal class Helpers
    {
        public static Type GetViewType(XDocument xml)
        {
            Logger.Log("Getting view type");
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

        public static XDocument ReadXaml(string path)
        {
            Logger.Log(String.Format("Reading XAML from {0}", path));
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

        public static IEnumerable<DependencyObject> FindVisualChildren(Type type, DependencyObject depObj)
        {
            if (depObj == null) yield break;

            if (depObj.GetType() == type)
                yield return depObj;

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