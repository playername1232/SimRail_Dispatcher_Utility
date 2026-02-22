using System.Configuration;
using System.Data;
using System.Windows;
using SimRailDispatcherUtility.Services;

namespace SimRailDispatcherUtility
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppConfig.Load();
            base.OnStartup(e);
        }
    }
}
