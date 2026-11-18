# Especificación de Controles Táctiles para Móvil — Android (Issue #18)

## Objetivo
Definir la adaptación de controles táctiles sobre `HUD_Canvas` (`ScaleWithScreenSize 1920×1080`) y su enlace con el sistema unificado `GameInput.cs` (`UnityEngine.InputSystem`) para dispositivos Android en orientación horizontal (*Landscape*).

## Distribución Ergonómica de Controles en Pantalla
- **Zona Inferior Izquierda (Pulgar Izquierdo — Movimiento y Escaleras):**
  - Cruceta direccional (`D-Pad` / joystick virtual) con ejes `Horizontal` (`MoveLeft` / `MoveRight`) y `Vertical` (`ClimbUp` / `ClimbDown`) usando assets de `Kenney UI Pack: Sci-Fi 2.0` (`button_square_depth.png`).
  - Zona segura de `40 px` respecto a bordes para pantallas con esquinas redondeadas.
- **Zona Inferior Derecha (Pulgar Derecho — Acciones de Combate y Esquive):**
  - **Disparo PEM (`Shoot`):** Botón principal con soporte de pulsación rápida (disparo normal) y pulsación sostenida (carga del pulso PEM, liberando al soltar el dedo).
  - **Ataque Katana (`Melee`):** Botón secundario cuerpo a cuerpo ubicado a la izquierda del botón de disparo.
  - **Salto / Planeo (`Jump`):** Botón inferior derecho con soporte de mantenimiento para caída suave.
  - **Dash (`Dash`):** Botón superior con indicador visual sincronizado al enfriamiento de `PlayerDash`.
- **Zona Superior:**
  - Botón táctil discreto de **Pausa (`Pause`)** junto al borde superior izquierdo, sin interferir con `Level_Progress_Panel` (centro) ni `Player_Health_Panel` (superior derecha).

## Activación Automática por Plataforma
- Activación condicional bajo `#if UNITY_ANDROID || UNITY_IOS` o detección de `Application.isMobilePlatform`, manteniendo la interfaz de escritorio limpia en PC/WebGL.
- Compatibilidad total con `InputSystemUIInputModule` para permitir multi-touch simultáneo (moverse + saltar + disparar).
