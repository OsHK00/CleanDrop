using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CleanDrop.Core.Models;
using CleanDrop.Core.Services;

namespace CleanDrop.Core.Repositories;


public class RuleRepository
{
    private readonly string _rutaJson;

    // usa la ruta de ConfigurationManager por defecto
    public RuleRepository(string rutaJson = null)
    {
        // Si no se especifica ruta, usa la de configuración
        _rutaJson = rutaJson ?? ConfigurationManager.RulesPath;
    }
    


    public List<Rule> ObtenerReglas()
    {
        if (!File.Exists(_rutaJson))
            return new List<Rule>();

        string json = File.ReadAllText(_rutaJson);
        return JsonSerializer.Deserialize<List<Rule>>(json);
    }

    public void GuardarReglas(List<Rule> reglas)
    {
        var opciones = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(reglas, opciones);
        File.WriteAllText(_rutaJson, json);
    }

    public void AgregarRegla(Rule nuevaRegla)
    {
        var reglas = ObtenerReglas();
        reglas.Add(nuevaRegla);
        GuardarReglas(reglas);
    }
}