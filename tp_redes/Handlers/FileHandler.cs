using System;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using TP_REDES.Config;

namespace TP_REDES.Handlers
{
    public static class FileHandler
    {
        private static string ruta = ServerConfigLoader.Load().Ruta;

        public static void ServeFile(StreamWriter writer, string url)
        {
            if (url == "/") url = "/index.html";

            string archivoRuta = Path.Combine(ruta, url.TrimStart('/'));
            archivoRuta = archivoRuta.Split('?')[0];
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
                    byte[] datosComprimidos = Comprimir(buffer);
                    writer.BaseStream.Write(datosComprimidos, 0, datosComprimidos.Length);
                }
            }
            else
            {
                writer.WriteLine("HTTP/1.1 404 Not Found");
                writer.WriteLine("Content-Type: text/html");
                writer.WriteLine("Content-Encoding: gzip");
                writer.WriteLine("Connection: close");
                writer.WriteLine();

                using (FileStream fs = new FileStream("C:\\xampp\\htdocs\\servidor_redes\\servidor\\tp_redes\\web\\error.html", FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    byte[] datosComprimidos = Comprimir(buffer);
                    writer.BaseStream.Write(datosComprimidos, 0, datosComprimidos.Length);
                }
            }
        }

        private static byte[] Comprimir(byte[] datos)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(ms, CompressionMode.Compress))
                {
                    gzip.Write(datos, 0, datos.Length);
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
