using System.Windows.Controls;

namespace XamlWatcher.Tests
{
    public static class Data
    {
        public const string XamlWithEverything = @"<Window x:Class=""WPF.MainWindow""
            xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
            xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
            xmlns:local=""clr-namespace:XamlWatcher.Tests"">
            <!--Comment-->
            <Window.Resources>
                <Style TargetType=""Button"" x:Key=""First""/>
                <Style TargetType=""Button"" x:Key=""Second""/>
            </Window.Resources>
            <Grid>
                <local:LocalButton/>   
            </Grid>
        </Window>";

        public const string XamlWithNamespaces = @"<Window x:Class=""WPF.MainWindow""
            xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
            xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
            xmlns:local=""clr-namespace:XamlWatcher.Tests"">
        </Window>";

        public const string XamlWithResources = @"<Window x:Class=""WPF.MainWindow""
            xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
            xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
            Title=""MainWindow"" Height=""350"" Width=""525"">
            <Window.Resources>
                <Style TargetType=""Button"" x:Key=""First""/>
                <Style TargetType=""Button"" x:Key=""Second""/>
            </Window.Resources>
        </Window>";

        public const string XamlWithContent = @"<Window x:Class=""WPF.MainWindow""
            xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
            xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
            <Grid/>
        </Window>";

        public const string XamlWithNamespacedContent = @"<Window x:Class=""WPF.MainWindow""
            xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
            xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
            xmlns:local=""clr-namespace:XamlWatcher.Tests"">
            <local:LocalButton/>   
        </Window>";
    }

    public class LocalButton : Button
    { }
}