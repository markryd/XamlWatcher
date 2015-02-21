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
            watcher.Renamed += watcher_Changed;
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

            var xml = Helpers.ReadXaml(path);

            Log("Getting view type");

            var type = Helpers.GetViewType(xml);

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

        private void ProcessView(XDocument xml, Type viewType)
        {
            Log("Getting context");
            var context = ParserContextBuilder.Build(xml, viewType);

            Log("Building holder");
            var holder = new Holder(xml)
                            .AddResourcesFrom(xml)
                            .AddContentsFrom(xml)
                            .Document;

            Log("Updating instances of view");
            UpdateLiveInstancesOfTheView(viewType, holder, context);
        }

        private void UpdateLiveInstancesOfTheView(Type type, XDocument holder, ParserContext context)
        {
            foreach (var tb in Helpers.FindVisualChildren(type, Application.Current.MainWindow).OfType<ContentControl>())
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
    }
}
