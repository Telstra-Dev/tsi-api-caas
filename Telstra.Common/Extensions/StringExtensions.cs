using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Telstra.Common
{
    public static class StringExtensions
    {
        private static readonly HashAlgorithm sha256Algorithm = SHA256.Create();

        /// <summary>
        /// Gets the hash of the specified string
        /// </summary>
        /// <param name="s">The string</param>
        /// <returns>The 256 bit hash value, represented by a string of 64 characters</returns>
        public static string ToSha256Hash(this string s)
        {
            // Get the hash bytes
            var hashBytes = sha256Algorithm.ComputeHash(Encoding.UTF8.GetBytes(s));

            // Convert to string
            return System.Convert.ToBase64String(hashBytes);
        }

        public static string LimitTo(this string data, int length)
        {
            return (data == null || data.Length <= length)
                ? data
                : data.Substring(0, length);
        }

        public static bool IsNull(this String s)
        {
            return String.IsNullOrEmpty(s);
        }

        public static bool IsNotNull(this string str, int minLength = 1)
        {
            return !string.IsNullOrEmpty(str) && (str ?? "").Length >= minLength;
        }

        public static string Join(this string[] data, string separater)
        {
            return string.Join(separater, data);
        }

        public static string Join(this int[] data, string separater)
        {
            return string.Join(separater, data);
        }



        public static byte[] GetBytes(this string data)
        {
            return Encoding.UTF8.GetBytes(data);
        }

        public static byte[] GetBytesFromBase64(this string data)
        {
            return Convert.FromBase64String(data);
        }

        public static string GetBase64(this string data)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
        }

        public static string GetString(this byte[] data)
        {
            return Convert.ToBase64String(data);
        }



        public static string Compress(this string uncompressedString)
        {
            byte[] compressedBytes;

            using (var uncompressedStream = new MemoryStream(Encoding.UTF8.GetBytes(uncompressedString)))
            {
                using (var compressedStream = new MemoryStream())
                {
                    using (var compressorStream = new DeflateStream(compressedStream, CompressionLevel.Optimal, true))
                    {
                        uncompressedStream.CopyTo(compressorStream);
                    }
                    compressedBytes = compressedStream.ToArray();
                }
            }

            return Convert.ToBase64String(compressedBytes);
        }

        public static string Decompress(this string compressedString)
        {
            byte[] decompressedBytes;

            var compressedStream = new MemoryStream(Convert.FromBase64String(compressedString));

            using (var decompressorStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
            {
                using (var decompressedStream = new MemoryStream())
                {
                    decompressorStream.CopyTo(decompressedStream);

                    decompressedBytes = decompressedStream.ToArray();
                }
            }

            return Encoding.UTF8.GetString(decompressedBytes);
        }



        public static int ToInt(this string value)
        {
            if (int.TryParse(value, out var output)) return output;
            throw new NotSupportedException("String invalid for conversion");
        }

        public static int ToInt(this double @this)
        {
            return (int)@this;
        }

        public static double ToDouble(this string value)
        {
            if (double.TryParse(value, out var output)) return output;
            throw new NotSupportedException("String invalid for conversion");
        }

        public static double ToDouble(this int @this)
        {
            return (double)@this;
        }

        public static T ToObject<T>(this string value)
        {
            return JsonConvert.DeserializeObject<T>(value, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
        }

        public static T ToAnonymousObject<T>(this string value, T anonymousModel)
        {
            return JsonConvert.DeserializeAnonymousType<T>(value, anonymousModel, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });
        }

        public static T ParseObject<T>(this JObject value)
        {
            return value.ToObject<T>(new JsonSerializer { DateParseHandling = DateParseHandling.None });
        }

        public static T GetPropertyValue<T>(this object input, string property)
        {
            var prop = input.GetType().GetProperties().FirstOrDefault(m => m.Name == property);
            if (prop != null)
                return (T)Convert.ChangeType(prop.GetValue(input), typeof(T));
            return default(T);
        }

        public static string Serialize(this object input)
        {
            return JsonConvert.SerializeObject(input);
        }

        public static T Deserialize<T>(this string input)
        {
            return JsonConvert.DeserializeObject<T>(input);
        }

        public static T DeserializeUrlEncoded<T>(this string input)
        {
            var items = input.Split('&').ToDictionary(m => m.Split('=')[0].Trim(), m => HttpUtility.UrlDecode(m.Split('=')[1]));
            return items.ToJson().ToObject<T>();
        }
    }
}
