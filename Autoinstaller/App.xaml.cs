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

            // Создаём экземпляр сервиса
            IInstallerService installerService = new InstallerService();

            // Создаём ViewModel, передаём в конструктор сервис
            var mainViewModel = new MainViewModel(installerService);

            // Создаём главное окно и назначаем DataContext
            MainWindow mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            mainWindow.Show();
        }
    }
}
