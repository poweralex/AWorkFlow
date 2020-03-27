using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace AWorkFlow2.Models.Working
{
    public class WorkingModelBase : TrackingChange
    {
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class TrackingChange
    {
        [IgnoreTracking]
        internal string OriginalMD5 { get; set; }
        [IgnoreTracking]
        public bool Inserted { get { return string.IsNullOrEmpty(OriginalMD5); } }
        [IgnoreTracking]
        public bool Updated { get { return !Inserted && !string.Equals(OriginalMD5, GetMD5()); } }

        public void AcceptChanges()
        {
            OriginalMD5 = GetMD5();
        }

        internal virtual string GetMD5()
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            StringBuilder sb = new StringBuilder();
            foreach (var property in properties)
            {
                if (property.GetCustomAttribute<IgnoreTrackingAttribute>() == null)
                {
                    sb.Append($"{property.Name}:{property.GetValue(this)},");
                }
            }
            return Encoding.UTF8.GetString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(sb.ToString())));
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreTrackingAttribute : Attribute
    { }
}
