using System;
using libantplus.Converters;
using libantplus.DataPages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace libantplus_tests
{
    [TestClass]
    public class ConvertTest
    {
        [TestMethod]
        public void TestValidTemplate()
        {
            byte[] generalMessageTest = { 0xA4, 0x09, 0x4E, 0x05, 0x10, 0x19, 0x04, 0x00, 0x00, 0x00, 0xFF, 0x30 };
            SerialMessage<BaseDataModel> dataModel = SerialMessageConverter.ConvertSerialMessage(generalMessageTest);

            Assert.IsTrue(dataModel.DataPage.DataPageNumber == 0x10);
        }

        [TestMethod]
        public void TestInvalidTemplate()
        {
            byte[] invGeneralMessageTest = { 0xA4, 0x09, 0x4E, 0x05, 0xAA, 0x19, 0x04, 0x00, 0x00, 0x00, 0xFF, 0x30 };
            bool thrown = false;

            try
            {
                SerialMessage<BaseDataModel> dataModel = SerialMessageConverter.ConvertSerialMessage(invGeneralMessageTest);
            }
            catch (InvalidCastException e)
            {
                thrown = true;
            }

            Assert.IsTrue(thrown);
        }
    }
}
