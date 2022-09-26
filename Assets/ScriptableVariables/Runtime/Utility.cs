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

        public static T JsonCopy<T>(T instance)
        {
            T output = default;
            string json = UnityEditor.EditorJsonUtility.ToJson(instance);
            UnityEditor.EditorJsonUtility.FromJsonOverwrite(json, output);

            return output;
        }

        public static bool TryDeepCopy<T>(T instance, out T output)
        {
            try
            {
               output = DeepCopy(instance);
                return true;
            }
            catch
            {
                UnityEngine.Debug.LogWarning($"Unable to DeepCopy type of {instance.GetType().Name}");
                output =  instance;
                return false;
            }
        }

        public static bool TryJsonCopy<T>(T instance, out T output)
        {
            try
            {
                output = JsonCopy(instance);
                return true;
            }
            catch
            {
                UnityEngine.Debug.LogWarning($"Unable to JsonCopy type of {instance.GetType().Name}");
                output = instance;
                return false;
            }
        }



    }
}