using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;
using System.Xml.Serialization;

using Newtonsoft.Json.Linq;
using Helpers.Mail;
using System.Net.Mail;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Helpers
{
    public static class Utilities
    {
        private static NLog.Logger _log = NLog.LogManager.GetCurrentClassLogger();


        public static T NewInstanceOf<T>(T input) where T : class
        {
            if (input == null)
                return null;

            var s = new XmlSerializer(input.GetType());
            var w = new StringWriter();
            T o = null;

            s.Serialize(w, input);
            var r = new StringReader(w.ToString());
            o = (T)s.Deserialize(r);

            w.Close();
            r.Close();

            return o;
        }

        public static T CastValue<T>(object value, T defaultValue)
        {
            T output;
            var stringValue = (value is string ? (string)value : null);

            var t = typeof(T);

            if (t.IsEnum)
            {
                try
                {
                    return (T)Enum.Parse(typeof(T), stringValue);
                }
                catch
                {
                    return defaultValue;
                }
            }
            else if (t == typeof(TimeSpan))
            {
                try
                {
                    var ts = TimeSpan.Parse(stringValue);
                    return (T)Convert.ChangeType(ts, t);
                }
                catch
                {
                    return defaultValue;
                }
            }

            var tc = Type.GetTypeCode(t);

            switch (tc)
            {
                case TypeCode.Boolean:

                    if (string.IsNullOrWhiteSpace(stringValue))
                        throw new NotSupportedException();

                    var val = 0;

                    if (stringValue.Trim().ToLower() == "true" ||
                        (int.TryParse(stringValue.Trim(), out val) && val >= 1))
                        output = (T)Convert.ChangeType(true, tc);
                    else
                        output = (T)Convert.ChangeType(false, tc);

                    return output;

                case TypeCode.Char:
                case TypeCode.Byte:
                case TypeCode.DateTime:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.String:

                    try
                    {
                        output = (T)Convert.ChangeType(value, tc);
                    }
                    catch
                    {
                        output = defaultValue;
                    }

                    return output;

                case TypeCode.Object:

                    if (typeof(T) == typeof(Dictionary<string, string>))
                    {
                        if (string.IsNullOrWhiteSpace(stringValue))
                            throw new NotSupportedException();

                        var d = new Dictionary<string, string>();

                        var entries = stringValue.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                        if (entries == null || entries.Length == 0)
                            return defaultValue;

                        foreach (var e in entries)
                        {
                            if (e.Contains(":"))
                            {
                                var entry = e.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                                if (entry == null || entry.Length != 2 ||
                                    string.IsNullOrWhiteSpace(entry[0]) || string.IsNullOrWhiteSpace(entry[1]))
                                    throw new FormatException();

                                d.Add(entry[0], entry[1]);
                            }
                            else
                                d.Add(e, "");
                        }

                        return (T)Convert.ChangeType(d, typeof(T));
                    }
                    else if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
                    {
                        if (string.IsNullOrWhiteSpace(stringValue))
                            throw new NotSupportedException();

                        var entries = stringValue.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                        if (entries == null || entries.Length == 0)
                            return defaultValue;

                        var argumentType = typeof(T).GetGenericArguments().First();
                        var convertedList = (System.Collections.IList)Activator.CreateInstance(typeof(T));

                        entries
                            .ToList()
                            .ForEach(f => convertedList.Add(Convert.ChangeType(f, argumentType)));

                        return (T)convertedList;
                    }
                    else if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        var ut = Nullable.GetUnderlyingType(typeof(T));

                        if (!ut.IsPrimitive)
                            throw new NotSupportedException();

                        if (string.IsNullOrWhiteSpace(stringValue))
                            return default(T);

                        return (T)Convert.ChangeType(stringValue, ut);
                    }
                    else
                    {
                        try
                        {
                            return (T)Convert.ChangeType(value, typeof(T));
                        }
                        catch (Exception)
                        {
                            throw new NotSupportedException();
                        }
                    }

                default:
                    throw new NotSupportedException();
            }
        }

        public static string ToJsonError(int code)
        {
            return ToJsonError(code, string.Empty, string.Empty);
        }

        public static string ToJsonError(int code, string description)
        {
            return ToJsonError(code, description, string.Empty);
        }

        public static string ToJsonError(int code, string description, string argument)
        {
            var jo = new JObject();

            jo.Add("code", code);

            if (!string.IsNullOrWhiteSpace(description))
                jo.Add("desc", description);

            if (!string.IsNullOrWhiteSpace(argument))
                jo.Add("arg", argument);

            return jo.ToString(Newtonsoft.Json.Formatting.None, null);
        }

        public static uint IPAddressToUInt(string ip)
        {
            var bytes = IPAddress.Parse(ip).GetAddressBytes();

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToUInt32(bytes, 0);
        }


        public static bool IsDevelopmentStage()
        {
            return (
                "Stage"
                    .FromAppSettings(string.Empty)
                    .Equals("development", StringComparison.OrdinalIgnoreCase)
            );
        }

        #region Cryptography

        //
        // http://stackoverflow.com/questions/273452/using-aes-encryption-in-c-sharp#26758901
        //

        private const string cryptoKey = "BeXD8Ppq6Uj9vA9FLASwIs5AEyzUipwtsbRpAkWAeUfFOiJWPUGWjfRNAvKFQjvM";
        private const string cryptoSalt = "Yaej8Alp";

        public enum CypherAction
        {
            Crypt,
            Decrypt
        }

        public static string Encrypt(string input, string saltValue = cryptoSalt, string passPhrase = cryptoKey)
        {
            return Encrypt(input, saltValue, passPhrase, "SHA1", 3, "6aQj56N8dE6VgK6J", 256);
        }

        public static string Encrypt(string input, string saltValue, string passPhrase, string hashAlgorithm, int passwordIterations, string initVector, int keysize)
        {
            string functionReturnValue = null;
            // Convert strings into byte arrays.
            // Let us assume that strings only contain ASCII codes.
            // If strings include Unicode characters, use Unicode, UTF7, or UTF8
            // encoding.
            byte[] initVectorBytes = null;
            initVectorBytes = Encoding.ASCII.GetBytes(initVector);
            byte[] saltValueBytes = null;
            saltValueBytes = Encoding.ASCII.GetBytes(saltValue);

            // Convert our plaintext into a byte array.
            // Let us assume that plaintext contains UTF8-encoded characters.
            byte[] pTextBytes = null;
            pTextBytes = Encoding.UTF8.GetBytes(input);
            // First, we must create a password, from which the key will be derived.
            // This password will be generated from the specified passphrase and
            // salt value. The password will be created using the specified hash
            // algorithm. Password creation can be done in several iterations.
            PasswordDeriveBytes password = default(PasswordDeriveBytes);
            password = new PasswordDeriveBytes(passPhrase, saltValueBytes, hashAlgorithm, passwordIterations);
            // Use the password to generate pseudo-random bytes for the encryption
            // key. Specify the size of the key in bytes (instead of bits).
            byte[] keyBytes = null;
            keyBytes = password.GetBytes(keysize / 8);
            // Create uninitialized Rijndael encryption object.
            RijndaelManaged symmetricKey = default(RijndaelManaged);
            symmetricKey = new RijndaelManaged();

            // It is reasonable to set encryption mode to Cipher Block Chaining
            // (CBC). Use default options for other symmetric key parameters.
            symmetricKey.Mode = CipherMode.CBC;
            // Generate encryptor from the existing key bytes and initialization
            // vector. Key size will be defined based on the number of the key
            // bytes.
            ICryptoTransform encryptor = default(ICryptoTransform);
            encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);

            // Define memory stream which will be used to hold encrypted data.
            MemoryStream memoryStream = default(MemoryStream);
            memoryStream = new MemoryStream();

            // Define cryptographic stream (always use Write mode for encryption).
            CryptoStream cryptoStream = default(CryptoStream);
            cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            // Start encrypting.
            cryptoStream.Write(pTextBytes, 0, pTextBytes.Length);

            // Finish encrypting.
            cryptoStream.FlushFinalBlock();
            // Convert our encrypted data from a memory stream into a byte array.
            byte[] cipherTextBytes = null;
            cipherTextBytes = memoryStream.ToArray();

            // Close both streams.
            memoryStream.Close();
            cryptoStream.Close();

            // Convert encrypted data into a base64-encoded string.
            string cipherText = null;
            cipherText = Convert.ToBase64String(cipherTextBytes);

            functionReturnValue = cipherText;
            return functionReturnValue;
        }

        public static List<string> Encrypt(List<string> input)
        {
            if (input == null || input.Count == 0)
                return null;

            List<string> retList = new List<string>();

            input.ForEach(s => retList.Add(Encrypt(s)));

            return retList;
        }

        public static string Decrypt(string input, string saltValue = cryptoSalt, string passPhrase = cryptoKey)
        {
            return Decrypt(input, saltValue, passPhrase, "SHA1", 3, "6aQj56N8dE6VgK6J", 256);
        }

        public static string Decrypt(string input, string saltValue, string passPhrase, string hashAlgorithm, int passwordIterations, string initVector, int keySize)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            string functionReturnValue = null;

            // Convert strings defining encryption key characteristics into byte
            // arrays. Let us assume that strings only contain ASCII codes.
            // If strings include Unicode characters, use Unicode, UTF7, or UTF8
            // encoding.


            byte[] initVectorBytes = null;
            initVectorBytes = Encoding.ASCII.GetBytes(initVector);

            byte[] saltValueBytes = null;
            saltValueBytes = Encoding.ASCII.GetBytes(saltValue);

            // Convert our ciphertext into a byte array.
            byte[] cipherTextBytes = null;
            cipherTextBytes = Convert.FromBase64String(input);

            // First, we must create a password, from which the key will be
            // derived. This password will be generated from the specified
            // passphrase and salt value. The password will be created using
            // the specified hash algorithm. Password creation can be done in
            // several iterations.
            PasswordDeriveBytes password = default(PasswordDeriveBytes);
            password = new PasswordDeriveBytes(passPhrase, saltValueBytes, hashAlgorithm, passwordIterations);

            // Use the password to generate pseudo-random bytes for the encryption
            // key. Specify the size of the key in bytes (instead of bits).
            byte[] keyBytes = null;
            keyBytes = password.GetBytes(keySize / 8);

            // Create uninitialized Rijndael encryption object.
            RijndaelManaged symmetricKey = default(RijndaelManaged);
            symmetricKey = new RijndaelManaged();

            // It is reasonable to set encryption mode to Cipher Block Chaining
            // (CBC). Use default options for other symmetric key parameters.
            symmetricKey.Mode = CipherMode.CBC;

            // Generate decryptor from the existing key bytes and initialization
            // vector. Key size will be defined based on the number of the key
            // bytes.
            ICryptoTransform decryptor = default(ICryptoTransform);
            decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);

            // Define memory stream which will be used to hold encrypted data.
            MemoryStream memoryStream = default(MemoryStream);
            memoryStream = new MemoryStream(cipherTextBytes);

            // Define memory stream which will be used to hold encrypted data.
            CryptoStream cryptoStream = default(CryptoStream);
            cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

            // Since at this point we don't know what the size of decrypted data
            // will be, allocate the buffer long enough to hold ciphertext;
            // plaintext is never longer than ciphertext.
            byte[] plainTextBytes = null;
            plainTextBytes = new byte[cipherTextBytes.Length + 1];

            // Start decrypting.
            int decryptedByteCount = 0;
            decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

            // Close both streams.
            memoryStream.Close();
            cryptoStream.Close();

            // Convert decrypted data into a string.
            // Let us assume that the original plaintext string was UTF8-encoded.
            string plainText = null;
            plainText = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);

            // Return decrypted string.
            functionReturnValue = plainText;


            return functionReturnValue;
        }


        public static T EntityCypher<T>(T entity, CypherAction action, params Expression<Func<T, object>>[] properties)
        {
            return (new List<T> { entity }).EntityCypher(action, properties).FirstOrDefault();
        }

        public static List<T> EntityCypher<T>(this List<T> entities, CypherAction action, params Expression<Func<T, object>>[] properties)
        {
            if (entities == null || entities.Count == 0)
                return new List<T>();

            if (properties == null || properties.Length == 0)
                throw new ArgumentNullException();

            var type = typeof(T);

            foreach (var e in entities)
            {
                foreach (var p in properties)
                {
                    var me = (MemberExpression)p.Body;
                    if (me == null)
                        throw new InvalidExpressionException();

                    var pi = (PropertyInfo)me.Member;
                    if (pi == null)
                        throw new InvalidExpressionException();

                    var value = pi.GetValue(e, null);

                    if (value == null || !value.GetType().IsAssignableFrom(typeof(string)) || string.IsNullOrWhiteSpace(value.ToString()))
                        continue;

                    try
                    {
                        switch (action)
                        {
                            case CypherAction.Crypt:
                                pi.SetValue(e, Encrypt(value.ToString()), null);
                                break;
                            case CypherAction.Decrypt:
                                pi.SetValue(e, Decrypt(value.ToString()), null);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Log(NLog.LogLevel.Error, ex.ToString());
                    }
                }
            }

            return entities;
        }

        public static List<T> EntityCypher<T>(this List<T> entities, ref List<T> unCypherEntities, params Expression<Func<T, object>>[] properties)
        {
            if (entities == null || entities.Count == 0)
                return new List<T>();

            if (properties == null || properties.Length == 0)
                throw new ArgumentNullException();

            var type = typeof(T);

            foreach (var e in entities)
            {
                foreach (var p in properties)
                {
                    var me = (MemberExpression)p.Body;
                    if (me == null)
                        throw new InvalidExpressionException();

                    var pi = (PropertyInfo)me.Member;
                    if (pi == null)
                        throw new InvalidExpressionException();

                    var value = pi.GetValue(e, null);

                    if (value == null || !value.GetType().IsAssignableFrom(typeof(string)) || string.IsNullOrWhiteSpace(value.ToString()))
                        continue;

                    try
                    {
                        pi.SetValue(e, Decrypt(value.ToString()), null);
                    }
                    catch (FormatException fex)
                    {
                        if (unCypherEntities == null)
                            unCypherEntities = new List<T>();

                        unCypherEntities.Add(e);

                        _log.Log(NLog.LogLevel.Error, fex.ToString());
                    }
                    catch (Exception ex)
                    {
                        _log.Log(NLog.LogLevel.Error, ex.ToString());
                    }
                }
            }

            return entities;
        }

        public static string GetMD5Hash(string input, bool uppercase = false)
        {
            var md5 = new MD5CryptoServiceProvider();
            var buffer = md5.ComputeHash(Encoding.ASCII.GetBytes(input));
            var hash = string.Concat(uppercase ? buffer.Select(s => s.ToString("X2")) : buffer.Select(s => s.ToString("x2")));

            return hash;
        }

        #endregion

        public static bool IsUserLoggedIn()
        {
            if (IsDevelopmentStage())
                return true;
            else
                return HttpContext.Current.User.Identity.IsAuthenticated;
        }

        public static void ValidateUserLogged()
        {
            if (!IsUserLoggedIn())
                FormsAuthentication.RedirectToLoginPage();
        }

        public static void CloseSession(bool isUnauthorizedAccess = false)
        {
            if (isUnauthorizedAccess)
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            FormsAuthentication.SignOut();
        }

        public static void Logout(bool isUnauthorizedAccess = false)
        {
            CloseSession(isUnauthorizedAccess);
            FormsAuthentication.RedirectToLoginPage(isUnauthorizedAccess ? "AuthErr=true" : String.Empty);
        }

        public static string GetResourceValue(string name, string key)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(key))
                return null;

            try
            {
                return HttpContext.GetGlobalResourceObject(name, key).ToString();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Busca el recurso en una jerarquia de archivos de recurso definida en la lista 'resourceFileNames'.
        /// </summary>
        public static string GetResourceValue(IEnumerable<string> resourceFileNames, string key)
        {
            if (resourceFileNames == null || resourceFileNames.Count() == 0)
                return null;

            foreach (var resFN in resourceFileNames)
            {
                try { return HttpContext.GetGlobalResourceObject(resFN, key).ToString(); }
                catch { }
            }

            return null;
        }

        public static string GetLocalizedHtml(string file)
        {
            return GetLocalizedHtml(file, new List<string>() { file });
            //return GetLocalizedHtml(file, new List<string>() { file, "common" });
        }

        public static string GetLocalizedHtml(string file, IEnumerable<string> resourceFileNames)
        {
            if (string.IsNullOrWhiteSpace(file))
                return null;

            var path = HostingEnvironment.MapPath("~/html/" + file + ".html");

            if (string.IsNullOrWhiteSpace(path))
                return null;

            var html = File.ReadAllText(path);

            if (string.IsNullOrWhiteSpace(html))
                return null;

            var matches = Regex.Matches(html, @"\[@[a-zA-Z\s]*\]");

            if (matches.Count == 0)
                return html;

            foreach (Match m in matches)
            {
                var key = Regex.Match(m.Value, @"^\[@([a-zA-Z\s]*)\]$").Groups[1].Value;
                var newValue = GetResourceValue(resourceFileNames, key.Replace(" ", ""));
                if (string.IsNullOrWhiteSpace(newValue))
                    newValue = key;

                html = html.Replace(m.Value, newValue);
            }

            return html;
        }

        public static int GetEdad(DateTime fechaCumpleannos)
        {
            int edad = DateTime.Now.Year - fechaCumpleannos.Year;
            if (DateTime.Now < fechaCumpleannos.AddYears(edad)) edad--;

            return edad;
        }

        public static void ValidateSession()
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated)
                FormsAuthentication.RedirectToLoginPage();
        }

        public static double MeassureDistance(double latitude1, double longitude1, double latitude2, double longitude2)
        {
            /* Conjuro oscuro Made by Tibu */
            var aPlana = 298.25722;
            var radioPolar = 6378137.00;

            var f4 = latitude1 / 180 * Math.PI;
            var f5 = latitude2 / 180 * Math.PI;
            var f6 = f4 + f5;

            var f8 = longitude1 / 180 * Math.PI;
            var f9 = longitude2 / 180 * Math.PI;

            var b12 = Math.Sin(f4);
            var b13 = Math.Sin(f5);
            var b15 = Math.Cos(f4);
            var b16 = Math.Cos(f5);
            var b21 = Math.Cos(f8);
            var b22 = Math.Cos(f9);
            var e21 = Math.Cos(f9 - f8);
            var e22 = Math.Sin(f9 - f8);
            var b24 = Math.Sin(f6 / 2);

            var b25 = Math.Pow(b24, 2);
            var b31 = radioPolar * (1 + (1 / aPlana) * b25);
            var b32 = Math.Pow((b16 * e21 - b15), 2);
            var b33 = Math.Pow((b16 * e22), 2);
            var jj = (1 - 2 * (1 / aPlana)) * (b13 - b12);
            var b34 = Math.Pow(jj, 2);
            var b35 = Math.Sqrt(b32 + b33 + b34);
            var b36 = b31 * b35;
            var b37 = Math.Pow(b36, 3);

            var kmsDist = (b36 + b37 / (9.77 * (Math.Pow(10, 14)))) / 1000;

            return Math.Round(kmsDist, 1);
        }

        public static byte[] GetFileBinaryContent(string filePath, string cacheTimeoutKey = "CacheFileTimeout")
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new Exception();

            if (string.IsNullOrWhiteSpace(cacheTimeoutKey))
                throw new Exception();

            var path = (
                HttpContext.Current == null
                    ? filePath
                    : HttpContext.Current.Server.MapPath(filePath)
            );

            if (!File.Exists(path))
                throw new FileNotFoundException(path);

            var ext = Path.GetExtension(path).Replace(".", string.Empty).ToUpper();
            if (string.IsNullOrWhiteSpace(ext))
                ext = "FILE";

            var cacheKey = string.Concat(ext, ".", Regex.Replace(filePath, "[^A-Z]+", ".", RegexOptions.IgnoreCase).ToUpper());

            return cacheKey.FromCache(
                () => File.ReadAllBytes(path),
                cacheTimeoutKey,
                false
            );
        }

        public static string GetFileTextContent(string filePath, string cacheTimeoutKey = "CacheFileTimeout", Encoding encoding = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new Exception();

            if (string.IsNullOrWhiteSpace(cacheTimeoutKey))
                throw new Exception();

            var path = (
                HttpContext.Current == null
                    ? filePath
                    : HttpContext.Current.Server.MapPath(filePath)
            );

            if (!File.Exists(path))
                throw new FileNotFoundException(path);

            var ext = Path.GetExtension(path).Replace(".", string.Empty).ToUpper();
            if (string.IsNullOrWhiteSpace(ext))
                ext = "FILE";

            var cacheKey = string.Concat(ext, ".", Regex.Replace(filePath, "[^A-Z]+", ".", RegexOptions.IgnoreCase).ToUpper());

            return cacheKey.FromCache(
                () => {
                    if (encoding == null)
                        return File.ReadAllText(path);
                    else
                        return File.ReadAllText(path, encoding);
                },
                cacheTimeoutKey,
                false
            );
        }

        public static string GetFileTextContentWithResolvePath(string filePath, string cacheTimeoutKey = "CacheFileTimeout", Encoding encoding = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new Exception();

            if (string.IsNullOrWhiteSpace(cacheTimeoutKey))
                throw new Exception();

            var path = ResolvePath(filePath);

            if (!File.Exists(path))
                throw new FileNotFoundException(path);

            var ext = Path.GetExtension(path).Replace(".", string.Empty).ToUpper();
            if (string.IsNullOrWhiteSpace(ext))
                ext = "FILE";

            var cacheKey = string.Concat(ext, ".", Regex.Replace(filePath, "[^A-Z]+", ".", RegexOptions.IgnoreCase).ToUpper());

            return cacheKey.FromCache(
                () => {
                    if (encoding == null)
                        return File.ReadAllText(path);
                    else
                        return File.ReadAllText(path, encoding);
                },
                cacheTimeoutKey,
                false
            );
        }

        public static string GetCurrentPath(string append = "")
        {
            if (HttpContext.Current == null)
                return Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), append);
            else
                return HttpContext.Current.Server.MapPath("." + (string.IsNullOrWhiteSpace(append) ? string.Empty : "\\" + append));
        }

        public static string ResolvePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            if (HttpContext.Current == null)
                return Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    Path.GetDirectoryName(path.Replace("~/", "")),
                    Path.GetFileName(path)
                );
            else
                return HttpContext.Current.Server.MapPath(path);
        }

        public static bool IsValidEmail(string strIn)
        {
            if (String.IsNullOrEmpty(strIn))
                return false;

            // Use IdnMapping class to convert Unicode domain names.
            try
            {
                strIn = Regex.Replace(strIn, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }

            // Return true if strIn is in valid e-mail format.
            try
            {
                return Regex.IsMatch(strIn,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
        public static bool ContieneCaracteresNumericos(string inputString)
        {
            Regex patrom = new Regex(@"([0-9]|\\s)+");

            if (patrom.IsMatch(inputString))
                return true;    // Contiene caracteres NUMERICO
            else
                return false;   // No contiene caracteres Numericos

        }
        private static string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            IdnMapping idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException e)
            {
                throw e;
            }
            return match.Groups[1].Value + domainName;
        }

        public static string MakeRequest(string url)
        {
            string response = null;

            try
            {
                // Pido el stream de response y leo todo el contenido.
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                using (var stream = new StreamReader(request.GetResponse().GetResponseStream()))
                {
                    // En este punto si el objeto Solicitud superó todas las validaciones de negocio,
                    // entonces la respuesta será un objeto JSON con todos los datos de la solicitud.

                    response = stream.ReadToEnd();
                }

                return response;
            }
            catch (WebException ex)
            {
                if (ex.Status != WebExceptionStatus.Success)
                    response = "El WebService no respondió correctamente: Status = " + ex.Status.ToString() + "(" + (int)ex.Status + ")";

                // Si hay una excepción de tipo WebException, me fijo si el código de estado http es 400 (BadRequest)

                if (ex.Response != null && ((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.BadRequest)
                {
                    // Si el código de estado es 400 entonces pido el stream de response y leo todo el contenido.
                    // En caso de error, la API REST devuelve un objeto JSON con la estructura { "code": "", "desc": "" } detallando el error de negocio.

                    using (var stream = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        response = stream.ReadToEnd();
                    }
                }

                return response;
            }
        }

        public static void RetryWhenFail(Func<int, bool> action, Exception throwWhenOut = null, int retries = 3)
        {
            if (action == null)
                return;

            var @try = 1;

            //OMI: Corrección: Cuando en el ultimo intento era el valido, quedaba try == 3 en  y de todas formas ejecutaba el throw
            var @hasError = true;

            while ((@try <= retries) && @hasError)
            {
                if (action.Invoke(@try))
                    hasError = false;

                @try++;
            }

            if ((@hasError) && (throwWhenOut != null))
                throw throwWhenOut;
        }

        public static bool IsBase64String(string value)
        {
            //
            // http://stackoverflow.com/questions/3355407/validate-string-is-base64-format-using-regex
            //

            var base64Characters = new HashSet<char>() {
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P',
                'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f',
                'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v',
                'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '+', '/',
                '='
            };

            value = value
                .Replace("\r", String.Empty)
                .Replace("\n", String.Empty);

            if (string.IsNullOrEmpty(value))
                return false;
            else if (value.Length == 0 || value.Length % 4 != 0)
                return false;
            else if (value.Any(c => !base64Characters.Contains(c)))
                return false;

            try
            {
                Convert.FromBase64String(value);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static async Task SendMail(Message mail)
        {
            #region Validations

            if (mail == null)
                throw new ArgumentNullException();

            if (string.IsNullOrWhiteSpace(mail.From))
                throw new ArgumentNullException();

            if (!Utilities.IsValidEmail(mail.From))
                throw new Exceptions.EmailFormatException("Email From");

            if (mail.To == null)
                throw new ArgumentNullException();

            if (mail.To.Count == 0)
                throw new ArgumentNullException();

            if (string.IsNullOrWhiteSpace(mail.To.First()))
                throw new ArgumentNullException();

            var to = mail.To
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .ToList();

            if (to.Any(email => !Utilities.IsValidEmail(email)))
                throw new Exceptions.EmailFormatException("Email To");

            if (!string.IsNullOrWhiteSpace(mail.ReplayTo) && !Utilities.IsValidEmail(mail.ReplayTo))
                throw new Exceptions.EmailFormatException("Email ReplyTo");

            #endregion

            var message = new MailMessage
            {
                From = new MailAddress(mail.From),
                Subject = mail.Subject,
                Body = mail.Body,
                IsBodyHtml = mail.IsBodyHtml
            };

            message.To.Add(string.Join(", ", to));

            if (!string.IsNullOrWhiteSpace(mail.ReplayTo))
                message.ReplyToList.Add(mail.ReplayTo);


            #region Adding linked resources

            var mediaType = (mail.IsBodyHtml ? "text/html" : "text/plain");

            if (mail.LinkedResources != null && mail.LinkedResources.Count > 0)
            {
                var view = AlternateView.CreateAlternateViewFromString(mail.Body, null, mediaType);

                mail.LinkedResources.ForEach(r =>
                {
                    var lr = new LinkedResource(r.Stream) { ContentId = r.ContentId };
                    view.LinkedResources.Add(lr);
                });

                message.AlternateViews.Add(view);
            }

            #endregion


            #region Adding attachments

            if (mail.Attachments != null && mail.Attachments.Count > 0)
            {
                mail.Attachments.ForEach(attach =>
                    message.Attachments.Add(new System.Net.Mail.Attachment(attach.Stream, attach.Name))
                );
            }

            #endregion


            #region SMTP Sending

            ServicePointManager.ServerCertificateValidationCallback = (s, certificate, chain, policyErrors) => true; ;

            using (var client = new SmtpClient
            {
                Host = "SMTPHost".FromAppSettings(string.Empty, true),
                Port = "SMTPPort".FromAppSettings(-1, true),
                Credentials = new NetworkCredential(
                        "SMTPUser".FromAppSettings(string.Empty, true),
                        "SMTPPassword".FromAppSettings(string.Empty, true)
                    ),
                EnableSsl = "SMTPEnableSSL".FromAppSettings(false),
                Timeout = "SMTPTimeout".FromAppSettings(30000)
            })
            {
                if (mail.Async)
                    await client.SendMailAsync(message);
                else
                    client.Send(message);
            }

            #endregion
        }

        /// <summary>
        /// Valor por defecto de un tipo
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetDefault<T>(this T type) where T : Type
        {
            //Type isNullable = Nullable.GetUnderlyingType(type);

            if (type.IsNullableType())
                return null;  //Activator.CreateInstance(isNullable);
            else if (type.IsValueType)
                return Activator.CreateInstance(type);
            else
                return null;
        }

        public static bool IsNullableType(this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        public static string HandleRequestError(Exception ex)
        {
            return HandleRequestError(ex, 0);
        }

        public static string HandleRequestError(Exception ex, int idAud)
        {
            var request = HttpContext.Current.Request;
            var url = request.RawUrl;
            var body = string.Empty;

            if (request.HttpMethod.Equals("post", StringComparison.InvariantCultureIgnoreCase))
            {
                request.InputStream.Position = 0;
                using (var sr = new StreamReader(request.InputStream))
                {
                    body = sr.ReadToEnd();
                }
            }

            _log.Error(ex,
                $"\r\nURL: {url}\r\n{(string.IsNullOrWhiteSpace(body) ? string.Empty : $"Body: {body}\r\n")}\r\n{(idAud == 0 ? string.Empty : $"idAud: {idAud}\r\n")}");

            return ex.ToJson();
        }

        public static string GetAssemblyVersion(string dllName = null)
        {
            string version = "Versión: x.x (Build x)";
            string path = null;

            AssemblyName assembly = null;
            FileVersionInfo fvi = null;

            try
            {
                if (string.IsNullOrWhiteSpace(dllName))
                    assembly = Assembly.GetExecutingAssembly()?.GetName();
                else
                {
                    //Puede que haya que usar GetCallingAssembly() si da error en el path
                    path = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().EscapedCodeBase).LocalPath);

                    if (!dllName.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
                        dllName += ".dll";

                    Assembly assm = Assembly.LoadFrom(Path.Combine(path, dllName));

                    if (assm != null)
                    {
                        assembly = assm.GetName();
                        fvi = FileVersionInfo.GetVersionInfo(assm.Location);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Log(NLog.LogLevel.Error, $"No se pudo cargar el ensamblado para {dllName}\nRuta: {(string.IsNullOrWhiteSpace(path) ? "No establecida" : path)} ");
                _log.Log(NLog.LogLevel.Error, ex.ToString());

                throw;
            }

            if (assembly != null)
                version = $"Versión: {assembly.Version.Major}.{assembly.Version.Minor} (Build {fvi.FileBuildPart})";

            return version;
        }

        public static IEnumerable<T> GetEnumValues<T>()
        {
            if (typeof(T).BaseType != typeof(Enum))
                throw new ArgumentException("T Debe ser de tipo Enum");

            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        /// <summary>
        /// Permite copiar las propiedades de dos clases (Tipos primitivos)
        /// </summary>
        /// <typeparam name="D">Tipo de la Clase Destino</typeparam>
        /// <typeparam name="T">Tipo de la Clase Origen</typeparam>
        /// <param name="from">Objeto a copiar</param>
        /// <returns></returns>
        public static D Copy<D, T>(T from)
            where T : class
            where D : class, new()
        {
            D to = new D();

            PropertyInfo[] toProps = to.GetType().GetProperties();
            PropertyInfo[] fromProps = from.GetType().GetProperties();

            foreach (PropertyInfo pInfo in toProps)
            {
                PropertyInfo pI = fromProps.FirstOrDefault(f => f.Name == pInfo.Name);
                if (pI != null)
                    pInfo.SetValue(to, pI.GetValue(from));
            }

            return to;
        }

        public static string UrlCombine(string urlBase, params string[] queryString)
        {
            if (urlBase.IsNull())
                throw new ArgumentNullException();

            if (queryString == null || queryString.Length == 0)
                return urlBase;

            var protocol = Regex.Match(urlBase, @"[a-zA-Z].*:\/\/").Value;

            if (!protocol.IsNull())
            {
                urlBase = urlBase.Replace(protocol, string.Empty);
                protocol = protocol.Replace("/", "\\");
            }

            urlBase = Regex.Replace(urlBase, @"(\/)\1{0,}", @"\");

            if (!urlBase.Equals(@"\"))
                urlBase = Regex.Replace(urlBase, @"^\\|\\$", string.Empty);

            var query = string.Empty;
            var aux = string.Empty;

            foreach (var qs in queryString)
            {
                aux = Regex.Replace(qs, @"(\/)\1{0,}", @"\");
                aux = Regex.Replace(aux, @"^\\|\\$", string.Empty);
                query = Path.Combine(query, aux);
            }

            var url = Path.Combine(protocol, Path.Combine(urlBase, query));

            return url.Replace("\\", "/");
        }

        public static List<T> AddEx<T>(List<T> input, T value)
        {
            input.Add(value);
            return input;
        }

        /// <summary>
        /// CleanCRLF
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// Utilizado para el envio de caracteres en SISE
        /// Quita los siguientes caracteres:
        /// \r = 13 = CR = Carriage return = Retorno de carro
        /// \n = 10 = LF = Line Feed = Avance de linea
        /// [@NewLine@]: Usar Replace, para colocar el caracter que se necesite/requiera
        /// example: Utilities.CleanCRLF(variableALimpiar).Replace("[@NewLine@]", " ").ToUpper()
        public static string CleanCRLF(string input)
        {
            return input
            .Replace("\r\n", "[@NewLine@]")
            .Replace("\r", "[@NewLine@]")
            .Replace("\n", "[@NewLine@]")
            .Replace("\\", "[@NewLine@]")
            .Replace("\"", "[@NewLine@]");
        }

        /// <summary>
        /// CleanTarjeta
        /// </summary>
        /// <param name="nroTarjeta"></param>
        /// <returns></returns>
        /// Utilizado para el envio del nro de tarjeta en SISE
        /// Quita los siguientes caracteres:
        /// -: guiones
        /// " ": espacios en blanco
        public static string CleanTarjeta(string nroTarjeta)
        {
            return nroTarjeta.Replace("-", string.Empty).Replace(" ", string.Empty);
        }

        public static string CleanCaracteresEspeciales(string palabra)
        {
            return
                palabra
                         //àáâäãåą
                         .Replace("à", "a")
                         .Replace("á", "a")
                         .Replace("â", "a")
                         .Replace("ä", "a")
                         .Replace("ã", "a")
                         .Replace("å", "a")
                         .Replace("ą", "a")

                        //ęèéêëė
                        .Replace("ę", "e")
                        .Replace("è", "e")
                        .Replace("é", "e")
                        .Replace("ê", "e")
                        .Replace("ë", "e")
                        .Replace("ė", "e")

                        //įìíîï
                        .Replace("į", "i")
                        .Replace("ì", "i")
                        .Replace("í", "i")
                        .Replace("î", "i")
                        .Replace("ï", "i")

                        //òóôöõ
                        .Replace("ò", "o")
                        .Replace("ó", "o")
                        .Replace("ô", "o")
                        .Replace("ö", "o")
                        .Replace("õ", "o")

                        //ùúûüųū
                        .Replace("ù", "u")
                        .Replace("ú", "u")
                        .Replace("û", "u")
                        .Replace("ü", "u")
                        .Replace("ų", "u")
                        .Replace("ū", "u")

                        //ÀÁÂÄÃÅĄ
                        .Replace("À", "A")
                        .Replace("Á", "A")
                        .Replace("Â", "A")
                        .Replace("Ä", "A")
                        .Replace("Å", "A")
                        .Replace("Ą", "A")

                        //ĖĘÈÉÊË
                        .Replace("Ė", "E")
                        .Replace("Ę", "E")
                        .Replace("È", "E")
                        .Replace("É", "E")
                        .Replace("Ê", "E")
                        .Replace("Ë", "E")

                        //ÌÍÎÏĮ
                        .Replace("Ì", "I")
                        .Replace("Í", "I")
                        .Replace("Î", "I")
                        .Replace("Ï", "I")
                        .Replace("Į", "I")

                        //ÒÓÔÖÕØ
                        .Replace("Ò", "O")
                        .Replace("Ó", "O")
                        .Replace("Ô", "O")
                        .Replace("Ö", "O")
                        .Replace("Õ", "O")
                        .Replace("Ø", "O")

                        //ÙÚÛÜŲŪ
                        .Replace("Ù", "U")
                        .Replace("Ú", "U")
                        .Replace("Û", "U")
                        .Replace("Ü", "U")
                        .Replace("Ų", "U")
                        .Replace("Ū", "U")

                        //øÿýżźçčćšž
                        .Replace("ø", "0")
                        .Replace("ÿ", "y")
                        .Replace("ý", "y")
                        .Replace("ż", "z")
                        .Replace("ź", "z")
                        .Replace("ç", "c")
                        .Replace("č", "c")
                        .Replace("ć", "c")
                        .Replace("š", "s")
                        .Replace("ž", "z")

                        //ĆČÇ
                        .Replace("Ć", "C")
                        .Replace("Č", "C")
                        .Replace("Ç", "C")

                        //ŁŃ
                        .Replace("Ł", "t")
                        .Replace("Ń", "N")
                        //ŸÝ
                        .Replace("Ÿ", "Y")
                        .Replace("Ý", "Y")

                        //ŻŹŽ
                        .Replace("Ż", "Z")
                        .Replace("Ź", "Z")
                        .Replace("Ž", "Z")

                        //ßŒÆŠ∂ð,.łń
                        .Replace("ß", "")
                        .Replace("Œ", "")
                        .Replace("Æ", "")
                        .Replace("Š", "S")
                        .Replace("∂", "")
                        .Replace("ð", "")
                        .Replace(",", "")
                        .Replace(".", "")
                        .Replace("ł", "")
                        .Replace(".", "")
                        .Replace("ń", "n");

        }

        #region CUITS
        public static string CUITSinFormato(string CUIT)
        {

            return CUIT.Replace("-", "");

        }

        public static string CUITFormateado(string CUIT)
        {
            CUIT = Utilities.CUITSinFormato(CUIT);

            if (CUIT.Length == 0) return string.Empty;
            return CUIT.Substring(0, 2) + "-" +
                   CUIT.Substring(2, 8) + "-" +
                   CUIT.Substring(10);

        }
        #endregion

        #region funcion parcea string y devuelve una lista 

        public static List<string> textoToListStringBloque(string texto, int limitexBloque)
        {
        texto = Utilities.CleanCRLF(texto).Replace("[@NewLine@]", " ").ToUpper();
        int longitudCadena = texto.Length;
        int cantBloques = longitudCadena / limitexBloque;
        if ((cantBloques * limitexBloque) < longitudCadena)
            cantBloques++;
            int posicionBloque = 1;
        List<string> lista = new List<string>();
        int desde = 0;
        while (posicionBloque <=cantBloques)
        {
        string cadenaBloque = texto.Substring(desde, ((longitudCadena - desde) >= limitexBloque ? limitexBloque : longitudCadena - desde));
        lista.Add(cadenaBloque);
        desde = desde + limitexBloque;
        posicionBloque++;
        }
        return lista;
}

        #endregion
    }
}
