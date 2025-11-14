using System;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using CleanDrop.Core.Services;


namespace TrayApp
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Prevenir múltiples instancias
            bool createdNew;
            using (var mutex = new System.Threading.Mutex(true, "CleanDropTrayApp", out createdNew))
            {
                if (!createdNew)
                {
                    MessageBox.Show(
                        "CleanDrop ya está ejecutándose.\nBusca el icono en la bandeja del sistema.",
                        "CleanDrop",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Inicializar configuración
                try
                {
                    ConfigurationManager.Inicializar();  
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error al inicializar la configuración:\n{ex.Message}",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                // Verificar si es primera ejecución
                var config = ConfigurationManager.CargarConfig(); 

                if (config.PrimeraEjecucion)
                {
                    var resultado = MessageBox.Show(
                        "¡Bienvenido a CleanDrop!\n\n" +
                        "¿Quieres configurar la aplicación ahora?\n\n" +
                        "- SÍ: Abrir panel de control\n" +
                        "- NO: Usar configuración por defecto",
                        "Primera ejecución",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );

                    if (resultado == DialogResult.Yes)
                    {
                        AbrirConsola();
                    }

                    config.PrimeraEjecucion = false;
                    ConfigurationManager.GuardarConfig(config);  
                }

                var trayContext = new TrayApplicationContext();
                Application.Run(trayContext);

                GC.KeepAlive(mutex);
            }
        }

        /// Abre el panel de control (consola)

        private static void AbrirConsola()
        {
            try
            {
                // Nombre correcto del ejecutable
                string consoleExeName = "CleanDrop.Console.exe";

                // Buscar en la misma carpeta que TrayApp
                string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, consoleExeName);

                if (File.Exists(exePath))
                {
                    Process.Start(exePath);
                    return;
                }

                var archivosDisponibles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.exe");

                MessageBox.Show(
                    $"No se encontró el panel de control.\n\n" +
                    $"Archivo esperado: {consoleExeName}\n" +
                    $"Ubicación: {AppDomain.CurrentDomain.BaseDirectory}\n\n" +
                    "Archivos .exe disponibles:\n" +
                    string.Join("\n", Array.ConvertAll(archivosDisponibles, Path.GetFileName)),
                    "Panel de control no encontrado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al abrir el panel de control:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}