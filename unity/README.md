# Piki Recovery · Fútbol — versión Unity (360° / VR)

Es la misma experiencia que la versión web (`index.html`), rehecha en C# para **Unity 6** (probada contra
la API de Unity 6.x; tu versión es 6000.4). Todo se genera por código al apretar **Play**: el estadio, el
vestuario, el holograma del cuerpo, los alimentos 3D, la interfaz y el sonido. No hace falta importar
modelos, imágenes ni audios.

## Instalación (2 minutos)

1. Descargá esta carpeta del repositorio (rama `claude/piki-recovery-vr-html-1flqhv`) o el `.zip` que te pasé.
2. Copiá la carpeta **`Assets/PikiRecovery`** dentro de la carpeta **`Assets`** de tu proyecto
   (`pikirecovery-unity 2/Assets/`). Podés arrastrarla desde el Finder a la ventana *Project* de Unity.
3. Esperá a que Unity compile (abajo a la derecha deja de girar el ícono).
4. Abrí cualquier escena (por ejemplo `SampleScene`) y apretá **▶ Play**.

Listo: la experiencia arranca sola. El script usa la cámara de la escena, apaga la luz que venga por defecto
y construye todo lo demás.

> Si querés arrancar directo en una etapa para probar: creá un GameObject vacío, agregale el componente
> **Piki Game** y elegí la etapa en **Start At** (Inicio, Deporte, Cancha, Física, Situación, Nutrición,
> Minijuego, Calma, Respiración, Final).

## Cómo se juega

| Dispositivo | Mirar en 360° | Elegir | Respiración (mantener al inhalar) |
|---|---|---|---|
| Mac / PC (editor o build) | Arrastrar con el mouse o flechas | Clic | Clic o **ESPACIO** |
| Celular (Android / iOS) | Mover el teléfono (giroscopio) o arrastrar | Tocar | Dedo apoyado |
| VR con controles (Meta Quest, OpenXR) | Girar la cabeza | Láser + **gatillo** | Gatillo |
| VR solo mirada | Girar la cabeza | Mirar fijo 1,4 s el botón | Modo guiado |

Atajos: **M** silencia el sonido y **R** recentra los paneles frente a la vista.

## Recorrido

1. **Inicio** y **¿Qué deporte practicás?**: Fútbol habilitado y el resto "Próximamente".
2. **Aparición en la cancha**: estadio al atardecer, tribunas llenas, silbato final, marcador *FINAL 2–1*,
   jugadores que se van al túnel y estadísticas del partido.
3. **Etapa 1, recuperación física**: holograma del cuerpo y 3 situaciones al azar (tipo, zona, lado,
   intensidad y señales). Elegís **Frío, Calor o Masajes**:
   - Si acertás, ves "Respuesta correcta" y se aplica el tratamiento, con partículas y reloj.
   - Si no, ves "No es la mejor opción…" con una pista, y volvés a intentar.
4. **Etapa 2, recuperación nutricional**: el vestuario, con un minijuego de 60 s en el que los alimentos 3D
   pasan a tu alrededor.
   - Barras de **Energía, Hidratación y Reparación**, con zona óptima entre 60 y 90 %.
   - Hay alimentos no prioritarios.
   - Tiene 3 niveles: más velocidad, más elementos y música más rápida.
5. **Etapa 3, vuelta a la calma**: estadio vacío de noche, con estrellas. La esfera luminosa marca
   **INHALÁ** (4 s) y **EXHALÁ** (6 s) durante 6 ciclos, y la frecuencia cardíaca baja.
6. **Final**: la cancha amanece en calma y aparece **RECUPERACIÓN COMPLETADA**, con el resumen de las 3 etapas.

## Publicar en cada plataforma (*File → Build Profiles*)

* **Mac / Windows:** funciona directo.
* **Android / iOS (360° con giroscopio):** cambiá la plataforma y hacé *Build*. Usá orientación horizontal
  (*Player → Resolution and Presentation → Landscape Left*).
* **Meta Quest / VR:**
  1. En *Edit → Project Settings → XR Plug-in Management* instalalo si te lo pide.
  2. Activá **OpenXR** en la pestaña Android y en la de PC. En *OpenXR → Features* activá
     *Meta Quest Support* y un perfil de controles (*Oculus Touch Controller Profile*).
  3. Dejá tildado *Initialize XR on Startup*.

  El juego detecta el visor solo: la cabeza mueve la cámara y los controles apuntan con láser.
* **WebGL (navegador):** funciona en 360° con mouse o toque, sin VR.

## Si algo se ve raro

* **Todo rosa o magenta:** el proyecto no es URP ni Built-in estándar. Los scripts usan
  `Universal Render Pipeline/Lit` (o `Standard`) y `Sprites/Default`.
* **En un build faltan partes, pero en el editor se ven bien:** agregá `Sprites/Default` y
  `Universal Render Pipeline/Lit` en *Project Settings → Graphics → Always Included Shaders*.
* **Aparece un error del Input System:** en *Project Settings → Player → Active Input Handling* elegí
  *Input System Package (New)* o *Both*. El código funciona con cualquiera de los dos sistemas.
* **La experiencia arranca sola en otras escenas del proyecto:** es a propósito (`PikiBoot`). Para
  desactivarlo, borrá el método `Boot` en `PikiGame.cs` y agregá el componente a mano.

## Archivos

| Archivo | Qué hace |
|---|---|
| `PikiGame.cs` | Flujo completo y todas las pantallas (arranque automático incluido) |
| `PikiRig.cs` | Cámara 360°, giroscopio, VR (cabeza y controles), puntero, mirada y "mantener presionado" |
| `PikiEnv.cs` | Cielo, luces, hub de inicio, estadio (tribunas, LED, arcos, torres, marcador, jugadores) y vestuario |
| `PikiModels.cs` | Holograma corporal, efectos de frío, calor y masaje, y los 22 alimentos 3D |
| `PikiContent.cs` | Zonas, situaciones al azar, tratamientos, pistas y alimentos con sus valores |
| `PikiUI.cs` | Paneles, textos, botones e íconos en 3D |
| `PikiAudio.cs` | Sonido sintetizado: público, silbato, música por nivel, ambiente de calma y respiración |
| `PikiCore.cs` | Materiales, texturas, íconos y mallas procedurales, más las animaciones |

---
*Contenido educativo. Ante un dolor intenso o persistente, consultá a un profesional de la salud.*
