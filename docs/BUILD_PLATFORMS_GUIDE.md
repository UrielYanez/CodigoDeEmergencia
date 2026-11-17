# Guía de Configuración y Generación de Builds Multiplataforma (Issue #19)

## Requisitos del Entorno
- **Motor:** Unity `6000.6.0f1` (Unity 6) con **Universal Render Pipeline (URP 2D)** y **New Input System** (`activeInputHandler = 1`).
- **Escenas registradas en `ProjectSettings/EditorBuildSettings.asset`** (en orden):
  1. `Assets/Scenes/MainMenu.unity` *(Índice 0 — Escena de inicio)*
  2. `Assets/Scenes/IntroCutscene.unity`
  3. `Assets/Scenes/TutorialScene.unity`
  4. `Assets/Scenes/SampleScene.unity` *(Nivel 1: Laboratorio de Cómputo)*
  5. `Assets/Scenes/Level2Scene.unity` *(Nivel 2: Centro de Redes)*
  6. `Assets/Scenes/Level3Scene.unity` *(Nivel 3: Torre de Servidores)*
  7. `Assets/Scenes/Level4Scene.unity` *(Nivel 4: Núcleo Central S.I.A.)*
  8. `Assets/Scenes/EndingCutscene.unity`

## Perfil 1 — PC (Windows 64-bit)
- **Target:** `StandaloneWindows64` (`IL2CPP` o `Mono`, arquitectura `x86_64`).
- **Resolución base UI:** `CanvasScaler` en modo `ScaleWithScreenSize` (`1920×1080`, `matchWidthOrHeight = 0.5`).
- **Controles:** Teclado + Mouse totalmente personalizables + soporte Gamepad vía `GameInput.cs`.

## Perfil 2 — WebGL
- **Target:** `WebGL` con compresión `Gzip` o `Brotli` y `Decompression Fallback` activado.
- **Audio:** `AudioClip` en formato WAV/Vorbis con carga en memoria compatible con navegadores modernos (reproducción iniciada tras primera interacción de usuario).
- **Renderizado:** URP 2D con luces 2D optimizadas y límite de resolución de navegador.

## Perfil 3 — Android (Móvil)
- **Target:** `Android` (`IL2CPP`, `ARM64`, API Level mínimo `Android 9.0 / API 28`, orientación `Landscape Left / Right`).
- **Gráficos:** OpenGLES3 / Vulkan con compresión de texturas `ASTC`.
- **Integración táctil:** Conecta los botones en pantalla definidos en `docs/MOBILE_TOUCH_CONTROLS_SPEC.md` con las entradas virtuales de `GameInput.cs`.

## Verificación Pre-Build
1. Ejecutar en Unity `Tools > Código de Emergencia > 🚀 Montar Todas las Escenas` para confirmar referencias actualizadas.
2. Confirmar compilación limpia (`0` errores) en `Assembly-CSharp` y `Assembly-CSharp-Editor`.
3. Validar el flujo completo `MainMenu -> IntroCutscene -> TutorialScene -> Niveles 1 a 4 -> EndingCutscene`.
