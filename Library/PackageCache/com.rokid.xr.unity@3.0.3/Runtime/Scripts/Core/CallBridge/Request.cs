using System;
using System.Collections.Generic;

namespace Rokid.UXR
{
    /// <summary>
    /// CallBridge Call Struct
    /// </summary>
    [Serializable]
    public class Request
    {
        public string name = null;
        public List<string> args = null;
        public Callback callback = null;

        private Request()
        {
        }

        [Serializable]
        public class Callback
        {
            public string name = null;
            public string method = null;
            public string param = null;
        }

        [NonSerialized]
        static Request request;
        [NonSerialized]
        public static List<string> cmdlist = new List<string>();

        public static Request Build()
        {
            request = new Request();
            cmdlist.Clear();
            return request;
        }

        public Request Name(string service_method)
        {
            this.name = service_method;
            return this;
        }

        public Request Param<T>(string key, T value)
        {
            cmdlist.Add(createParamStr(key, value));
            this.args = cmdlist;
            return this;
        }

        public Request AndroidCallback(Callback callback)
        {
            this.callback = callback;
            return this;
        }

        public static string createParamStr<T>(string name, T value)
        {
            string baseCmd = "{\"name\": \"tempName\",\"value\": \"tempValue\"}";
            string formatStr0 = baseCmd.Replace("\"tempName\"", "\"" + name + "\"");
            string formatStr1 = formatStr0.Replace("\"tempValue\"", "\"" + value + "\"");

            return formatStr1;
        }
    }
}