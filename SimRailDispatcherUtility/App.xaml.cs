using System.Net.Http;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimRailDispatcherUtility.Services;
using Microsoft.Extensions.Logging;
using SimRailDispatcherUtility.Windows;

namespace SimRailDispatcherUtility
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            AppConfig.Load();

            base.OnStartup(e);

            var sc = new ServiceCollection();

            RegisterSingletons(sc);
            RegisterWindows(sc);

            Services = sc.BuildServiceProvider();

            var main = Services.GetRequiredService<MainWindow>();
            main.Show();
        }

        private void RegisterWindows(ServiceCollection sc)
        {
            sc.AddSingleton<MainWindow>();
            sc.AddTransient<AddTrainWindow>();
            sc.AddTransient<Func<AddTrainWindow>>(sp => () => sp.GetRequiredService<AddTrainWindow>());
        }

        private void RegisterSingletons(ServiceCollection sc)
        {
            sc.AddLogging(builder =>
            {
                builder.AddDebug();
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            sc.AddSingleton<StationService>();
            sc.AddSingleton<HttpClient>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (Services is IDisposable d)
                d.Dispose();

            base.OnExit(e);
        }
    }
}
