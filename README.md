# Piki Recovery · Fútbol (VR 360°)

Experiencia inmersiva de **recuperación post-partido** para fútbol, en un único archivo `index.html`
(A-Frame / WebXR). Funciona igual en **Web (360° con mouse o celular)** y en **realidad virtual**
(Meta Quest, visores WebXR y Cardboard). La narrativa, los escenarios, los objetivos y los resultados
son los mismos; solo cambia la forma de interactuar.

> Concepto: **el trabajo invisible**, todo lo que pasa después del partido, que no se ve,
> pero forma parte de la recuperación y del rendimiento.

## Recorrido

| # | Momento | Entorno | Qué pasa |
|---|---------|---------|----------|
| 0 | Inicio | Espacio neutro (grilla + partículas) | Presentación de Piki Recovery y de las 3 etapas |
| 1 | ¿Qué deporte practicás? | Espacio neutro | 6 disciplinas; **Fútbol** está habilitado (el resto aparece como "Próximamente") |
| 2 | Aparición en la cancha | Estadio al atardecer, tribunas llenas, silbato final | Marcador *FINAL 2–1*, jugadores que se retiran, estadísticas del partido (km, sprints, FC, sudor, kcal) |
| 3 | **Etapa 1 · Recuperación física** | Estadio + holograma corporal | 3 situaciones **aleatorias** (tipo: dolor / molestia / fatiga / rigidez; zona; lado; intensidad 1–10; señales). El usuario interpreta y elige **Frío, Calor o Masajes**. Si acierta: *Respuesta correcta* y se aplica el tratamiento con animación y reloj. Si no: *No es la mejor opción para esta situación*, una pista y vuelve a intentar (se registra en el resultado). |
| 4 | **Etapa 2 · Recuperación nutricional** | Vestuario (taquillas, camisetas, pizarra, mesa de hidratación) | Minijuego: los alimentos pasan alrededor del usuario y hay que elegir. Indicadores **Energía, Hidratación y Reparación** con zona óptima (60–90 %). Lo que falta resta y lo que sobra desequilibra. Hay alimentos no prioritarios (gaseosa, cerveza, papas fritas…). Los niveles bajan con el tiempo. **3 niveles de dificultad**: más velocidad, más elementos y menos tiempo para decidir; la música se acelera. |
| 5 | **Etapa 3 · Vuelta a la calma** | Estadio vacío, de noche, con estrellas | Baja la música y el movimiento. Una **esfera luminosa** guía la respiración: **INHALÁ** (crece, 4 s) / **EXHALÁ** (se achica, 6 s), 6 ciclos. La frecuencia cardíaca baja de a poco. |
| 6 | Final | La cancha vuelve de a poco, tranquila, al amanecer | **RECUPERACIÓN COMPLETADA** con el resumen: 🟢 Recuperación física · 🟢 Recuperación nutricional · 🟢 Vuelta a la calma, más los datos de cada etapa. Se puede jugar de nuevo con nuevas situaciones. |

### Lógica de la etapa física

| Situación | Señales | Mejor opción |
|-----------|---------|--------------|
| Dolor (golpe reciente) | hinchazón, calor local | **Frío** |
| Molestia de intensidad alta | pinchazo, hinchazón leve | **Frío** |
| Molestia leve o moderada | sobrecarga, sin golpe ni hinchazón | **Masajes** |
| Fatiga | pesadez, cansancio muscular | **Masajes** |
| Rigidez | poca movilidad, sin inflamación | **Calor** |

Zonas típicas del fútbol: isquiotibiales, cuádriceps, gemelos, rodilla, tobillo, aductores, zona lumbar y trapecio.

## Interacción

| Medio | Seleccionar | Respiración (seguir el ritmo) |
|-------|-------------|-------------------------------|
| Web (PC) | Clic. Arrastrá para mirar en 360° | Mantener **clic** o **ESPACIO** al inhalar y soltar al exhalar |
| Celular | Tocar. Mover el teléfono o arrastrar para mirar | Mantener el dedo apoyado al inhalar |
| VR con controles (Quest) | Láser + **gatillo** | Mantener el **gatillo** al inhalar (con vibración en cada cambio de fase) |
| VR solo mirada (Cardboard) | Mirar fijo el botón (cursor con carga) | Modo guiado: respirar junto a la esfera |

Atajos: `M` silencia el sonido y `R` recentra los paneles frente a la vista.

Todo el sonido se genera en el navegador con Web Audio (público, silbato, música del minijuego,
ambiente de calma y efectos), así que no hace falta ningún archivo de audio.

## Cómo abrirlo

* **Local:** abrí `index.html` en Chrome, Edge o Firefox. Si el CDN no responde, se carga la copia de
  A-Frame que está en `lib/`.
* **Realidad virtual:** WebXR necesita **HTTPS**. Publicalo, por ejemplo con GitHub Pages
  (*Settings → Pages → Deploy from branch*), abrí la URL en el navegador del visor y tocá el botón **VR**.
* También podés servirlo en tu red local con `npx serve .` o `python3 -m http.server`.

### Accesos directos para probar etapas

Agregá `?etapa=` a la URL: `deporte`, `cancha`, `fisica`, `situacion`, `nutricion`, `minijuego`,
`calma`, `respiracion` y `final`.

## Versión Unity

En la carpeta [`unity/`](unity/README.md) está la misma experiencia en C# para **Unity 6** (360° en PC y celular, y VR con OpenXR / Meta Quest).

## Estructura

```
index.html                 Toda la experiencia (escenas 3D, UI, lógica y audio)
lib/aframe-v1.6.0.min.js   Copia local de A-Frame 1.6.0 (licencia MIT, lib/AFRAME-LICENSE)
```

---
*Contenido educativo. Ante un dolor intenso o persistente, consultá a un profesional de la salud.*
