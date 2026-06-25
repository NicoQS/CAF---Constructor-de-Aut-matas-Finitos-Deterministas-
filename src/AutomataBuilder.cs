using System;
using System.Collections.Generic;
using System.Linq;

public class AutomataBuilder
{
    private AFD automataActual;
    private bool tieneTransiciones = false;

    public static string UltimoArchivoHTML { get; set; }

    public void IniciarAutomata(string nombre)
    {
        automataActual = new AFD(nombre);
        tieneTransiciones = false;
        Console.WriteLine(string.Format("Iniciando construcción del autómata: {0}", nombre));
    }

    public void AgregarSimbolo(string simbolo)
    {
        if (automataActual != null)
        {
            automataActual.AgregarSimbolo(simbolo);
        }
    }

    public void AgregarEstado(string nombreEstado)
    {
        if (automataActual != null)
        {
            automataActual.AgregarEstado(nombreEstado);
        }
    }

    public void DefinirEstadoInicial(string nombreEstado)
    {
        if (automataActual != null)
        {
            automataActual.DefinirEstadoInicial(nombreEstado);
        }
    }

    public void MarcarEstadoFinal(string nombreEstado)
    {
        if (automataActual != null)
        {
            automataActual.MarcarEstadoFinal(nombreEstado);
        }
    }

    public void AgregarTransicion(string estadoOrigen, string estadoDestino, string simbolo)
    {
        if (automataActual != null)
        {
            automataActual.AgregarTransicion(estadoOrigen, estadoDestino, simbolo);
            tieneTransiciones = true;
        }
    }

    public bool TieneTransiciones()
    {
        return tieneTransiciones;
    }

    public void FinalizarConValidacion()
    {
        if (automataActual != null)
        {
            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("AUTÓMATA FINITO DETERMINISTA CONSTRUIDO EXITOSAMENTE");
            Console.WriteLine(new string('=', 60));

            automataActual.MostrarInformacion();
            automataActual.GenerarVisualizacion();

            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("Se ha generado el archivo: " + UltimoArchivoHTML);
            Console.WriteLine("Ábrelo en tu navegador para ver el autómata gráficamente");
            Console.WriteLine(new string('=', 60));
        }
    }

    public void FinalizarSinVisualizacion()
    {
        // No genera HTML ni muestra información cuando hay errores
    }

    public AFD ObtenerAutomata()
    {
        return automataActual;
    }
}

public class AFD
{
    public string Nombre { get; private set; }
    public HashSet<string> Estados { get; private set; }
    public HashSet<string> Alfabeto { get; private set; }
    public string EstadoInicial { get; private set; }
    public HashSet<string> EstadosFinales { get; private set; }
    public Dictionary<string, Dictionary<string, string>> Transiciones { get; private set; }

    public AFD(string nombre)
    {
        Nombre = nombre;
        Estados = new HashSet<string>();
        Alfabeto = new HashSet<string>();
        EstadosFinales = new HashSet<string>();
        Transiciones = new Dictionary<string, Dictionary<string, string>>();
        EstadoInicial = null;
    }

    public void AgregarSimbolo(string simbolo)
    {
        Alfabeto.Add(simbolo);
    }

    public void AgregarEstado(string estado)
    {
        if (!Estados.Contains(estado))
        {
            Estados.Add(estado);
            Console.WriteLine(string.Format("  Estado agregado: {0}", estado));
        }
    }

    public void DefinirEstadoInicial(string estado)
    {
        if (!Estados.Contains(estado))
        {
            AgregarEstado(estado);
        }
        EstadoInicial = estado;
        Console.WriteLine(string.Format("  Estado inicial definido: {0}", estado));
    }

    public void MarcarEstadoFinal(string estado)
    {
        if (Estados.Contains(estado))
        {
            EstadosFinales.Add(estado);
            Console.WriteLine(string.Format("  Estado final marcado: {0}", estado));
        }
    }

    public void AgregarTransicion(string origen, string destino, string simbolo)
    {
        // Asegurar que los estados existen
        if (!Estados.Contains(origen))
        {
            AgregarEstado(origen);
        }
        if (!Estados.Contains(destino))
        {
            AgregarEstado(destino);
        }

        // Agregar símbolo al alfabeto
        if (!Alfabeto.Contains(simbolo))
        {
            Alfabeto.Add(simbolo);
        }

        // Agregar transición
        if (!Transiciones.ContainsKey(origen))
        {
            Transiciones[origen] = new Dictionary<string, string>();
        }

        Transiciones[origen][simbolo] = destino;
        Console.WriteLine(string.Format("  Transición agregada: {0} --{1}--> {2}", origen, simbolo, destino));
    }

    public void MostrarInformacion()
    {
        Console.WriteLine(string.Format("\nNombre: {0}", Nombre));
        Console.WriteLine(string.Format("\nEstados: {{ {0} }}", string.Join(", ", Estados)));
        Console.WriteLine(string.Format("Alfabeto: {{ {0} }}", string.Join(", ", Alfabeto)));
        Console.WriteLine(string.Format("Estado Inicial: {0}", EstadoInicial));
        Console.WriteLine(string.Format("Estados Finales: {{ {0} }}", string.Join(", ", EstadosFinales)));

        Console.WriteLine("\nTabla de Transiciones:");
        Console.WriteLine("Estado\t| " + string.Join("\t| ", Alfabeto));
        Console.WriteLine(new string('-', 50));

        foreach (var estado in Estados.OrderBy(e => e))
        {
            Console.Write(estado + "\t| ");
            foreach (var simbolo in Alfabeto)
            {
                if (Transiciones.ContainsKey(estado) && Transiciones[estado].ContainsKey(simbolo))
                {
                    Console.Write(Transiciones[estado][simbolo] + "\t| ");
                }
                else
                {
                    Console.Write("-\t| ");
                }
            }
            Console.WriteLine();
        }
    }

    public void GenerarVisualizacion()
    {
        AutomataVisualizador.GenerarVisualizacion(this);
    }
}