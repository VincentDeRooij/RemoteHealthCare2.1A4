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
    /// <Class TestFileIO >
    /// 
    /// This is a seperate class for the unit-tests
    /// To test the functionality of the FileIO classes and its components
    /// 
    /// </summary>

    public class FileIOUnitTest 
    {
        public static void Main(string[] args)
        {
            LogWriter.TestLogWriter();
            DataWriter.TestHistoryWriter();

            Console.WriteLine("---Check log files inside of the bin directories");

            Console.ReadLine();
        }
    }
}


