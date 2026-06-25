const canvas = document.getElementById('canvas');
const ctx = canvas.getContext('2d');

const estados = automataData.estados;
const transiciones = automataData.transiciones;
const estadoInicial = automataData.estadoInicial;
const estadosFinales = new Set(automataData.estadosFinales);

let scale = 1;
let offsetX = 0;
let offsetY = 0;
let isDragging = false;
let lastX, lastY;

function zoomIn() { 
    scale *= 1.2; 
    dibujar(); 
}

function zoomOut() { 
    scale /= 1.2; 
    dibujar(); 
}

function resetZoom() { 
    scale = 1; 
    offsetX = 0; 
    offsetY = 0; 
    dibujar(); 
}

// Event listeners del canvas
canvas.addEventListener('mousedown', (e) => {
    isDragging = true;
    lastX = e.offsetX;
    lastY = e.offsetY;
});

canvas.addEventListener('mousemove', (e) => {
    if (isDragging) {
        offsetX += (e.offsetX - lastX) / scale;
        offsetY += (e.offsetY - lastY) / scale;
        lastX = e.offsetX;
        lastY = e.offsetY;
        dibujar();
    }
});

canvas.addEventListener('mouseup', () => { 
    isDragging = false; 
});

canvas.addEventListener('mouseleave', () => { 
    isDragging = false; 
});

canvas.addEventListener('wheel', (e) => {
    e.preventDefault();
    const zoom = e.deltaY < 0 ? 1.1 : 0.9;
    scale *= zoom;
    dibujar();
});

// Funciones de dibujo
function dibujar() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.save();
    ctx.translate(canvas.width / 2, canvas.height / 2);
    ctx.scale(scale, scale);
    ctx.translate(offsetX, offsetY);

    // Agrupar transiciones para manejar múltiples transiciones entre los mismos estados
    const transicionesAgrupadas = {};
    
    for (const [key, trans] of Object.entries(transiciones)) {
        const [origen, simbolo] = key.split('|');
        const destino = trans;
        const clavePar = origen < destino ? `${origen}-${destino}` : `${destino}-${origen}`;
        const direccion = origen < destino ? 'normal' : 'reversa';
        
        if (!transicionesAgrupadas[clavePar]) {
            transicionesAgrupadas[clavePar] = [];
        }
        
        transicionesAgrupadas[clavePar].push({
            origen: origen,
            destino: destino,
            simbolo: simbolo,
            direccion: direccion
        });
    }

    // Dibujar transiciones agrupadas
    for (const [clavePar, grupo] of Object.entries(transicionesAgrupadas)) {
        if (grupo.length === 1 && grupo[0].origen === grupo[0].destino) {
            // Auto-transición (bucle)
            const pos = estados[grupo[0].origen];
            dibujarAutoTransicion(pos.x, pos.y, grupo[0].simbolo);
        } else {
            // Transiciones entre diferentes estados
            dibujarGrupoTransiciones(grupo);
        }
    }

    // Dibujar estados
    for (const [nombre, pos] of Object.entries(estados)) {
        const esInicial = nombre === estadoInicial;
        const esFinal = estadosFinales.has(nombre);
        dibujarEstado(pos.x, pos.y, nombre, esInicial, esFinal);
    }

    ctx.restore();
}

function dibujarGrupoTransiciones(grupo) {
    if (grupo.length === 1) {
        // Una sola transición
        const t = grupo[0];
        const posOrigen = estados[t.origen];
        const posDestino = estados[t.destino];
        dibujarFlecha(posOrigen.x, posOrigen.y, posDestino.x, posDestino.y, t.simbolo);
    } else {
        // Múltiples transiciones - agrupar etiquetas por dirección
        const porDireccion = {};
        for (const t of grupo) {
            const clave = `${t.origen}-${t.destino}`;
            if (!porDireccion[clave]) {
                porDireccion[clave] = [];
            }
            porDireccion[clave].push(t.simbolo);
        }
        
        // Dibujar una flecha por dirección con múltiples etiquetas
        for (const [clave, simbolos] of Object.entries(porDireccion)) {
            const [origen, destino] = clave.split('-');
            const posOrigen = estados[origen];
            const posDestino = estados[destino];
            const etiquetaCombinada = simbolos.join(', ');
            dibujarFlecha(posOrigen.x, posOrigen.y, posDestino.x, posDestino.y, etiquetaCombinada);
        }
    }
}

function dibujarEstado(x, y, nombre, esInicial, esFinal) {
    const radio = 35;
    
    // Flecha de entrada para estado inicial
    if (esInicial) {
        ctx.strokeStyle = '#28a745';
        ctx.fillStyle = '#28a745';
        ctx.lineWidth = 3;
        ctx.beginPath();
        ctx.moveTo(x - 70, y);
        ctx.lineTo(x - radio - 5, y);
        ctx.stroke();
        // Punta de flecha
        ctx.beginPath();
        ctx.moveTo(x - radio - 5, y);
        ctx.lineTo(x - radio - 15, y - 5);
        ctx.lineTo(x - radio - 15, y + 5);
        ctx.closePath();
        ctx.fill();
    }

    // Círculo del estado
    ctx.strokeStyle = esInicial ? '#28a745' : esFinal ? '#dc3545' : '#333';
    ctx.fillStyle = esInicial ? '#d4edda' : esFinal ? '#f8d7da' : 'white';
    ctx.lineWidth = 3;
    ctx.beginPath();
    ctx.arc(x, y, radio, 0, 2 * Math.PI);
    ctx.fill();
    ctx.stroke();

    // Círculo doble para estados finales
    if (esFinal) {
        ctx.beginPath();
        ctx.arc(x, y, radio - 7, 0, 2 * Math.PI);
        ctx.stroke();
    }

    // Nombre del estado
    ctx.fillStyle = '#000';
    ctx.font = 'bold 16px Arial';
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText(nombre, x, y);
}

