namespace H5MotaUpdate.ViewModels
{
    internal class ServerTableMigrator
    {
        string sourcePath, destPath;
        Version version;
        readonly string FILENAME = "_server/table";
        public ServerTableMigrator(string oldRootDirectory, string newRootDirectory, Version ver)
        {
            sourcePath = System.IO.Path.Combine(oldRootDirectory, FILENAME);
            destPath = System.IO.Path.Combine(newRootDirectory, FILENAME);
            this.version = ver;
        }

        public void Migrate()
        {
            try
            {
                MigrateDirect();
                MessageBox.Show("迁移" + FILENAME + "文件夹完成。");
            }
            catch (Exception e)
            {
                MessageBox.Show("迁移" + FILENAME + $"过程中出现错误: {e.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        void MigrateDirect()
        {
            FileUtils.CopyFolderContentsAndSubFolders(sourcePath, destPath);
        }
    }
}
