using Autoinstaller.Services;
using Autoinstaller.Services.Interfaces;
using Autoinstaller.ViewModels;
using System.Windows;

namespace Autoinstaller
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Создаём сервис установки
            IInstallerService installerService = new InstallerService();

            // Создаём сервис проверки
            IProgramCheckerService checkerService = new ProgramCheckerService();

            // Создаём ViewModel и передаём сервис
            var mainViewModel = new MainViewModel(installerService ,checkerService);

            // Создаём окно и назначаем DataContext
            MainWindow mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
            mainWindow.Show();
        }
    }
}
