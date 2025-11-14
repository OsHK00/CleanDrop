using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CleanDrop.Core.Models;        
using CleanDrop.Core.Services;     
using CleanDrop.Core.Repositories;



namespace CleanDrop.Core.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("╔═══════════════════════════════════════╗");
            Console.WriteLine("║  ORGANIZADOR DE ARCHIVOS v1.0         ║");
            Console.WriteLine("╚═══════════════════════════════════════╝\n");

            try
            {
                ConfigurationManager.Inicializar();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fatal: {ex.Message}");
                Console.WriteLine("\nPresiona cualquier tecla para salir...");
                Console.ReadKey();
                return;
            }


            var config = ConfigurationManager.CargarConfig();
            var ruleRepo = new RuleRepository(); // Usa ruta automática
            List<Rule> reglas = ruleRepo.ObtenerReglas();

            if (reglas.Count == 0)
            {
                Console.WriteLine("No hay reglas configuradas");
                Console.WriteLine("Presiona cualquier tecla para salir...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"{reglas.Count} reglas cargadas");
            Console.WriteLine($"Carpeta a monitorear: {config.CarpetaMonitoreada}\n");


            var ruleEngine = new RuleEngine(reglas);
            var organizer = new FileOrganizer(config.CarpetaMonitoreada, ruleEngine);

            if (args.Length > 0 && args[0] == "monitor")
            {
                IniciarMonitoreo(config.CarpetaMonitoreada, organizer);
            }
            else
            {
                MostrarMenu(config, organizer, ruleRepo);
            }
        }


        static void MostrarMenu(AppConfig config, FileOrganizer organizer, RuleRepository repo)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("╔═══════════════════════════════════════╗");
                Console.WriteLine("║  ORGANIZADOR DE ARCHIVOS v1.0         ║");
                Console.WriteLine("╚═══════════════════════════════════════╝");
                Console.WriteLine($"\nCarpeta: {config.CarpetaMonitoreada}");
                Console.WriteLine();
                Console.WriteLine("1. Organizar carpeta ahora");
                Console.WriteLine("2. Agregar nueva regla");
                Console.WriteLine("3. Ver reglas actuales");
                
                Console.WriteLine("4. Abrir carpeta de configuración");
                Console.WriteLine("5. Cambiar carpeta a monitorear");
                Console.WriteLine("6. Salir");
                Console.WriteLine("Puedes usar la bandeja de accesos directos en el menu de iconos ocultos");
                Console.WriteLine();
                Console.Write("Selecciona una opción: ");

                string opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "1":
                        OrganizarCarpetaCompleta(config.CarpetaMonitoreada, organizer);
                        break;
                    case "2":
                        AgregarRegla(repo);
                        break;
                    case "3":
                        MostrarReglas(repo);
                        break;
                    case "4":
                        ConfigurationManager.AbrirCarpetaConfig();
                        Console.WriteLine("✓ Carpeta abierta en el explorador");
                        break;
                    case "5":
                        CambiarCarpeta(config);
                        // Reinicia el organizer con la nueva carpeta
                        var reglas = repo.ObtenerReglas();
                        var engine = new RuleEngine(reglas);
                        organizer = new FileOrganizer(config.CarpetaMonitoreada, engine);
                        break;
                    case "6":
                        return;
                    default:
                        Console.WriteLine("Opción inválida");
                        break;
                }

                Console.WriteLine("\nPresiona cualquier tecla para continuar...");
                Console.ReadKey();
            }
        }

        static void CambiarCarpeta(AppConfig config)
        {
            Console.Clear();
            Console.WriteLine("CAMBIAR CARPETA A MONITOREAR\n");
            Console.WriteLine($"Carpeta actual: {config.CarpetaMonitoreada}\n");
            Console.Write("Nueva ruta (o ENTER para cancelar): ");

            string nuevaRuta = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(nuevaRuta))
            {
                Console.WriteLine("Operación cancelada");
                return;
            }

            if (!Directory.Exists(nuevaRuta))
            {
                Console.WriteLine("La carpeta no existe");
                return;
            }

            config.CarpetaMonitoreada = nuevaRuta;
            ConfigurationManager.GuardarConfig(config);

            Console.WriteLine($"Carpeta actualizada: {nuevaRuta}");
        }


        static void IniciarMonitoreo(string carpeta, FileOrganizer organizer)
        {
            var watcher = new FileSystemWatcher(carpeta);

            watcher.Created += (sender, e) =>
            {
                Console.WriteLine($"\nNuevo archivo detectado: {e.Name}");
                organizer.OrganizarArchivo(e.FullPath, pedirConfirmacion: false);
            };

            watcher.EnableRaisingEvents = true;

            Console.WriteLine($"Monitoreando: {carpeta}");
            Console.WriteLine("Presiona ENTER para detener...");
            Console.ReadLine();

            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }

        static void OrganizarCarpetaCompleta(string carpeta, FileOrganizer organizer)
        {
            Console.Clear();
            Console.WriteLine("Buscando archivos...\n");

            string[] archivos = Directory.GetFiles(carpeta);

            Console.WriteLine($"Se encontraron {archivos.Length} archivos.");
            Console.Write("¿Deseas organizarlos todos? (s/n): ");

            if (Console.ReadLine()?.ToLower() != "s")
            {
                Console.WriteLine("Operación cancelada");
                return;
            }

            Console.WriteLine("\nOrganizando...\n");

            int exitosos = 0;
            int fallidos = 0;

            foreach (string archivo in archivos)
            {
                bool exito = organizer.OrganizarArchivo(archivo, pedirConfirmacion: false);

                if (exito)
                    exitosos++;
                else
                    fallidos++;
            }

            Console.WriteLine($"\nOrganizados: {exitosos}");
            Console.WriteLine($"Fallidos: {fallidos}");
        }

        static void AgregarRegla(RuleRepository repo)
        {
            Console.Clear();
            Console.WriteLine("AGREGAR NUEVA REGLA\n");

            Console.Write("Nombre de la regla: ");
            string nombre = Console.ReadLine();

            Console.Write("Extensiones (separadas por comas, ej: .mp4,.avi): ");
            string[] extensionesArray = Console.ReadLine().Split(',');

            List<string> extensiones = new List<string>();
            foreach (string ext in extensionesArray)
            {
                string limpia = ext.Trim().ToLower();
                if (!limpia.StartsWith("."))
                    limpia = "." + limpia;
                extensiones.Add(limpia);
            }

            Console.Write("Carpeta destino: ");
            string carpeta = Console.ReadLine();

            var nuevaRegla = new Rule
            {
                Nombre = nombre,
                Extensiones = extensiones,
                CarpetaDestino = carpeta
            };

            repo.AgregarRegla(nuevaRegla);

            Console.WriteLine("\n Regla agregada exitosamente");
        }

        static void MostrarReglas(RuleRepository repo)
        {
            Console.Clear();
            Console.WriteLine(" REGLAS CONFIGURADAS\n");

            List<Rule> reglas = repo.ObtenerReglas();

            if (reglas.Count == 0)
            {
                Console.WriteLine(" No hay reglas configuradas");
                return;
            }

            foreach (var regla in reglas)
            {
                Console.WriteLine($"{regla.Nombre}");
                Console.WriteLine($"   Extensiones: {string.Join(", ", regla.Extensiones)}");
                Console.WriteLine($"   Destino: {regla.CarpetaDestino}");
                Console.WriteLine();
            }
        }
    }
}