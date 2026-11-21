# Reporte de QA, Balanceo de IA y Optimización (Issue #21)

## 1. Auditoría de Compilación y Arquitectura (Unity 6)
- **Total de scripts C#:** `23` archivos bajo `Assets/` (sin duplicados ni dependencias circulares).
- **Compilación Roslyn Unity `6000.6.0f1`:** `Assembly-CSharp` (`EXIT=0`) y `Assembly-CSharp-Editor` (`EXIT=0`), **0 errores y 0 advertencias**.
- **Compatibilidad API Unity 6:** Migrado a `FindObjectsByType<T>()` y `UnityEngine.InputSystem` (`activeInputHandler = 1`).

## 2. Balanceo Progresivo por Nivel (`+20%` por Nivel)
| Nivel | Escena | Temática | Multiplicador Base | Checkpoints |
| :--- | :--- | :--- | :---: | :---: |
| Tutorial | `TutorialScene.unity` | Laboratorio de Entrenamiento (`310 m`) | `1.00×` (Mini-Jefe) | `8` |
| Nivel 1 | `SampleScene.unity` | Laboratorio de Cómputo (`1,260 m`) | `1.00×` | `8` |
| Nivel 2 | `Level2Scene.unity` | Centro de Redes (`1,260 m`) | `1.20×` | `8` |
| Nivel 3 | `Level3Scene.unity` | Torre de Servidores (`1,260 m`) | `1.44×` | `8` |
| Nivel 4 | `Level4Scene.unity` | Núcleo Central S.I.A. (`1,260 m`) | `1.728×` | `8` |

## 3. Verificación de Correcciones Clave de Jugabilidad
- **Sin retroceso en enemigos:** Se eliminó el empuje hacia atrás al golpear enemigos para evitar abuso del spam del pulso PEM, conservando el retroceso que los enemigos causan al jugador.
- **Checkpoints mejorados:** Colisionadores de altura completa (`180 m`) para activación aunque el jugador cruce por plataformas elevadas; restauran **100% de salud** o otorgan **+1 vida extra** si ya está al 100% y faltan vidas; aviso visual discreto sin sonido repetitivo.
- **Aterrizaje exacto de docentes liberados:** Los jefes purificados descienden hasta la superficie exacta del suelo (`h.point.y`) sin quedar flotando.
- **HUD y progreso en vivo:** Barra superior central de porcentaje del nivel sincronizada con la posición del jugador, textos HD super-muestreados `2x` en asignación de teclas (`fontSize = 42` con `BestFit`) y assets reales de Kenney y Wenrexa en barras, paneles y balizas.
