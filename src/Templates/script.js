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

    // Paso 1: combinar los simbolos de una misma transicion dirigida (origen -> destino)
    // Ej: "q2 -> q2 con 0, 1" son dos entradas en el JSON pero una sola flecha con etiqueta "0, 1"
    const dirigidas = {};
    for (const [key, destino] of Object.entries(transiciones)) {
        const [origen, simbolo] = key.split('|');
        const claveDirigida = `${origen}>>>${destino}`;
        if (!dirigidas[claveDirigida]) {
            dirigidas[claveDirigida] = { origen, destino, simbolos: [] };
        }
        dirigidas[claveDirigida].simbolos.push(simbolo);
    }

    // Paso 2: separar auto-transiciones (bucles) de transiciones entre estados distintos.
    // Los bucles se dibujan aparte para no depender de la distancia entre dos puntos iguales.
    const bucles = [];
    const paresDeEstados = {};
    for (const info of Object.values(dirigidas)) {
        const etiqueta = info.simbolos.join(', ');
        if (info.origen === info.destino) {
            bucles.push({ estado: info.origen, etiqueta });
        } else {
            const clavePar = info.origen < info.destino
                ? `${info.origen}-${info.destino}`
                : `${info.destino}-${info.origen}`;
            if (!paresDeEstados[clavePar]) {
                paresDeEstados[clavePar] = [];
            }
            paresDeEstados[clavePar].push({ origen: info.origen, destino: info.destino, etiqueta });
        }
    }

    // Paso 3: dibujar transiciones entre estados distintos.
    // Si hay ida y vuelta entre el mismo par, se curvan hacia lados opuestos para que no se tapen.
    for (const grupo of Object.values(paresDeEstados)) {
        if (grupo.length === 1) {
            const t = grupo[0];
            const posOrigen = estados[t.origen];
            const posDestino = estados[t.destino];
            dibujarFlecha(posOrigen.x, posOrigen.y, posDestino.x, posDestino.y, t.etiqueta);
        } else {
            // Offset perpendicular calculado UNA sola vez con una dirección de referencia fija
            // (la del primer elemento del grupo). Si se calculara por transición, la ida y la
            // vuelta tienen ángulos opuestos y el offset alternado se cancela, volviendo a
            // superponer ambas curvas en el mismo punto de control.
            const posRefOrigen = estados[grupo[0].origen];
            const posRefDestino = estados[grupo[0].destino];
            const anguloBase = Math.atan2(posRefDestino.y - posRefOrigen.y, posRefDestino.x - posRefOrigen.x);
            const anguloPerpendicular = anguloBase + Math.PI / 2;
            const offsetX = Math.cos(anguloPerpendicular) * 24;
            const offsetY = Math.sin(anguloPerpendicular) * 24;

            grupo.forEach((t, i) => {
                const posOrigen = estados[t.origen];
                const posDestino = estados[t.destino];
                const signo = i === 0 ? 1 : -1;
                dibujarFlechaCurva(posOrigen.x, posOrigen.y, posDestino.x, posDestino.y, t.etiqueta, offsetX * signo, offsetY * signo);
            });
        }
    }

    // Paso 4: dibujar los bucles (auto-transiciones), uno por estado, con todos sus simbolos combinados
    for (const bucle of bucles) {
        const pos = estados[bucle.estado];
        dibujarAutoTransicion(pos.x, pos.y, bucle.etiqueta);
    }

    // Dibujar estados
    for (const [nombre, pos] of Object.entries(estados)) {
        const esInicial = nombre === estadoInicial;
        const esFinal = estadosFinales.has(nombre);
        dibujarEstado(pos.x, pos.y, nombre, esInicial, esFinal);
    }

    ctx.restore();
}

function dibujarFlechaCurva(x1, y1, x2, y2, etiqueta, offsetX, offsetY) {
    const radio = 35;
    const distancia = Math.sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));

    // Evitar dibujar si los estados están muy cerca
    if (distancia < radio * 2.5) return;

    // El offset se recibe ya calculado por el llamador con una dirección de referencia fija
    // para todo el par de estados (ver dibujar()). No se recalcula acá a partir de x1,y1,x2,y2
    // porque esos varían según la dirección de CADA transición, y eso haría que ida y vuelta
    // terminen con el mismo punto de control (superpuestas otra vez).
    const xControl = (x1 + x2) / 2 + offsetX;
    const yControl = (y1 + y2) / 2 + offsetY;

    // Ángulos hacia/desde el punto de control, para que la línea nazca y termine
    // en el borde del círculo apuntando en la dirección real de la curva
    const anguloInicio = Math.atan2(yControl - y1, xControl - x1);
    const anguloFin = Math.atan2(y2 - yControl, x2 - xControl);
    const margen = 5;

    const xOrigen = x1 + Math.cos(anguloInicio) * (radio + margen);
    const yOrigen = y1 + Math.sin(anguloInicio) * (radio + margen);
    const xDestino = x2 - Math.cos(anguloFin) * (radio + margen);
    const yDestino = y2 - Math.sin(anguloFin) * (radio + margen);

    // Curva
    ctx.strokeStyle = '#333';
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.moveTo(xOrigen, yOrigen);
    ctx.quadraticCurveTo(xControl, yControl, xDestino, yDestino);
    ctx.stroke();

    // Punta de flecha, tangente a la curva en el punto final
    const anguloFlecha = 0.4;
    const longitudFlecha = 12;
    ctx.fillStyle = '#333';
    ctx.beginPath();
    ctx.moveTo(xDestino, yDestino);
    ctx.lineTo(
        xDestino - longitudFlecha * Math.cos(anguloFin - anguloFlecha),
        yDestino - longitudFlecha * Math.sin(anguloFin - anguloFlecha)
    );
    ctx.lineTo(
        xDestino - longitudFlecha * Math.cos(anguloFin + anguloFlecha),
        yDestino - longitudFlecha * Math.sin(anguloFin + anguloFlecha)
    );
    ctx.closePath();
    ctx.fill();

    // Etiqueta en el punto de control (donde la curva se separa de la línea recta)
    ctx.font = 'bold 14px Arial';
    const medidaTexto = ctx.measureText(etiqueta);
    const anchoFondo = Math.max(medidaTexto.width + 8, 20);
    const altoFondo = 20;

    ctx.fillStyle = 'white';
    ctx.strokeStyle = '#ddd';
    ctx.lineWidth = 1;
    ctx.fillRect(xControl - anchoFondo / 2, yControl - altoFondo / 2, anchoFondo, altoFondo);
    ctx.strokeRect(xControl - anchoFondo / 2, yControl - altoFondo / 2, anchoFondo, altoFondo);

    ctx.fillStyle = '#667eea';
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.fillText(etiqueta, xControl, yControl);
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
