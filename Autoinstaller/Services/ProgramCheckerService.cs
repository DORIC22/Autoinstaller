using Autoinstaller.Services.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Autoinstaller.Services
{
    public class ProgramCheckerService : IProgramCheckerService
    {
        /// <summary>
        /// Класс для десериализации объектов из JSON.
        /// </summary>
        private class InstalledProgram
        {
            public string DisplayName { get; set; }
            public string DisplayVersion { get; set; }
        }

        /// <summary>
        /// Возвращает список установленных программ (DisplayName и DisplayVersion) через PowerShell.
        /// </summary>
        public List<string> GetInstalledPrograms()
        {
            var installedPrograms = new List<string>();

            try
            {
                // PowerShell-команда для получения установленных программ в формате JSON.
                string command = "Get-ItemProperty -Path 'HKLM:\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\*' | " +
                                 "Select-Object DisplayName, DisplayVersion | ConvertTo-Json";

                var startInfo = new ProcessStartInfo("powershell", "-NoProfile -Command \"" + command + "\"")
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    // Десериализация JSON-вывода.
                    // Попытаемся десериализовать как массив объектов.
                    List<InstalledProgram> programs = null;
                    try
                    {
                        programs = JsonConvert.DeserializeObject<List<InstalledProgram>>(output);
                    }
                    catch
                    {
                        // Если не массив, пробуем один объект
                        var singleProgram = JsonConvert.DeserializeObject<InstalledProgram>(output);
                        if (singleProgram != null)
                        {
                            programs = new List<InstalledProgram> { singleProgram };
                        }
                    }

                    if (programs != null)
                    {
                        foreach (var program in programs)
                        {
                            if (!string.IsNullOrWhiteSpace(program.DisplayName))
                            {
                                string fullInfo = program.DisplayName;
                                if (!string.IsNullOrWhiteSpace(program.DisplayVersion))
                                {
                                    fullInfo += " " + program.DisplayVersion;
                                }
                                installedPrograms.Add(fullInfo.Trim());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при получении установленных программ: " + ex.Message);
            }

            // Вывод списка установленных программ в консоль
            Console.WriteLine("Список установленных программ:");
            foreach (var program in installedPrograms)
            {
                Console.WriteLine(program);
            }

            return installedPrograms;
        }
    }
}
