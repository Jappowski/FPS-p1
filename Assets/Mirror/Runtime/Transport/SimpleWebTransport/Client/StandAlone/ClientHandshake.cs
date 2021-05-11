using System;
using System.Security.Cryptography;
using System.Text;

namespace Mirror.SimpleWeb
{
    /// <summary>
    ///     Handles Handshake to the server when it first connects
    ///     <para>The client handshake does not need buffers to reduce allocations since it only happens once</para>
    /// </summary>
    internal class ClientHandshake
    {
        public bool TryHandshake(Connection conn, Uri uri)
        {
            try
            {
                var stream = conn.stream;

                var keyBuffer = new byte[16];
                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(keyBuffer);
                }

                var key = Convert.ToBase64String(keyBuffer);
                var keySum = key + Constants.HandshakeGUID;
                var keySumBytes = Encoding.ASCII.GetBytes(keySum);
                Log.Verbose($"Handshake Hashing {Encoding.ASCII.GetString(keySumBytes)}");

                var keySumHash = SHA1.Create().ComputeHash(keySumBytes);

                var expectedResponse = Convert.ToBase64String(keySumHash);
                var handshake =
                    "GET /chat HTTP/1.1\r\n" +
                    $"Host: {uri.Host}:{uri.Port}\r\n" +
                    "Upgrade: websocket\r\n" +
                    "Connection: Upgrade\r\n" +
                    $"Sec-WebSocket-Key: {key}\r\n" +
                    "Sec-WebSocket-Version: 13\r\n" +
                    "\r\n";
                var encoded = Encoding.ASCII.GetBytes(handshake);
                stream.Write(encoded, 0, encoded.Length);

                var responseBuffer = new byte[1000];

                var lengthOrNull = ReadHelper.SafeReadTillMatch(stream, responseBuffer, 0, responseBuffer.Length,
                    Constants.endOfHandshake);

                if (!lengthOrNull.HasValue)
                {
                    Log.Error("Connected closed before handshake");
                    return false;
                }

                var responseString = Encoding.ASCII.GetString(responseBuffer, 0, lengthOrNull.Value);

                var acceptHeader = "Sec-WebSocket-Accept: ";
                var startIndex = responseString.IndexOf(acceptHeader) + acceptHeader.Length;
                var endIndex = responseString.IndexOf("\r\n", startIndex);
                var responseKey = responseString.Substring(startIndex, endIndex - startIndex);

                if (responseKey != expectedResponse)
                {
                    Log.Error($"Response key incorrect, Response:{responseKey} Expected:{expectedResponse}");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Log.Exception(e);
                return false;
            }
        }
    }
}