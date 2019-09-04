using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace libantplus.DataPages
{
    public static class SerialMessageConstants
    {
        public const sbyte INDEX_SYNC       = 0;
        public const sbyte INDEX_LENGTH     = 1;
        public const sbyte INDEX_TYPE       = 2;
        public const sbyte INDEX_CHANNEL    = 3;
        public const sbyte INDEX_DATA       = 4;
    }

    public class SerialMessage<T> where T : BaseDataModel
    {
        private T dataPage;
        public T DataPage => dataPage;

        private byte sync, length, type, channel, checksum;
        public byte Sync => sync;
        public byte Length => length;
        public byte Type => type;
        public byte Channel => channel;
        public byte Checksum => checksum;

        public SerialMessage(T dataPage, byte[] messageData)
        {
            this.dataPage   = dataPage;
            this.sync       = messageData[SerialMessageConstants.INDEX_SYNC];
            this.length     = messageData[SerialMessageConstants.INDEX_LENGTH];
            this.type       = messageData[SerialMessageConstants.INDEX_TYPE];
            this.channel    = messageData[SerialMessageConstants.INDEX_CHANNEL];
            this.checksum   = messageData[this.length + 2];
        }
    }

    public interface IDataPageModel
    {
        byte DataPageNumber { get; }
    }

    public class BaseDataModel : IDataPageModel
    {
        public byte[] PageData { get; private set; }
        public byte DataPageNumber => PageData[0];

        public BaseDataModel(byte[] pageData)
        {
            this.PageData = pageData;
        }
    }

    [DataPageModel(0x10)]
    public class GeneralFEData : BaseDataModel
    {
        public GeneralFEData(byte[] data)
            : base(data)
        {

        }
    }


    [DataPageModel(0x11)]
    public class GeneralSettings : BaseDataModel
    {
        public GeneralSettings(byte[] pageData)
            : base(pageData)
        {

        }
    }
}
