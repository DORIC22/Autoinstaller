using Autoinstaller.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Autoinstaller.Services
{
    public class InstallerService : IInstallerService
    {
        /// <summary>
        /// Получает список установленных программ через команду PowerShell.
        /// </summary>
        public List<string> GetInstalledPrograms()
        {
            var installedPrograms = new List<string>();

            try
            {
                // Команда PowerShell для получения установленных программ
                string command = "Get-ItemProperty -Path 'HKLM:\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\*' | Select-Object DisplayName, DisplayVersion";

                // Запускаем PowerShell процесс
                var startInfo = new ProcessStartInfo("powershell", "-Command " + command)
                {
                    RedirectStandardOutput = true, // Перехватываем вывод
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = Process.Start(startInfo);
                using (var reader = process.StandardOutput)
                {
                    string output = reader.ReadToEnd();
                    // Разбираем вывод команды (вывод будет в текстовом формате)
                    var lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                    // Пропускаем первую строку (заголовки) и обрабатываем остальные
                    foreach (var line in lines.Skip(2))
                    {
                        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 1)
                        {
                            // Формируем строку с именем программы
                            var displayName = string.Join(" ", parts.Take(parts.Length - 1));
                            var displayVersion = parts.Last();
                            installedPrograms.Add($"{displayName} {displayVersion}");
                        }
                    }
                }

                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении установленных программ: {ex.Message}");
            }

            return installedPrograms;
        }

        public void InstallMsi(string installerPath)
        {
            if (string.IsNullOrEmpty(installerPath))
                throw new ArgumentNullException(nameof(installerPath));

            var startInfo = new ProcessStartInfo("msiexec.exe")
            {
                Arguments = $"/i \"{installerPath}\" /quiet /norestart",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(startInfo);
            process?.WaitForExit();
        }

        /// <summary>
        /// Запускает .exe-файл без дополнительных аргументов (если не заданы).
        /// </summary>
        /// <param name="installerPath">Путь к .exe-файлу</param>
        /// <param name="arguments">Дополнительные аргументы (по умолчанию пусто)</param>
        public void InstallExe(string installerPath, string arguments = "")
        {
            if (string.IsNullOrEmpty(installerPath))
                throw new ArgumentNullException(nameof(installerPath));

            var startInfo = new ProcessStartInfo(installerPath)
            {
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(startInfo);
            process?.WaitForExit();
        }
    }
}
