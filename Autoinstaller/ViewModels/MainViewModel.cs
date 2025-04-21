using Autoinstaller.Helpers;
using Autoinstaller.Models;
using Autoinstaller.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Autoinstaller.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IInstallerService _installerService;
        private readonly IProgramCheckerService _checkerService;

        public MainViewModel(IInstallerService installerService, IProgramCheckerService checkerService)
        {
            _installerService = installerService;
            _checkerService = checkerService;

            // Получаем информацию о сети (например, для адаптера "Ethernet")
            var networkInfo = GetNetworkInfoByInterfaceName("Ethernet");
            LocalIpAddress = networkInfo.IpAddress;
            LocalSubnetMask = networkInfo.SubnetMask;
            LocalGateway = networkInfo.Gateway;

            // 1) Получаем список уже установленных программ (DisplayName) из реестра
            InstalledPrograms = _checkerService.GetInstalledPrograms();
            if (InstalledPrograms != null && InstalledPrograms.Count > 0)
            {
                string programsText = string.Join("\n", InstalledPrograms);
                Console.WriteLine("Список установленных программ:");
                Console.WriteLine(programsText);
            }
            else
            {
                Console.WriteLine("Программ не найдено.");
            }

            // 2) Инициализируем список программ для установки
            // Для каждой программы добавлено поле Timeout (в секундах)
            ProgramList = new ObservableCollection<Programitem>
{
            new Programitem
            {
                Name = "Компас 21",
                InstallerPath = @"KOMPAS-3D v21 x64\KOMPAS-3D_v21_x64.msi",
                Association = "v21",
                Status = "Не установлено",
                SourceReplacePath = @"KOMPAS-3D v21 x64\! Crack !",
                TargetReplacePath = @"C:\Program Files\ASCON\KOMPAS-3D v21",
                // Указываем имя ярлыка для проверки
                ShortcutName = "КОМПАС-3D v21.lnk",
                Timeout = 500
            },
            new Programitem //7z2409-x64.msi
            {
                Name = "7z",
                InstallerPath = @"7z2409-x64.msi",
                Association = "Агент",
                Status = "Не установлено",
                SourceReplacePath = "",
                TargetReplacePath = "",
                ShortcutName = "",
                Timeout = 20
            },
            new Programitem
            {
                Name = "Office 2007",
                InstallerPath = @"Office_2007\SETUP.exe",
                Association = "Office",
                Status = "Не установлено",
                SourceReplacePath = "",
                TargetReplacePath = "",
                ShortcutName = "",
                Timeout = 300
            },
            new Programitem
            {
                Name = "LM Server",
                InstallerPath = @"LmServer\LiteManagerProServer - All.msi",
                Association = "Lite",
                Status = "Не установлено",
                SourceReplacePath = "",
                TargetReplacePath = "",
                ShortcutName = "",
                Timeout = 60
            },
            new Programitem
            {
                Name = "Blender",
                InstallerPath = @"Blender\blender-4.3.2-windows-x64.msi",
                Association = "Blender",
                Status = "Невозможно определить",
                SourceReplacePath = "",
                TargetReplacePath = "",
                ShortcutName = "Blender 4.3.lnk",
                Timeout = 240
            },
            new Programitem
            {
                Name = "Агент администрирования",
                InstallerPath = @"NetAgent_14.2.0.26967\setup.exe",
                Association = "Агент",
                Status = "Невозможно определить",
                SourceReplacePath = "",
                TargetReplacePath = "",
                ShortcutName = "",
                Timeout = 180
            },
            new Programitem
            {
                Name = "KES",
                InstallerPath = @"KES_12.3.0.493\setup.exe",
                Association = "Endpoint",
                Status = "Не установлено",
                SourceReplacePath = "",
                TargetReplacePath = "",
                ShortcutName = "",
                Timeout = 240
            },
            new Programitem
            {
                Name = "Acrobat",
                InstallerPath = @"AcroRdrDC1900820071_ru_RU.exe",
                Association = "Acrobat",
                Status = "Не установлено",
                SourceReplacePath = "",
                TargetReplacePath = "",
                ShortcutName = "Acrobat Reader DC.lnk"
            },
        };

            // 3) Проверяем, установлены ли программы уже (по реестру)
            CheckIfAlreadyInstalled();

            // Дополнительная проверка наличия ярлыков
            foreach (var program in ProgramList)
            {
                if (!string.IsNullOrEmpty(program.ShortcutName))
                {
                    bool shortcutExists = CheckShortcut(program.ShortcutName);
                    if (shortcutExists)
                    {
                        Console.WriteLine($"При запуске найден ярлык {program.ShortcutName} для {program.Name}");
                        program.Status = "Установлено";
                    }
                }
            }

            // Команда для установки выбранных программ
            InstallSelectedCommand = new RelayCommand(async _ => await InstallSelectedAsync());

            // Команда для сохранения сетевых настроек (редактируемых в текстовых полях)
            SaveNetworkSettingsCommand = new RelayCommand(_ => SaveNetworkSettings());
        }

        #region Свойства для сетевой информации

        private string _localIpAddress;
        public string LocalIpAddress
        {
            get => _localIpAddress;
            set => SetProperty(ref _localIpAddress, value);
        }

        private string _localSubnetMask;
        public string LocalSubnetMask
        {
            get => _localSubnetMask;
            set => SetProperty(ref _localSubnetMask, value);
        }

        private string _localGateway;
        public string LocalGateway
        {
            get => _localGateway;
            set => SetProperty(ref _localGateway, value);
        }

        #endregion

        // Свойство для списка установленных программ (из реестра)
        private List<string> _installedPrograms;
        public List<string> InstalledPrograms
        {
            get => _installedPrograms;
            set => SetProperty(ref _installedPrograms, value);
        }

        // Коллекция программ для установки (с галочками)
        public ObservableCollection<Programitem> ProgramList { get; }

        // Команда для кнопки "Установить выбранное"
        public ICommand InstallSelectedCommand { get; }

        // Команда для сохранения сетевых настроек
        public ICommand SaveNetworkSettingsCommand { get; }

        // Метод сохранения сетевых настроек (например, можно сохранить их в конфигурационный файл)
        private void SaveNetworkSettings()
        {
            SetNetworkSettings(LocalIpAddress, LocalSubnetMask, LocalGateway);
            Console.WriteLine("Настройки сети сохранены:");
            Console.WriteLine($"IP: {LocalIpAddress}");
            Console.WriteLine($"Маска: {LocalSubnetMask}");
            Console.WriteLine($"Шлюз: {LocalGateway}");
        }

        // Метод установки сетевых настроек через netsh
        private void SetNetworkSettings(string ipAddress, string subnetMask, string gateway)
        {
            try
            {
                string command = $"netsh interface ip set address name=\"Ethernet\" static {ipAddress} {subnetMask} {gateway}";
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/C " + command,
                    Verb = "runas",
                    CreateNoWindow = true,
                    UseShellExecute = true
                });
                Console.WriteLine("Сетевые настройки успешно применены.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при применении сетевых настроек: {ex.Message}");
            }
        }

        // Метод проверки, есть ли запись (Association) в списке установленных программ
        private void CheckIfAlreadyInstalled()
        {
            if (InstalledPrograms == null || InstalledPrograms.Count == 0)
                return;

            foreach (var program in ProgramList)
            {
                if (!string.IsNullOrEmpty(program.Association))
                {
                    bool found = InstalledPrograms
                        .Any(installed => installed.ToLower()
                                           .Contains(program.Association.ToLower()));
                    if (found)
                        program.Status = "Установлено";
                }
            }
        }

        // Метод установки выбранных программ с проверкой реестра каждые 10 секунд в течение заданного времени (Timeout)
        private async Task InstallSelectedAsync()
        {
            Console.WriteLine("Начало установки программ...");
            var selectedPrograms = ProgramList.Where(p => p.IsSelected).ToList();

            foreach (var program in selectedPrograms)
            {
                if (program.Status == "Установлено")
                    continue;

                bool isAlreadyInstalled = InstalledPrograms
                    .Any(installed => !string.IsNullOrEmpty(program.Association) &&
                                      installed.ToLower().Contains(program.Association.ToLower()));
                if (isAlreadyInstalled)
                {
                    Console.WriteLine($"Программа {program.Name} уже установлена (Association: {program.Association}).");
                    program.Status = "Установлено";
                    continue;
                }

                program.Status = "Установка...";
                await Task.Run(() =>
                {
                    string extension = Path.GetExtension(program.InstallerPath).ToLower();
                    if (extension == ".msi")
                    {
                        _installerService.InstallMsi(program.InstallerPath);
                    }
                    else if (extension == ".exe")
                    {
                        // Запускаем .exe без аргументов
                        _installerService.InstallExe(program.InstallerPath, "");
                    }
                    else
                    {
                        Console.WriteLine($"Неизвестное расширение для файла: {program.InstallerPath}");
                    }
                });

                // Определяем количество попыток на основе времени ожидания (Timeout) и интервала (10 сек)
                int delay = 10000; // 10 секунд интервал
                int iterations = program.Timeout > 0 ? program.Timeout / 10 : 12; // Если Timeout не задан, по умолчанию 12 (120 секунд)
                bool isInstalledNow = false;

                for (int i = 0; i < iterations; i++)
                {
                    await Task.Delay(delay);
                    InstalledPrograms = _checkerService.GetInstalledPrograms();
                    isInstalledNow = InstalledPrograms
                        .Any(installed => !string.IsNullOrEmpty(program.Association) &&
                                          installed.ToLower().Contains(program.Association.ToLower()));
                    if (isInstalledNow)
                    {
                        Console.WriteLine($"Программа {program.Name} успешно установлена (Association: {program.Association}).");
                        break;
                    }
                }

                if (!isInstalledNow)
                {
                    Console.WriteLine($"Ошибка: программа {program.Name} не найдена в реестре (или ярлык) после установки за отведённое время.");
                    program.Status = "Ошибка установки";
                    // Переходим к следующей программе, даже если установка не завершилась\n                    continue;
                }

                // Замена файлов, если пути указаны
                if (!string.IsNullOrEmpty(program.SourceReplacePath) && !string.IsNullOrEmpty(program.TargetReplacePath))
                {
                    program.Status = ReplaceFiles(program)
                        ? "Установлено"
                        : "Ошибка при замене файлов";
                }
                else
                {
                    program.Status = "Установлено";
                }
            }
        }

        // Метод проверки наличия ярлыка по указанному имени (например, "КОМПАС-3D v21.lnk")
        private bool CheckShortcut(string shortcutName)
        {
            Console.WriteLine($"Запуск проверки ярлыка: {shortcutName}");
            var possiblePaths = new List<string>
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), shortcutName),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), shortcutName),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms), shortcutName),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms), @"ASCON", shortcutName)
            };

            foreach (var path in possiblePaths)
            {
                Console.WriteLine($"Проверка пути: {path}");
                if (File.Exists(path))
                {
                    Console.WriteLine($"Найден ярлык: {path}");
                    return true;
                }
            }
            Console.WriteLine($"Ярлык {shortcutName} не найден.");
            return false;
        }

        // Класс для хранения сетевой информации
        public class NetworkInfo
        {
            public string IpAddress { get; set; }
            public string SubnetMask { get; set; }
            public string Gateway { get; set; }
        }

        // Метод получения информации о сети для заданного адаптера по имени
        private NetworkInfo GetNetworkInfoByInterfaceName(string interfaceName)
        {
            try
            {
                var networkInterface = NetworkInterface
                    .GetAllNetworkInterfaces()
                    .FirstOrDefault(ni =>
                        ni.Name.Equals(interfaceName, StringComparison.OrdinalIgnoreCase) &&
                        ni.OperationalStatus == OperationalStatus.Up);

                if (networkInterface == null)
                    return new NetworkInfo
                    {
                        IpAddress = $"Интерфейс '{interfaceName}' не найден или не активен.",
                        SubnetMask = "",
                        Gateway = ""
                    };

                var ipProps = networkInterface.GetIPProperties();
                var unicast = ipProps.UnicastAddresses
                    .FirstOrDefault(addr => addr.Address.AddressFamily == AddressFamily.InterNetwork);
                var gatewayInfo = ipProps.GatewayAddresses
                    .FirstOrDefault(g => g.Address.AddressFamily == AddressFamily.InterNetwork);

                return new NetworkInfo
                {
                    IpAddress = unicast?.Address.ToString() ?? "IPv4 не найден",
                    SubnetMask = unicast?.IPv4Mask.ToString() ?? "Маска не найдена",
                    Gateway = gatewayInfo?.Address.ToString() ?? "Шлюз не найден"
                };
            }
            catch (Exception ex)
            {
                return new NetworkInfo
                {
                    IpAddress = $"Ошибка: {ex.Message}",
                    SubnetMask = $"Ошибка: {ex.Message}",
                    Gateway = $"Ошибка: {ex.Message}"
                };
            }
        }

        // Метод замены файлов (рекурсивное копирование)
        private bool ReplaceFiles(Programitem program)
        {
            try
            {
                Console.WriteLine($"Начата замена файлов для программы: {program.Name}");
                if (!Directory.Exists(program.SourceReplacePath))
                {
                    Console.WriteLine($"Исходная папка не найдена: {program.SourceReplacePath}");
                    return false;
                }
                if (!Directory.Exists(program.TargetReplacePath))
                {
                    Directory.CreateDirectory(program.TargetReplacePath);
                    Console.WriteLine($"Создана целевая папка: {program.TargetReplacePath}");
                }
                CopyDirectory(program.SourceReplacePath, program.TargetReplacePath);
                Console.WriteLine($"Файлы для программы {program.Name} успешно заменены.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при замене файлов: {ex.Message}");
                return false;
            }
        }

        // Рекурсивное копирование содержимого одной директории в другую
        private void CopyDirectory(string sourceDir, string targetDir)
        {
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var fileName = Path.GetFileName(file);
                var destFile = Path.Combine(targetDir, fileName);
                if (File.Exists(destFile))
                {
                    File.Delete(destFile);
                    Console.WriteLine($"Удален файл: {destFile}");
                }
                File.Copy(file, destFile, overwrite: true);
                Console.WriteLine($"Скопирован файл: {file} -> {destFile}");
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                var dirName = Path.GetFileName(dir);
                var destDir = Path.Combine(targetDir, dirName);
                CopyDirectory(dir, destDir);
            }
        }
    }
}
