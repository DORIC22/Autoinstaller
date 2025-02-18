using Autoinstaller.Services.Interfaces;
using Microsoft.Win32;
using System.Collections.Generic;

namespace Autoinstaller.Services
{
    public class ProgramCheckerService : IProgramCheckerService
    {
        /// <summary>
        /// Возвращает список установленных программ (DisplayName) из реестра.
        /// Читает HKLM и HKCU, а также 64-бит и 32-бит ветки (Wow6432Node).
        /// </summary>
        public List<string> GetInstalledPrograms()
        {
            var installedPrograms = new List<string>();

            // Список базовых веток (локальная машина и текущий пользователь)
            var baseKeys = new[]
            {
                Registry.LocalMachine,
                Registry.CurrentUser
            };

            // Подпути, где обычно хранятся данные об установленных программах
            var subPaths = new[]
            {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            // Перебираем все комбинации (HKLM + HKCU) × (обычный путь + Wow6432Node)
            foreach (var baseKey in baseKeys)
            {
                foreach (var subPath in subPaths)
                {
                    using (var key = baseKey.OpenSubKey(subPath))
                    {
                        if (key == null)
                            continue;

                        // Перебираем все подпапки внутри ветки Uninstall
                        foreach (var subKeyName in key.GetSubKeyNames())
                        {
                            using (var subKey = key.OpenSubKey(subKeyName))
                            {
                                if (subKey == null)
                                    continue;

                                // Пытаемся получить значение "DisplayName"
                                var displayName = subKey.GetValue("DisplayName") as string;
                                if (!string.IsNullOrEmpty(displayName))
                                {
                                    // Чтобы не дублировать в списке, проверим, есть ли уже такой DisplayName
                                    if (!installedPrograms.Contains(displayName))
                                    {
                                        installedPrograms.Add(displayName);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return installedPrograms;
        }
    }
}
