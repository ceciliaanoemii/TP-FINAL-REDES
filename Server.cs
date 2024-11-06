using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;

namespace TP_REDES
{
    internal class Server
    {
        private static string rootDirectory = "C:\\Users\\Usuario\\source\\repos\\TP-REDES\\TP-REDES\\wwwroot\\"; // Carpeta por defecto para archivos.
        private static int port = 8080; // Puerto por defecto.
        private static string configFile = "C:\\Users\\Usuario\\source\\repos\\TP-REDES\\TP-REDES\\server.config.json"; // Archivo de configuración.

        static void Main(string[] args)
        {
            // Cargar la configuración desde archivo externo
            LoadConfiguration();

            // Crear el socket TCP para escuchar conexiones
            TcpListener server = new TcpListener(IPAddress.Any, port);
            server.Start();
            Console.WriteLine($"Servidor escuchando en el puerto {port}");

            while (true)
            {
                // Aceptar conexiones de clientes de manera concurrente
                TcpClient client = server.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(HandleRequest, client);
            }
        }

        private static ServerConfig LoadConfiguration()
        {
            string jsonContent = File.ReadAllText(configFile);
            ServerConfig config = JsonSerializer.Deserialize<ServerConfig>(jsonContent);

            if (config.Port == null)
            {
                config.Port = port;
            }
            if (config.Path == null || config.Path == "")
            {
                config.Path = rootDirectory;
            }

            return config;
        }

        private static void HandleRequest(object obj)
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

            try
            {
                // Leer la solicitud HTTP
                string requestLine = reader.ReadLine();
                if (string.IsNullOrEmpty(requestLine)) return;

                string[] tokens = requestLine.Split(' ');
                if (tokens.Length < 2) return;
                string method = tokens[0];
                string url = tokens[1];

                // Obtener la IP de origen del cliente
                string clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();

                Console.WriteLine($"Solicitud {method} {url} desde {clientIP}");

                // Llamar a LogRequest para registrar los detalles de la solicitud
                LogRequest(method, url, clientIP);

                // Loguear parámetros de consulta si existen
                LogQueryParameters(url);

                // Manejar solicitudes GET y POST
                if (method == "GET")
                {
                    ServeFile(writer, url);
                }
                else if (method == "POST")
                {
                    // Loguear datos del POST
                    LogRequestData(reader);
                    writer.WriteLine("HTTP/1.1 200 OK");
                    writer.WriteLine("Content-Type: text/html");
                    writer.WriteLine("Connection: close");
                    writer.WriteLine();
                    writer.WriteLine("<h1>Datos POST recibidos y logueados</h1>");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                stream.Close();
                client.Close();
            }
        }

        private static void ServeFile(StreamWriter writer, string url)
        {
            if (url == "/") url = "/index.html"; // Servir index.html por defecto

            string filePath = Path.Combine(rootDirectory, url.TrimStart('/'));
            if (File.Exists(filePath))
            {
                string fileExtension = Path.GetExtension(filePath);
                string mimeType = GetMimeType(fileExtension);

                writer.WriteLine("HTTP/1.1 200 OK");
                writer.WriteLine($"Content-Type: {mimeType}");
                writer.WriteLine("Content-Encoding: gzip"); // Agregar el encabezado de compresión
                writer.WriteLine("Connection: close");
                writer.WriteLine(); // Fin de headers

                // Enviar archivo comprimido
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);

                    byte[] compressedData = Compress(buffer);
                    writer.BaseStream.Write(compressedData, 0, compressedData.Length);
                }
            }
            else
            {
                // Archivo no encontrado, devolver 404
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

        private static void LogRequestData(StreamReader reader)
        {
            // Leer cuerpo del POST y loguear
            string data = reader.ReadLine();
            Console.WriteLine($"Datos POST recibidos: {data}");

            // Registrar en el archivo de log
            string logFileName = $"log_{DateTime.Now:yyyy-MM-dd}.txt";
            using (StreamWriter logFile = new StreamWriter(logFileName, true))
            {
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Datos POST: {data}";
                logFile.WriteLine(logEntry);
            }
        }

        private static void LogRequest(string method, string url, string ip)
        {
            // Crear un nombre de archivo basado en la fecha actual
            string logFileName = $"log_{DateTime.Now:yyyy-MM-dd}.txt";

            // Crear o abrir el archivo de log en modo de agregar
            using (StreamWriter logFile = new StreamWriter(logFileName, true))
            {
                // Formato de la entrada de log
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - IP: {ip} - Método: {method} - URL: {url}";

                // Escribir en el archivo de log
                logFile.WriteLine(logEntry);
            }
        }

        private static void LogQueryParameters(string url)
        {
            if (url.Contains("?"))
            {
                var uri = new Uri("http://localhost" + url); // Simulamos una URI completa para extraer los parámetros
                var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
                foreach (string key in queryParams.Keys)
                {
                    string logEntry = $"Parámetro: {key} - Valor: {queryParams[key]}";
                    Console.WriteLine(logEntry);
                    // Registrar en el archivo de log
                    string logFileName = $"log_{DateTime.Now:yyyy-MM-dd}.txt";
                    using (StreamWriter logFile = new StreamWriter(logFileName, true))
                    {
                        logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {logEntry}");
                    }
                }
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