function dibujarFlecha(x1, y1, x2, y2, etiqueta) {
    const radio = 35;
    const distancia = Math.sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
    
    // Evitar dibujar si los estados están muy cerca
    if (distancia < radio * 2.5) return;
    
    const angulo = Math.atan2(y2 - y1, x2 - x1);
    const margen = 5; // Margen adicional para evitar cortes
    
    // Calcular puntos de inicio y fin considerando el radio y margen
    const xOrigen = x1 + Math.cos(angulo) * (radio + margen);
    const yOrigen = y1 + Math.sin(angulo) * (radio + margen);
    const xDestino = x2 - Math.cos(angulo) * (radio + margen);
    const yDestino = y2 - Math.sin(angulo) * (radio + margen);

    // Línea principal
    ctx.strokeStyle = '#333';
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.moveTo(xOrigen, yOrigen);
    ctx.lineTo(xDestino, yDestino);
    ctx.stroke();

    // Punta de flecha mejorada
    const anguloFlecha = 0.4;
    const longitudFlecha = 12;
    ctx.fillStyle = '#333';
    ctx.beginPath();
    ctx.moveTo(xDestino, yDestino);
    ctx.lineTo(
        xDestino - longitudFlecha * Math.cos(angulo - anguloFlecha),
        yDestino - longitudFlecha * Math.sin(angulo - anguloFlecha)
    );
    ctx.lineTo(
        xDestino - longitudFlecha * Math.cos(angulo + anguloFlecha),
        yDestino - longitudFlecha * Math.sin(angulo + anguloFlecha)
    );
    ctx.closePath();
    ctx.fill();

    // Etiqueta con fondo mejorado
    const xMedio = (xOrigen + xDestino) / 2;
    const yMedio = (yOrigen + yDestino) / 2;
    
    // Medir el texto para ajustar el fondo
    ctx.font = 'bold 14px Arial';
    const medidaTexto = ctx.measureText(etiqueta);
    const anchoFondo = Math.max(medidaTexto.width + 8, 20);
    const altoFondo = 20;
    
    // Fondo de la etiqueta
    ctx.fillStyle = 'white';
    ctx.strokeStyle = '#ddd';
    ctx.lineWidth = 1;
    ctx.fillRect(xMedio - anchoFondo/2, yMedio - altoFondo/2, anchoFondo, altoFondo);
    ctx.strokeRect(xMedio - anchoFondo/2, yMedio - altoFondo/2, anchoFondo, altoFondo);
    
    // Texto de la etiqueta
    ctx.fillStyle = '#667eea';
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText(etiqueta, xMedio, yMedio);
}

function dibujarAutoTransicion(x, y, etiqueta) {
    const radio = 35;
    const radioLoop = 35;
    const offsetY = 15; // Separación del estado
    
    // Centro del círculo del bucle (arriba del estado)
    const xCentro = x;
    const yCentro = y - radio - offsetY;
    
    // Ángulo donde el bucle toca el estado
    const anguloInicio = Math.PI * 0.7; // ~126 grados
    const anguloFin = Math.PI * 0.3;    // ~54 grados
    
    // Dibujar el arco del bucle (desde la izquierda hacia la derecha, pasando por arriba)
    ctx.strokeStyle = '#333';
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.arc(xCentro, yCentro, radioLoop, anguloInicio, anguloFin, false);
    ctx.stroke();
    
    // Calcular el punto final del arco para colocar la flecha
    const xFlechaBase = xCentro + radioLoop * Math.cos(anguloFin);
    const yFlechaBase = yCentro + radioLoop * Math.sin(anguloFin);
    
    // Calcular el ángulo tangente al círculo en ese punto
    const anguloTangente = anguloFin - Math.PI / 2;
    
    // Dibujar la punta de flecha
    const longitudFlecha = 12;
    ctx.fillStyle = '#333';
    ctx.beginPath();
    ctx.moveTo(xFlechaBase, yFlechaBase);
    ctx.lineTo(
        xFlechaBase - longitudFlecha * Math.cos(anguloTangente - 0.3),
        yFlechaBase - longitudFlecha * Math.sin(anguloTangente - 0.3)
    );
    ctx.lineTo(
        xFlechaBase - longitudFlecha * Math.cos(anguloTangente + 0.3),
        yFlechaBase - longitudFlecha * Math.sin(anguloTangente + 0.3)
    );
    ctx.closePath();
    ctx.fill();

    // Etiqueta del bucle (en la parte superior)
    const xEtiqueta = x;
    const yEtiqueta = yCentro - radioLoop - 10;
    
    // Medir el texto para ajustar el fondo
    ctx.font = 'bold 14px Arial';
    const medidaTexto = ctx.measureText(etiqueta);
    const anchoFondo = Math.max(medidaTexto.width + 8, 20);
    const altoFondo = 20;
    
    // Fondo de la etiqueta
    ctx.fillStyle = 'white';
    ctx.strokeStyle = '#ddd';
    ctx.lineWidth = 1;
    ctx.fillRect(xEtiqueta - anchoFondo/2, yEtiqueta - altoFondo/2, anchoFondo, altoFondo);
    ctx.strokeRect(xEtiqueta - anchoFondo/2, yEtiqueta - altoFondo/2, anchoFondo, altoFondo);
    
    // Texto de la etiqueta
    ctx.fillStyle = '#667eea';
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText(etiqueta, xEtiqueta, yEtiqueta);
}

// Iniciar el dibujo
dibujar();
