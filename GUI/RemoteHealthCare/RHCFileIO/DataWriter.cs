using RHCFileIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RHCFileIO
{
    /// <Usage DataWriter>
    /// 
    /// To initialize create a DataWriter attribute
    /// So DataWriter dataWriter = new DataWriter();
    /// 
    /// From there you can WriteTheCompleteHistory, and read back a certain patients history
    /// 
    /// </summary>
    /// 

    #region DataWriter

    public class DataWriter
    {
        private PatientOverview history;

        public PatientOverview GetHistory
        { get { return this.history; } set { this.history = value; } }

        private DirectoryInfo dataSaveDirectory;
        private DirectoryInfo historyPath;
        private StreamWriter streamWriter;
        private string dataPathOutput;
        private bool appendBoolean;


        public DataWriter(PatientOverview history)
        {
            this.history = history;

            this.dataSaveDirectory = Directory.CreateDirectory(Directory.GetCurrentDirectory().Replace("Debug", "Data")); // save location of the server data pre-set directory for history saving and access
            this.historyPath = CreateSubFolder("DataHistory");
            this.dataPathOutput = Path.Combine(dataSaveDirectory.FullName, "Rando.data");
            this.appendBoolean = true;
        }

        private void WriteStringToFile(string filePath, string dataString)
        {
            if (File.Exists(Path.Combine(this.historyPath.FullName, filePath + ".data")))
            {
                this.appendBoolean = true;
            }
            else
            {
                this.appendBoolean = false;
            }
            this.streamWriter = new StreamWriter(filePath, appendBoolean);

            streamWriter.Write(dataString);
            streamWriter.Flush();
            streamWriter.Close();
        }

        private DirectoryInfo CreateSubFolder(string path) // creates a new directory inside the project and returns the directory path
        {
            DirectoryInfo dirPath = Directory.CreateDirectory(this.dataSaveDirectory.FullName + @"/" + path);
            return dirPath;
        }

        private string CreateNewFile(string filePath, string fileName)
        {
            return Path.Combine(filePath, fileName);
        }

        public void WriteAllPatientsData()
        {
            string jsonData = "";
            foreach (PatientData data in this.history.HistoryData)
            {
                jsonData = JsonConvert.SerializeObject(data);
                WriteStringToFile(CreateNewFile(this.historyPath.FullName, data.patientID + ".data"), jsonData);
            }
        }

        public void WriteHistoryOfPatient(PatientData data) 
        {
            string jsonData = "";
            jsonData = JsonConvert.SerializeObject(data);
            WriteStringToFile(CreateNewFile(this.historyPath.FullName, data.patientID + ".data"), jsonData);
        }

        private dynamic ReadPatientData(string patientID)
        {
            using (StreamReader streamReader = new StreamReader(Path.Combine(this.historyPath.FullName, patientID + ".data")))
            {
                dynamic json = streamReader.ReadToEnd();
                streamReader.Close();
                return json;
            }
        }

        public string TestReadPatientData(string patientID)
        {
            return ReadPatientData(patientID);
        }

        public PatientData GetPatientData(string patientID)
        {
            JObject jObject = JObject.Parse(ReadPatientData(patientID));
            dynamic dataObject = jObject.Value<JObject>().ToObject<PatientData>();

            return dataObject;
        }

        public static void TestHistoryWriter()
        {
            PatientOverview historyManager = new PatientOverview();

            BikeData bikeData1 = new BikeData("00463");

            HeartData heartData1 = new HeartData();

            PatientData dataOverviewHH = new PatientData("Harry Harolds", bikeData1, heartData1);

            BikeData bikeData3 = new BikeData("004413");

            HeartData heartData3 = new HeartData();

            PatientData dataOverviewHH2 = new PatientData("Harry Harolds", bikeData3, heartData3);

            BikeData bikeData2 = new BikeData("12432");

            HeartData heartData2 = new HeartData();

            PatientData dataOverviewBH = new PatientData("Barry Harolds", bikeData2, heartData2);

            BikeData bikeData4 = new BikeData("432141");
          
            HeartData heartData4 = new HeartData();

            PatientData dataOverviewHH3 = new PatientData("Harry Harolds-Other", bikeData4, heartData4);

            historyManager.HistoryData.Add(dataOverviewHH);
            historyManager.HistoryData.Add(dataOverviewBH);
            historyManager.HistoryData.Add(dataOverviewHH2);

            DataWriter data = new DataWriter(new PatientOverview()); // new HistoryManager(); irrelevant for testing
            data.GetHistory = historyManager;
            data.WriteAllPatientsData();
            string json = data.TestReadPatientData("Harry Harolds");
            Console.WriteLine(json);
            PatientData dataHH = data.GetPatientData("Harry Harolds");
            Console.WriteLine(dataHH.patientID);
            data.WriteHistoryOfPatient(dataOverviewHH3);
            Console.WriteLine(data.ReadPatientData("Harry Harolds-Other"));
            //Console.WriteLine(dataOverviewHH3.patientID);

        }
    }

    #endregion

}
