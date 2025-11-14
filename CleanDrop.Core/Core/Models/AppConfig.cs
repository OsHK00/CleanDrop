using System;

namespace CleanDrop.Core.Models;

public class AppConfig
{
    public string CarpetaMonitoreada { get; set; } = string.Empty;
    public bool IniciarConWindows { get; set; }
    public bool ModoSilencioso { get; set; }
    public bool CrearSubcarpetasPorFecha { get; set; }

    public bool IniciarMonitoreoAutomatico { get; set; } = true;
    public bool PrimeraEjecucion { get; set; } = true;
}