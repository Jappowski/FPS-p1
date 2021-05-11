using System.IO;
using UnityEngine;

namespace Mirror.SimpleWeb
{
    internal class SslConfigLoader
    {
        internal static SslConfig Load(SimpleWebTransport transport)
        {
            // don't need to load anything if ssl is not enabled
            if (!transport.sslEnabled)
                return default;

            var certJsonPath = transport.sslCertJson;

            var cert = LoadCertJson(certJsonPath);

            return new SslConfig(
                transport.sslEnabled,
                sslProtocols: transport.sslProtocols,
                certPath: cert.path,
                certPassword: cert.password
            );
        }

        internal static Cert LoadCertJson(string certJsonPath)
        {
            var json = File.ReadAllText(certJsonPath);
            var cert = JsonUtility.FromJson<Cert>(json);

            if (string.IsNullOrEmpty(cert.path))
                throw new InvalidDataException("Cert Json didn't not contain \"path\"");
            if (string.IsNullOrEmpty(cert.password))
                // password can be empty
                cert.password = string.Empty;

            return cert;
        }

        internal struct Cert
        {
            public string path;
            public string password;
        }
    }
}