using FileManager;
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
        private HistoryManager history;

        public HistoryManager GetHistory
        { get { return this.history; } set { this.history = value; } }

        private DirectoryInfo dataSaveDirectory;
        private DirectoryInfo historyPath;
        private StreamWriter streamWriter;
        public string dataPathOutput;
        public bool appendBoolean;


        public DataWriter(HistoryManager history)
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

        public void WriteCompleteHistory()
        {
            string jsonData = "";
            foreach (DataOverview data in this.history.HistoryData)
            {
                jsonData = JsonConvert.SerializeObject(data);
                WriteStringToFile(CreateNewFile(this.historyPath.FullName, data.patientID + ".data"), jsonData);
            }
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

        public DataOverview GetPatientData(string patientID)
        {
            JObject jObject = JObject.Parse(ReadPatientData(patientID));
            dynamic dataObject = jObject.Value<JObject>().ToObject<DataOverview>();

            return dataObject;
        }

        public HistoryManager TestHistoryWriter()
        {
            HistoryManager historyManager = new HistoryManager();

            BikeData bikeData1 = new BikeData();
            bikeData1.addToListAVG_RPM(10);
            bikeData1.addToListAVG_Speed(19);
            bikeData1.addToListCUR_RPM(20);
            bikeData1.addToListCUR_Speed(10);
            bikeData1.addToListDistanceTraversed(20);
            bikeData1.addToListTorque(30);

            HeartData heartData1 = new HeartData();
            heartData1.addToListAVG(60);
            heartData1.addToListCUR(9000);

            DataOverview dataOverviewHH = new DataOverview("Harry Harolds", bikeData1, heartData1);

            BikeData bikeData3 = new BikeData();
            bikeData3.addToListAVG_RPM(10);
            bikeData3.addToListAVG_Speed(19);
            bikeData3.addToListCUR_RPM(20);
            bikeData3.addToListCUR_Speed(10);
            bikeData3.addToListDistanceTraversed(20);
            bikeData3.addToListTorque(30);

            HeartData heartData3 = new HeartData();
            heartData3.addToListAVG(60);
            heartData3.addToListCUR(1000000);

            DataOverview dataOverviewHH2 = new DataOverview("Harry Harolds", bikeData3, heartData3);

            BikeData bikeData2 = new BikeData();
            bikeData2.addToListAVG_RPM(10);
            bikeData2.addToListAVG_Speed(19);
            bikeData2.addToListCUR_RPM(20);
            bikeData2.addToListCUR_Speed(10);
            bikeData2.addToListDistanceTraversed(20);
            bikeData2.addToListTorque(30);

            HeartData heartData2 = new HeartData();
            heartData2.addToListAVG(61);
            heartData2.addToListCUR(90);

            DataOverview dataOverviewBH = new DataOverview("Barry Harolds", bikeData2, heartData2);

            historyManager.HistoryData.Add(dataOverviewHH);
            historyManager.HistoryData.Add(dataOverviewBH);
            historyManager.HistoryData.Add(dataOverviewHH2);

            return historyManager;
        }
    }

    #endregion

}
