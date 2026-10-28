# Guía de Diseño del Tutorial Interactivo — *Código de Emergencia* (Issue #08)

## Objetivo
Capacitar al jugador paso a paso en las mecánicas de movimiento, exploración vertical, esquive y combate electromagnético dentro de `Assets/Scenes/TutorialScene.unity` antes de iniciar la campaña en el Nivel 1 (`SampleScene.unity`).

## Estructura del Nivel (`310 m`, `X = -26` a `+282`)
- **Separación amplia entre estaciones:** de `32 m` a `38 m` entre cada acción y checkpoint para practicar sin activar la instrucción siguiente de forma accidental.
- **8 Checkpoints (`Checkpoint_1` a `Checkpoint_8`):** detección vertical completa (`180 m`), regeneración al **100% de salud** y **+1 vida extra** si ya se cuenta con salud completa.

## Las 7 Estaciones Interactivas + Mini-Jefe
1. **1/7 — Movimiento (`X = -21.5`):** Desplazamiento lateral (`MoveLeft` / `MoveRight`).
2. **2/7 — Escaleras (`X = 12.0`):** Ascenso y descenso vertical (`ClimbUp` / `ClimbDown`).
3. **3/7 — Salto y Caída Suave (`X = 46.0`):** Salto y planeo manteniendo la tecla de salto; ataques y Dash quedan bloqueados mientras se planea o escala.
4. **4/7 — Esquive Dash (`X = 82.0`):** Impulso rápido con invulnerabilidad temporal (`Dash`).
5. **5/7 — Corte de Katana (`X = 116.0`):** Ataque cuerpo a cuerpo (`Melee`) frente a dron de práctica.
6. **6/7 — Disparo PEM (`X = 150.0`):** Apuntado direccional con cursor y disparo normal (`Shoot`).
7. **7/7 — Disparo Cargado (`X = 184.0`):** Carga sostenida de pulso PEM de alto impacto.
8. **Arena del Mini-Jefe (`X = 232..280`):** Enfrentamiento introductorio contra `Teacher_Boss_Tutorial` (`100 HP` de pulsera) y cinemática de liberación en 3 escenas.
