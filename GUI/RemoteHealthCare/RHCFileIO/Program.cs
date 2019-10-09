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
    public class Program 
    {
        public static void Main(string[] args)
        {
            DataWriter data = new DataWriter(new HistoryManager()); // new HistoryManager(); irrelevant for testing
            data.GetHistory = data.TestHistoryWriter();
            data.WriteCompleteHistory();
            string json = data.TestReadPatientData("Harry Harolds");
            Console.WriteLine(json);
            DataOverview dataHH = data.GetPatientData("Harry Harolds");
            Console.WriteLine(dataHH.patientID);
            Console.ReadLine();

        }
    }
}


