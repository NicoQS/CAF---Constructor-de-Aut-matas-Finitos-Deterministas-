using System;
using System.Collections.Generic;
using System.Linq;

// Tabla de símbolos para validaciones semánticas
public class TablaSimbolos
{
    private HashSet<string> estados;
    private HashSet<string> simbolos;
    private Dictionary<string, string> transiciones; // Clave: "origen-simbolo", Valor: destino

    public TablaSimbolos()
    {
        estados = new HashSet<string>();
        simbolos = new HashSet<string>();
        transiciones = new Dictionary<string, string>();
    }

    public void AgregarEstado(string estado)
    {
        estados.Add(estado);
    }

    public void AgregarSimbolo(string simbolo)
    {
        simbolos.Add(simbolo);
    }

    public void AgregarTransicion(string clave, string destino)
    {
        transiciones[clave] = destino;
    }

    public bool EstadoExiste(string estado)
    {
        return estados.Contains(estado);
    }

    public bool SimboloExiste(string simbolo)
    {
        return simbolos.Contains(simbolo);
    }

    public bool TransicionExiste(string clave)
    {
        return transiciones.ContainsKey(clave);
    }

    public int NumeroEstados()
    {
        return estados.Count;
    }

    public int NumeroSimbolos()
    {
        return simbolos.Count;
    }

    public int NumeroTransiciones()
    {
        return transiciones.Count;
    }

    public void MostrarTablaSimbolos()
    {
        Console.WriteLine("\n" + new string('-', 50));
        Console.WriteLine("TABLA DE SÍMBOLOS");
        Console.WriteLine(new string('-', 50));
        
        Console.WriteLine(string.Format("Estados declarados ({0}): {{ {1} }}", estados.Count, string.Join(", ", estados.OrderBy(e => e))));
        Console.WriteLine(string.Format("Símbolos del alfabeto ({0}): {{ {1} }}", simbolos.Count, string.Join(", ", simbolos.OrderBy(s => s))));
        Console.WriteLine(string.Format("Transiciones definidas ({0}):", transiciones.Count));
        
        foreach (var kvp in transiciones.OrderBy(t => t.Key))
        {
            string[] partes = kvp.Key.Split('-');
            if (partes.Length >= 2)
            {
                string origen = partes[0];
                string simbolo = "";
                for (int i = 1; i < partes.Length; i++)
                {
                    if (i > 1) simbolo += "-";
                    simbolo += partes[i];
                }
                Console.WriteLine(string.Format("  {0} --{1}--> {2}", origen, simbolo, kvp.Value));
            }
        }
        
        Console.WriteLine(new string('-', 50));
    }
}