using RHCFileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RHCFileIO
{
    public class PatientData
    {
        /// <Class PatientData>
        /// 
        /// This class is the data class for each patient, here data gets saved that's received from the clients
        /// Afterwards it gets written to a special file specific for each patient
        /// 
        /// </summary>

        public string patientID { get; set; }
        public BikeData bikeData { get; set; }
        public HeartData heartData { get; set; }

        public PatientData(string patientID, BikeData bikeData, HeartData heartData)
        {
            this.patientID = patientID;
            this.bikeData = bikeData;
            this.heartData = heartData;
        }
    }
}
