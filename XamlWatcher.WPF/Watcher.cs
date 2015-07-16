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

        public Action<Exception> OnError { private get; set; }

        public Action<ContentControl> OnRefreshed { private get; set; }

        public Action<string> OnLog
        {
            set { Logger.OnLog = value; }
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Logger.Log(String.Format("File changed {0}", e.FullPath));
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

            var xml = Helpers.ReadXaml(path);

            var type = Helpers.GetViewType(xml);

            if (type != null)
            {
                ProcessView(xml, type);
            }

            if (xml.Root.Name.LocalName == "ResourceDictionary")
            {
                Logger.Log("Processing Resource Dictionary");
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
            Logger.Log(String.Format("Processing view type {0}", viewType.FullName));

            var context = ParserContextBuilder.Build(xml, viewType);

            var holder = HolderBuilder.Build(xml);

            UpdateLiveInstancesOfTheView(viewType, holder, context);
        }

        private void UpdateLiveInstancesOfTheView(Type type, XDocument holder, ParserContext context)
        {
            Logger.Log("Updating instances of view");
            foreach (var window in Application.Current.Windows.OfType<Window>())
            {
                foreach (var tb in Helpers.FindVisualChildren(type, window).OfType<ContentControl>())
                {
                    var stream = new MemoryStream(); // Create a stream
                    holder.Save(stream); // Save XDocument into the stream
                    stream.Position = 0; // Rewind the stream ready to read from it elsewhere

                    var content = XamlReader.Load(stream, context) as DependencyObject;
                    tb.Content = content;

                    Logger.Log("Updating instance of view");
                    tb.UpdateLayout();

                    if (OnRefreshed != null)
                        OnRefreshed(tb);
                }
            }
        }
    }

    internal static class Logger
    {
        internal static Action<string> OnLog { get; set; }

        internal static void Log(string message, params object[] parameters)
        {
            if (OnLog == null)
                return;

            var s = String.Format(message, parameters);
            OnLog(s);
        }
    }
}
