using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace Util
{
    public class SslHandshakeTester
    {
        public class CertificateInfo
        {
            public string StoreLocation { get; set; }
            public string StoreName { get; set; }
            public string Certificate { get; set; }
        }

        public static List<CertificateInfo> GetAllCertificatesInfo()
        {
            List<CertificateInfo> certificates = new List<CertificateInfo>();

            // Define the stores we want to check
            StoreLocation[] locations = { StoreLocation.CurrentUser, StoreLocation.LocalMachine };
            StoreName[] storeNames = (StoreName[])Enum.GetValues(typeof(StoreName));

            foreach (var location in locations)
            {
                foreach (var storeName in storeNames)
                {
                    try
                    {
                        using (X509Store store = new X509Store(storeName, location))
                        {
                            store.Open(OpenFlags.ReadOnly);

                            foreach (X509Certificate2 cert in store.Certificates)
                            {
                                certificates.Add(new CertificateInfo
                                {
                                    StoreLocation = location.ToString(),
                                    StoreName = storeName.ToString(),
                                    Certificate = cert.ToString()
                                });
                            }

                            store.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.Diagnostics.Event.Error($"Error accessing store {storeName} at {location}", ex);
                    }
                }
            }

            return certificates;
        }

        public static string GetAllCertificatesInfoFormatted()
        {
            using (StringWriter stringWriter = new StringWriter())
            {
                // Define the stores we want to check
                StoreLocation[] locations = { StoreLocation.CurrentUser, StoreLocation.LocalMachine };
                StoreName[] storeNames = (StoreName[])Enum.GetValues(typeof(StoreName));

                foreach (var location in locations)
                {
                    stringWriter.WriteLine($"Location: {location}");
                    foreach (var storeName in storeNames)
                    {
                        try
                        {
                            using (X509Store store = new X509Store(storeName, location))
                            {
                                store.Open(OpenFlags.ReadOnly);
                                stringWriter.WriteLine($"\tStore: {storeName}");
                                foreach (X509Certificate2 cert in store.Certificates)
                                {
                                    stringWriter.WriteLine($"\t\tSubject: {cert.Subject}");
                                    stringWriter.WriteLine($"\t\tIssuer: {cert.Issuer}");
                                    stringWriter.WriteLine($"\t\tThumbprint: {cert.Thumbprint}");
                                    stringWriter.WriteLine($"\t\tExpiration Date: {cert.NotAfter}");
                                    stringWriter.WriteLine($"\t\tFriendly Name: {cert.FriendlyName}");
                                    stringWriter.WriteLine("\t\t------------------------------");
                                }
                                store.Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            stringWriter.WriteLine($"\t\tError accessing store: {ex.Message}");
                        }
                    }
                    stringWriter.WriteLine();
                }

                return stringWriter.ToString();
            }
        }

        public static void TestSslTlsHandshake(
            string hostname,
            int port,
            StoreName storeName,
            StoreLocation storeLocation,
            string clientCertThumbprint,
            bool ignoreServerCertificate = false)
        {

            X509Certificate2 clientCertificate = CertificateHelper.GetCertificateFromStore(storeName, storeLocation, clientCertThumbprint);
            TestSslTlsHandshake(hostname, port, clientCertificate, ignoreServerCertificate);
        }

        public static void TestSslTlsHandshake(
            string hostname,
            int port,
            string clientCertThumbprint,
            bool ignoreServerCertificate = false)
        {

            X509Certificate2 clientCertificate = CertificateHelper.GetCertificateFromStore(clientCertThumbprint);
            TestSslTlsHandshake(hostname, port, clientCertificate, ignoreServerCertificate);
        }

        public static void TestSslTlsHandshake(
            string hostname,
            int port,
            bool ignoreServerCertificate = false)
        {
            string logMessage = ignoreServerCertificate
                ? "Running SSL/TLS handshake test, server authentication, ignoring server certificate"
                : "Running SSL/TLS handshake test, server authentication";

            Util.Diagnostics.Event.Info(logMessage);

            try
            {
                using (var client = new TcpClient(hostname, port))
                using (var sslStream = new SslStream(client.GetStream(), false,
                    new RemoteCertificateValidationCallback(ignoreServerCertificate ?
                        (RemoteCertificateValidationCallback)IgnoreServerCertificate :
                        ValidateServerCertificate), null))
                {
                    // Pass an empty X509CertificateCollection for client certificates
                    sslStream.AuthenticateAsClient(hostname, null, System.Security.Authentication.SslProtocols.Tls12, false);
                    Util.Diagnostics.Event.Info("SSL/TLS handshake successful");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to establish SSL/TLS handshake", ex);
            }
        }

        public static void TestSslTlsHandshake(
            string hostname,
            int port,
            X509Certificate clientCertificate,
            bool ignoreServerCertificate = false)
        {
            string logMessage = ignoreServerCertificate
            ? "Running SSL/TLS handshake test, client and server authentication, ignoring server certificate"
            : "Running SSL/TLS handshake test, client and server authentication";

            Util.Diagnostics.Event.Info(logMessage);

            try
            {
                using (var client = new TcpClient(hostname, port))
                using (var sslStream = new SslStream(client.GetStream(), false,
                    new RemoteCertificateValidationCallback(ignoreServerCertificate ?
                        (RemoteCertificateValidationCallback)IgnoreServerCertificate :
                        ValidateServerCertificate), null))
                {
                    sslStream.AuthenticateAsClient(hostname, new X509CertificateCollection { clientCertificate }, System.Security.Authentication.SslProtocols.Tls12, false);
                    Util.Diagnostics.Event.Info("SSL/TLS handshake successful");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to establish SSL/TLS handshake", ex);
            }
        }

        private static bool ValidateServerCertificate(
            object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            Util.Diagnostics.Event.Info($"Received server's certificate: {certificate.ToString()}");
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;
            Util.Diagnostics.Event.Error($"Server certificate error: {sslPolicyErrors}");
            return false;
        }

        private static bool IgnoreServerCertificate(
            object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            Util.Diagnostics.Event.Info($"Received server's certificate: {certificate.ToString()}");
            return true;
        }
    }

}