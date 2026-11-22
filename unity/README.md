# Piki Game · Fútbol — versión Unity (360° / VR)

**Piki** (antes *PikiRecovery*) · *Recuperar también es entrenar.*

Es la misma experiencia que la versión web (`index.html`), rehecha en C# para **Unity 6** (compilada contra la
API de Unity; tu versión es 6000.4). El proyecto trae **5 escenas ya armadas** (estadio, vestuario, etc.) que
podés ver y editar en el editor. Un constructor genera las escenas y guarda todo como assets.

La versión 14 aplica el **Manual de marca de Piki**: nombre *Piki Game*, isologo e isotipo, tipografía
**Poppins** (titulares en ExtraBold cursiva), paneles en **petróleo** y acentos en **lima**. También suma un menú de
**Accesibilidad** (ver más abajo).

## Instalación y escenas

1. Copiá la carpeta **`Assets/PikiGame`** dentro de la carpeta **`Assets`** de tu proyecto.
   Si tenías una versión anterior (**`Assets/PikiRecovery`**), borrala antes.
2. Cuando Unity termine de compilar aparece la ventana **"¿Construir ahora las 5 escenas?"**: tocá
   **Construir**. También podés hacerlo desde el menú **Piki Game ▸ Construir escenas**.
3. Se crean 5 escenas y 4 fundidos en `Assets/PikiGame/Scenes`, con todo ya armado y visible en el editor:

| Escena | Qué tiene |
|---|---|
| `00_Inicio` | Hub de inicio (grilla, pelota, partículas), panel de bienvenida y elección de deporte |
| `01_Cancha` | Estadio completo: césped con líneas, tribunas con público, techo, torres de luz, carteles LED, arcos con red, banderines, marcador, bancos, túnel, jugadores, pelota y botellas. Etapa 1 (física) |
| `02_Vestuario` | Vestuario: piso y paredes de azulejos, 23 taquillas con camisetas numeradas, bancos, escudo, pizarra táctica, puerta y mesa de hidratación. Etapa 2 (nutrición) |
| `03_Calma` | El mismo estadio, vacío y de noche, con estrellas. Etapa 3 (respiración) y resumen de la recuperación |
| `F1…F4_Fundido_…` | Pantallas negras con el cartel entre etapas (FÚTBOL · Estadio Piki, ETAPA 2 DE 3, etc.). Los textos (objetos *Kicker*, *Título*, *Subtítulo*) y los tiempos (componente *Piki Transition*) se editan en la escena |
| `04_Partido` | Tu próximo partido: jugás con el #10 y una barra muestra tu rendimiento según cómo te recuperaste |

4. Abrí **`00_Inicio`** y apretá **▶ Play**. El juego pasa solo de una escena a la otra.
   También podés abrir cualquier escena y darle Play para probar solo esa etapa.

En cada escena, en la ventana *Hierarchy*, vas a encontrar:
* **Entornos**: todo el escenario (piso, tribunas, luces, cielo…). Se puede mover, cambiar de color o borrar,
  y los cambios se mantienen.
* **PikiRig**: la cámara y el cursor para VR.
* **Piki Game (juego)**: la lógica. En el Inspector podés elegir con qué etapa arranca (**Start At**).
* **Pantallas (vista previa…)**: todas las pantallas de esa etapa (paneles, holograma, herramientas, minijuego,
  respiración, final), una por objeto. Activá cada una para verla en el editor. Al dar Play se regeneran con los
  datos de la partida (situaciones al azar, resultados, etc.).
* **Procesos (animaciones)**: el objeto que corre las animaciones durante el juego.

Las texturas, materiales y mallas quedan guardados en `Assets/PikiGame/Generated`. Si reconstruís las escenas
desde el menú, se reemplazan y se pierden los cambios que hayas hecho a mano.

## Cómo se juega

| Dispositivo | Mirar en 360° | Elegir | Aplicar frío / calor / masaje | Respiración (seguir el camino) |
|---|---|---|---|---|
| Mac / PC (editor o build) | Arrastrar con el mouse o flechas | Clic | Mantener apretado sobre la herramienta y arrastrarla hasta la zona | Mover el mouse sin hacer clic |
| Celular (Android / iOS) | Mover el teléfono (giroscopio) o arrastrar | Tocar | Apoyar el dedo en la herramienta y arrastrarla | Deslizar el dedo o mover el teléfono |
| VR con controles (Meta Quest, OpenXR) | Girar la cabeza | Láser + **gatillo** | Apuntar, mantener el gatillo y llevarla | Apuntar con el control |
| VR solo mirada | Girar la cabeza | Mirar fijo 1,4 s el botón | Mirar la herramienta para agarrarla y después mirar la zona | Mover la mirada |

