using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace Grafer
{
    public class FileCompiler
    {
        public string Path { get; } = string.Empty;

        public bool HaveHead { get; }

        public string Head { get; private set; } = string.Empty;

        public string ErrorMessage { get; private set; } = string.Empty;

        public List<string> Data { get; } = new List<string>();

        public string Name { get; private set; } = string.Empty;

        public Encoding Encoding { get; }

        public char SplittingChar { get; } = '\0';

        public string PureName { get; private set; } = string.Empty;

        public string Type { get; private set; } = string.Empty;

        public bool IsOK { get; }

        public bool IsDataOK { get; private set; }

        public bool WrittenSuccesfuly { get; private set; }

        public enum FileType
        {
            TXT,
            CSV
        }

        private int splittingCharCount;

        private int splittingCharCountErrorLine = -1;

        private readonly FileInfo? fileInfo;

        private bool readedSuccessfuly;

        public FileCompiler()
        {
            Path = "";
            HaveHead = false;
            Encoding = Encoding.UTF8;
            Data = new List<string>();
            SplittingChar = '\0';
        }

        public FileCompiler(string filePath, bool haveFileHead)
        {
            Path = filePath;
            HaveHead = haveFileHead;
            Encoding = Encoding.UTF8;
            Data = new List<string>();
            fileInfo = new FileInfo(Path);
            if (IsOK = IsFileOK() == true)
            {
                SetFileInfo();
            }
            SplittingChar = (Type == FileType.CSV.ToString().ToLower()) ? ';' : '\0';
        }

        public FileCompiler(string filePath, bool haveFileHead, char splittingChar)
        {
            Path = filePath;
            HaveHead = haveFileHead;
            SplittingChar = splittingChar;
            Encoding = Encoding.UTF8;
            Data = new List<string>();
            fileInfo = new FileInfo(Path);
            if (IsOK = IsFileOK() == true)
            {
                SetFileInfo();
            }
        }

        public FileCompiler(string filePath, bool haveFileHead, char splittingChar, Encoding encoding)
        {
            Path = filePath;
            HaveHead = haveFileHead;
            SplittingChar = splittingChar;
            Encoding = encoding;
            Data = new List<string>();
            fileInfo = new FileInfo(Path);
            if (IsOK = IsFileOK() == true)
            {
                SetFileInfo();
            }
        }

        //Jestli je soubor v pořádku.
        private bool IsFileOK()
        {
            return FileExists() && HaveFileExtension() && IsFileTypeOK();
        }

        //Získání informací o souboru.
        private void SetFileInfo()
        {
            Name = GetFileName();
            PureName = Name.Split('.')[0];
            Type = Name.Split('.')[1];
        }

        //Jestli má soubor příponu.
        private bool HaveFileExtension()
        {
            ErrorMessage = (fileInfo.Extension == "") ? "File doesn't have extension" : "";
            return ErrorMessage.Length == 0;
        }

        //Získání jména souboru.
        private string GetFileName()
        {
            string[] filePath = Path.Split('\\');
            return filePath[^1];
        }
        /// <summary>
        /// Read file, and if everything is OK, then it will save to Data property. If not then you can display ErrorMessage where problem is stored.  
        /// </summary>
        public void Read()
        {
            if (IsOK)
            {
                ReadFile();

                if (readedSuccessfuly)
                {
                    RemoveBlankLines();

                    if (HaveHead)
                    {
                        GetFileHead();
                    }

                    CheckData();
                }
            }
        }

        //Přečtení souboru.
        private void ReadFile()
        {
            try
            {
                Data.AddRange(File.ReadAllLines(Path));
                readedSuccessfuly = true;
            }
            catch (IOException)
            {
                ErrorMessage = "File is used by another program.";
                readedSuccessfuly = false;
            }
        }

        //Kontrola zda nikde nechybí středník.
        private void CheckData()
        {
            if (Type == FileType.CSV.ToString().ToLower() && !IsFileEmpty())
            {
                ErrorMessage += EqualSplittingCharCount() ? "" : "First and " + splittingCharCountErrorLine + " line don't have same count of splitting chars.";
            }

            IsDataOK = ErrorMessage.Length == 0;
        }

        //Získání hlavičky souboru.
        private void GetFileHead()
        {
            if (Data.Count != 0)
            {
                Head = Data[0];
                Data.RemoveAt(0);
            }
        }

        //Odebrání prázdných řádků.
        private void RemoveBlankLines()
        {
            Data.RemoveAll(s => s.Trim() == "");
        }

        /// <summary>
        /// Creates a new file and write specified data.
        /// </summary>
        public void Write(string path, List<string> data)
        {
            WriteToFile(path, data);
        }

        /// <summary>
        /// Creates a new file and write specified data to the file with head at top of data.
        /// </summary>
        public void Write(string path, List<string> data, string head)
        {
            data.Insert(0, head);
            WriteToFile(path, data);
        }

        //Zápis do souboru.
        private void WriteToFile(string path, List<string> data)
        {
            try
            {
                File.WriteAllLines(path, data.ToArray());
            }
            catch
            {
                ErrorMessage = "Writing to this directory is denied.";
                WrittenSuccesfuly = false;
            }
        }

        //Porovnání zda má každý řádek stejný počet oddělovacích znaků.
        private bool EqualSplittingCharCount()
        {
            splittingCharCount = GetDefaultSplittingCharCount();

            for (int i = 0; i < Data.Count && splittingCharCountErrorLine == -1; i++)
            {
                if (splittingCharCount != GetLineSplitingCharCount(Data[i]))
                {
                    splittingCharCountErrorLine = (HaveHead) ? (i + 2) : (i + 1);
                }
            }

            return splittingCharCountErrorLine == -1;
        }

        //Získání základního počtu oddělovačů na řádku.
        private int GetDefaultSplittingCharCount()
        {
            return HaveHead ? GetLineSplitingCharCount(Head) : GetLineSplitingCharCount(Data[0]);
        }

        //Získání počtu oddělovacích znaků.
        private int GetLineSplitingCharCount(string line)
        {
            int lineSplitingCharCount = 0;

            foreach (char character in line)
            {
                if (character == SplittingChar)
                {
                    lineSplitingCharCount++;
                }
            }

            return lineSplitingCharCount;
        }

        //Je soubor txt nebo csv.
        private bool IsFileTypeOK()
        {
            ErrorMessage = Enum.IsDefined(typeof(FileType), fileInfo.Extension.Remove(0, 1).ToUpper()) ? "" : "File is not txt or csv type.";
            return ErrorMessage.Length == 0;
        }

        //Jestli existuje soubor.
        private bool FileExists()
        {
            ErrorMessage = File.Exists(Path) ? "" : "File doesn't exists.";
            return ErrorMessage.Length == 0;
        }

        //Jestli je soubor prádzný.
        private bool IsFileEmpty()
        {
            ErrorMessage = Data.Count == 0 ? "File is empty." : "";
            return ErrorMessage.Length != 0;
        }
    }

}

