using System.Collections.Generic;

namespace Rokid.UXR
{
    /// <summary>
    /// Data Cache
    /// </summary>
    public class DataCache : Singleton<DataCache>
    {
        Dictionary<string, object> data = new Dictionary<string, object>();

        /// <summary>
        /// Add Data
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="cover"></param>
        public void Add(string key, object value, bool cover = false)
        {
            if (data.ContainsKey(key) && cover == false)
            {
                RKLog.KeyInfo($"====DataCache=== Add Key Repetition {key}: {value}");
                return;
            }
            else if (data.ContainsKey(key) && cover == true)
            {
                data[key] = value;
            }
            else
            {
                data.Add(key, value);
            }
        }

        /// <summary>
        ///  Add Data
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cover"></param>
        public void Add(object value, bool cover = false)
        {
            string key = value.GetType().FullName;
            Add(key, value, cover);
        }

        /// <summary>
        /// Update Data
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void UpdateData(string key, object value)
        {
            if (data.ContainsKey(key))
            {
                data[key] = value;
            }
            else
            {
                RKLog.KeyInfo($"====DataCache=== UpdateData Key Repetition {key}: {value}");
            }
        }

        /// <summary>
        /// Get Value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            object obj = null;
            if (data.TryGetValue(key, out obj))
            {
                return (T)obj;
            }
            else
            {
                RKLog.KeyInfo($"====DataCache=== Get Key Not Exist {key}");
            }
            return default;
        }

        /// <summary>
        /// Get Value
        /// </summary>
        /// <typeparam name="T">Data Object</typeparam>
        /// <param name="key">Data Key</param>
        /// <returns></returns>
        public T Get<T>()
        {
            string key = typeof(T).FullName;
            return Get<T>(key);
        }

        /// <summary>
        /// Get All Keys
        /// </summary>
        /// <returns></returns>
        public List<string> Keys()
        {
            return new List<string>(data.Keys);
        }

        /// <summary>
        /// Clear Data
        /// </summary>
        public void Clear()
        {
            data.Clear();
        }
    }


}
