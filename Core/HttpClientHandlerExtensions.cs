using System.Security.Cryptography.X509Certificates;
using System.Net.Http;
using System;

namespace Util
{
    public static class HttpClientHandlerExtensions
    {
        public static void AddCertificatesFromStore(this HttpClientHandler handler, StoreName storeName, StoreLocation storeLocation)
        {
            X509Store store = new X509Store(storeName, storeLocation);
            try
            {
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                X509Certificate2Collection fcollection = store.Certificates;
                foreach (X509Certificate2 x509 in fcollection)
                {
                    handler.ClientCertificates.Add(x509);
                }
            }
            finally
            {
                store.Close();
            }
        }

        public static void AddCertificateFromStoreByThumbprint(this HttpClientHandler handler, StoreName storeName, StoreLocation storeLocation, string thumbprint, bool validOnly)
        {
            X509Store store = new X509Store(storeName, storeLocation);
            try
            {
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                X509Certificate2Collection fcollection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, validOnly: validOnly);

                if (fcollection.Count > 0)
                {
                    handler.ClientCertificates.Add(fcollection[0]);
                }
                else
                {
                    throw new Exception($"Certificate with thumbprint {thumbprint} not found.");
                }
            }
            finally
            {
                store.Close();
            }
        }
    }
}