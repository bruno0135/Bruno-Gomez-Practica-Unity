# 🎮 Guía de Juego: Bruno Gomez Stirparo Practica

Esta guía detalla los controles y las mecánicas principales implementadas en el proyecto para disfrutar de la experiencia sonora.

## ⌨️ Controles (Teclado y Ratón)

| Acción | Tecla / Ratón |
| :--- | :--- |
| **Movimiento** | `W`, `A`, `S`, `D` |
| **Mirar / Cámara** | Ratón |
| **Saltar** | `Espacio` |
| **Correr** | `Shift Izquierdo` (Mantener) |
| **Interactuar (Puertas)** | `E` |

---

## 🎧 Experiencia Sonora y Mecánicas

El proyecto se centra en la inmersión a través del audio. Mientras exploras la escena, presta atención a las siguientes funciones:

### 1. Oclusión Física de Sonido
Si te sitúas detrás de una pared o una estructura mientras suena una alarma:
- El sonido se volverá **más apagado y grave** automáticamente.
- Esto simula cómo los objetos físicos bloquean las frecuencias altas, aumentando el realismo.

### 2. Puertas Automáticas e Interactivas
- **Interacción**: Acércate a una puerta y presiona `E` para abrirla o cerrarla.
- **Sincronización**: El sonido de la puerta está perfectamente sincronizado con su movimiento físico.
- **Cierre por proximidad**: Si dejas una puerta abierta y te alejas, se cerrará sola automáticamente.

### 3. Música Vertical Dinámica
La música del juego no es lineal:
- Notarás que la intensidad o las capas de la música cambian según tu posición o la zona en la que te encuentres.
- Las transiciones están diseñadas para ser fluidas y sin cortes.

### 4. Zonas de Ambiente (Interiores/Exteriores)
- Al entrar en diferentes salas o salir al exterior, la acústica general cambiará.
- El sistema detecta tu ubicación y ajusta los efectos de reverberación y el sonido ambiente global.

---

## 💡 Notas Importantes
- **Control de Cámara**: El cursor desaparece al iniciar para que puedas mover la cámara. Presiona `Esc` si necesitas recuperar el puntero del ratón.
- **Mezcla de Audio**: Todo el sonido está procesado a través de un **Audio Mixer**, lo que garantiza que los efectos, la música y el ambiente se escuchen en equilibrio profesional.
