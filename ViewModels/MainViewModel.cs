using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace H5MotaUpdate.ViewModels
{
    //silverCoin->Wand
    class MainViewModel : INotifyPropertyChanged
    {
        private string? _sourceRootDirectory;
        private string? _destRootDirectory;
        private string? _versionString;
        private string? SourceProjectDirectory, DestProjectDirectroy;
        private bool _migrateServerTable;

        public string? SourceRootDirectory
        {
            get { return _sourceRootDirectory; }
            set
            {
                _sourceRootDirectory = value;
                OnPropertyChanged();
            }
        }

        public string? DestRootDirectory
        {
            get => _destRootDirectory;
            set
            {
                _destRootDirectory = value;
                OnPropertyChanged();
            }
        }

        public string? VersionString
        {
            get => _versionString;
            set
            {
                _versionString = value;
                OnPropertyChanged();
            }
        }

        public bool MigrateServerTable
        {
            get => _migrateServerTable;
            set
            {
                _migrateServerTable = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ColoredString> ErrorMessages => ErrorLogger.ErrorMessages;

        public ICommand SelectSourceCommand { get; set; }
        public ICommand SelectDestCommand { get; set; }
        public ICommand MigrateCommand { get; set; }
        public ICommand HelpCommand { get; set; }

        public MainViewModel()
        {
            ErrorLogger.LogError("111", "red");
            ErrorLogger.LogError("222", "");
            ErrorLogger.LogError("222", "");
            ErrorLogger.LogError("222", "");
            ErrorLogger.LogError("222", "");
            ErrorLogger.LogError("222", "");
            ErrorLogger.LogError("222", "");
            SourceRootDirectory = "请选择包含要翻新的旧塔的文件夹";
            DestRootDirectory = "请选择一个包含新的2.10.3样板的文件夹";
            VersionString = "-";
            SelectSourceCommand = new RelayCommand(SelectSourceRootFolder);
            SelectDestCommand = new RelayCommand(SelectDestRootFolder);
            MigrateCommand = new RelayCommand(StartMigrate);
            HelpCommand = new RelayCommand(FileUtils.ShowHelp);
            MigrateServerTable = false;
        }

        private void SelectSourceRootFolder()
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    SourceRootDirectory = folderBrowserDialog.SelectedPath;
                }
                VersionString = VersionUtils.GetVersion(SourceRootDirectory);
            }
        }

        public void SelectDestRootFolder()
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DestRootDirectory = folderBrowserDialog.SelectedPath;
                }
            }
        }

        // 检查源文件夹和目标文件夹是否存在project子文件夹
        bool CheckValid()
        {
            if (!FileUtils.IsFolderPathValid(SourceRootDirectory, "源")) return false;
            if (!FileUtils.IsFolderPathValid(DestRootDirectory, "目标")) return false;
            SourceProjectDirectory = Path.Combine(SourceRootDirectory, "project");
            DestProjectDirectroy = Path.Combine(DestRootDirectory, "project");
            if (!FileUtils.IsFolderPathValid(SourceProjectDirectory, "源/project")) return false;
            if (!FileUtils.IsFolderPathValid(DestProjectDirectroy, "目标/project")) return false;
            if (!VersionUtils.IsValidVersion(VersionString))
            {
                MessageBox.Show("版本号格式不合法！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
            return true;
        }


        // 本按键功能：样板版本在2.7及以上，直接复制project文件夹下的animates, autotiles, bgms, fonts, images, materials, sounds, tilesets七个文件夹
        // 否则，复制animates文件夹，然后：根据data.js和icons.js等文件的注册信息，拆分sounds文件夹为sounds和bgms，拆分images文件夹
        // 在原工程未注册的素材不会复制，如有需要请手动复制
        // 需要配合“复制全塔属性”“复制素材信息”按钮完成素材的自动注册。之后请手动检查工程是否能打开，迁移结果是否正确，并进行相应调整。
        public void StartMigrate()
        {
            if (!CheckValid()) return;
            Version ver;
            Version.TryParse(VersionString, out ver);

            #region
            // 从libs/core.js中读取塔的默认长宽，若不为13，需要写入新样板的core.js中
            int width, height;
            string sourceCoreJSPath = Path.Combine(SourceRootDirectory, "libs/core.js"),
                destCoreJSPath = Path.Combine(DestRootDirectory, "libs/core.js");
            (width, height) = StringUtils.ReadMapWidth(sourceCoreJSPath);

            if (width != 13 || height != 13)
            {
                StringUtils.WriteMapWidth(destCoreJSPath, width, height);
            }
            #endregion

            DataJSMigrator dataJSMigrator = new(SourceProjectDirectory, DestProjectDirectroy, ver);
            EnemysJSMigrator enemysJSMigrator = new(SourceProjectDirectory, DestProjectDirectroy, ver);
            IconsJSMigrator iconsJSMigrator = new(SourceProjectDirectory, DestProjectDirectroy, ver);
            ItemsJSMigrator itemsJSMigrator = new(SourceProjectDirectory, DestProjectDirectroy, ver);
            MapsJSMigrator mapsJSMigrator = new(SourceProjectDirectory, DestProjectDirectroy, ver);
            FloorsMigrator floorsMigrator = new(SourceProjectDirectory, DestProjectDirectroy, ver, width, height);
            MediaSourceMigrator mediaSourceJSMigrator = new(SourceProjectDirectory, DestProjectDirectroy, ver);

            dataJSMigrator.Migrate();
            enemysJSMigrator.Migrate();
            iconsJSMigrator.Migrate();
            itemsJSMigrator.Migrate();
            mapsJSMigrator.Migrate();
            floorsMigrator.Migrate(mapsJSMigrator.mapsIndexArray);
            mediaSourceJSMigrator.Migrate();

            if (MigrateServerTable)
            {
                ServerTableMigrator serverTableJSMigrator = new(SourceRootDirectory, DestRootDirectory, ver);
                serverTableJSMigrator.Migrate();
            }

        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class RelayCommand : ICommand
        {
            public event EventHandler? CanExecuteChanged;

            private Action action;
            public RelayCommand(Action action)
            {
                this.action = action;
            }

            public bool CanExecute(object? parameter)
            {
                return true;
            }

            public void Execute(object? parameter)
            {
                action?.Invoke();
            }
        }
    }


}
