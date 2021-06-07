using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;

namespace Utils.Core
{
    [Serializable]
    public sealed class CipherUtilException : Exception
    {
        public string ErrorDescription { get; set; }

        public Exception Exc { get; set; }

        public string No { get; set; }
        public string MethodCaller { get; set; } 

        public Dictionary<string, string> Properties { get; set; }

        public CipherUtilException(string no, string errorDescription, Dictionary<string, string> properties = null, Exception exc = null, [CallerMemberName] string caller = null)
        {
            this.No = no;
            this.ErrorDescription = errorDescription;
            this.Exc = exc;
            this.MethodCaller = caller;

            properties ??= new Dictionary<string, string>();
            properties.Add("No", No);
            properties.Add("FriendlyMessage", ErrorDescription);
            if (!string.IsNullOrEmpty(caller)) {
                properties.Add("MethodCaller", caller);
            }
            if (!(Exc is null)) {
                properties.Add("Ex.Messsage", Exc.Message);
            }

            this.Properties = properties;
        }

        private CipherUtilException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
            // ...
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            // ...
        }
    }
}
