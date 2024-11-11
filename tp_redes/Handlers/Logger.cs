using System;
using System.IO;

namespace TP_REDES.Handlers
{
    public static class Logger
    {
        // Define la ruta de la carpeta de logs
        private static string carpetaLog = "C:\\xampp\\htdocs\\servidor_redes\\servidor\\tp_redes\\logs";
        private static readonly object logLock = new object();

        public static void LogRequest(string metodo, string url, string ip)
        {
            string nombreArchivoLog = Path.Combine(carpetaLog, $"log_{DateTime.Now:yyyy-MM-dd}.txt");
            Directory.CreateDirectory(carpetaLog); // Asegura que la carpeta de logs exista

            lock (logLock)
            {
                using (StreamWriter archivoLog = new StreamWriter(nombreArchivoLog, true))
                {
                    string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - IP: {ip} - Método: {metodo} - URL: {url}";
                    Console.WriteLine($"Método: {metodo} - URL: {url}");
                    archivoLog.WriteLine(logEntry);
                }
            }
        }

        public static void LogRequestData(StreamReader reader)
        {
            string datos =reader.ReadLine();
            string nombreArchivoLog = Path.Combine(carpetaLog, $"log_{DateTime.Now:yyyy-MM-dd}.txt");
            Directory.CreateDirectory(carpetaLog);
            lock (logLock)
            {
                using (StreamWriter archivoLog= new StreamWriter(nombreArchivoLog, true))
                {
                    string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Datos POST: {datos}";
                    archivoLog.WriteLine(logEntry);
                }
            }
        }

        public static void LogQueryParameters(string url)
        {
            if (url.Contains("?"))
            {
                var uri = new Uri("http://localhost" + url);
                var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
                string nombreArchivoLog = Path.Combine(carpetaLog, $"log_{DateTime.Now:yyyy-MM-dd}.txt");
                Directory.CreateDirectory(carpetaLog);
                foreach (string clave in queryParams.Keys)
                {
                    lock (logLock)
                    {
                        using (StreamWriter archivoLog= new StreamWriter(nombreArchivoLog, true))
                        {
                            archivoLog.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Clave: {clave} - Valor: {queryParams[clave]}");
                        }
                    }
                }
            }
        }
    }
}
