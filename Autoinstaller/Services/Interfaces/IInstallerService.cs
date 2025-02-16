using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autoinstaller.Services.Interfaces
{
    public interface IInstallerService
    {
        /// <summary>
        /// Устанавливает MSI-файл по указанному пути (в тихом режиме, без диалоговых окон).
        /// </summary>
        /// <param name="msiPath">Полный путь к MSI-файлу</param>
        void InstallMsi(string msiPath);
    }
}

