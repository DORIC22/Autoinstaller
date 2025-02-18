using Autoinstaller.Helpers;
using Autoinstaller.Models;
using Autoinstaller.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
            ProgramList = new ObservableCollection<Programitem>
            {
                new Programitem
                {
                    Name = "Creality Print",
                    InstallerPath = @"\\10.137.200.14\its\Soft\!!!!MSISOFT_AI\CrealityPrint_6.0.2.1574_Release.msi",
                    Association = "Creality Print",
                    Status = "Не установлено",
                    SourceReplacePath = @"C:\Users\Km\Desktop\KOMPAS-3D v21 x64\! Crack !", // Папка с файлами для замены
                    TargetReplacePath = @"C:\\Program Files\\ASCON\\KOMPAS-3D v21" // Папка, куда копируются файлы
                },
                new Programitem
                {
                    Name = "Компас 21",
                    InstallerPath = @"\\10.137.200.14\its\Soft\!Все программы\KOMPAS-3D v21 x64\KOMPAS-3D v21 x64\KOMPAS-3D_v21_x64.msi",
                    Association = "KOMPAS",
                    Status = "Не установлено",
                    SourceReplacePath = @"\\10.137.200.14\its\Soft\!Все программы\KOMPAS-3D v21 x64\KOMPAS-3D v21 x64\KOMPAS-3D v21",
                    TargetReplacePath = @"C:\\Program Files\\ASCON\\KOMPAS-3D v21"
                },
                // \\10.137.200.14\its\Soft\!Все программы\KOMPAS-3D v21 x64\KOMPAS-3D v21 x64\KOMPAS-3D v21
                // C:\Program Files\ASCON
                new Programitem
                {
                    Name = "Программа 2",
                    InstallerPath = @"C:\Installers\Program2.msi",
                    Association = "Program2",
                    Status = "Не установлено",
                    SourceReplacePath = "", // Замена файлов не требуется
                    TargetReplacePath = ""
                }
            };

            // 3) Сразу после инициализации ProgramList проверяем, установлены ли они уже
            CheckIfAlreadyInstalled();

            // Команда, вызывающая метод установки выбранных программ
            InstallSelectedCommand = new RelayCommand(async _ => await InstallSelectedAsync());
        }

        // Список уже установленных программ (DisplayName)
        private List<string> _installedPrograms;
        public List<string> InstalledPrograms
        {
            get => _installedPrograms;
            set => SetProperty(ref _installedPrograms, value);
        }

        // Коллекция программ (с галочками)
        public ObservableCollection<Programitem> ProgramList { get; }

        // Команда для кнопки "Установить выбранное"
        public ICommand InstallSelectedCommand { get; }

        // Метод проверки, есть ли ассоциация в списке установленных программ
        private void CheckIfAlreadyInstalled()
        {
            // Если InstalledPrograms пуст или null, пропускаем
            if (InstalledPrograms == null || InstalledPrograms.Count == 0)
                return;

            foreach (var program in ProgramList)
            {
                // Проверяем, что поле Association не пустое
                if (!string.IsNullOrEmpty(program.Association))
                {
                    // Смотрим, содержит ли хоть один установленный DisplayName нашу строку
                    bool found = InstalledPrograms
                        .Any(installed => installed.ToLower()
                                           .Contains(program.Association.ToLower()));

                    if (found)
                    {
                        program.Status = "Установлено";
                    }
                }
            }
        }


        private async Task InstallSelectedAsync()
        {
            // Выбираем только те программы, которые отмечены для установки
            var selectedPrograms = ProgramList.Where(p => p.IsSelected).ToList();

            foreach (var program in selectedPrograms)
            {
                // Если программа уже установлена, пропускаем
                if (program.Status == "Установлено")
                    continue;

                // Установка программы
                program.Status = "Установка...";
                await Task.Run(() => _installerService.InstallMsi(program.InstallerPath));

                // Ждем 10 секунд перед заменой файлов (если нужно)
                await Task.Delay(15000); // 10 секунд ожидания

                // Замена файлов, если указаны пути
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



        /// <summary>
        /// Выполняет замену файлов в папке установки программы.
        /// </summary>
        /// <param name="program">Объект программы для установки.</param>
        /// <returns>True, если замена файлов прошла успешно, иначе False.</returns>
        private bool ReplaceFiles(Programitem program)
        {
            try
            {
                // Проверяем, существуют ли исходная и целевая папки
                if (!System.IO.Directory.Exists(program.SourceReplacePath))
                {
                    Console.WriteLine($"Исходная папка {program.SourceReplacePath} не найдена.");
                    return false;
                }

                if (!System.IO.Directory.Exists(program.TargetReplacePath))
                {
                    System.IO.Directory.CreateDirectory(program.TargetReplacePath);
                }

                // Копируем файлы из исходной папки в целевую папку
                foreach (var file in System.IO.Directory.GetFiles(program.SourceReplacePath))
                {
                    var fileName = System.IO.Path.GetFileName(file);
                    var destFile = System.IO.Path.Combine(program.TargetReplacePath, fileName);

                    try
                    {
                        // Удаляем существующий файл
                        if (System.IO.File.Exists(destFile))
                        {
                            System.IO.File.Delete(destFile);
                            Console.WriteLine($"Удален файл: {destFile}");
                        }

                        // Копируем файл с перезаписью
                        System.IO.File.Copy(file, destFile, overwrite: true);
                        Console.WriteLine($"Скопирован файл: {file} -> {destFile}");
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Console.WriteLine($"Ошибка доступа при обработке файла {file}: {ex.Message}");
                        return false;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при копировании файла {file}: {ex.Message}");
                        return false;
                    }
                }

                Console.WriteLine($"Файлы для {program.Name} успешно заменены.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при замене файлов для {program.Name}: {ex.Message}");
                return false;
            }
        }

    }
}
