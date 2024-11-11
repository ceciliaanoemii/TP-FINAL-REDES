using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TP_REDES.Config;
using TP_REDES.Handlers;
using TP_REDES.Models;

namespace TP_REDES
{
    internal class Server
    {
        private static ServerConfig configuracion;

        static void Main(string[] args)
        {
            configuracion = ServerConfigLoader.Load();
            TcpListener server = new TcpListener(IPAddress.Any, configuracion.Puerto);
            server.Start();
            Console.WriteLine($"Servidor escuchando en el puerto {configuracion.Puerto}");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(RequestHandler.HandleRequest, client);
            }
        }
    }
}
