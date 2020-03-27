using Newtonsoft.Json.Linq;

namespace AWorkFlow2
{
    /// <summary>
    /// extensions on string
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// convert str to json object
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static dynamic ToJsonObject(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            else
            {
                JToken o = JToken.Parse(str);
                return o;
            }
        }

        /// <summary>
        /// convert str to int?
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int? ToNullableInt(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            if (int.TryParse(str, out int i))
            {
                return i;
            }

            return null;
        }
    }
}
