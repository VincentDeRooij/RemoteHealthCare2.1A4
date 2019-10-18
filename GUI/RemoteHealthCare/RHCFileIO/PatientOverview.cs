using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RHCFileIO
{
    /// <Class PatientOveriew>
    /// 
    /// This class manages a list of the patients 
    /// 
    /// </summary>

    public class PatientOverview
    {
        public List<PatientData> PatientDataBase;
        public PatientOverview()
        {
            this.PatientDataBase = new List<PatientData>();
        }
    }
}
