using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace Util
{
    public class SslHandshakeTester
    {
        public static void TestSslTlsHandshake(string hostname, int port, string clientCertThumbprint = null)
        {
            try
            {
                using (var client = new TcpClient(hostname, port))
                using (var sslStream = new SslStream(client.GetStream(), false,
                    new RemoteCertificateValidationCallback(ValidateServerCertificate), null))
                {
                    // Load client certificate if specified
                    X509Certificate2 clientCertificate = null;
                    if (!string.IsNullOrEmpty(clientCertThumbprint))
                    {
                        clientCertificate = GetCertificateFromStore(clientCertThumbprint);
                    }

                    // Authenticate as client with optional client certificate
                    sslStream.AuthenticateAsClient(hostname, new X509CertificateCollection { clientCertificate }, System.Security.Authentication.SslProtocols.Tls12, false);
                    Console.WriteLine("SSL/TLS handshake successful");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to establish SSL/TLS handshake: {ex.Message}");
            }
        }

        // Example of server certificate validation callback
        private static bool ValidateServerCertificate(
            object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine($"Certificate error: {sslPolicyErrors}");
            return false;
        }

        // Retrieve client certificate from the Windows Certificate Store by thumbprint
        private static X509Certificate2 GetCertificateFromStore(string thumbprint)
        {
            using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly);
                var certs = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                return certs.Count > 0 ? certs[0] : null;
            }
        }
    }

}