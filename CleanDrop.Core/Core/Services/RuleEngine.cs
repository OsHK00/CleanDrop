using CleanDrop.Core.Models;  

namespace CleanDrop.Core.Services;

public class RuleEngine
{

    private readonly List<Rule> _reglas;


    public RuleEngine(List<Rule> reglas)
    {
        _reglas = reglas;
    }


    public Rule ObtenerReglaPara(string rutaArchivo)
    {
 
        string extension = Path.GetExtension(rutaArchivo).ToLower();
   

        foreach (var regla in _reglas)
        {

            if (regla.Extensiones.Contains(extension))
            {
                return regla; 
            }
        }


        return null;
    }

  
    public string ObtenerSubCarpeta(FileInfo archivo, Rule regla)
    {
        // Para despues
        return null;
    }
}

