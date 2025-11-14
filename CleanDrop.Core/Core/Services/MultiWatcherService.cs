using System;
using System.IO;
using CleanDrop.Core.Models;  

namespace CleanDrop.Core.Services;

public class MultiWatcherService : IDisposable
{
    private FileSystemWatcher _watcher;
    private readonly RuleEngine _ruleEngine;
    private FileOrganizer _organizer;
    private bool _estaActivo;

    public event EventHandler<ArchivoOrganizadoEventArgs> ArchivoOrganizado;
    public event EventHandler<ErrorEventArgs> ErrorOcurrido;

    public bool EstaActivo => _estaActivo;
    public int CarpetasActivas => _estaActivo ? 1 : 0; /

    public MultiWatcherService(RuleEngine ruleEngine)
    {
        _ruleEngine = ruleEngine;
    }

    
    /// Inicia el monitoreo de una carpeta
  
    public void Iniciar(string carpeta)
    {
        if (_estaActivo)
        {
            Detener();
        }

        try
        {
            _organizer = new FileOrganizer(carpeta, _ruleEngine);

            _watcher = new FileSystemWatcher(carpeta)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };

            _watcher.Created += OnArchivoCreado;
            _watcher.Error += OnError;

            _estaActivo = true;
        }
        catch (Exception ex)
        {
            ErrorOcurrido?.Invoke(this, new ErrorEventArgs(ex));
            throw;
        }
    }

    public void Detener()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Created -= OnArchivoCreado;
            _watcher.Error -= OnError;
            _watcher.Dispose();
            _watcher = null;
        }

        _estaActivo = false;
    }

    private void OnArchivoCreado(object sender, FileSystemEventArgs e)
    {
        try
        {
            // Organizar el archivo
            bool exito = _organizer.OrganizarArchivo(e.FullPath, pedirConfirmacion: false);

            if (exito)
            {
                
                var regla = _ruleEngine.ObtenerReglaPara(e.FullPath);
                string carpetaDestino = regla?.CarpetaDestino ?? "Desconocido";

               
                ArchivoOrganizado?.Invoke(this, new ArchivoOrganizadoEventArgs
                {
                    NombreArchivo = Path.GetFileName(e.FullPath),
                    RutaOrigen = e.FullPath,
                    CarpetaDestino = carpetaDestino
                });
            }
        }
        catch (Exception ex)
        {
            ErrorOcurrido?.Invoke(this, new ErrorEventArgs(ex));
        }
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        ErrorOcurrido?.Invoke(this, e);
    }

    public void Dispose()
    {
        Detener();
    }
}


public class ArchivoOrganizadoEventArgs : EventArgs
{
    public string NombreArchivo { get; set; }
    public string RutaOrigen { get; set; }
    public string CarpetaDestino { get; set; }
}