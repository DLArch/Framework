using System;
using System.Linq;

namespace DLA
{
    public class Archive_creator
    {
        /// <summary>
        /// Создает новый архив
        /// </summary>
        /// <param name="Spath"> Путь к папке с файлами для архивации </param>
        /// <param name="Apath"> Путь к архиву </param>
        /// <param name="Method"> Номер метода для файлов </param>
        public Archive_creator(string Spath, string Apath = @"789987", UInt16 Method = 0)
        {
            init(Apath, Spath, Method);
            Create_Archive(Spath);
        }
        private Archive_creator(string Spath, System.IO.BinaryWriter Wr, string FilesPath, string TemporaryFolder, string Apath = "789987", UInt16 Method = 0)
        {
            this.FilesPath = FilesPath;
            this.ArchPath = Apath;
            this.MethodIndex = Method;
            this.TemporaryFolder = TemporaryFolder;
            /// <summary>
            /// Для предотвращения вылета при отказе в доступе к папке
            /// </summary>
            try
            {
                foreach (var z in System.IO.Directory.EnumerateFileSystemEntries(Spath))
                {
                    Compress(z, Wr);
                }
            }
            catch (System.Security.SecurityException)
            {
                Console.WriteLine("Except: Отказанно в доступе к папке: {0}", Spath);
            }
        }
        /// <summary>
        /// Инициализирует поля класса
        /// </summary>
        /// <param name="path"> Путь для инициализации класса </param>
        /// <param name="method"> Номер метода для файлов </param>
        private void init(string ArcPath, string StartPath, UInt16 method)
        {
            if (ArcPath == "789987")
            {
                ArcPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + System.IO.Path.DirectorySeparatorChar + Archive_creator.DefaultArchiveName;
            }
            if (ArcPath.Where(x => x == DefaultExtensionDelimiter).Count() != 0)
            {
                this.ArchPath = string.Concat(ArcPath.Take(ArcPath.Length - (string.Concat(ArcPath.Reverse().TakeWhile(x => x != DefaultExtensionDelimiter)) + DefaultExtensionDelimiter).Length)) + Archive_creator.Extension;
            }
            else
            {
                this.ArchPath = ArcPath + Archive_creator.Extension;
            }
            this.MethodIndex = method;
            if (System.IO.Directory.Exists(StartPath))
            {
                this.FilesPath = System.IO.Path.GetDirectoryName(StartPath);
            }
            else
            {
                this.FilesPath = System.IO.Path.GetFullPath(StartPath);
            }
            if (this.FilesPath == null)
            {
                this.FilesPath = StartPath;
            }
            this.TemporaryFolder = System.IO.Path.GetTempPath() + System.IO.Path.DirectorySeparatorChar + TemporaryFolderBaseName;
            if (!System.IO.Directory.Exists(this.TemporaryFolder))
            {
                System.IO.Directory.CreateDirectory(this.TemporaryFolder);
            }
        }
        /// <summary>
        /// Создает и заполняет архивный файл
        /// </summary>
        /// <param name="Spath"> Путь к папке/файлам для архивации </param>
        private void Create_Archive(string Spath)
        {
            System.IO.FileStream CreatedFile = System.IO.File.Create(this.ArchPath);

            ///
            /// TODO: Переделать  задание аттрибутов архиву
            ///
            System.IO.File.SetAttributes(this.ArchPath, System.IO.File.GetAttributes(this.ArchPath) | System.IO.FileAttributes.Archive | System.IO.FileAttributes.ReparsePoint | System.IO.FileAttributes.Compressed);

            CreatedFile.Close();

            using (System.IO.FileStream StreamOfCreatedFile = new System.IO.FileStream(this.ArchPath, System.IO.FileMode.Append, System.IO.FileAccess.Write))
            {
                System.IO.BinaryWriter BinFileWriter = new System.IO.BinaryWriter(StreamOfCreatedFile);

                Compress(Spath, BinFileWriter);

                BinFileWriter.Close();
                StreamOfCreatedFile.Close();
            }

            if (System.IO.File.Exists(this.TemporaryFile))
            {
                System.IO.File.Delete(this.TemporaryFile);
            }
            if (System.IO.Directory.Exists(this.TemporaryFolder))
            {
                System.IO.Directory.Delete(this.TemporaryFolder);
            }
        }
        /// <summary>
        /// Сжимает папку/файл
        /// </summary>
        /// <param name="path"> Путь к файлу/папке, который[ую] необходимо сжать </param>
        /// <param name="BinFileWriter"> Поток записи в архив </param>
        private void Compress(string path, System.IO.BinaryWriter BinFileWriter)
        {
            ///
            /// Если архив
            ///
            if (path == this.ArchPath || path == (AppDomain.CurrentDomain.BaseDirectory + this.ArchPath))
            {
                Console.WriteLine("---------------------\n|      Except\n---------------------\n|Невозможно считать файл, в котором формируется архив\n|Файл записан не будет\n|Путь к файлу: {0}\n---------------------", path);
                return;
            }

            this.NotTempFile = false;

            if (System.IO.File.Exists(path))
            {
                try
                {
                    this.TemporaryFile = System.IO.Path.GetTempFileName();

                    this.StreamOfBaseFile = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read);

                    ++StreamOfBaseFile.Position;

                    this.TemporaryFileStream = System.IO.File.Create(this.TemporaryFile);

                    this.NeededAssembly = System.Environment.CurrentDirectory + System.IO.Path.DirectorySeparatorChar + AssemblysFolderName + System.IO.Path.DirectorySeparatorChar + @"M" + this.MethodIndex + @".dll";

                    try
                    {
                        if (System.IO.File.Exists(this.NeededAssembly))
                        {
                            this.MethodAssembly = System.Reflection.Assembly.LoadFile(this.NeededAssembly);
                        }
                        else
                        {
                            Console.WriteLine("Файл отсутствуeт: {1} Переустановите программу!", this.NeededAssembly);
                            throw new Exception();
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Сборка {0} повреждена. Загрузите ее заново!", this.NeededAssembly);
                        throw new Exception();
                    }

                    /// TDOD: Проанализировать возможность выполнения через System.AppDomain.CurrentDomain.ExecuteAssembly
                    /// Без загрузки в текуший домен
                    ///
                    ///---------------------------------------------------
                    ///
                    /// Вытаскивание конструкторов из класса и вызов 0ого.
                    /// TODO: Конкретизировать выбор конструктора
                    ///
                    this.AssemblyConstructorType = this.MethodAssembly.GetType("Method.Method");
                    this.ClassConstructors = this.AssemblyConstructorType.GetConstructors();
                    //System.Reflection.ConstructorInfo ci = ti.GetConstructor(new Type[1] { tis/*this.TemporaryFile.GetType()*//*, true.GetType()*/});

                    if (this.ClassConstructors != null && ClassConstructors.Count() > 0)
                    {
                        this.ClassConstructors[0].Invoke(new object[3] { this.StreamOfBaseFile, this.TemporaryFileStream, true });
                    }
                    else
                    {
                        Console.WriteLine("Файлы поврежены. Неудалось загрузить из сбоки {0} тип {1}", this.AssemblyConstructorType.Module.FullyQualifiedName, this.AssemblyConstructorType.FullName);
                        throw new Exception();
                    }
                    ///---------------------------------------------------
                    /// Проверка изменения пути к файлу и удаление отосланного, если он был переписан в новый
                    ///

                    if (this.TemporaryFileStream.Length == 0)
                    {
                        if (System.IO.File.Exists(this.TemporaryFile))
                        {
                            this.TemporaryFileStream.Close();
                            System.IO.File.Delete(this.TemporaryFile);
                            this.NotTempFile = true;
                        }
                        this.TemporaryFile = path;
                    }
                    this.TemporaryFileStream.Close();
                    this.StreamOfBaseFile.Close();

                    this.StreamOfBaseFile = new System.IO.FileStream(this.TemporaryFile, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                }
                catch (System.IO.IOException)
                {
                    Console.WriteLine("Невозможно получить доступ к файлу: " + path);
                    Console.WriteLine("Продолжить работу с ошибкой [y/n]");
                    var KeyPressed = Console.ReadKey().KeyChar;
                    if (KeyPressed == 'y' || KeyPressed == 'Y')
                    {
                        return;
                    }
                    else
                    {
                        BinFileWriter.BaseStream.Close();
                        BinFileWriter.Close();
                        throw new Exception("Программа остановлена по запросу пользователя");
                    }
                }

                MakeFileInArchive(path, this.TemporaryFile, BinFileWriter);

                /// <summary>
                /// Чтение файла из потока StreamOfBaseFile
                /// TODO: Ускорить чтение, путем чтения не одного байта, а набора байтов сразу
                /// Количество выделяемых байт под буффер должно определяться автоматически,
                /// в зависимости от количества доступной оперативной памяти
                /// </summary>

                for (this.Byte_Buff = 0; this.StreamOfBaseFile.Position < this.StreamOfBaseFile.Length;)
                {
                    this.Byte_Buff = (byte)this.StreamOfBaseFile.ReadByte();
                    BinFileWriter.Write(this.Byte_Buff);
                }

                this.StreamOfBaseFile.Close();

                ///
                /// Удаление временного файла
                ///
                if (System.IO.File.Exists(this.TemporaryFile) && !this.NotTempFile)
                {
                    System.IO.File.Delete(this.TemporaryFile);
                }
            }
            else
            {
                MakeFileInArchive(path, this.TemporaryFile, BinFileWriter);
                //MakeFileInArchive(this.TemporaryFile, BinFileWriter);
                ///
                /// По идее кушает памяти больше чем ниже преведенный фрагмент
                ///
                //new Archive_creator(path, BinFileWriter, this.FilesPath, this.TemporaryFolder, this.ArchPath, this.MethodIndex);
                /// <summary>
                /// Для предотвращения вылета при отказе в доступе к папке
                /// </summary>
                try
                {
                    foreach (var npath in System.IO.Directory.EnumerateFileSystemEntries(path))
                    {
                        Compress(npath, BinFileWriter);
                    }
                }
                catch (System.Security.SecurityException)
                {
                    Console.WriteLine("Except: Отказанно в доступе к папке: {0}", path);
                }
            }
        }
        /// <summary>
        /// Записывает файловую запись в архив
        /// </summary>
        /// <param name="path"> Путь к файлу/папке, свединия о котором необходимо занести в архив </param>
        /// <param name="BinFileWriter"> Поток записи в архив </param>
        public void MakeFileInArchive(string PathOfOriginalSource, string PathOfRealSource, System.IO.BinaryWriter BinFileWriter)
        {
            System.IO.FileInfo FileAttrib = new System.IO.FileInfo(PathOfOriginalSource);
            BinFileWriter.Write('|');
            BinFileWriter.Write(this.MethodIndex);
            BinFileWriter.Write('|');
            BinFileWriter.Write((Int32)FileAttrib.Attributes);
            BinFileWriter.Write('|');
            BinFileWriter.Write(FileAttrib.CreationTime.Ticks);
            BinFileWriter.Write('|');
            BinFileWriter.Write(FileAttrib.LastAccessTime.Ticks);
            BinFileWriter.Write('|');
            BinFileWriter.Write(FileAttrib.LastWriteTime.Ticks);
            BinFileWriter.Write('|');
            BinFileWriter.Write(FileAttrib.Name);
            BinFileWriter.Write('|');

            if (FileAttrib.DirectoryName != null)
            {
                ///Убирает общую часть пути
                StrBuff = string.Concat(FileAttrib.DirectoryName.Where((x, i) => i > this.FilesPath.Length));
                Console.WriteLine(StrBuff + System.IO.Path.DirectorySeparatorChar + FileAttrib.Name);
            }
            else
            {
                StrBuff = this.FilesPath;
            }
            BinFileWriter.Write(StrBuff);
            BinFileWriter.Write('|');
            if (System.IO.File.Exists(PathOfOriginalSource))
            {
                FileAttrib = new System.IO.FileInfo(PathOfRealSource);
                BinFileWriter.Write(FileAttrib.Length);
            }
            BinFileWriter.Write('|');
        }
        /// <summary>
        /// Файловый поток
        /// </summary>
        public System.IO.FileStream StreamOfBaseFile
        {
            get;
            set;
        }
        /// <summary>
        /// Строковый буфер
        /// </summary>
        public string StrBuff
        {
            get;
            set;
        }
        /// <summary>
        /// Байтовый буфер
        /// </summary>
        public byte Byte_Buff
        {
            get;
            set;
        }
        /// <summary>
        /// Путь к временной папке
        /// </summary>
        public string TemporaryFolder
        {
            get;
            set;
        }
        /// <summary>
        /// Имя временного файла во временной папке
        /// </summary>
        public string TemporaryFile
        {
            get;
            set;
        }
        public System.IO.FileStream TemporaryFileStream
        {
            get;
            set;
        }
        public bool NotTempFile
        {
            get;
            set;
        }
        /// <summary>
        /// Разделитель расширения
        /// </summary>
        public const char DefaultExtensionDelimiter = '.';
        /// <summary>
        /// Расширение архива
        /// </summary>
        public const string Extension = @".dla";
        /// <summary>
        /// Стандартное имя временной папки
        /// </summary>
        public const string TemporaryFolderBaseName = @"Dla";
        /// <summary>
        /// Путь и расширение log - файла
        /// </summary>
        public const string LogFileName = @"Log.dla";
        /// <summary>
        /// Стандартное название архива
        /// </summary>
        public const string DefaultArchiveName = @"Arch0.dla";
        public const string AssemblysFolderName = "Assemblys";
        /// <summary>
        /// Путь к архиву
        /// </summary>
        public string ArchPath
        {
            get;
            set;
        }
        /// <summary>
        /// Путь к файлам архивации
        /// </summary>
        public string FilesPath
        {
            get;
            set;
        }
        /// <summary>
        /// Номер метода для сжатия. Если 0, система
        /// автоматически определяет наиболее подходящий
        /// </summary>
        public UInt16 MethodIndex
        {
            get;
            set;
        }
        /// <summary>
        /// Загружаемая сборка
        /// </summary>
        public System.Reflection.Assembly MethodAssembly
        {
            get;
            set;
        }
        /// <summary>
        /// Метод из загружаемой сборки
        /// </summary>
        public object MethodClass
        {
            get;
            set;
        }
        /// <summary>
        /// Путь к сборке
        /// </summary>
        public string NeededAssembly
        {
            get;
            set;
        }
        /// <summary>
        /// Тип из сборки(класс)
        /// </summary>
        public System.Type AssemblyConstructorType
        {
            get;
            set;
        }
        /// <summary>
        /// Массив конструкторов класса
        /// </summary>
        public System.Reflection.ConstructorInfo[] ClassConstructors
        {
            get;
            set;
        }
    }
}