@echo off
echo ========================================
echo   Compilador de Automatas - Build
echo ========================================
echo.

REM Crear carpetas necesarias
if not exist src (
    mkdir src
    echo Carpeta src creada
) else (
    echo Carpeta src ya existe
)

if not exist out (
    mkdir out
    echo Carpeta out creada
) else (
    echo Carpeta out ya existe
)
echo.

REM Limpiar archivos generados anteriormente
echo Eliminando archivos generados anteriormente...
if exist src\Scanner.cs (
    del src\Scanner.cs
    echo    src\Scanner.cs eliminado
)
if exist src\Parser.cs (
    del src\Parser.cs
    echo    src\Parser.cs eliminado
)
echo.

REM Verificar si existe Coco.exe
if not exist Coco.exe (
    echo ERROR: Coco.exe no encontrado en el directorio actual
    echo Por favor, descarga Coco/R desde http://www.ssw.uni-linz.ac.at/Coco/
    pause
    exit /b 1
)

REM Verificar si existe el archivo de gramática
if not exist grammar\Automata.atg (
    echo ERROR: grammar\Automata.atg no encontrado
    echo Por favor, asegurate de que el archivo de gramatica este en la carpeta grammar/
    pause
    exit /b 1
)

REM Paso 1: Generar Scanner y Parser
echo [1/3] Generando Scanner.cs y Parser.cs con Coco/R...
REM Copiar gramática temporalmente a la raíz para Coco
copy grammar\Automata.atg .\Automata_temp.atg
Coco.exe Automata_temp.atg

if errorlevel 1 (
    echo ERROR: Fallo al generar el scanner y parser
    pause
    exit /b 1
)

if not exist Scanner.cs (
    echo ERROR: Scanner.cs no fue generado
    pause
    exit /b 1
)

if not exist Parser.cs (
    echo ERROR: Parser.cs no fue generado
    pause
    exit /b 1
)

echo    OK - Scanner.cs y Parser.cs generados exitosamente
REM Limpiar archivo temporal
del Automata_temp.atg
echo.

REM Paso 2: Mover archivos generados a src/
echo [2/3] Moviendo archivos generados a src/...
move Scanner.cs src\
move Parser.cs src\
echo    Archivos movidos a src/
echo.

REM Paso 3: Compilar con csc.exe
echo [3/3] Compilando AutomataCompiler.exe...
csc /out:out\AutomataCompiler.exe Program.cs src\Scanner.cs src\Parser.cs src\AutomataBuilder.cs src\AutomataVisualizador.cs src\TablaSimbolos.cs

if errorlevel 1 (
    echo ERROR: Fallo la compilacion
    pause
    exit /b 1
)

goto :success

:success
echo.
echo ========================================
echo   COMPILACION EXITOSA DEL CAF!
echo ========================================
echo.
echo Presiona cualquier tecla para continuar...
echo Se ha generado: out\AutomataCompiler.exe
echo.
pause >nul
exit /b 0