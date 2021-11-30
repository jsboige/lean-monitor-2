using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;
using Panoptes.Model;
using Panoptes.Model.Sessions;
using Panoptes.Model.Settings;
using Panoptes.Model.Settings.Json;
using Panoptes.Model.Statistics;
using Panoptes.ViewModels;
using Panoptes.ViewModels.Charts;
using Panoptes.ViewModels.NewSession;
using Panoptes.ViewModels.Panels;
using System;
using System.Threading.Tasks;

namespace Panoptes
{
    public class App : Application
    {
        public App()
        {
            Name = Global.AppName;
            System.Threading.Thread.CurrentThread.Name = $"{Global.AppName} UI Thread";
            Services = ConfigureServices();
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override async void OnFrameworkInitializationCompleted()
        {
            base.OnFrameworkInitializationCompleted();

            try
            {
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                    await LoadSplashScreen().ConfigureAwait(true);

                    desktop.MainWindow = new MainWindow();
                    desktop.ShutdownMode = ShutdownMode.OnLastWindowClose;
                    desktop.MainWindow.Show();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task LoadSplashScreen()
        {
            // Show splash screen
            var splashScreen = new Views.Windows.SplashScreenWindow();
            splashScreen.Show();

            // Wait 1 second to make sure the splash screen is visible
#if DEBUG
            await Task.Delay(10_000).ConfigureAwait(true);
#else
            await Task.Delay(1_000).ConfigureAwait(true);
#endif
            // We need to get user setting before loading the UI
            await ((ISettingsManager)Services.GetService(typeof(ISettingsManager))).InitialiseAsync().ConfigureAwait(true);

            splashScreen.Close();
        }

        /// <summary>
        /// Gets the current <see cref="App"/> instance in use.
        /// </summary>
        public new static App Current => (App)Application.Current;

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public IServiceProvider Services { get; }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Messenger
            services.AddSingleton<IMessenger, WeakReferenceMessenger>();

            // Settings
            services.AddSingleton<ISettingsManager, JsonSettingsManager>();

            // Results
            services.AddSingleton<IResultConverter, ResultConverter>();
            services.AddSingleton<IResultSerializer, AdvancedResultSerializer>();
            services.AddSingleton<IResultMutator, BenchmarkResultMutator>();
            services.AddSingleton<IStatisticsFormatter, StatisticsFormatter>();

            // Session
            services.AddSingleton<ISessionService, SessionService>(); //SessionService>();

            // Api
            //For<IApiClient>().Singleton().Use<ApiClient>();

            // Sessions
            services.AddSingleton<INewSessionViewModel, NewStreamSessionViewModel>();
            services.AddSingleton<INewSessionViewModel, NewMongoSessionViewModel>();
            services.AddSingleton<INewSessionViewModel, NewFileSessionViewModel>();

            // Viewmodels
            services.AddTransient<StatusViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<NewSessionWindowViewModel>();
            //services.AddTransient<AboutWindowViewModel>();
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<StatisticsPanelViewModel>();
            services.AddTransient<RuntimeStatisticsPanelViewModel>();
            services.AddTransient<TradesPanelViewModel>();
            services.AddTransient<HoldingsPanelViewModel>();
            services.AddTransient<CashBookPanelViewModel>();
            services.AddTransient<ProfitLossPanelViewModel>();
            services.AddTransient<LogPanelViewModel>();
            services.AddTransient<OxyPlotSelectionViewModel>();

            //services.AddTransient<ToolPaneViewModel>(); // abstract

            return services.BuildServiceProvider();
        }
    }
}
