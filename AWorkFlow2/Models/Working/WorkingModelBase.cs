using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace AWorkFlow2.Models.Working
{
    /// <summary>
    /// working model base
    /// </summary>
    public class WorkingModelBase : TrackingChange
    {
        /// <summary>
        /// updated user
        /// </summary>
        public string UpdatedBy { get; set; }
        /// <summary>
        /// updated time
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// tracking change helper
    /// </summary>
    public class TrackingChange
    {
        [JsonIgnore]
        [IgnoreTracking]
        internal string OriginalMD5 { get; set; }
        /// <summary>
        /// if this object is new
        /// </summary>
        [IgnoreTracking]
        public bool Inserted { get { return string.IsNullOrEmpty(OriginalMD5); } }
        /// <summary>
        /// if this object is updated
        /// </summary>
        [IgnoreTracking]
        public bool Updated { get { return !Inserted && !string.Equals(OriginalMD5, GetMD5()); } }

        /// <summary>
        /// mark this object as no changes
        /// </summary>
        public void AcceptChanges()
        {
            OriginalMD5 = GetMD5();
        }

        internal virtual string GetContentBrief()
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            StringBuilder sb = new StringBuilder();
            foreach (var property in properties)
            {
                if (property.GetCustomAttribute<IgnoreTrackingAttribute>() == null)
                {
                    sb.Append($"{property.Name}:{JsonConvert.SerializeObject(property.GetValue(this))},");
                }
            }
            return sb.ToString();
        }

        internal virtual string GetMD5()
        {
            return Encoding.UTF8.GetString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(GetContentBrief())));
        }
    }

    /// <summary>
    /// attribute that marks property as no-need to tracking
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreTrackingAttribute : Attribute
    { }
}
