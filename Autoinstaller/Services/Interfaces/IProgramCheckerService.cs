using System.Collections.Generic;

namespace Autoinstaller.Services.Interfaces
{
    public interface IProgramCheckerService
    {
        /// <summary>
        /// Возвращает список установленных программ (DisplayName).
        /// </summary>
        /// <returns>Список названий программ</returns>
        List<string> GetInstalledPrograms();
    }
}
