using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PIModel;

namespace PIModelFileAccess
{
    public abstract class ModelManager<T> : IModelManager<T> where T : PersistentObjectBase
    {

        #region "-- public --"

        public T Load(string path, string file)
        {
            T data = default(T);

            if (!DoesFileExist(path, file)) return data;

            var rawData = ReadFile(path, file);

            if (rawData == null) return data;

            data = Deserialize(rawData);
            return data;
        }

        public bool Save(string path, string file, T data)
        {
            CreateFolder(path);
            var rawData = Serialize(data);
            WriteFile(path, file, rawData);
            return true;
        }

        #endregion

        #region "-- private JSON --"

        private string Serialize(T data)
        {
            return JsonConvert.SerializeObject(data,Formatting.Indented);
        }

        private T Deserialize(string serializedData)
        {
            return JsonConvert.DeserializeObject<T>(serializedData);
        }

        #endregion

        #region "-- private Files --"

        private bool DoesFileExist(string path, string file)
        {
            if (!DoesFolderExist(path)) return false;
            return System.IO.File.Exists(path + file);
        }

        protected bool DoesFolderExist(string path)
        {
            return System.IO.Directory.Exists(path);
        }

        private void CreateFolder(string path)
        {
            if (!DoesFolderExist(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
        }

        private string ReadFile(string path, string file)
        {
            string data = System.IO.File.ReadAllText(path + file);
            return data;
        }

        private bool WriteFile(string path, string file, string data)
        {
            System.IO.File.Delete(path + file);
            System.IO.File.WriteAllText(path + file, data);
            return true;
        }

        #endregion

    }
}
