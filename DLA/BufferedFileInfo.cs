using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLA
{
    public class BufferedFileInfo
    {
        /// <summary>
        /// Зоздание файла/папки по пути
        /// </summary>
        /// <param name="Path"> Относительный путь </param>
        public void MakeFile(string Path)
        {
            Path = PathModifier(Path);
            Console.WriteLine(Path);
            if (IsFolder)
            {
                FolderCreate(Path);
            }
            else
            {
                FileCreate(Path);
            }

            ///Console.WriteLine(Path);
        }
        /// <summary>
        /// Процедура модификации пути
        /// </summary>
        /// <param name="Path"> Относительный путь </param>
        /// <returns> Модифицированный относительный путь </returns>
        public string PathModifier(string Path)
        {
            this.FileDirectoryName = string.Concat(this.FileDirectoryName.Where(x => x != System.IO.Path.VolumeSeparatorChar));
            this.FileName = string.Concat(this.FileName.Where(x => x != System.IO.Path.VolumeSeparatorChar));
            if (this.FileDirectoryName != "")
            {
                Path += System.IO.Path.DirectorySeparatorChar + this.FileDirectoryName + System.IO.Path.DirectorySeparatorChar + this.FileName;
            }
            else
            {
                Path += System.IO.Path.DirectorySeparatorChar + this.FileName;
            }

            Path = string.Concat(Path.Where(x => System.IO.Path.GetInvalidPathChars().Where(y => y == x).Count() <= 0));

            return Path;
        }
        /// <summary>
        /// Создание папки
        /// </summary>
        /// <param name="Path"> Модифицированный относительный путь </param>
        private void FolderCreate(string Path)
        {
            Path = System.Environment.CurrentDirectory + Path;

            if (Path.Length < 248)
            {
                System.IO.Directory.CreateDirectory(Path);
            }
            else
            {
                this.NotReadFile = true;
                this.LogFileHandle.Write("Папка " + Path + " не была записанна из-за слишком длинного пути.");
                Console.WriteLine("Папка {0} не была записанна из-за слишком длинного пути.", Path);
            }
        }
        /// <summary>
        /// Создание файла
        /// </summary>
        /// <param name="Path"> Модифицированный относительный путь </param>
        private void FileCreate(string Path)
        {
            Path = System.Environment.CurrentDirectory + Path;

            try
            {
                if (!System.IO.File.Exists(Path) && Path.Length < 260)
                {
                    System.IO.File.Create(Path).Close();
                }
                else
                {
                    if (System.IO.File.Exists(Path) && Path.Length < 260)
                    {
                        ///Console.WriteLine("Файл {0} уже существует. Файл будет перезаписан.", Path);
                        this.LogFileHandle.Write("Файл " + Path + " уже существует. Файл будет перезаписан.");
                        System.IO.File.Delete(Path);
                        System.IO.File.Create(Path).Close();
                    }
                    else
                    {
                        this.NotReadFile = true;
                        this.LogFileHandle.Write("Файл " + Path + " не был записан из-за слишком длинного пути.");
                        Console.WriteLine("Файл {0} не был записан из-за слишком длинного пути.", Path);
                    }
                }
            }
            catch
            {
                this.LogFileHandle.Write("Файл " + Path + " не был записан из-за слишком длинного пути.");
                Console.WriteLine("Файл {0} не был записан из-за слишком длинного пути.", Path);
            }
        }
        /// <summary>
        /// Запись атрибутов
        /// </summary>
        /// <param name="Path"> Абсолютный путь </param>
        public void WriteAttribs(string Path)
        {
            if (this.IsFolder)
            {
                if (!System.IO.Directory.Exists(Path))
                {
                    System.IO.DirectoryInfo FileInfo = new System.IO.DirectoryInfo(Path);

                    FileInfo.CreationTime = this.FileCreationTime;
                    FileInfo.LastAccessTime = this.FileLastAccessTime;
                    FileInfo.LastWriteTime = this.FileLastWriteTime;
                    FileInfo.Attributes = this.FileAttributes;
                }
            }
            else
            {
                System.IO.FileInfo FileInfo = new System.IO.FileInfo(Path);

                FileInfo.CreationTime = this.FileCreationTime;
                FileInfo.LastAccessTime = this.FileLastAccessTime;
                FileInfo.LastWriteTime = this.FileLastWriteTime;
                FileInfo.Attributes = this.FileAttributes;
            }
        }
        public System.IO.BinaryWriter LogFileHandle
        {
            get;
            set;
        }
        /// <summary>
        /// Атрибуты файла
        /// </summary>
        public System.IO.FileAttributes FileAttributes
        {
            get;
            set;
        }
        /// <summary>
        /// Время создания файла
        /// </summary>
        public DateTime FileCreationTime
        {
            get;
            set;
        }
        /// <summary>
        /// Время последнего доступа к фалу
        /// </summary>
        public DateTime FileLastAccessTime
        {
            get;
            set;
        }
        /// <summary>
        /// Время последней записи в файл
        /// </summary>
        public DateTime FileLastWriteTime
        {
            get;
            set;
        }
        /// <summary>
        /// Имя файла
        /// </summary>
        public string FileName
        {
            get;
            set;
        }
        /// <summary>
        /// Относительный путь к файлу
        /// </summary>
        public string FileDirectoryName
        {
            get;
            set;
        }
        /// <summary>
        /// Размер файла
        /// </summary>
        public Int64 FileLength
        {
            get;
            set;
        }
        /// <summary>
        /// Указывает, читать ли файл или нет
        /// True = нечитать
        /// </summary>
        public bool NotReadFile
        {
            get;
            set;
        }
        /// <summary>
        /// Является ли объект папкой true = папка
        /// </summary>
        public bool IsFolder
        {
            get;
            set;
        }
        /// <summary>
        /// Позиция указателя в файле
        /// </summary>
        public Int64 PosBuff
        {
            get;
            set;
        }
    }
}
