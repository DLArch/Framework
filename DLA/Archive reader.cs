using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLA
{
    public class Archive_reader
    {
        /// <summary>
        /// Распаковывает архив из ArchivePath в папку DestinationPath,
        /// создавая ее если она отсутствует.
        /// </summary>
        /// <param name="ArchivePath"> Абсолютный/относительный путь к архиву </param>
        /// <param name="DestinationPath"> Абсолютный/относительный путь к папке </param>
        public Archive_reader(string ArchivePath, string DestinationPath)
        {
            this.WorkFolderPath = System.Environment.CurrentDirectory;

            if (!System.IO.Directory.Exists(System.Environment.CurrentDirectory + System.IO.Path.GetDirectoryName(Archive_reader.LogFilePath)))
            {
                System.IO.Directory.CreateDirectory(System.Environment.CurrentDirectory + System.IO.Path.GetDirectoryName(Archive_reader.LogFilePath));
            }

            System.IO.FileStream StreamOfLog = new System.IO.FileStream((Environment.CurrentDirectory + Archive_reader.LogFilePath), System.IO.FileMode.Append, System.IO.FileAccess.Write);
            System.IO.BinaryWriter BinLogWriter = new System.IO.BinaryWriter(StreamOfLog);

            System.IO.FileStream StreamOfAr = new System.IO.FileStream(ArchivePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            System.IO.BinaryReader BinFileReader = new System.IO.BinaryReader(StreamOfAr);

            if (System.IO.Path.IsPathRooted(DestinationPath))
            {
                System.Environment.CurrentDirectory = DestinationPath;
            }
            else
            {
                System.Environment.CurrentDirectory += System.IO.Path.DirectorySeparatorChar + DestinationPath;
            }

            BufferedFileInfo FileInfo = new BufferedFileInfo();

            FileInfo.LogFileHandle = BinLogWriter;

            this.MakeFileFromArchive("", BinFileReader, FileInfo);

            BinFileReader.Close();
            StreamOfAr.Close();

            BinLogWriter.Close();
            StreamOfLog.Close();
        }
        public void MakeFileFromArchive(string Path, System.IO.BinaryReader BinFileReader, BufferedFileInfo FileInfo)
        {
            FileInfo.IsFolder = false;
            FileInfo.NotReadFile = false;

            ++BinFileReader.BaseStream.Position;                                                                        ///|
            this.Method = BinFileReader.ReadInt16();                                                                    ///|Method
            ++BinFileReader.BaseStream.Position;                                                                        ///|Method|
            FileInfo.FileAttributes = (System.IO.FileAttributes)BinFileReader.ReadInt32();                              ///|Method|Атрибуты
            ++BinFileReader.BaseStream.Position;                                                                        ///|Method|Атрибуты|
            FileInfo.FileCreationTime = new DateTime(BinFileReader.ReadInt64());                                        ///|Method|Атрибуты|Д
            ++BinFileReader.BaseStream.Position;                                                                        ///|Method|Атрибуты|Д|
            FileInfo.FileLastAccessTime = new DateTime(BinFileReader.ReadInt64());                                      ///|Method|Атрибуты|Д|Д
            ++BinFileReader.BaseStream.Position;                                                                        ///|Method|Атрибуты|Д|Д|
            FileInfo.FileLastWriteTime = new DateTime(BinFileReader.ReadInt64());                                       ///|Method|Атрибуты|Д|Д|Д
            ++BinFileReader.BaseStream.Position;                                                                        ///|Method|Атрибуты|Д|Д|Д|

            ++BinFileReader.BaseStream.Position;
            FileInfo.FileName = "";
            for (this.buff = BinFileReader.ReadChar(); this.buff != Archive_reader.FileNameDelim; this.buff = BinFileReader.ReadChar())
            {
                FileInfo.FileName += this.buff;
            }                                                                                                           ///|Method|Атрибуты|Д|Д|Д|-Name|

            ++BinFileReader.BaseStream.Position;
            FileInfo.FileDirectoryName = "";
            for (this.buff = BinFileReader.ReadChar(); this.buff != Archive_reader.FileNameDelim; this.buff = BinFileReader.ReadChar())
            {
                FileInfo.FileDirectoryName += this.buff;
            }                                                                                                           ///|Method|Атрибуты|Д|Д|Д|-Name|-Path|

            if ((FileInfo.FileAttributes & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory)
            {
                FileInfo.IsFolder = true;                                                                               ///|Method|Атрибуты|Д|Д|Д|-Name|-Path|
            }
            else
            {
                FileInfo.FileLength = BinFileReader.ReadInt64();                                                        ///|Method|Атрибуты|Д|Д|Д|-Name|-Path|12345
            }
            ++BinFileReader.BaseStream.Position;                                                                        ///|Method|Атрибуты|Д|Д|Д|-Name|-Path|12345|

            /// Создание файла/папки
            FileInfo.MakeFile(Path);

            /// Запись в файл
            if (!FileInfo.IsFolder)
            {
                ///
                /// Конкретизировать обработчик ошибок
                /// Перехватывает пользовательские исключения
                ///
                try
                {
                    /// Изменение указателя в потоке при невозможности записи в файл
                    if (FileInfo.NotReadFile)
                    {
                        BinFileReader.BaseStream.Position += FileInfo.FileLength;
                    }
                    else
                    {
                        /// Проверка на повреждение последнего файла в архиве
                        if ((BinFileReader.BaseStream.Position + FileInfo.FileLength) <= BinFileReader.BaseStream.Length)
                        {
                            ///
                            /// Пишем в bFS с архива
                            ///

                            this.TemporaryFile = System.IO.Path.GetTempFileName();

                            //using (System.IO.FileStream bFS = new System.IO.FileStream(System.Environment.CurrentDirectory + FileInfo.PathModifier(Path), System.IO.FileMode.Open, System.IO.FileAccess.Write))
                            using (System.IO.FileStream bFS = new System.IO.FileStream(this.TemporaryFile, System.IO.FileMode.Open, System.IO.FileAccess.Write))
                            {
                                for (FileInfo.PosBuff = 0; FileInfo.PosBuff < FileInfo.FileLength; ++FileInfo.PosBuff)
                                {
                                    this.ByteBuff = BinFileReader.ReadByte();
                                    bFS.WriteByte(this.ByteBuff);
                                }
                                bFS.Close();
                            }

                            /// Открываем Temp file на чтение для работы метода

                            using (System.IO.FileStream bFS = new System.IO.FileStream(this.TemporaryFile, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                            {
                                ///
                                /// Настройка пути к папке со сборками
                                ///
                                this.NeededAssembly = this.WorkFolderPath + System.IO.Path.DirectorySeparatorChar + AssemblysFolderName + System.IO.Path.DirectorySeparatorChar + @"M" + this.Method + @".dll";

                                ///
                                /// Загрузка сборки сборки
                                ///
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

                                using (System.IO.FileStream DestinationFileStream = new System.IO.FileStream(System.Environment.CurrentDirectory + FileInfo.PathModifier(Path), System.IO.FileMode.Open, System.IO.FileAccess.Write))
                                {
                                    if (this.ClassConstructors != null && ClassConstructors.Count() > 0)
                                    {
                                        this.ClassConstructors[0].Invoke(new object[3] { bFS, DestinationFileStream, false });
                                    }
                                    else
                                    {
                                        Console.WriteLine("Файлы поврежены. Неудалось загрузить из сбоки {0} тип {1}", this.AssemblyConstructorType.Module.FullyQualifiedName, this.AssemblyConstructorType.FullName);
                                        throw new Exception();
                                    }
                                    DestinationFileStream.Close();
                                }
                                bFS.Close();
                            }

                            ///
                            /// Старое чтение
                            ///
                            /*
                            using (System.IO.FileStream bFS = new System.IO.FileStream(System.Environment.CurrentDirectory + FileInfo.PathModifier(Path), System.IO.FileMode.Open, System.IO.FileAccess.Write))
                            {
                                using (System.IO.BinaryReader brFS = new System.IO.BinaryReader(new System.IO.FileStream(this.TemporaryFile, System.IO.FileMode.Open, System.IO.FileAccess.Read)))
                                { 
                                    for (FileInfo.PosBuff = 0; FileInfo.PosBuff < FileInfo.FileLength; ++FileInfo.PosBuff)
                                    {
                                        this.ByteBuff = brFS.ReadByte();
                                        bFS.WriteByte(this.ByteBuff);
                                    }
                                    brFS.Close();
                                }
                                bFS.Close();
                            }
                            */

                            if (System.IO.File.Exists(this.TemporaryFile))
                            {
                                System.IO.File.Delete(this.TemporaryFile);
                            }
                        }
                        else
                        {
                            FileInfo.LogFileHandle.Write("Файл " + System.Environment.CurrentDirectory + FileInfo.PathModifier(Path) + " поврежден при записи и не был разархивирован!");
                            Console.WriteLine("Файл {0} поврежден при записи и не был разархивирован!", System.Environment.CurrentDirectory + FileInfo.PathModifier(Path));
                            return;
                        }
                    }
                }
                catch
                {
                    FileInfo.LogFileHandle.Write("Программа не может получить доступ к " + System.Environment.CurrentDirectory + FileInfo.PathModifier(Path));
                    Console.WriteLine("Программа не может получить доступ к {0}", System.Environment.CurrentDirectory + FileInfo.PathModifier(Path));
                    FileInfo.NotReadFile = true;
                    //BinFileReader.BaseStream.Position += FileInfo.FileLength;
                }
            }

            /// Запись атрибутов
            if (!FileInfo.NotReadFile)
            {
                FileInfo.WriteAttribs(System.Environment.CurrentDirectory + FileInfo.PathModifier(Path));
            }

            if (BinFileReader.BaseStream.Position < BinFileReader.BaseStream.Length)
            {
                MakeFileFromArchive(Path, BinFileReader, FileInfo);
            }
        }
        public string NeededAssembly
        {
            get;
            set;
        }
        public System.Reflection.Assembly MethodAssembly
        {
            get;
            set;
        }
        public System.Type AssemblyConstructorType
        {
            get;
            set;
        }
        public System.Reflection.ConstructorInfo[] ClassConstructors
        {
            get;
            set;
        }
        public string WorkFolderPath
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
        /// <summary>
        /// Символьный буфер
        /// </summary>
        public char buff
        {
            get;
            set;
        }
        /// <summary>
        /// Байтовый буфер. Используется
        /// для снятия данных из архива
        /// </summary>
        public byte ByteBuff
        {
            get;
            set;
        }
        /// <summary>
        /// Номер метода обрабатываемого файла
        /// </summary>
        public Int16 Method
        {
            get;
            set;
        }
        /// <summary>
        /// Разделитель в файловой записи архива
        /// </summary>
        public const char FileNameDelim = '|';
        public const string LogFilePath = @"\Logs\Log.dlal";
        public const string AssemblysFolderName = @"Assemblys";
    }
}
