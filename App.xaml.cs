using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using NuGetSwitch.Service;
using NuGetSwitch.ViewModel;

namespace NuGetSwitch
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs" /> that contains the event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            IServiceProvider services = ConfigureServices();

            MainWindow mainWindow = services.GetRequiredService<MainWindow>();
            MainWindowViewModel mainWindowViewModel = services.GetRequiredService<MainWindowViewModel>();

            // Hook up manually since there's a cyclic dependency between MainWindow and
            // MainWindowViewModel, due to the IDialogService interface
            mainWindow.DataContext = mainWindowViewModel;

            MainWindow = mainWindow;
            mainWindow.Show();
        }

        
        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        private static IServiceProvider ConfigureServices()
        {
            ServiceCollection services = new ServiceCollection();

            // Services
            services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
            services.AddSingleton<StorageService>();
            services.AddSingleton<IDialogService>(s => s.GetRequiredService<MainWindow>()); // Ensures only one MainWindow instance is created

            // View models
            services.AddSingleton<StatusMessageViewModel>();
            services.AddSingleton<GitViewModel>();
            services.AddSingleton<MainWindowViewModel>();

            // Views
            services.AddSingleton<MainWindow>();

            return services.BuildServiceProvider();
        }
    }
}
