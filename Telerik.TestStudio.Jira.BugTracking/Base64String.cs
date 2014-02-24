using System;
using System.Text;

using Newtonsoft.Json;

namespace Telerik.TestStudio.Jira.BugTracking
{
    /// <summary>
    /// A utility class for creating and minipulating Base64 strings. It is used to
    /// try and keep the Password somewhat protected i.e. never shown in plain text.
    /// </summary>
    [Serializable]
    [JsonObject]
    public class Base64String
    {
        #region Fields

        // Storage for the Base64 encoded string
        private string base64value;

        #endregion

        #region Properties

        /// <summary>
        /// Return the length of the encoded string.
        /// </summary>
        [JsonIgnore]
        public int Length
        {
            get
            {
                return this.base64value.Length;
            }
        }

        public string Base64value
        {
            get { return this.base64value; }
            set { this.base64value = value; }   // set is only used for deserialization
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public Base64String()
        {
            this.base64value = string.Empty;
        }

        /// <summary>
        /// Constructor that takes a plain string and encodes it.
        /// </summary>
        /// <param name="plainString">Plain text string to be encoded.</param>
        public Base64String(string plainString)
        {
            this.base64value = EncodeTo64(plainString);
        }

        /// <summary>
        /// Constructor that takes a string and optionally encodes it.
        /// </summary>
        /// <param name="plainString">The string to optionally encode.</param>
        /// <param name="encode">Whether or not the string needs to be encoded.
        /// If false we assume the string is already Base64 encoded.</param>
        public Base64String(string plainString, bool encode)
        {
            if (encode == false)
            {
                this.base64value = plainString;
            }
            else
            {
                this.base64value = EncodeTo64(plainString);
            }
        }

        /// <summary>
        /// Constructor that copies another Base64String value
        /// </summary>
        /// <param name="encodedString"></param>
        public Base64String(Base64String encodedString)
        {
            this.base64value = encodedString.base64value;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Convert a plain string into a Base64String object,
        /// encoding it in the process
        /// </summary>
        /// <param name="plainString">The plain string to encode</param>
        /// <returns>A Base64 encoded string object</returns>
        public static implicit operator Base64String(string plainString)
        {
            return new Base64String(plainString);
        }

        /// <summary>
        /// Convert a Base64String into an ordinary string, leaving it encoded
        /// </summary>
        /// <param name="encodedString">The Base64String to extract the encode string from</param>
        /// <returns>A plain string that is still Base64 encoded.</returns>
        public static implicit operator string(Base64String encodedString)
        {
            if (null != encodedString)
            {
                return encodedString.base64value;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Override to return the encoded string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.base64value;
        }

        /// <summary>
        /// Insert the string in front of this Base64 encoded string
        /// </summary>
        /// <param name="plainText">The plain text string to insert then re-encode the result.</param>
        /// <returns>A new Base64String object.</returns>
        public Base64String prefix(string plainText)
        {
            return EncodeTo64(plainText + DecodeFrom64(this.base64value));
        }

        #endregion

        #region Private methods for internal use only

        /// <summary>
        /// The method create a Base64 encoded string from a normal string.
        /// </summary>
        /// <param name="toEncode">The String containing the characters to encode.</param>
        /// <returns>The Base64 encoded string.</returns>
        private static Base64String EncodeTo64(string toEncode)
        {
            return new Base64String(Convert.ToBase64String(Encoding.Default.GetBytes(toEncode)), false);
        }

        /// <summary>
        /// The method to Decode your Base64 strings.
        /// </summary>
        /// <param name="encodedData">The String containing the characters to decode.</param>
        /// <returns>A String containing the results of decoding the specified sequence of bytes.</returns>
        private static string DecodeFrom64(string encodedData)
        {
            return Encoding.Default.GetString(Convert.FromBase64String(encodedData));
        }

        #endregion
    }
}
