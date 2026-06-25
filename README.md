# Compilador de Automatas Finitos Deterministas (AFD) con Coco/R

Este es un compilador completo que permite definir Automatas Finitos Deterministas usando un lenguaje de alto nivel facil de usar, y genera una visualizacion grafica interactiva en HTML.

## Requisitos Previos

- **Coco/R para C#**: Descarga de [http://www.ssw.uni-linz.ac.at/Coco/](http://www.ssw.uni-linz.ac.at/Coco/)
- **.NET Framework 4.5+** o **.NET Core 3.1+**
- Compilador de C# (csc.exe o dotnet)

## Estructura del Proyecto

```
/Proyecto Final/
│
├── Coco.exe                  (Generador de compiladores Coco/R)
├── Parser.frame              (Frame del parser - NO MODIFICAR)
├── Scanner.frame             (Frame del scanner - NO MODIFICAR)
├── Program.cs                (Programa principal)
├── build.bat                 (Script automatico de compilacion)
├── README.md                 (Este archivo)
│
├── /grammar/
│   └── Automata.atg          (Gramatica del lenguaje)
│
├── /src/
│   ├── AutomataBuilder.cs    (Constructor del automata)
│   ├── AutomataVisualizador.cs  (Generador de visualizacion HTML)
│   ├── TablaSimbolos.cs      (Tabla de simbolos)
│   ├── Scanner.cs            (Generado por Coco/R)
│   ├── Parser.cs             (Generado por Coco/R)
│   └── /Templates/
│       ├── template.html     (Plantilla HTML base)
│       ├── styles.css        (Estilos CSS)
│       ├── script.js         (JavaScript para interactividad)
│       └── README.md         (Documentacion de plantillas)
│
├── /ejemplos/
│   ├── ejemplo1.aut          (Cadenas con dos ceros consecutivos)
│   ├── ejemplo2.aut          (Numeros divisibles por 3)
│   ├── ejemplo3.aut          (Puerta automatica)
│   ├── ejemplo_errores.aut   (Ejemplo con errores)
│   └── ejemplo_incompleto.aut (Ejemplo incompleto)
│
└── /out/
    ├── AutomataCompiler.exe  (Ejecutable compilado)
    └── /htmls_generados/     (Archivos HTML de salida)
```

## Pasos para Compilar y Ejecutar

### Metodo 1: Usando el script automatico (Recomendado)

Ejecuta el script `build.bat` que automatiza todo el proceso:

```powershell
.\build.bat
```

Este script realiza automaticamente:
1. Crea las carpetas necesarias (`src/` y `out/`)
2. Elimina archivos generados anteriormente
3. Genera Scanner.cs y Parser.cs con Coco/R desde `grammar/Automata.atg`
4. Mueve los archivos generados a `src/`
5. Compila el proyecto completo con csc.exe
6. Genera el ejecutable en `out/AutomataCompiler.exe`

### Ejecutar el compilador

```powershell
# Forma 1: Especificando el archivo como argumento
.\out\AutomataCompiler.exe ejemplos\ejemplo1.aut

# Forma 2: Sin argumentos (el programa pedira la ruta)
.\out\AutomataCompiler.exe
```

### Ver el resultado

Despues de una compilacion exitosa, se generara un archivo HTML en la carpeta `out/htmls_generados/` con el formato `NombreAutomata_YYYYMMDD_HHMMSS.html`. Abrelo en cualquier navegador web para ver la visualizacion interactiva del automata.

Los archivos HTML se almacenan con timestamp para mantener un historial de las compilaciones y evitar sobrescribir versiones anteriores.

## Sintaxis del Lenguaje

### Estructura basica

```
AUTOMATA NombreDelAutomata

[ALFABETO: simbolo1, simbolo2, ...]

ESTADOS: estado1, estado2, estado3, ...

INICIAL: estadoInicial

[FINALES: estadoFinal1, estadoFinal2, ...]

TRANSICIONES:
    origen -> destino con 'simbolo'
    origen -> destino con 'a', 'b', 'c'
    ...

FIN
```

### Reglas del Lenguaje

1. **Palabras clave** (case-sensitive):
   - `AUTOMATA` - Declara el inicio del automata
   - `ALFABETO` - Define explicitamente el alfabeto
   - `ESTADOS` - Lista todos los estados
   - `INICIAL` - Define el estado inicial
   - `FINALES` - (Opcional) Lista los estados de aceptacion
   - `TRANSICIONES` - Define las transiciones del automata
   - `FIN` - Marca el final de la definicion

