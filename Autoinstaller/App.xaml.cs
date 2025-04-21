using Autoinstaller.Services;
using Autoinstaller.Services.Interfaces;
using Autoinstaller.ViewModels;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

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
            var mainViewModel = new MainViewModel(installerService, checkerService);

            // Создаём окно и назначаем DataContext
            MainWindow mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            // Устанавливаем иконку после создания MainWindow
            var icon = new BitmapImage(new Uri("pack://application:,,,/Recources/Group-18.ico"));
            mainWindow.Icon = icon;

            // Показываем окно
            mainWindow.Show();
        }
    }
}
