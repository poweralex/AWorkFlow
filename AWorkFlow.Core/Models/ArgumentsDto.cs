using System.Collections.Generic;

namespace AWorkFlow.Core.Models
{
    public class ArgumentsDto
    {
        public Dictionary<string, string> PublicVariables { get; private set; } = new Dictionary<string, string>();

        public Dictionary<string, string> PrivateVariables { get; private set; } = new Dictionary<string, string>();

        public ArgumentsDto(Dictionary<string, string> publicVariables = null, Dictionary<string, string> privateVariables = null)
        {
            if (publicVariables != null)
            {
                PublicVariables = publicVariables;
            }
            if (privateVariables != null)
            {
                PrivateVariables = privateVariables;
            }
        }

        public bool ContainsKey(string key)
        {
            var res = ContainsPublic(key);
            if (!res)
            {
                res = ContainsPrivate(key);
            }
            return res;
        }

        public bool ContainsPublic(string key)
        {
            return ContainsKey(PublicVariables, key);
        }

        public bool ContainsPrivate(string key)
        {
            return ContainsKey(PrivateVariables, key);
        }

        private bool ContainsKey(Dictionary<string, string> dic, string key)
        {
            return dic?.ContainsKey(key) ?? false;
        }

        public string Get(string key)
        {
            var res = GetPublic(key);
            if (res == null)
            {
                res = GetPrivate(key);
            }
            return res;
        }

        public string GetPrivate(string key)
        {
            return GetVariable(PrivateVariables, key);
        }

        public string GetPublic(string key)
        {
            return GetVariable(PublicVariables, key);
        }

        private string GetVariable(Dictionary<string, string> dic, string key)
        {
            if (dic?.ContainsKey(key) == true)
            {
                return dic[key];
            }
            return null;
        }

        public void PutPrivate(string key, string value)
        {
            PutVariable(PrivateVariables, key, value);
        }

        public void PutPublic(string key, string value)
        {
            PutVariable(PublicVariables, key, value);
        }

        private void PutVariable(Dictionary<string, string> dic, string key, string value)
        {
            dic[key] = value;
        }

        public ArgumentsDto Copy()
        {
            var newItem = new ArgumentsDto();
            if (PublicVariables != null)
            {
                newItem.PublicVariables = new Dictionary<string, string>(PublicVariables);
            }
            if (PrivateVariables != null)
            {
                newItem.PrivateVariables = new Dictionary<string, string>(PrivateVariables);
            }
            return newItem;
        }
    }
}
