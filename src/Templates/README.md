# Templates de Visualización

Esta carpeta contiene los archivos de plantilla para generar la visualización HTML de los autómatas.

## Archivos

- **template.html**: Plantilla HTML principal con la estructura completa de la página
- **styles.css**: Todos los estilos CSS para la visualización
- **script.js**: Código JavaScript completo para el canvas interactivo

## Placeholders en template.html

Los siguientes placeholders serán reemplazados por contenido dinámico generado desde C#:

### Datos del autómata:
- `{{TITULO}}`: Nombre del autómata
- `{{ESTADOS}}`: Lista de estados separados por comas
- `{{ALFABETO}}`: Lista de símbolos del alfabeto
- `{{ESTADO_INICIAL}}`: Estado inicial del autómata
- `{{ESTADOS_FINALES}}`: Lista de estados finales

### Recursos:
- `{{CSS}}`: Contenido del archivo styles.css
- `{{JS}}`: Contenido del archivo script.js

### Tabla de transiciones:
- `{{TABLA_HEADERS}}`: Encabezados de columnas (símbolos del alfabeto)
- `{{TABLA_FILAS}}`: Filas de la tabla con las transiciones

### Datos JSON:
- `{{AUTOMATA_JSON}}`: Objeto JSON con toda la información del autómata para JavaScript
  ```json
  {
    "estados": { "q0": {"x": 0, "y": -200}, ... },
    "transiciones": { "q0|a": "q1", ... },
    "estadoInicial": "q0",
    "estadosFinales": ["qf"]
  }
  ```

## Personalización

### Estilos (styles.css)
Puedes modificar:
- Colores del tema (actualmente púrpura/violeta)
- Fuentes y tamaños de texto
- Diseño y espaciado
- Efectos visuales y sombras

### Lógica JavaScript (script.js)
Puedes modificar:
- Funciones de zoom y navegación
- Algoritmos de dibujo del canvas
- Posicionamiento de estados y transiciones
- Interacciones del mouse

### Estructura HTML (template.html)
Puedes modificar:
- Orden de las secciones
- Agregar nuevos elementos
- Cambiar la disposición de la información

## Ventajas de esta arquitectura

✅ **Separación de responsabilidades**: C# solo genera datos, no código de presentación
✅ **Fácil mantenimiento**: Edita HTML/CSS/JS sin recompilar
✅ **Colaboración**: Diseñadores pueden trabajar sin conocer C#
✅ **Reutilización**: Un solo conjunto de plantillas para todos los autómatas