2. **Simbolos**:
   - Entre comillas simples: `'a'`, `'0'`, `'1'`
   - Entre comillas dobles: `"start"`, `"sensor"`
   - Pueden ser letras, numeros o palabras

3. **Transiciones**:
   - Formato: `origen -> destino con simbolo`
   - Alternativas: `→` en lugar de `->`
   - Alternativas: `mediante` o `usando` en lugar de `con`
   - Multiples simbolos: `q0 -> q1 con 'a', 'b', 'c'`

4. **Comentarios**:
   - Linea simple: `// comentario`
   - Multiples lineas: `/* comentario */`

## Ejemplos de Uso

### Ejemplo 1: Reconocedor de cadenas con dos ceros consecutivos

```
AUTOMATA DoscerosConsecutivos

ESTADOS: q0, q1, q2

INICIAL: q0

FINALES: q2

TRANSICIONES:
    q0 -> q1 con '0'
    q0 -> q0 con '1'
    q1 -> q2 con '0'
    q1 -> q0 con '1'
    q2 -> q2 con '0', '1'

FIN
```

**Acepta**: "00", "100", "0011", "10010"  
**Rechaza**: "0", "1", "01", "101"

### Ejemplo 2: Números binarios divisibles por 3

```
AUTOMATA DivisiblesPor3

ALFABETO: '0', '1'

ESTADOS: S0, S1, S2

INICIAL: S0

FINALES: S0

TRANSICIONES:
    S0 -> S0 con '0'
    S0 -> S1 con '1'
    S1 -> S2 con '0'
    S1 -> S0 con '1'
    S2 -> S1 con '0'
    S2 -> S2 con '1'

FIN
```

### Ejemplo 3: Puerta automatica (simbolos tipo cadena)

```
AUTOMATA PuertaAutomatica

ESTADOS: cerrada, abriendo, abierta, cerrando

INICIAL: cerrada

FINALES: cerrada

TRANSICIONES:
    cerrada  → abriendo con "sensor"
    abriendo → abierta  mediante "timeout"
    abierta  → cerrando usando "timeout"
    abierta  → abierta  con "sensor"
    cerrando → abriendo con "sensor"
    cerrando → cerrada  con "timeout"

FIN
```

## Visualizacion Grafica

El compilador genera un archivo HTML interactivo con:

- **Dibujo del automata**: Estados y transiciones visualizados graficamente
- **Codigo de colores**:
  - Verde: Estado inicial
  - Rojo (doble circulo): Estados finales
  - Negro: Estados normales
- **Tabla de transiciones**: Representacion tabular completa
- **Controles interactivos**:
  - Zoom in/out con botones o scroll del mouse
  - Arrastrar para mover el automata
  - Restablecer vista

## Solucion de Problemas

### Error: "Cannot open file..."
- Verifica que el archivo .aut existe
- Verifica la ruta del archivo

### Error en la compilacion de Coco/R
- Asegurate de tener los archivos .frame correctos (Parser.frame y Scanner.frame)
- Verifica que el archivo grammar/Automata.atg no tiene errores de sintaxis
- Asegurate de que Coco.exe esta en el directorio raiz del proyecto

### Error al compilar el codigo C#
- Verifica que todos los archivos .cs estan presentes en las carpetas correctas
- Asegurate de tener el .NET Framework/Core instalado
- Verifica que csc.exe esta disponible en tu PATH
- Ejecuta `build.bat` para una compilacion automatica

### El HTML no se genera correctamente
- Verifica que la carpeta `src/Templates/` contiene todos los archivos necesarios
- Asegurate de que la carpeta `out/htmls_generados/` existe
- Revisa los mensajes de error del compilador

## Notas Adicionales

- Los archivos .frame **NO deben modificarse** ya que son necesarios para que Coco.exe genere correctamente el compilador base
- El lenguaje es case-sensitive para las palabras clave
- Los nombres de estados e identificadores pueden usar letras, numeros y guiones bajos
- El alfabeto puede ser inferido automaticamente de las transiciones
- Los archivos generados (Scanner.cs y Parser.cs) se crean en `src/` y son sobrescritos en cada compilacion
- Los HTMLs generados se guardan en `out/htmls_generados/` con timestamp para evitar sobrescribir versiones anteriores

## Contribuciones

Este compilador fue diseñado para ser educativo y facil de usar. Se puede extender con nuevas caracteristicas como:
- Validacion de completitud del automata
- Minimizacion de automatas
- Conversion de AFND a AFD
- Simulacion interactiva con cadenas de entrada

## Licencia

Este proyecto utiliza Coco/R que esta bajo licencia GPL. El codigo generado puede usarse libremente segun los terminos de la excepcion de plugin de Coco/R.