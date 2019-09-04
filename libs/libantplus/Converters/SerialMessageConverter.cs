namespace libantplus.Converters
{
    using libantplus.DataPages;
    using System;
    using System.Diagnostics;
    using System.Reflection;

    public static class SerialMessageConverter
    {
        public static SerialMessage<BaseDataModel> ConvertSerialMessage(byte[] data)
        {
            foreach (Type dataModelType in Assembly.GetAssembly(typeof(SerialMessageConverter)).GetTypes())
            {
                if (dataModelType.IsSubclassOf(typeof(BaseDataModel)))
                {
                    DataPageModelAttribute dataPageModelAttribute = (DataPageModelAttribute)dataModelType.GetCustomAttribute(typeof(DataPageModelAttribute));
                    if (dataPageModelAttribute != null)
                    {
                        if (dataPageModelAttribute.DataPageNumber == data[SerialMessageConstants.INDEX_DATA])
                        {
                            return new SerialMessage<BaseDataModel>(Activator.CreateInstance(dataModelType, CreateDataSubset(data)) as BaseDataModel, data);
                        }
                    }
                }
            }

            throw new InvalidCastException($"Could not parse message data -> {string.Join(" ", data)} ");
        }

        private static byte[] CreateDataSubset(byte[] data)
        {
            byte[] result = new byte[data[SerialMessageConstants.INDEX_LENGTH] - 1];
            Array.Copy(data, SerialMessageConstants.INDEX_DATA, result, 0, result.Length);
            return result;
        }
    }
}
