using System.Security.Cryptography;

namespace Autoinstaller.Services.Interfaces
{
    public interface IInstallerService
    {
        /// <summary>
        /// Устанавливает MSI-файл по указанному пути в тихом режиме.
        /// </summary>
        /// <param name="msiPath">Путь к MSI-файлу</param>
        void InstallMsi(string msiPath);

        void InstallExe(string installerPath, string arguments = "");
    }
}
