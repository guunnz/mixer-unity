using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace AxieLandBattleTarget
{
    public enum Method
    {
        Get,
        Patch,
        Post,
        Put,
        Delete,
        MultipartPost
    }

    public abstract class HyperTarget
    {
        public virtual string baseURL => "APIURL";
        public abstract string path { get; }

        public abstract Method method { get; }

        public virtual List<IMultipartFormSection> formData { get; }
        public abstract Dictionary<string, object> param { get; }
        
        public virtual string body => JsonConvert.SerializeObject(param);

        public virtual string query
        {
            get
            {
                if (param == null)
                    return "";
                List<string> queries = new();

                foreach (var (key, value) in param)
                {
                    string keyString = UnityWebRequest.EscapeURL(key);
                    string valueString = UnityWebRequest.EscapeURL($"{value}");
                    queries.Add($"{keyString}={valueString}");
                }

                return string.Join("&", queries);
            }
        }

        public string contentType = "application/json;charset=utf-8";
        public abstract bool requiresAuth { get; }

        public string url => string.IsNullOrEmpty(path) ? $"{baseURL}" : $"{baseURL}/{path}";
    }
}