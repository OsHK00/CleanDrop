using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Diagnostics;
using CleanDrop.Core.Models;  

namespace CleanDrop.Core.Services;

public class ConfigurationManager
{

    // rutas de win

    private static readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private static readonly string AppName = "FileOrganizer";

    // Carpeta principal: C:\Users\Usuario\AppData\Local\FileOrganizer
    public static readonly string AppFolder = Path.Combine(AppDataPath, AppName);

    // Archivos de configuración
    public static readonly string RulesPath = Path.Combine(AppFolder, "rules.json");
    public static readonly string ConfigPath = Path.Combine(AppFolder, "config.json");
    public static readonly string LogsFolder = Path.Combine(AppFolder, "logs");


    public static void Inicializar()
    {
        try
        {
            // 1. Crear carpeta principal
            if (!Directory.Exists(AppFolder))
            {
                Directory.CreateDirectory(AppFolder);
                Console.WriteLine($"Carpeta de configuración creada: {AppFolder}");
            }

            // 2. Crear carpeta de logs
            if (!Directory.Exists(LogsFolder))
            {
                Directory.CreateDirectory(LogsFolder);
            }

            // 3. Crear rules.json si no existe
            if (!File.Exists(RulesPath))
            {
                CrearReglasDefault();
                Console.WriteLine($"Archivo de reglas creado: {RulesPath}");
            }

            // 4. Crear config.json si no existe
            if (!File.Exists(ConfigPath))
            {
                CrearConfigDefault();
                Console.WriteLine($"Archivo de configuración creado: {ConfigPath}");
            }

            Console.WriteLine("Configuración inicializada correctamente\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inicializando configuración: {ex.Message}");
            throw;
        }
    }


    private static void CrearReglasDefault()
    {
        var reglasDefault = new List<Rule>
        {
            new Rule
            {
                Nombre = "Imágenes",
                Extensiones = new List<string> { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg", ".webp" },
                CarpetaDestino = "Imagenes"
            },
            new Rule
            {
                Nombre = "Documentos",
                Extensiones = new List<string> { ".pdf", ".docx", ".doc", ".txt", ".xlsx", ".xls", ".pptx", ".ppt" },
                CarpetaDestino = "Documentos"
            },
            new Rule
            {
                Nombre = "Videos",
                Extensiones = new List<string> { ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".webm" },
                CarpetaDestino = "Videos"
            },
            new Rule
            {
                Nombre = "Música",
                Extensiones = new List<string> { ".mp3", ".wav", ".flac", ".m4a", ".aac", ".ogg" },
                CarpetaDestino = "Musica"
            },
            new Rule
            {
                Nombre = "Comprimidos",
                Extensiones = new List<string> { ".zip", ".rar", ".7z", ".tar", ".gz" },
                CarpetaDestino = "Comprimidos"
            },
            new Rule
            {
                Nombre = "Instaladores",
                Extensiones = new List<string> { ".exe", ".msi", ".dmg", ".pkg" },
                CarpetaDestino = "Instaladores"
            },
            new Rule
            {
                Nombre = "Código",
                Extensiones = new List<string> { ".cs", ".java", ".py", ".js", ".html", ".css", ".cpp", ".c", ".h" },
                CarpetaDestino = "Codigo"
            }
        };


        var opciones = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(reglasDefault, opciones);
        File.WriteAllText(RulesPath, json);
    }


    private static void CrearConfigDefault()
    {
        // Detecta la carpeta de descargas del usuario automáticamente
        string carpetaDescargas = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads"
        );

        var configDefault = new AppConfig
        {
            CarpetaMonitoreada = carpetaDescargas,
            IniciarConWindows = false,
            ModoSilencioso = false,
            CrearSubcarpetasPorFecha = false
        };

        var opciones = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(configDefault, opciones);
        File.WriteAllText(ConfigPath, json);
    }


    public static AppConfig CargarConfig()
    {
        if (!File.Exists(ConfigPath))
        {
            CrearConfigDefault();
        }

        string json = File.ReadAllText(ConfigPath);
        return JsonSerializer.Deserialize<AppConfig>(json);
    }


    public static void GuardarConfig(AppConfig config)
    {
        var opciones = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(config, opciones);
        File.WriteAllText(ConfigPath, json);
    }

    public static void AbrirCarpetaConfig()
    {
        if (Directory.Exists(AppFolder))
        {
            System.Diagnostics.Process.Start("explorer.exe", AppFolder);
        }
    }
}