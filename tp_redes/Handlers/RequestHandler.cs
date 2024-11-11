using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using TP_REDES.Models;

namespace TP_REDES.Handlers
{
    public static class RequestHandler
    {
        public static void HandleRequest(object obj)
        {
            TcpClient cliente = (TcpClient)obj;
            NetworkStream stream = cliente.GetStream();
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

            try
            {
                string requestLine = reader.ReadLine();
                if (string.IsNullOrEmpty(requestLine)) return;

                string[] tokens = requestLine.Split(' ');
                if (tokens.Length < 2) return;
                string method = tokens[0];
                string url = tokens[1];

                string clienteIP = ((IPEndPoint)cliente.Client.RemoteEndPoint).Address.ToString();
                Logger.LogRequest(method, url, clienteIP);
                Logger.LogQueryParameters(url);

                if (method == "GET")
                {
                    FileHandler.ServeFile(writer, url);
                }
                else if (method == "POST")
                {
                    Logger.LogRequestData(reader);
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
                cliente.Close();
            }
        }
    }
}
