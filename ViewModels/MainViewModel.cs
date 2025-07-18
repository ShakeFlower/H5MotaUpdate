﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace H5MotaUpdate.ViewModels
{
    class MainViewModel : INotifyPropertyChanged
    {
        private string? _sourceRootDirectory;
        private string? _destRootDirectory;
        private string? _versionString;
        private string? SourceProjectDirectory, DestProjectDirectroy;
        private bool _migrateServerTable;
        private bool _isAvailable;

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

        public bool IsAvailable
        {
            get => _isAvailable;
            set
            {
                _isAvailable = value;
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
            SourceRootDirectory = "请选择包含要翻新的旧塔的文件夹";
            DestRootDirectory = "请选择一个包含新的2.10.3样板的文件夹";
            VersionString = "-";
            SelectSourceCommand = new RelayCommand(SelectSourceRootFolder);
            SelectDestCommand = new RelayCommand(SelectDestRootFolder);
            MigrateCommand = new RelayCommand(StartMigrate);
            HelpCommand = new RelayCommand(FileUtils.ShowHelp);
            MigrateServerTable = false;
            IsAvailable = true;
        }

        /// <summary>
        /// 选择要翻新的旧塔的文件夹，会自动读取旧塔的版本号
        /// </summary>
        private void SelectSourceRootFolder()
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                // 设置初始路径为上一次选择的路径
                folderBrowserDialog.SelectedPath = Properties.Settings.Default.LastSourceFolderPath;

                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    SourceRootDirectory = folderBrowserDialog.SelectedPath;
                    // 保存本次选择的路径
                    Properties.Settings.Default.LastSourceFolderPath = SourceRootDirectory;
                    Properties.Settings.Default.Save();
                }

                VersionString = VersionUtils.GetVersion(SourceRootDirectory);
            }
        }

        /// <summary>
        /// 选择新样板文件夹
        /// </summary>
        public void SelectDestRootFolder()
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                // 设置初始路径为上一次选择的路径
                folderBrowserDialog.SelectedPath = Properties.Settings.Default.LastDestFolderPath;

                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DestRootDirectory = folderBrowserDialog.SelectedPath;
                    // 保存本次选择的路径
                    Properties.Settings.Default.LastDestFolderPath = DestRootDirectory;
                    Properties.Settings.Default.Save();
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
                ErrorLogger.LogError("版本号格式不合法！", "red");
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
            IsAvailable = false;
            Version ver;
            Version.TryParse(VersionString, out ver);

            ErrorLogger.Clear();
            // 每次开始迁移，清空之前报错信息

            #region 读取塔的尺寸
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
            string endMsg = "迁移完成，请仔细核对结果。";
            MessageBox.Show(endMsg);
            ErrorLogger.LogError(endMsg);
            IsAvailable = true;
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
