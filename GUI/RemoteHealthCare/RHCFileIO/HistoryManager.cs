using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RHCFileIO
{
    public class HistoryManager
    {
        public List<DataOverview> historyData;
        public HistoryManager()
        {
            this.historyData = new List<DataOverview>();
        }

        public List<DataOverview> HistoryData
        {
            get
            {
                return this.historyData;
            }
        }
    }
}
