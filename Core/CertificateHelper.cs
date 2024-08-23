using System;
using System.Security.Cryptography.X509Certificates;

namespace Util
{

    public class CertificateHelper
    {
        public static X509Certificate2 GetCertificateFromStore(string thumbprint)
        {
            Util.Diagnostics.Event.Info($"Searching cetificate {thumbprint} in all stores and locations");
            if (string.IsNullOrWhiteSpace(thumbprint))
                throw new ArgumentNullException(nameof(thumbprint), "Thumbprint cannot be null or empty.");

            // Normalize the thumbprint to remove any whitespace
            thumbprint = thumbprint.Replace(" ", string.Empty).ToUpperInvariant();

            // List of store locations to search
            StoreLocation[] storeLocations = { StoreLocation.CurrentUser, StoreLocation.LocalMachine };

            // List of store names to search
            StoreName[] storeNames =
            {
            StoreName.My,
            StoreName.Root,
            StoreName.TrustedPeople,
            StoreName.TrustedPublisher,
            StoreName.AuthRoot,
            StoreName.CertificateAuthority,
            StoreName.AddressBook
        };

            foreach (var storeLocation in storeLocations)
            {
                foreach (var storeName in storeNames)
                {
                    using (X509Store store = new X509Store(storeName, storeLocation))
                    {
                        try
                        {
                            store.Open(OpenFlags.ReadOnly);
                            foreach (var cert in store.Certificates)
                            {
                                if (cert.Thumbprint != null && cert.Thumbprint.Equals(thumbprint, StringComparison.OrdinalIgnoreCase))
                                {
                                    return cert;
                                }
                            }
                        }
                        finally
                        {
                            store.Close();
                        }
                    }
                }
            }

            throw new Exception($"Certificate with thumbprint {thumbprint} not found in any store.");
        }

        public static X509Certificate2 GetCertificateFromStore(StoreName storeName, StoreLocation storeLocation, string thumbprint)
        {
            Util.Diagnostics.Event.Info($"Searching cetificate {thumbprint} in store {storeName} and location {storeLocation}");
            using (var store = new X509Store(storeName, storeLocation))
            {
                store.Open(OpenFlags.ReadOnly);
                var certs = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                if (certs.Count == 0)
                {
                    throw new Exception($"Certificate {thumbprint} not found in store {storeName} at {storeLocation}.");
                }
                return certs[0];
            }
        }
    }

}