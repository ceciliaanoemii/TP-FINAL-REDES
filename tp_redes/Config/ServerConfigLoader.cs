using System.IO;
using System.Text.Json;
using TP_REDES.Models;

namespace TP_REDES.Config
{
    public static class ServerConfigLoader
    {
        private static string archivoDeConfiguracion = "C:\\xampp\\htdocs\\servidor_redes\\servidor\\tp_redes\\server.config.json";

        public static ServerConfig Load()
        {
            string json = File.ReadAllText(archivoDeConfiguracion);
            ServerConfig configuracion = JsonSerializer.Deserialize<ServerConfig>(json);

            if (configuracion.Puerto == null)
                configuracion.Puerto = 8080;
            if (configuracion.Ruta == null || configuracion.Ruta == "")
                configuracion.Ruta = "C:\\xampp\\htdocs\\servidor_redes\\servidor\\tp_redes\\web\\";
            return configuracion;
        }
    }
}
