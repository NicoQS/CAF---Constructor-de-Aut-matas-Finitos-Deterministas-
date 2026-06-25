using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

public static class AutomataVisualizador
{
    private static string ObtenerRutaTemplates()
    {
        // Lista de rutas posibles para buscar la carpeta Templates
        var rutasPosibles = new List<string>();
        
        // 1. Desde el directorio de trabajo actual (cuando se ejecuta desde la raíz del proyecto)
        string directorioActual = Directory.GetCurrentDirectory();
        rutasPosibles.Add(Path.Combine(directorioActual, "src", "Templates"));
        
        // 2. Desde la ubicación del ejecutable
        string directorioBase = AppDomain.CurrentDomain.BaseDirectory;
        rutasPosibles.Add(Path.Combine(directorioBase, "src", "Templates"));
        
        // 3. Un nivel arriba del ejecutable (si está en out/)
        string directorioPadre = Directory.GetParent(directorioBase).FullName;
        rutasPosibles.Add(Path.Combine(directorioPadre, "src", "Templates"));
        
        // 4. Dos niveles arriba del ejecutable (por si está en out/Debug/ o similar)
        if (Directory.GetParent(directorioPadre) != null)
        {
            string directorioAbuelo = Directory.GetParent(directorioPadre).FullName;
            rutasPosibles.Add(Path.Combine(directorioAbuelo, "src", "Templates"));
        }
        
        // 5. Buscar Templates directamente en el mismo directorio del ejecutable
        rutasPosibles.Add(Path.Combine(directorioBase, "Templates"));
        
        // Buscar la primera ruta que existe
        foreach (var ruta in rutasPosibles)
        {
            if (Directory.Exists(ruta))
            {
                return ruta;
            }
        }
        
        // Si no se encuentra, mostrar todas las rutas intentadas
        var mensaje = new StringBuilder();
        mensaje.AppendLine("No se encontró la carpeta Templates. Rutas intentadas:");
        foreach (var ruta in rutasPosibles)
        {
            mensaje.AppendLine(string.Format("  - {0}", ruta));
        }
        
        throw new DirectoryNotFoundException(mensaje.ToString());
    }
    
    public static void GenerarVisualizacion(AFD automata)
    {
        // Crear la carpeta htmls_generados si no existe
        string carpetaSalida = "htmls_generados";
        if (!Directory.Exists(carpetaSalida))
        {
            Directory.CreateDirectory(carpetaSalida);
        }
        
        // Generar nombre del archivo con timestamp para evitar sobrescribir
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string nombreArchivo = string.Format("{0}_{1}.html", automata.Nombre, timestamp);
        string rutaCompleta = Path.Combine(carpetaSalida, nombreArchivo);
        
        var html = GenerarHTMLCompleto(automata);
        File.WriteAllText(rutaCompleta, html);
        
        // Guardar la ruta del último archivo generado para mostrarla después
        AutomataBuilder.UltimoArchivoHTML = rutaCompleta;
    }
    
    private static string GenerarHTMLCompleto(AFD automata)
    {
        string rutaTemplates = ObtenerRutaTemplates();
        
        // Leer plantillas desde archivos
        string plantillaHTML = File.ReadAllText(Path.Combine(rutaTemplates, "template.html"));
        string css = File.ReadAllText(Path.Combine(rutaTemplates, "styles.css"));
        string js = File.ReadAllText(Path.Combine(rutaTemplates, "script.js"));
        
        // Generar datos del autómata en formato JSON
        string automataJSON = GenerarAutomataJSON(automata);
        
        // Generar headers y filas de la tabla
        string tablaHeaders = GenerarTablaHeaders(automata);
        string tablaFilas = GenerarTablaFilas(automata);
        
        // Reemplazar placeholders con el contenido dinámico
        string html = plantillaHTML
            .Replace("{{TITULO}}", automata.Nombre)
            .Replace("{{CSS}}", css)
            .Replace("{{ESTADOS}}", string.Join(", ", automata.Estados))
            .Replace("{{ALFABETO}}", string.Join(", ", automata.Alfabeto))
            .Replace("{{ESTADO_INICIAL}}", automata.EstadoInicial)
            .Replace("{{ESTADOS_FINALES}}", string.Join(", ", automata.EstadosFinales))
            .Replace("{{TABLA_HEADERS}}", tablaHeaders)
            .Replace("{{TABLA_FILAS}}", tablaFilas)
            .Replace("{{AUTOMATA_JSON}}", automataJSON)
            .Replace("{{JS}}", js);
        
        return html;
    }
    