Atajos: **M** silencia el sonido y **R** recentra los paneles frente a la vista.

## Accesibilidad

En la pantalla de inicio, el botón **Accesibilidad** abre las opciones. Quedan guardadas en el equipo; para
volver a las de fábrica usá **Restablecer** o el menú **Piki Game ▸ Restablecer opciones de accesibilidad**.

| Tipo | Opción | Qué hace |
|---|---|---|
| Visual | **Texto grande** | Paneles, textos y botones un 15 % más grandes (y la barra de instrucciones en PC) |
| Visual | **Alto contraste** | Paneles opacos, bordes blancos y textos secundarios más claros |
| Visual · mareo | **Reducir movimiento** | Sin temblor de cámara en la respiración: la agitación se ve con el borde rojo y el punto naranja |
| Auditiva | **Subtítulos de sonidos** (activado) | Muestra en texto el silbato, los aciertos, los errores, la respiración y la música de fondo |
| Auditiva | **Volumen de la música** | 100 %, 60 %, 30 % o sin música, para concentrarse en las consignas |
| Motriz | **Agarre fácil** | Un toque agarra la herramienta y otro la suelta; el masaje no exige mover en círculos; más margen sobre la zona |
| Cognitiva | **Ritmo tranquilo** | Minijuego de 50 s con barras que bajan más lento, alimentos más lentos y un camino de respiración más ancho |

Además, siempre: se juega con un solo botón o solo con la mirada (VR), la interfaz se acomoda a la altura de la
cabeza (sentado o parado), cada sonido importante tiene su señal visual y vibración en los controles, en VR la
barra de instrucciones flota frente a la vista, y nunca se usa solo el color (siempre va con texto o ícono).

## Recorrido

1. **Inicio** y **¿Qué deporte practicás?**: Fútbol habilitado y el resto "Próximamente".
2. **Aparición en la cancha**: estadio al atardecer, tribunas llenas, silbato final, marcador *FINAL 2–1*,
   jugadores que se van al túnel y estadísticas del partido.
3. **Etapa 1, recuperación física**: holograma del cuerpo y 3 situaciones al azar (tipo, zona, lado,
   intensidad y señales). Agarrás la herramienta que corresponde (**bolsa de hielo, compresa tibia o pelota de masaje**) y la pasás vos mismo por la zona marcada. Para el masaje hay que moverla en círculos:
   - Si acertás, ves "Respuesta correcta" y se aplica el tratamiento, con partículas y reloj.
   - Si no, ves "No es la mejor opción…" con una pista, y volvés a intentar.
4. **Etapa 2, recuperación nutricional**: el vestuario, con un minijuego de 35 s en el que los alimentos 3D
   pasan a tu alrededor.
   - Barras de **Energía, Hidratación y Reparación**, con zona óptima entre 60 y 90 %.
   - Hay alimentos no prioritarios.
   - Tiene 3 niveles: más velocidad, más elementos y música más rápida.
5. **Etapa 3, vuelta a la calma**: estadio vacío de noche, con estrellas. Un camino luminoso sube al **INHALAR** (4 s) y baja al
   **EXHALAR** (6 s): hay que seguirlo con el puntero durante 3 ciclos. Si te salís, la cámara tiembla como si te
   agitaras y sube el pulso.
6. **Final**: la cancha amanece en calma y aparece **RECUPERACIÓN COMPLETADA**, con el resumen de las 3 etapas.
   Si te agitaste demasiado en la respiración, la misión falla (podés reintentar o seguir).
7. **Tu próximo partido**: jugás con el #10 y el reloj avanza. Según tu recuperación jugás los 90 minutos o el DT
   te cambia antes. Al final, una barra va de *"El DT te cambió por bajo rendimiento"* a
   *"Jugaste los 90 minutos rindiendo al 100%"*, con el puntaje de cada etapa.

