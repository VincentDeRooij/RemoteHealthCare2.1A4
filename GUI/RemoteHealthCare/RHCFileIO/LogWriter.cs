using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RHCFileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileManager
{
    /// <Usage LogWriter>
    /// 
    /// To initialize create a LogWriter attribute
    /// So LogWriter logWriter = new LogWriter();
    /// 
    /// From there you can ask for write methods, folder/file create methods, logOutputPath (this is always inside of the bin folder of a the project it runs from)
    /// C:\Users\{your_username}\OneDrive\Documenten\....\{Solution_name}\RHCFileIO\Logs (standard path)
    /// 
    /// </summary>
    /// 

    #region LogWriter

    public class LogWriter
    {
        private DirectoryInfo logDirectory;
        private int logNumber;
        private string logEntry;
        private string dateTime;
        private string logPathOutput = Path.Combine(Directory.GetCurrentDirectory().Replace("Debug", "Logs"));
        private StreamWriter streamWriter;

        public LogWriter()
        {
            this.logDirectory = Directory.CreateDirectory(Directory.GetCurrentDirectory().Replace("Debug", "Logs"));
            NewLogFile("Server.log");
            this.logEntry = this.GetLastEntryNumber().ToString();
            this.dateTime = DateTime.Now.ToString() + ":   ";
            Int32.TryParse(logEntry, out logNumber);
        }

        public DirectoryInfo CreateNewFolderInsideProject(string path) // creates a new directory inside the project and returns the directory path
        {
            DirectoryInfo dirPath = Directory.CreateDirectory(Directory.GetCurrentDirectory().Replace("Debug", path));
            return dirPath;
        }

        public int GetLastEntryNumber()
        {
            int parsedInt = 0;
            StreamReader reader;
            try
            {
                reader = new StreamReader(logPathOutput);
            }
            catch (FileNotFoundException)
            {
                return 1;
            }

            while (!reader.EndOfStream)
            {
                try
                {
                    string read = reader.ReadLine();
                    string entryNumber = read.Substring(read.IndexOf(" "), read.IndexOf(" )"));

                    Int32.TryParse(entryNumber, out parsedInt);
                }
                catch (NullReferenceException ex)
                {
                    reader.Close();
                    return 1;
                }
            }
            reader.Close();
            return parsedInt;
        }

        public string GetLogPath()
        {
            return this.logPathOutput;
        }

        public void NewLogFile(string newFile)
        {
            this.logPathOutput = Path.Combine(Directory.GetCurrentDirectory().Replace("Debug", "Logs"), newFile);
        }

        public DirectoryInfo CreateNewFolder(string path)
        {
            DirectoryInfo dirPath = Directory.CreateDirectory(path);
            return dirPath;
        }

        public void WriteTextToFile(string filePath, string logString)
        {
            this.streamWriter = new StreamWriter(logPathOutput, true);
            this.logPathOutput = filePath;
            this.logEntry = this.logNumber.ToString();
            this.dateTime = DateTime.Now.ToString() + ":   ";
            streamWriter.WriteLine("( " + this.logEntry + " )  " + this.dateTime + logString);
            streamWriter.Flush();
            streamWriter.Close();
            this.logNumber++;
        }

        public void WriteBytesToFile(string filePath, byte[] logBytes)
        {
            this.streamWriter = new StreamWriter(filePath, true);
            this.logEntry = this.logNumber.ToString();
            this.dateTime = DateTime.Now.ToString() + ":  ";
            string data = Encoding.UTF8.GetString(logBytes);
            streamWriter.WriteLine("( " + this.logEntry + " )  " + this.dateTime + data);
            streamWriter.Close();
            streamWriter.Flush();
            this.logNumber++;
        }
    }

    #endregion

    }


