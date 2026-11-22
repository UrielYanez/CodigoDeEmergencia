# ⚡ Código de Emergencia

Videojuego de acción y plataformas 2D desarrollado en **Unity 6 (URP 2D)** en la **Universidad Tecnológica del Norte de Guanajuato**.

## Resumen del Juego
Infiltrándote en un campus universitario tomado por la IA **S.I.A.**, deberás recorrer cuatro edificios temáticos, evadir defensas, destruir drones hostiles y sobrecargar con pulsos electromagnéticos (PEM) y Katana las pulseras que mantienen en trance a los docentes jefes para liberarlos a salvo.

### Características Principales
- **Campaña completa en 8 escenas:** Menú Principal, Cinemática de Historia (5 actos), Tutorial Interactivo espacioso (`310 m` con 7 estaciones y Mini-Jefe), 4 Niveles temáticos de `1,260 m` con escalado progresivo de dificultad (`+20%` por nivel) y Cinemática Final de 8 escenas con ceremonia de premiación.
- **4 personajes jugables y Nickname personalizado** integrado en diálogos, guardado y cinemáticas.
- **Sistema de 8 Checkpoints por nivel** con detección vertical completa, restauración al **100% de salud** y bonificación de **+1 vida extra** cuando se llega con salud completa.
- **HUD Sci-Fi con assets reales** (Kenney & Wenrexa) y barra superior central de progreso del nivel (`%`).
- **Configuración unificada en 3 pestañas** (Audio, Video/Juego y reasignación de 9 controles teclado/mouse sin duplicados) y **4 ranuras de guardado persistente**.

## Desarrolladores
- Cynthia Janeth Granados Estrada
- Uriel Yáñez Aguayo
- Antigravity

## Estructura del Proyecto
- `Assets/_Project/Scripts/` — 23 scripts C# organizados en `Core`, `Player`, `Combat`, `Enemies`, `UI` y `Editor`.
- `Assets/Scenes/` — Escenas jugables y cinemáticas (`MainMenu`, `IntroCutscene`, `TutorialScene`, `SampleScene`, `Level2Scene`, `Level3Scene`, `Level4Scene`, `EndingCutscene`).
- `Assets/_Project/Sprites/`, `Animations/`, `Audio/`, `Prefabs/`, `Materials/` — Recursos gráficos, animaciones y pistas de audio.
- `docs/` — Especificaciones técnicas, guía de builds, QA, créditos/licencias y [Manual de Usuario](docs/MANUAL_DE_USUARIO.md).

## Cómo Abrir y Ejecutar en Unity
1. Abrir el proyecto con **Unity `6000.6.0f1`** (URP 2D + New Input System).
2. Abrir `Assets/Scenes/MainMenu.unity` (o ejecutar `Tools > Código de Emergencia > 🚀 Montar Todas las Escenas` para regenerar todas las escenas automáticamente).
3. Pulsar **Play** en el Editor.

## Créditos y Licencias
Consulta el detalle completo de atribuciones, audio sintetizado con IA y licencias de terceros en [docs/CREDITS_AND_LICENSES.md](docs/CREDITS_AND_LICENSES.md).
