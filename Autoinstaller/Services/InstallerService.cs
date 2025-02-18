using Autoinstaller.Services.Interfaces;
using System.Diagnostics;

namespace Autoinstaller.Services
{
    public class InstallerService : IInstallerService
    {
        public void InstallMsi(string msiPath)
        {
            var arguments = $"/i \"{msiPath}\" /quiet /norestart";

            var startInfo = new ProcessStartInfo("msiexec.exe", arguments)
            {
                UseShellExecute = true,
                Verb = "runas" // Запуск от имени администратора (вызовет UAC при необходимости)
            };

            var process = Process.Start(startInfo);
            process?.WaitForExit();
        }
    }
}
