using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Variables
{
    public static class Utility
    {
        public static T DeepCopy<T>(T instance)
        {
            if (instance == null)
                return default;


            BinaryFormatter formatter = new BinaryFormatter();

            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, instance);
                stream.Position = 0;

                return (T)formatter.Deserialize(stream);
            }
        }

    }
}