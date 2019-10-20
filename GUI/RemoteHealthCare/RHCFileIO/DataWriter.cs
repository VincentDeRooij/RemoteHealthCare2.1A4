using RHCFileIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

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
        private DirectoryInfo patientListingPath;
        private string patientListingFile;
        public List<string> patients;
        private StreamWriter streamWriter;
        private string dataPathOutput;
        private bool appendBoolean;

        public DataWriter(PatientOverview history)
        {
            this.history = history;
            this.patientListingPath = Directory.CreateDirectory(Directory.GetCurrentDirectory().Replace("Debug", "Patient"));
            this.patientListingFile = CreateNewFile(patientListingPath.FullName, "patient.list");
            this.dataSaveDirectory = Directory.CreateDirectory(Directory.GetCurrentDirectory().Replace("Debug", "Data")); // save location of the server data pre-set directory for history saving and access
            this.historyPath = CreateSubFolder("DataHistory");
            this.dataPathOutput = Path.Combine(dataSaveDirectory.FullName, "Rando.data");
            this.appendBoolean = true;
        }

        private void WriteStringToFile(string filePath, string dataString)
        {
            if (File.Exists(Path.Combine(this.historyPath.FullName, filePath)) || (!(File.Exists(Path.Combine(this.patientListingPath.FullName, patientListingFile)))))
            {
                this.appendBoolean = false;
            }
            else
            {
                this.appendBoolean = true;
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
            if (!File.Exists(Path.Combine(filePath, fileName)))
            {
                using (File.Create(Path.Combine(filePath, fileName)))
                {
                }
            }
            return Path.Combine(filePath, fileName);
        }

        public void WriteAllPatientsData()
        {
            string jsonData = "";
            foreach (PatientData data in this.history.PatientDataBase)
            {
                WritePatientIDs(data.patientID);
                jsonData = JsonConvert.SerializeObject(data);
                WriteStringToFile(CreateNewFile(this.historyPath.FullName, data.patientID + ".data"), jsonData);
            }
        }

        public void WritePatientIDs(string patient) 
        {
            if (File.Exists(Path.Combine(this.patientListingPath.FullName, patientListingFile)))
            {
                this.appendBoolean = true;
            }
            else
            {
                this.appendBoolean = false;
            }
            try
            {
                List<string> patientInside = ReadPatients();
                if (!patientInside.Contains(patient))
                {
                    this.streamWriter = new StreamWriter(Path.Combine(this.historyPath.FullName, patientListingFile), appendBoolean);
                    streamWriter.Write(patient + "_");
                    streamWriter.Flush();
                    streamWriter.Close();
                }
            }
            catch (FileNotFoundException)
            {
                
            }
        }
        
        public List<string> ReadPatients() 
        {
            List<string> patientIDs = new List<string>();
            using (StreamReader streamReader = new StreamReader(Path.Combine(this.historyPath.FullName, patientListingFile)))
            {
                dynamic ids = streamReader.ReadToEnd();

                string[] id = Regex.Split(ids, "_");
                List<string> idList = id.ToList();
               

                foreach (var item in idList)
                {
                    bool duplicate = idList.Contains(item);
                    try
                    {
                        if (duplicate && !patientIDs.Contains(item) && !item.Equals(""))
                        {
                            patientIDs.Add(item);
                        }
                        else
                        {
                            // filter to filter out any of the same occurances
                        }
                    }
                    catch  (ArgumentOutOfRangeException) 
                    { 
                    
                    }
                    catch (IndexOutOfRangeException)
                    {
                        // catching the error
                    }
                }
                streamReader.Close();
                return patientIDs;
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

        public PatientOverview ReadAllData(List<string> ids) 
        {
            PatientOverview patientOverview = new PatientOverview();
            foreach (var item in ids)
            {
                PatientData data = GetPatientData(item);
                patientOverview.PatientDataBase.Add(data);
            }
            return patientOverview;
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

            historyManager.PatientDataBase.Add(dataOverviewHH);
            historyManager.PatientDataBase.Add(dataOverviewBH);
            historyManager.PatientDataBase.Add(dataOverviewHH2);

            DataWriter data = new DataWriter(new PatientOverview()); // new HistoryManager(); irrelevant for testing
            data.GetHistory = historyManager;
            data.WriteAllPatientsData();

            data.patients = data.ReadPatients();

            string json = data.TestReadPatientData("Harry Harolds");
            Console.WriteLine(json);
            PatientData dataHH = data.GetPatientData("Harry Harolds");
            Console.WriteLine(dataHH.patientID);
            data.WriteHistoryOfPatient(dataOverviewHH3);
            Console.WriteLine(data.ReadPatientData("Harry Harolds-Other"));

            PatientOverview patientOverview = data.ReadAllData(data.patients);
            //Console.WriteLine(dataOverviewHH3.patientID);

        }
    }

    #endregion

}
