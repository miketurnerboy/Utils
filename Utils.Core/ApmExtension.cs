using Elastic.Apm;
using Elastic.Apm.Api;
using Serilog;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Web;

namespace Utils.Core
{
    public static class ApmExtension
    {
        public static ITransaction TransactionInit {
            get { return (ITransaction)HttpContext.Current.Items["transactionInit"]; }
            set { HttpContext.Current.Items["transactionInit"] = value; }
        }

        public static string TransactionInitName {
            get { return (string)HttpContext.Current.Items["transactionInitName"]; }
            set { HttpContext.Current.Items["transactionInitName"] = value; }
        }

        public static ITransaction TransactionSub
        {
            get { return (ITransaction)HttpContext.Current.Items["transactionSub"]; }
            set { HttpContext.Current.Items["transactionSub"] = value; }
        }

        public static string Identifier {
            get { return (string)HttpContext.Current.Items["identifier"]; }
            set { HttpContext.Current.Items["indetifier"] = value; }
        }

        public static void Start(string type = ApiConstants.ActionExec, string identifier = null, [CallerMemberName] string caller = null) {
            try
            {
                TransactionInit = Agent.Tracer.StartTransaction(caller, type);
                TransactionInitName = TransactionInit.OutgoingDistributedTracingData.SerializeToString();
                if (!string.IsNullOrEmpty(Identifier)) {
                    Identifier = identifier;
                    TransactionInit.Labels.Add("IdentificadorEntidad", identifier);
                }
            }
            catch (Exception e) {
                Log.Error(e, $"Fail Initialze APM Exception: {e.Message} | {e.StackTrace}");
            }
        }

        public static void StartSubTransaction(string type = ApiConstants.ActionExec, [CallerMemberName] string caller = null) {
            try
            {
                TransactionSub = Agent.Tracer.StartTransaction(caller, type, DistributedTracingData.TryDeserializeFromString(TransactionInitName));
            }
            catch (Exception e){
                Log.Error(e, $"APM Sub Exception: {e.Message} | {e.StackTrace}");
            }
        }

        public static void InsertLabelSub(string key, string value) {
            TransactionSub?.Labels.Add(key, value);
        }

        public static void InsertLabelInit(string key, string value) {
            TransactionInit?.Labels.Add(key, value);
        }

        public static void InsertLabelsSub(Dictionary<string,string> properties)
        {
            foreach (KeyValuePair<string, string> p in properties) { TransactionSub?.Labels.Add(p.Key, p.Value); }
        }

        public static void InsertLabelsInit(Dictionary<string, string> properties)
        {
            foreach (KeyValuePair<string, string> p in properties) { TransactionInit?.Labels.Add(p.Key, p.Value); }
        }

        public static void AddExceptionInit(Exception e) {
            TransactionInit?.CaptureException(e);
        }

        public static void AddExceptionSub(Exception e) {
            TransactionSub?.CaptureException(e);
        }

        public static void EndSub() {
            TransactionSub?.End();
        }

        public static void EndSub(out double duration)
        {
            TransactionSub?.End();
            duration = TransactionSub?.Duration ?? 0.0;
        }

        public static void End() {
            TransactionSub?.End();
            TransactionInit?.End();

            int size = HttpContext.Current.Items.Count;
            if (size > 0) {
                var keys = new object[size];
                HttpContext.Current.Items.Keys.CopyTo(keys, 0);

                for (int i = 0; i < size; i++) {
                    var obj = HttpContext.Current.Items[keys[i]] as IDisposable;
                    obj?.Dispose();
                }
            }
        }

        public static void End(out double duration)
        {
            TransactionSub?.End();
            TransactionInit?.End();

            duration = TransactionInit?.Duration ?? 0.0;

            int size = HttpContext.Current.Items.Count;
            if (size > 0)
            {
                var keys = new object[size];
                HttpContext.Current.Items.Keys.CopyTo(keys, 0);

                for (int i = 0; i < size; i++)
                {
                    var obj = HttpContext.Current.Items[keys[i]] as IDisposable;
                    obj?.Dispose();
                }
            }
        }
    }
}
