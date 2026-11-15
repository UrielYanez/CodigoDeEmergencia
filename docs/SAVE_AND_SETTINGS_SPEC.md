# Sistema de Guardado Persistente y Configuración Unificada (Issue #17)

## Sistema de Guardado en 4 Ranuras (`PlayerPrefs`)
Implementado en `HUDController.cs` y restaurado al iniciar nivel en `PlayerHealth.cs`:
- **4 ranuras independientes** (`CDE_SaveSlot_0_`..`CDE_SaveSlot_3_`) con metadatos completos:
  - Escena activa (`TutorialScene`, `SampleScene`, `Level2Scene`, `Level3Scene`, `Level4Scene`)
  - Personaje seleccionado (`0..3`) y **Nickname obligatorio** del jugador (`CDE_PlayerNickname`)
  - Salud actual, vidas restantes, último checkpoint alcanzado (`0..8`), posición exacta de reaparición y fecha/hora
  - Bandera de campaña completada (`Completed = 1`) desde la cinemática final
- Confirmación de seguridad antes de eliminar cualquier ranura guardada.

## Menú de Configuración Unificado en 3 Pestañas
Accesible tanto desde el **Menú Principal** como desde el **Menú de Pausa** con aplicación inmediata en tiempo real:

### 1. Pestaña Audio
- Deslizadores independientes de **Volumen Maestro**, **Música (BGM)**, **Efectos de Combate (SFX)** y **Entorno**.
- Botón de silenciado global (`AudioListener.pause`), interruptor de sonido de salto (`CDE_JumpSfxEnabled`) y prueba de SFX.

### 2. Pestaña Video y Juego
- **Pantalla completa / Ventana**, **Resolución** (`1280×720`, `1600×900`, `1920×1080`, `Nativa`), **V-Sync**, **Límite de FPS** (`30`, `60`, `120`, `Sin límite`) y **Contador FPS en vivo**.
- **Brillo del entorno** (ajusta la `Light2D` global en vivo), **Zoom de cámara** (`Cercana`, `Normal`, `Amplia`), **Sacudida y destellos**, **Visibilidad de barras de enemigos** y **Tamaño del HUD** (`Compacto`, `Normal`, `Grande`).
- **Dificultad general** (`Asistido`, `Normal`, `Difícil`): ajusta daño recibido, vidas iniciales (`5 / 3 / 2`), invulnerabilidad, vida de enemigos y frecuencia de spawn.

### 3. Pestaña Controles Personalizables
- Reasignación libre de las **9 acciones** (`MoveLeft`, `MoveRight`, `ClimbUp`, `ClimbDown`, `Jump`, `Dash`, `Shoot`, `Melee`, `Pause`) a cualquier tecla o botón de mouse mediante `GameInput.TryAssignBinding`.
- Validación estricta **sin teclas repetidas**, cancelación con `ESC` y botón de restauración de valores predeterminados.
