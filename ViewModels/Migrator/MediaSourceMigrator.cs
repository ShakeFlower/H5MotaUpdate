using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing;

namespace H5MotaUpdate.ViewModels
{
    public class MediaSourceMigrator
    {
        string oldProjectDirectory, newProjectDirectory;
        Version version;

        /// <summary>
        /// 请输入新旧Project文件夹的路径
        /// </summary>
        public MediaSourceMigrator(string oldProjectDirectory, string newProjectDirectory, Version ver)
        {
            this.oldProjectDirectory = oldProjectDirectory;
            this.newProjectDirectory = newProjectDirectory;
            this.version = ver;
        }

        public void Migrate()
        {
            try
            {
                if (version.CompareTo(new Version(2, 7)) >= 0)
                {
                    MigrateDirect();
                }
                else
                {
                    Convert();
                }
                MessageBox.Show("迁移素材文件完成。");
            }
            catch (Exception e)
            {
                MessageBox.Show("迁移素材文件过程中出现错误: " + $"{e.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        public void MigrateDirect()
        {
            string[] subFolderNames = ["animates", "autotiles", "bgms", "fonts", "images", "materials", "sounds", "tilesets"];
            foreach (string folderName in subFolderNames)
            {
                try
                {
                    string sourcePath = Path.Combine(oldProjectDirectory, folderName),
                        destPath = Path.Combine(newProjectDirectory, folderName);
                    FileUtils.CopyFolderContents(sourcePath, destPath);
                    MessageBox.Show("project/" + folderName + "文件夹迁移完成");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("project/" + folderName + $"文件夹迁移失败，原因:{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        public void Convert()
        {
            try
            {
                string datasJsPath = Path.Combine(oldProjectDirectory, "data.js"),
                    iconsJsPath = Path.Combine(oldProjectDirectory, "icons.js");

                List<string> imagesList = [],
                    bgmsList = [],
                    soundsList = [],
                    autotilesList = [],
                    tilesetsList = [];
                JObject datasJObject = StringUtils.getValidJson(datasJsPath),
                    iconsJObject = StringUtils.getValidJson(iconsJsPath);

                if (datasJObject["main"]["images"] is JArray imagesJsonList && imagesJsonList.Count > 0)
                {
                    imagesList = imagesJsonList.ToObject<List<string>>();
                    imagesList.Add("hero.png"); // 2.7以前hero.png硬编码在loader.prototype._loadExtraImages中
                    imagesList.Add("ground.png");

                }
                if (datasJObject["main"]["bgms"] is JArray bgmsJsonList && bgmsJsonList.Count > 0)
                {
                    bgmsList = bgmsJsonList.ToObject<List<string>>();
                }
                if (datasJObject["main"]["sounds"] is JArray soundsJsonList && soundsJsonList.Count > 0)
                {
                    soundsList = soundsJsonList.ToObject<List<string>>();
                }
                if (datasJObject["main"]["tilesets"] is JArray tilesetsJsonList && tilesetsJsonList.Count > 0)
                {
                    tilesetsList = tilesetsJsonList.ToObject<List<string>>();
                }
                if (iconsJObject["autotile"] is JObject autotilesJsonList && autotilesJsonList.Count > 0)
                {
                    autotilesList = autotilesJsonList.Properties().Select(prop => prop.Name + ".png").ToList(); //autotile只允许是.png格式
                }
                TransferOldAnimate();
                TransferOldImages(imagesList, autotilesList, tilesetsList);
                TransferSounds(bgmsList, soundsList);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"素材文件夹迁移出错，原因:{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        void TransferOldAnimate()
        {
            try
            {
                string inputDirectory = Path.Combine(oldProjectDirectory, "animates"),
                    outputDirectory = Path.Combine(newProjectDirectory, "animates");
                FileUtils.CopyFolderContents(inputDirectory, outputDirectory);
                MessageBox.Show("project/animates文件夹迁移完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"project/animates文件夹迁移出错，原因:{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        void TransferOldImages(List<string> imagesList, List<string> autotilesList, List<string> tilesetsList)
        {
            try
            {
                string inputDirectory = Path.Combine(oldProjectDirectory, "images");
                string[] files = Directory.GetFiles(inputDirectory);

                string[] materialsFiles = ["airwall.png",
                    "animates.png",
                    "enemy48.png",
                    "enemys.png",
                    "fog.png",
                    "ground.png",
                    "icons.png",
                    "icons_old.png",
                    "items.png",
                    "keyboard.png",
                    "npc48.png",
                    "npcs.png",
                    "terrains.png"];
                foreach (string filePath in files)
                {
                    string fileName = Path.GetFileName(filePath);
                    if (fileName == "icons.png" && version<new Version(2,5,4)) //检查icons长度
                    {
                        using (Bitmap image = new Bitmap(filePath))
                        {
                            int width = image.Width;
                            int height = image.Height;
                            if (height < 1120)
                            {
                                MessageBox.Show("警告：原塔的icons.png长度不足！请对照最新样板使用PS工具补齐数字键和Alt图标等。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                        }
                    }

                    if (imagesList.Contains(fileName))
                    {
                        File.Copy(filePath, Path.Combine(newProjectDirectory, "images/" + fileName), true);
                    }
                    if (materialsFiles.Contains(fileName))
                    {
                        File.Copy(filePath, Path.Combine(newProjectDirectory, "materials/" + fileName), true);
                    }
                    if (autotilesList.Contains(fileName))
                    { // 样板会将autotile强制转换为.png
                        File.Copy(filePath, Path.Combine(newProjectDirectory, "autotiles/" + fileName), true);
                    }
                    if (tilesetsList.Contains(fileName))
                    {
                        File.Copy(filePath, Path.Combine(newProjectDirectory, "tilesets/" + fileName), true);
                    }
                }
                MessageBox.Show("project/images文件夹迁移完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"project/images文件夹迁移出错，原因:{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        void TransferSounds(List<string> bgmsList, List<string> soundsList)
        {
            try
            {
                string inputDirectory = Path.Combine(oldProjectDirectory, "sounds");
                string[] files = Directory.GetFiles(inputDirectory);

                foreach (string filePath in files)
                {
                    string fileName = Path.GetFileName(filePath);
                    if (bgmsList.Contains(fileName))
                    {
                        File.Copy(filePath, Path.Combine(newProjectDirectory, "bgms/" + fileName), true);
                    }
                    if (soundsList.Contains(fileName))
                    {
                        File.Copy(filePath, Path.Combine(newProjectDirectory, "sounds/" + fileName), true);
                    }
                }
                MessageBox.Show("project/sounds文件夹迁移完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"project/sounds文件夹迁移出错，原因:{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}
