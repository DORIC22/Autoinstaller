using Autoinstaller.ViewModels;

namespace Autoinstaller.Models
{
    public class Programitem : BaseViewModel
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _installerPath;
        public string InstallerPath
        {
            get => _installerPath;
            set => SetProperty(ref _installerPath, value);
        }

        private string _status;
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        // Новое поле "Ассоциация"
        private string _association;
        public string Association
        {
            get => _association;
            set => SetProperty(ref _association, value);
        }

        // Новое поле "Путь источника замены"
        private string _sourceReplacePath;
        public string SourceReplacePath
        {
            get => _sourceReplacePath;
            set => SetProperty(ref _sourceReplacePath, value);
        }

        // Новое поле "Целевой путь замены"
        private string _targetReplacePath;
        public string TargetReplacePath
        {
            get => _targetReplacePath;
            set => SetProperty(ref _targetReplacePath, value);
        }

        private string _shortcutName;
        public string ShortcutName
        {
            get => _shortcutName;
            set => SetProperty(ref _shortcutName, value);
        }

        // Новое поле "Timeout" (в секундах)
        private int _timeout;
        public int Timeout
        {
            get => _timeout;
            set => SetProperty(ref _timeout, value);
        }
    }
}
