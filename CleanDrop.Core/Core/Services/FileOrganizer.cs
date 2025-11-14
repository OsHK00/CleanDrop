using System;
using System.IO;
using System.Threading;
using CleanDrop.Core.Models;  

namespace CleanDrop.Core.Services;

public class FileOrganizer
{

    private readonly string _carpetaBase;      // Carpeta de descargas
    private readonly RuleEngine _ruleEngine;   // Motor de reglas


    public FileOrganizer(string carpetaBase, RuleEngine ruleEngine)
    {
        _carpetaBase = carpetaBase;
        _ruleEngine = ruleEngine;
    }


    public bool OrganizarArchivo(string rutaArchivo, bool pedirConfirmacion = true)
    {
        try
        {

            EsperarDisponibilidad(rutaArchivo);


            Rule regla = _ruleEngine.ObtenerReglaPara(rutaArchivo);

            if (regla == null)
            {
                Console.WriteLine($"No hay regla para: {Path.GetFileName(rutaArchivo)}");
                return false;
            }

            if (pedirConfirmacion)
            {
                Console.Write($"¿Mover '{Path.GetFileName(rutaArchivo)}' a '{regla.CarpetaDestino}'? (s/n): ");
                string respuesta = Console.ReadLine()?.ToLower();

                if (respuesta != "s")
                {
                    Console.WriteLine("✗ Operación cancelada");
                    return false;
                }
            }

            string carpetaDestino = Path.Combine(_carpetaBase, regla.CarpetaDestino);
            Directory.CreateDirectory(carpetaDestino); 


            string nuevoPath = Path.Combine(carpetaDestino, Path.GetFileName(rutaArchivo));
            nuevoPath = ObtenerNombreUnico(nuevoPath);

            File.Move(rutaArchivo, nuevoPath);

            Console.WriteLine($"✓ Movido: {Path.GetFileName(rutaArchivo)} → {regla.CarpetaDestino}");
            return true;
        }
        catch (Exception ex)
        {

            Console.WriteLine($"✗ Error organizando '{Path.GetFileName(rutaArchivo)}': {ex.Message}");
            return false;
        }
    }


    private void EsperarDisponibilidad(string rutaArchivo)
    {

        while (true)
        {
            try
            {
  
                using (FileStream stream = File.Open(rutaArchivo, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return;
                }
            }
            catch (IOException)
            {
                
                Thread.Sleep(500);
            }
        }
    }


    private string ObtenerNombreUnico(string rutaArchivo)
    {
        if (!File.Exists(rutaArchivo))
            return rutaArchivo; 

  
        string directorio = Path.GetDirectoryName(rutaArchivo);
        string nombre = Path.GetFileNameWithoutExtension(rutaArchivo);
        string extension = Path.GetExtension(rutaArchivo);

        int contador = 1;
        string nuevaRuta;

   
        do
        {
            nuevaRuta = Path.Combine(directorio, $"{nombre} ({contador}){extension}");
            contador++;
        }
        while (File.Exists(nuevaRuta));

        return nuevaRuta;
    }
}

