using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RHCFileIO
{
    public class HeartData
    {
        public List<int> currentHRTRate { get; set; }
        public List<int> averageHRTRate { get; set; }
        public int totalAverage { get; set; }

        public HeartData()
        {
            this.averageHRTRate = new List<int>();
            this.currentHRTRate = new List<int>();
        }

        public void addToListAVG(int data)
        {
            this.averageHRTRate.Add(data);
        }

        public void addToListCUR(int data)
        {
            this.currentHRTRate.Add(data);
        }

        public int CalcTotalAverageHR() // calculates the mean average over the entire routine. 
        {
            int total = 0;
            int amountOfData = this.currentHRTRate.Count;

            foreach (int item in currentHRTRate)
            {
                total += item;
            }
            return total / amountOfData;
        }
    }
}
