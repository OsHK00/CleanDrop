using System;
using System.Collections.Generic;

namespace CleanDrop.Core.Models;

public class Rule
{

    public string Nombre { get; set; }         
    public List<string> Extensiones { get; set; }
    public string CarpetaDestino { get; set; }


    public Rule()
    {
        Extensiones = new List<string>();
    }
}

