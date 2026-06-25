using System;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  COMPILADOR DE AUTÓMATAS FINITOS DETERMINISTAS (AFD)         ║");
        Console.WriteLine("║  Versión 1.0 - Generado con Coco/R                           ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝\n");

        string archivoEntrada = null;

        if (args.Length > 0)
        {
            archivoEntrada = args[0];
        }
        else
        {
            archivoEntrada = MenuInteractivo();
        }

        while (true)
        {
            if (string.IsNullOrWhiteSpace(archivoEntrada))
            {
                Console.WriteLine("\n❌ Operación cancelada.");
                Console.WriteLine("\nPresione cualquier tecla para salir...");
                Console.ReadKey();
                return;
            }

            if (!File.Exists(archivoEntrada))
            {
                Console.WriteLine(string.Format("\n❌ Error: El archivo '{0}' no existe.", archivoEntrada));
                Console.WriteLine("\nPresione cualquier tecla para continuar...");
                Console.ReadKey();
                archivoEntrada = MenuInteractivo();
                continue;
            }

            bool continuar = CompilarArchivo(archivoEntrada);
            
            if (!continuar)
            {
                break;
            }
            
            archivoEntrada = MenuInteractivo();
        }

        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("¡Hasta luego!");
        Console.WriteLine("Presione cualquier tecla para salir...");
        Console.ReadKey();
    }

    static string MenuInteractivo()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  COMPILADOR DE AUTÓMATAS FINITOS DETERMINISTAS (AFD)          ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝\n");
            
            Console.WriteLine("MENÚ PRINCIPAL\n");
            Console.WriteLine("  [1] Seleccionar archivo desde carpeta 'ejemplos'");
            Console.WriteLine("  [2] Ingresar ruta manualmente");
            Console.WriteLine("  [3] Salir\n");
            Console.Write("Seleccione una opción: ");

            string opcion = Console.ReadLine();

            switch (opcion)
            {
                case "1":
                    string archivo = NavegarEjemplos();
                    if (archivo != null) return archivo;
                    break;
                case "2":
                    Console.Write("\nIngrese la ruta del archivo: ");
                    string ruta = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(ruta)) return ruta;
                    break;
                case "3":
                    return null;
                default:
                    Console.WriteLine("\n❌ Opción inválida. Presione cualquier tecla para continuar...");
                    Console.ReadKey();
                    break;
            }
        }
    }

    static string NavegarEjemplos()
    {
        // Buscar carpeta ejemplos en diferentes ubicaciones
        string carpetaEjemplos = Path.Combine(Directory.GetCurrentDirectory(), "ejemplos");
        
        // Si no existe en el directorio actual, buscar en el directorio del ejecutable
        if (!Directory.Exists(carpetaEjemplos))
        {
            string directorioEjecutable = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            carpetaEjemplos = Path.Combine(directorioEjecutable, "ejemplos");
        }
        
        // Si aún no existe, buscar un nivel arriba
        if (!Directory.Exists(carpetaEjemplos))
        {
            string directorioActual = Directory.GetCurrentDirectory();
            string directorioPadre = Directory.GetParent(directorioActual).FullName;
            carpetaEjemplos = Path.Combine(directorioPadre, "ejemplos");
        }

        if (!Directory.Exists(carpetaEjemplos))
        {
            Console.WriteLine("\n❌ La carpeta 'ejemplos' no existe.");
            Console.WriteLine("\nPresione cualquier tecla para volver...");
            Console.ReadKey();
            return null;
        }

        var archivos = Directory.GetFiles(carpetaEjemplos, "*.aut")
                                .Select(f => Path.GetFileName(f))
                                .OrderBy(f => f)
                                .ToArray();

        if (archivos.Length == 0)
        {
            Console.WriteLine("\n❌ No se encontraron archivos .aut en la carpeta 'ejemplos'.");
            Console.WriteLine("\nPresione cualquier tecla para volver...");
            Console.ReadKey();
            return null;
        }

        while (true)
        {
            Console.Clear();
            Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  SELECCIONAR ARCHIVO DE EJEMPLO                               ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝\n");
            
            Console.WriteLine(string.Format(" Carpeta: {0}\n", carpetaEjemplos));

            for (int i = 0; i < archivos.Length; i++)
            {
                Console.WriteLine(string.Format("  [{0}] {1}", i + 1, archivos[i]));
            }
            
            Console.WriteLine(string.Format("\n  [0] Volver al menú principal\n"));
            Console.Write("Seleccione un archivo: ");

            string seleccion = Console.ReadLine();

            if (seleccion == "0")
            {
                return null;
            }

            int indice;
            if (int.TryParse(seleccion, out indice) && indice >= 1 && indice <= archivos.Length)
            {
                string archivoSeleccionado = Path.Combine(carpetaEjemplos, archivos[indice - 1]);
                
                // Mostrar vista previa
                Console.WriteLine(string.Format("\n Archivo seleccionado: {0}\n", archivos[indice - 1]));
                Console.WriteLine("Vista previa:");
                Console.WriteLine(new string('-', 60));
                
                try
                {
                    string[] lineas = File.ReadAllLines(archivoSeleccionado);
                    int lineasAMostrar = Math.Min(15, lineas.Length);
                    for (int i = 0; i < lineasAMostrar; i++)
                    {
                        Console.WriteLine(lineas[i]);
                    }
                    if (lineas.Length > 15)
                    {
                        Console.WriteLine("...");
                    }
                }
                catch
                {
                    Console.WriteLine("(No se pudo cargar la vista previa)");
                }
                
                Console.WriteLine(new string('-', 60));
                Console.Write("\n¿Compilar este archivo? (S/N): ");
                string confirmar = Console.ReadLine();
                if (confirmar != null)
                {
                    confirmar = confirmar.ToUpper();
                }
                
                if (confirmar == "S" || confirmar == "SI")
                {
                    return archivoSeleccionado;
                }
            }
            else
            {
                Console.WriteLine("\n❌ Selección inválida. Presione cualquier tecla para continuar...");
                Console.ReadKey();
            }
        }
    }

    static bool CompilarArchivo(string archivoEntrada)
    {
        try
        {
            Console.Clear();
            Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║  COMPILANDO ARCHIVO                                           ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝\n");
            
            Console.WriteLine(string.Format("📄 Archivo: {0}", Path.GetFileName(archivoEntrada)));
            Console.WriteLine(string.Format("📍 Ruta: {0}", archivoEntrada));
            Console.WriteLine(new string('-', 60));

            Scanner scanner = new Scanner(archivoEntrada);
            Parser parser = new Parser(scanner);
            
            parser.Parse();

            Console.WriteLine(new string('-', 60));
            if (parser.errors.count == 0)
            {
                Console.WriteLine("\n✅ Compilación exitosa - Sin errores");
            }
            else
            {
                Console.WriteLine(string.Format("\n❌ Compilación terminada con {0} error(es)", parser.errors.count));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(string.Format("\n❌ Error fatal: {0}", ex.Message));
            Console.WriteLine(ex.StackTrace);
        }

        // Menú post-compilación
        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("\n¿Qué desea hacer?");
        Console.WriteLine("  [1] Compilar otro archivo");
        Console.WriteLine("  [2] Salir\n");
        Console.Write("Seleccione una opción: ");
        
        string opcion = Console.ReadLine();
        return opcion == "1";
    }
}