## Publicar en cada plataforma (*File → Build Profiles*)

* **Mac / Windows:** funciona directo.
* **Android / iOS (360° con giroscopio):** cambiá la plataforma y hacé *Build*. Usá orientación horizontal
  (*Player → Resolution and Presentation → Landscape Left*).
* **Meta Quest / VR:**
  1. En *Edit → Project Settings → XR Plug-in Management* instalalo si te lo pide.
  2. Activá **OpenXR** en la pestaña Android y en la de PC. En *OpenXR → Features* activá
     *Meta Quest Support* y un perfil de controles (*Oculus Touch Controller Profile*; en PC también
     *Valve Index* y *HTC Vive* si los vas a usar).
  3. Dejá tildado *Initialize XR on Startup*.

  El juego detecta el visor solo: la cabeza mueve la cámara y los controles apuntan con láser.

  **Visores compatibles (vía OpenXR):** Meta Quest 2, 3, 3S y Pro (build Android, standalone) y visores de PC
  (Quest con Link/Air Link, Valve Index, HTC Vive). **Capacidades que usa:** seguimiento de cabeza 6DoF con origen
  en el piso, controles con seguimiento (láser), gatillo y grip, vibración háptica y modo solo mirada sin controles.
* **WebGL (navegador):** funciona en 360° con mouse o toque, sin VR.

## Si algo se ve raro


* **Todo rosa o magenta:** el proyecto no es URP ni Built-in estándar. Los scripts usan
  `Universal Render Pipeline/Lit` (o `Standard`) y `Sprites/Default`.
* **En un build faltan partes, pero en el editor se ven bien:** agregá `Sprites/Default` y
  `Universal Render Pipeline/Lit` en *Project Settings → Graphics → Always Included Shaders*.
* **No hace falta instalar ningún paquete:** la interfaz usa solo componentes que vienen con Unity.
* **Aparece un error del Input System:** en *Project Settings → Player → Active Input Handling* elegí
  *Input System Package (New)* o *Both*. El código funciona con cualquiera de los dos sistemas.
* **Aparece "Scene couldn't be loaded" al pasar de etapa:** las escenas tienen que estar en *File ▸ Build Profiles ▸
  Scene List*. El constructor las agrega solo; si las sacaste, volvé a construir.

## Archivos

| Archivo | Qué hace |
|---|---|
| `PikiGame.cs` | Flujo completo, todas las pantallas y el paso entre escenas |
| `Editor/PikiSceneBuilder.cs` | Menú *Piki Game*: construye y guarda las 5 escenas y los 4 fundidos, y pone nombre e ícono a la app |
| `Editor/PikiImportSettings.cs` | Importa los logos como Sprite y la fuente Poppins con la línea base correcta |
| `PikiBrand.cs` | Marca: nombre, claim, logos (`Resources/PikiMarca`) y tipografía Poppins (`Resources/PikiFuentes`, licencia OFL) |
| `PikiAccess.cs` | Opciones de accesibilidad (se guardan en el equipo) |
| `PikiRig.cs` | Cámara 360°, giroscopio, VR (cabeza y controles), puntero, mirada y "mantener presionado" |
| `PikiEnv.cs` | Cielo, luces, hub de inicio, estadio (tribunas, LED, arcos, torres, marcador, jugadores) y vestuario |
| `PikiModels.cs` | Holograma corporal, efectos de frío, calor y masaje, y los 22 alimentos 3D |
| `PikiContent.cs` | Zonas, situaciones al azar, tratamientos, pistas y alimentos con sus valores |
| `PikiUI.cs` | Paneles, textos, botones e íconos en 3D (sin paquetes extra: SpriteRenderer + TextMesh) |
| `Audio/` | Audios de fondo. Cada escena tiene un objeto **Audio de la escena** con un *AudioSource* (Loop + Play On Awake): público en 01 y 04, música en 02 y música de calma bien baja (volumen 0,12) en 03. Se cambian desde el Inspector |
| `PikiAudio.cs` | Sonido sintetizado: público, silbato, música por nivel, ambiente de calma y respiración (con subtítulos) |
| `PikiCore.cs` | Materiales, texturas, íconos y mallas procedurales, más las animaciones |

---
*Contenido educativo. Ante un dolor intenso o persistente, consultá a un profesional de la salud.*
