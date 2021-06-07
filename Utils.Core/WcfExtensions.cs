using System;
using System.ServiceModel;

namespace Utils.Core
{
    public static class WcfExtensions
    {
        public static void Using<T>(this T client,Action<T> work)
            where T : ICommunicationObject{
            try
            {
                work(client);
                client.Close();
            }
            catch (CommunicationException)
            {
                client.Abort();
                throw;
            }
            catch (TimeoutException)
            {
                client.Abort();
                throw;
            }
            catch (Exception) {
                client.Abort();
                throw;
            }
        }

    }
}
