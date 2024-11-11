using System;
using System.IO;

namespace TP_REDES.Handlers
{
    public static class Logger
    {
        // Define la ruta de la carpeta de logs
        private static string logDirectory = "C:\\xampp\\htdocs\\servidor_redes\\servidor\\tp_redes\\logs";


        public static void LogRequest(string method, string url, string ip)
        {
            string logFileName = Path.Combine(logDirectory, $"log_{DateTime.Now:yyyy-MM-dd}.txt");
            Directory.CreateDirectory(logDirectory); // Asegura que la carpeta de logs exista
            using (StreamWriter logFile = new StreamWriter(logFileName, true))
            {
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - IP: {ip} - Método: {method} - URL: {url}";
                logFile.WriteLine(logEntry);
            }
        }

        public static void LogRequestData(StreamReader reader)
        {
            string data = reader.ReadLine();
            string logFileName = Path.Combine(logDirectory, $"log_{DateTime.Now:yyyy-MM-dd}.txt");
            Directory.CreateDirectory(logDirectory);
            using (StreamWriter logFile = new StreamWriter(logFileName, true))
            {
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Datos POST: {data}";
                logFile.WriteLine(logEntry);
            }
        }

        public static void LogQueryParameters(string url)
        {
            if (url.Contains("?"))
            {
                var uri = new Uri("http://localhost" + url);
                var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
                string logFileName = Path.Combine(logDirectory, $"log_{DateTime.Now:yyyy-MM-dd}.txt");
                Directory.CreateDirectory(logDirectory);
                foreach (string key in queryParams.Keys)
                {
                    using (StreamWriter logFile = new StreamWriter(logFileName, true))
                    {
                        logFile.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Parámetro: {key} - Valor: {queryParams[key]}");
                    }
                }
            }
        }
    }
}
