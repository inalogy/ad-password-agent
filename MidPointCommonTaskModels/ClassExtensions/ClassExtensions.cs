using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AnySerializer.Extensions;

namespace MidPointUpdatingService.ClassExtensions
{
    public static class ClassExtensions
    {

        public static string FormatDict(this string format, IDictionary<string, object> values)
        {
            var matches = Regex.Matches(format, @"\{(.+?)\}");
            List<string> words = (from Match matche in matches select matche.Groups[1].Value).ToList();

            return words.Aggregate(
                format,
                (current, key) =>
                {
                    int colonIndex = key.IndexOf(':');
                    return current.Replace(
                    "{" + key + "}",
                    colonIndex > 0
                        ? string.Format("{0:" + key.Substring(colonIndex + 1) + "}", values[key.Substring(0, colonIndex)])
                        : values[key].ToString());
                });
        }
        // extension method for combining Dictionaries
        public static Dictionary<string, object> Combine(this Dictionary<string, object> self, Dictionary<string, object> q)
        {
            foreach (var i in q)
            {
                if (!self.ContainsKey(i.Key))
                {
                    self.Add(i.Key.ToString(), i.Value.ToString());
                } else
                {
                    if (self[i.Key.ToString()].ToString() != i.Value.ToString())
                    {
                        throw (new Exception($"Dictionary.Combine duplicate key conflict {i.Key.ToString()}:{self[i.Key.ToString()]}<>{i.Value.ToString()}"));
                    }
                }
            }
            return self;
        }


    }
    public static class Helpers
    {
        // Convert an object to a byte array
        public static byte[] ObjectToByteArray(Object obj)
        {
            return obj.Serialize(AnySerializer.SerializerOptions.EmbedTypes);
        }

        // Convert a byte array to an Object
        public static Object ByteArrayToObject(byte[] arrBytes, Type dataType)
        {
            return arrBytes.Deserialize(dataType, AnySerializer.SerializerOptions.EmbedTypes);
        }
    }
}
