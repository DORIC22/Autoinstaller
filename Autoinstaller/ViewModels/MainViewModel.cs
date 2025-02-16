using Autoinstaller.Helpers;
using Autoinstaller.Services.Interfaces;
using System.Windows.Input;

namespace Autoinstaller.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IInstallerService _installerService;

        public MainViewModel(IInstallerService installerService)
        {
            _installerService = installerService;

            // Создаём команду, которая всегда доступна для выполнения
            InstallCommand = new RelayCommand(InstallMsi);
        }

        public ICommand InstallCommand { get; }

        private void InstallMsi(object obj)
        {
            string msiPath = @"C:\Users\Km\Desktop\CrealityPrint_6.0.2.1574_Release.msi";
            _installerService.InstallMsi(msiPath);
        }
    }
}
