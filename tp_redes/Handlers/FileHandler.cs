using System;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using TP_REDES.Config;

namespace TP_REDES.Handlers
{
    public static class FileHandler
    {
        private static string ruta = ServerConfigLoader.Load().Path;

        public static void ServeFile(StreamWriter writer, string url)
        {
            if (url == "/") url = "/index.html";

            string archivoRuta = Path.Combine(ruta , url.TrimStart('/'));
            if (File.Exists(archivoRuta))
            {
                string mimeType = GetMimeType(Path.GetExtension(archivoRuta));
                writer.WriteLine("HTTP/1.1 200 OK");
                writer.WriteLine($"Content-Type: {mimeType}");
                writer.WriteLine("Content-Encoding: gzip");
                writer.WriteLine("Connection: close");
                writer.WriteLine();

                using (FileStream fs = new FileStream(archivoRuta, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    byte[] compressedData = Compress(buffer);
                    writer.BaseStream.Write(compressedData, 0, compressedData.Length);
                }
            }
            else
            {
                writer.WriteLine("HTTP/1.1 404 Not Found");
                writer.WriteLine("Content-Type: text/html");
                writer.WriteLine("Connection: close");
                writer.WriteLine();
                writer.WriteLine("<h1>Error 404 - Archivo no encontrado</h1>");
            }
        }

        private static byte[] Compress(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(ms, CompressionMode.Compress))
                {
                    gzip.Write(data, 0, data.Length);
                }
                return ms.ToArray();
            }
        }

        private static string GetMimeType(string extension)
        {
            return extension switch
            {
                ".html" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".png" => "image/png",
                _ => "application/octet-stream",
            };
        }
    }
}
