using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libantplus.DataPages
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    internal class DataPageModelAttribute : Attribute
    {
        private byte dataPageNumber;
        public byte DataPageNumber => dataPageNumber;

        public DataPageModelAttribute(byte dataPageNumber)
        {
            this.dataPageNumber = dataPageNumber;
        }
    }
}