    private static string GenerarAutomataJSON(AFD automata)
    {
        var sb = new StringBuilder();
        sb.Append("{\n");
        
        // Estados con posiciones
        sb.Append("        \"estados\": ");
        sb.Append(GenerarEstadosJSON(automata));
        sb.Append(",\n");
        
        // Transiciones
        sb.Append("        \"transiciones\": ");
        sb.Append(GenerarTransicionesJSON(automata));
        sb.Append(",\n");
        
        // Estado inicial
        sb.AppendFormat("        \"estadoInicial\": \"{0}\",\n", automata.EstadoInicial);
        
        // Estados finales
        sb.Append("        \"estadosFinales\": [");
        sb.Append(string.Join(", ", automata.EstadosFinales.Select(e => string.Format("\"{0}\"", e))));
        sb.Append("]\n");
        
        sb.Append("    }");
        
        return sb.ToString();
    }
    
    private static string GenerarEstadosJSON(AFD automata)
    {
        var estadosList = automata.Estados.ToList();
        int numEstados = estadosList.Count;
        double angleStep = 2 * Math.PI / numEstados;
        int radioCirculo = Math.Min(400, 150 + numEstados * 20);
        
        var sb = new StringBuilder();
        sb.Append("{\n");
        
        for (int i = 0; i < numEstados; i++)
        {
            double angle = i * angleStep - Math.PI / 2;
            int x = (int)(Math.Cos(angle) * radioCirculo);
            int y = (int)(Math.Sin(angle) * radioCirculo);
            
            sb.AppendFormat("            \"{0}\": {{\"x\": {1}, \"y\": {2}}}", estadosList[i], x, y);
            if (i < numEstados - 1) sb.Append(",");
            sb.Append("\n");
        }
        
        sb.Append("        }");
        return sb.ToString();
    }
    
    private static string GenerarTransicionesJSON(AFD automata)
    {
        var sb = new StringBuilder();
        sb.Append("{\n");
        
        var transiciones = new List<string>();
        foreach (var origen in automata.Transiciones.Keys)
        {
            foreach (var simbolo in automata.Transiciones[origen].Keys)
            {
                var destino = automata.Transiciones[origen][simbolo];
                transiciones.Add(string.Format("            \"{0}|{1}\": \"{2}\"", origen, simbolo, destino));
            }
        }
        
        sb.Append(string.Join(",\n", transiciones));
        sb.Append("\n        }");
        
        return sb.ToString();
    }
    
    private static string GenerarTablaHeaders(AFD automata)
    {
        var alfabetoOrdenado = automata.Alfabeto.OrderBy(s => s).ToList();
        var headers = new StringBuilder();
        
        foreach (var simbolo in alfabetoOrdenado)
        {
            headers.AppendFormat("<th>{0}</th>", simbolo);
        }
        
        return headers.ToString();
    }
    
    private static string GenerarTablaFilas(AFD automata)
    {
        var alfabetoOrdenado = automata.Alfabeto.OrderBy(s => s).ToList();
        var estadosOrdenados = automata.Estados.OrderBy(e => e).ToList();
        var filas = new StringBuilder();
        
        foreach (var estado in estadosOrdenados)
        {
            var marcador = "";
            if (estado == automata.EstadoInicial) marcador = "→ ";
            if (automata.EstadosFinales.Contains(estado)) marcador += "((";
            
            if (estado == automata.EstadoInicial || automata.EstadosFinales.Contains(estado))
            {
                filas.AppendFormat("<tr><td><strong>{0}{1}{2}</strong></td>", 
                    marcador, 
                    estado, 
                    (automata.EstadosFinales.Contains(estado) && marcador.Contains("((") ? "))" : ""));
            }
            else
            {
                filas.AppendFormat("<tr><td>{0}</td>", estado);
            }
            
            foreach (var simbolo in alfabetoOrdenado)
            {
                if (automata.Transiciones.ContainsKey(estado) && automata.Transiciones[estado].ContainsKey(simbolo))
                {
                    filas.AppendFormat("<td>{0}</td>", automata.Transiciones[estado][simbolo]);
                }
                else
                {
                    filas.Append("<td>-</td>");
                }
            }
            filas.Append("</tr>");
        }
        
        return filas.ToString();
    }
}