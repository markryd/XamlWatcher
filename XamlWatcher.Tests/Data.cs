using System.Windows.Controls;

namespace XamlWatcher.Tests
{
    public static class Data
    {
        public const string SourceXaml = @"<Window x:Class=""WPF.MainWindow""
            xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
            xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
            xmlns:local=""clr-namespace:XamlWatcher.Tests""
            Title=""MainWindow"" Height=""350"" Width=""525"">
            <!--Comment-->
            <Window.Resources>
                <Style TargetType=""Button"" x:Key=""First""/>
                <Style TargetType=""Button"" x:Key=""Second""/>
            </Window.Resources>
            <Grid>
                <local:LocalButton/>   
            </Grid>
        </Window>";
    }

    public class LocalButton : Button
    { }
}