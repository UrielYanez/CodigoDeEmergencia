# 📋 Plan de Desarrollo y Tablero de Sprints (Scrum)
## Videojuego: *Código de Emergencia*
**Materia:** Optativa I - Creación de Videojuegos (Unidad 2 - Actividad 1 & Desafío)  
**Equipo de Desarrollo:**
- **Uriel Yáñez Aguayo** (Desarrollador / Programación de Sistemas & Arquitectura)
- **Cynthia Janeth Granados Estrada** (Desarrolladora / Diseño de Niveles, UI & Mecánicas)

---

## 🛠️ Flujo de Trabajo en Git y Convención de Ramas

Para cumplir rigurosamente con la rúbrica (Pull Requests frecuentes de ambos integrantes, commits balanceados y trazabilidad):

1. **Ramas Principales:**
   - `main`: Rama de producción estable. Solo recibe código probado mediante Pull Requests.
   - `develop` (opcional o integración directa a `main` mediante PR).
2. **Convención de Ramas:**
   - Cada issue debe desarrollarse en su propia rama: `feature/<nombre-de-la-funcionalidad>`.
   - Ejemplo: `feature/player-movement-physics`, `feature/bracelet-emp-decorruption`.
3. **Columnas del Tablero (GitHub Projects):**
   - **To Do (Por hacer):** Tareas planificadas para el Sprint actual aún no iniciadas.
   - **In Progress (En progreso):** Rama creada y código en desarrollo activo por el integrante asignado.
   - **In Review (En revisión):** Pull Request abierto en GitHub. **Obligatoriamente revisado y aprobado por el otro integrante** (ej. Cynthia revisa los PRs de Uriel y viceversa).
   - **Done (Hecho):** PR fusionado en `main`, build probado y criterios de aceptación verificados.

---

## 📅 Distribución de los 5 Sprints (Fechas Oficiales de Entrega)

| Sprint | Fechas | Entregable Clave | Responsable Principal de Cierre |
| :--- | :--- | :--- | :--- |
| **Sprint 1** | 14 Oct - 16 Oct 2026 | **Prototipo Base:** Movimiento, pulso PEM, colisiones, greybox y repo base. | Uriel & Cynthia |
| **Sprint 2** | 17 Oct - 30 Oct 2026 | **Nivel 1 Completo:** 4 estados (Jugar, Pausa, Victoria, Derrota) y Menú Principal. | Cynthia & Uriel |
| **Sprint 3** | 31 Oct - 06 Nov 2026 | **Niveles 2 y 3:** HUD completo, Audio y Enemigos con IA de patrullaje y des-corrupción. | Uriel & Cynthia |
| **Sprint 4** | 07 Nov - 13 Nov 2026 | **Nivel 4 (Jefe Director):** Narrativa/diálogos, guardado persistente y menú de opciones. | Cynthia & Uriel |
| **Sprint 5** | 14 Nov - 20/27 Nov 2026 | **Builds Multiplataforma (PC, WebGL, Android):** Testing, optimización y entrega final. | Uriel & Cynthia |

---

## 📦 Desglose de los 22 Issues Equitativos

### 🟢 SPRINT 1: Prototipo Base & Mecánica Principal (Límite: 16 de Octubre)

#### Issue #01: Controlador de Movimiento 2D y Físicas del Jugador
- **Asignado a:** Uriel Yáñez
- **Revisor:** Cynthia Granados
- **Rama:** `feature/player-movement-physics`
- **Historia de Usuario:**
  > *Como* jugador,  
  > *quiero* desplazar al protagonista hacia la izquierda y derecha y saltar con fluidez y precisión,  
  > *para* poder explorar los laboratorios del edificio y sortear obstáculos.
- **Criterios de Aceptación:**
  - [ ] El personaje se mueve horizontalmente respondiendo a teclado (`A`/`D`, Flechas) y Gamepad.
  - [ ] Detección precisa de suelo mediante `Physics2D.OverlapCircle` o `BoxCast` para evitar saltos infinitos.
  - [ ] La orientación (flip horizontal en `SpriteRenderer`) cambia según la dirección de avance.
  - [ ] Físicas configuradas en `Rigidbody2D` con gravedad y masa calibradas para evitar sensación de flotabilidad.
- **Tareas Técnicas:**
  1. Crear script `PlayerMovement.cs` con Input System de Unity.
  2. Implementar `FixedUpdate` para aplicación de velocidad en `Rigidbody2D`.
  3. Crear capa (Layer) `Ground` y configurar `GroundCheck`.
  4. Configurar colisionador `CapsuleCollider2D` para evitar atoramientos en bordes de tiles.

---

#### Issue #02: Mecánica de Esquive (Dash) y Máquina de Estados de Animación
- **Asignado a:** Cynthia Granados
- **Revisor:** Uriel Yáñez
- **Rama:** `feature/player-dash-dodge`
- **Historia de Usuario:**
  > *Como* jugador,  
  > *quiero* realizar un impulso rápido o esquive (dash) y ver animaciones sincronizadas con mis acciones,  
  > *para* evadir el contacto cercano con los profesores en trance.
- **Criterios de Aceptación:**
  - [ ] Al presionar la tecla de esquive (`Shift` o botón asignado), el jugador se desplaza rápidamente durante 0.2s.
  - [ ] Existe un tiempo de reutilización (cooldown) visible para evitar spam de esquives.
  - [ ] El `Animator` transiciona fluidamente entre: `Idle`, `Run`, `Jump`, `Fall` y `Dash`.
  - [ ] Durante el dash se activa una invulnerabilidad temporal o ignorancia de colisión con enemigos.
- **Tareas Técnicas:**
  1. Crear script `PlayerDash.cs` con rutina `IEnumerator DashRoutine()`.
  2. Configurar máquina de estados en el `Animator Controller` del jugador con parámetros (`Speed`, `isGrounded`, `isDashing`).
  3. Vincular los spritesheets seleccionados a las animaciones de movimiento.

---

#### Issue #03: Mecánica de Disparo de Pulso Electromagnético (PEM)
- **Asignado a:** Uriel Yáñez
- **Revisor:** Cynthia Granados
- **Rama:** `feature/emp-pulse-mechanic`
- **Historia de Usuario:**
  > *Como* jugador,  
  > *quiero* disparar un pulso electromagnético desde el celular modificado,  
  > *para* impactar a distancia las pulseras de los profesores sin causarles daño físico.
- **Criterios de Aceptación:**
  - [ ] Al presionar clic izquierdo o barra espaciadora, se dispara un proyectil/onda PEM en la dirección que mira el jugador.
  - [ ] El proyectil viaja a velocidad constante y se autodestruye tras un tiempo de vida o al impactar una superficie.
  - [ ] El disparo consume energía de batería o cuenta con un tiempo de enfriamiento breve.
  - [ ] Se genera un prefab con `CircleCollider2D` en modo trigger y script `EMPPulse.cs`.
- **Tareas Técnicas:**
  1. Diseñar prefab de proyectil PEM con `Rigidbody2D` kinemático y partículas eléctricas.
  2. Implementar script `PlayerShooting.cs` con punto de salida `FirePoint`.
  3. Configurar capa `Projectile` y matriz de colisiones para detectar enemigos y muros.

---

#### Issue #04: Nivel Prototipo (Greybox), Tilemaps y Cámara Cinemachine
- **Asignado a:** Cynthia Granados
- **Revisor:** Uriel Yáñez
- **Rama:** `feature/prototype-greybox-level`
- **Historia de Usuario:**
  > *Como* equipo de desarrollo,  
  > *queremos* contar con un nivel de prueba con plataformas, colisiones y cámara seguidora,  
  > *para* validar la jugabilidad, el salto y el disparo PEM antes de incorporar el arte final.
- **Criterios de Aceptación:**
  - [ ] Grid 2D con capas de Tilemap (`Ground`, `Obstacles`, `Background`) con `TilemapCollider2D` y `CompositeCollider2D`.
  - [ ] Cámara virtual Cinemachine 2D siguiendo al jugador suavemente (Damping) con límites de confiner (PolygonCollider2D).
  - [ ] Escena de prueba guardada en `Assets/Scenes/TestingGrounds.unity`.
- **Tareas Técnicas:**
  1. Instalar y configurar paquete Cinemachine en Unity 6.
  2. Crear paleta de tiles básica para pisos, rampas y plataformas flotantes.
  3. Configurar `CinemachineConfiner2D` para restringir la cámara a los límites del mapa.

---

### 🟡 SPRINT 2: Nivel 1 Completo, 4 Estados y Menú Principal (Límite: 30 de Octubre)

#### Issue #05: Diseño y Arte del Nivel 1 (Laboratorios de Planta Baja)
- **Asignado a:** Cynthia Granados
- **Revisor:** Uriel Yáñez
- **Rama:** `feature/level-1-laboratories`
- **Historia de Usuario:**
  > *Como* jugador,  
  > *quiero* recorrer un escenario escolar creíble y detallado ambientado en los laboratorios de cómputo del Edificio F,  
  > *para* sumergirme en la atmósfera de emergencia universitaria.
- **Criterios de Aceptación:**
  - [ ] Tilemap completo con estilo de laboratorio (pisos técnicos, mesas, computadoras, cables de red, pizarrones).
  - [ ] Fondos en capas con efecto Parallax sutil.
  - [ ] Iluminación 2D (Universal Render Pipeline - 2D Lights) con luces de emergencia rojas/azules parpadeantes.
- **Tareas Técnicas:**
  1. Integrar tilesets de aula/laboratorio en `Assets/Art/Environments/Level1_Labs/`.
  2. Crear script `ParallaxBackground.cs` para scroll relativo a la cámara.
  3. Configurar fuentes de luz `Light2D` puntuales y globales en la escena.

---

#### Issue #06: Administrador de Estados de Nivel (Jugar, Pausa, Victoria, Derrota)
- **Asignado a:** Uriel Yáñez
- **Revisor:** Cynthia Granados
- **Rama:** `feature/game-state-manager`
- **Historia de Usuario:**
  > *Como* jugador,  
  > *quiero* que cada nivel maneje adecuadamente los 4 estados de juego obligatorios,  
  > *para* poder pausar, ganar al cumplir el objetivo o reiniciar si soy interceptado.
- **Criterios de Aceptación:**
  - [ ] **Jugar:** Nivel activo, input habilitado y tiempo corriendo.
  - [ ] **Pausado:** Tecla `Esc` o `P` congela `Time.timeScale = 0`, abre menú con botones: Reanudar, Reiniciar, Opciones, Salir al Menú.
  - [ ] **Fin de Nivel (Victoria):** Al alcanzar la salida del laboratorio, muestra pantalla con tiempo transcurrido, puntaje y botón "Siguiente Nivel".
  - [ ] **Game Over (Derrota):** Al agotarse la vida, muestra pantalla de derrota con botones "Reintentar" y "Volver al Menú".
- **Tareas Técnicas:**
  1. Programar `LevelManager.cs` utilizando patrón Singleton y eventos en C#.
  2. Crear prefabs de UI: `PausePanel`, `VictoryPanel`, `GameOverPanel`.
  3. Controlar activación/desactivación de inputs mediante eventos del `GameState`.

---

#### Issue #07: Menú Principal y Sistema de Navegación de Escenas
- **Asignado a:** Cynthia Granados
- **Revisor:** Uriel Yáñez
- **Rama:** `feature/main-menu-navigation`
- **Historia de Usuario:**
  > *Como* usuario,  
  > *quiero* acceder a un menú de inicio profesional con opciones de navegación claras,  
  > *para* iniciar una nueva partida, continuar mi progreso o salir de la aplicación.
- **Criterios de Aceptación:**
  - [ ] Pantalla de Inicio / Splash con logo del equipo y título animado "Código de Emergencia".
  - [ ] Botones interactivos con 3 estados visuales (Normal, Resaltado/Hover, Presionado): *Jugar*, *Continuar*, *Opciones*, *Créditos*, *Salir*.
  - [ ] Pantalla de Selección de Niveles con al menos 4 botones (Nivel 1 desbloqueado, Niveles 2, 3 y 4 con candado bloqueado según progreso).
  - [ ] Carga asíncrona de escenas con pantalla de transición (`SceneManager.LoadSceneAsync`).
- **Tareas Técnicas:**
  1. Diseñar escena `MainMenu.unity` en Unity Canvas adaptable (`CanvasScaler` a 1920x1080).
  2. Implementar script `MainMenuController.cs` con métodos de carga de escenas.
  3. Crear pantalla de selección de niveles con verificación de niveles desbloqueados en `PlayerPrefs`.

---

#### Issue #08: Sistema de Tutorial Integrado en Nivel 1
- **Asignado a:** Uriel Yáñez
- **Revisor:** Cynthia Granados
- **Rama:** `feature/tutorial-system`
- **Historia de Usuario:**
  > *Como* jugador primerizo,  
  > *quiero* recibir instrucciones claras sobre los controles y mecánicas en los primeros metros del nivel,  
  > *para* comprender el uso del celular y la evasión antes de enfrentar desafíos mayores.
- **Criterios de Aceptación:**
  - [ ] Gatillos (`BoxCollider2D` trigger) en el mapa que activan carteles contextuales o popups con iconos de controles.
  - [ ] Primer cartel enseña movimiento y salto; segundo cartel enseña disparo PEM; tercer cartel enseña esquive (dash).
  - [ ] Los mensajes desaparecen al salir del área o tras presionar un botón de confirmación.
- **Tareas Técnicas:**
  1. Crear prefab `TutorialTrigger` con script `TutorialPrompt.cs`.
  2. Diseñar sprites de teclas (`WASD`, `Espacio`, `Shift`, `Click Izq`).
  3. Añadir animación suave de entrada y salida (Fade In / Fade Out) con CanvasGroup.

---

### 🔵 SPRINT 3: Niveles 2 y 3, HUD, Audio e IA de Profesores (Límite: 06 de Noviembre)

#### Issue #09: IA de Patrullaje y Detección de Profesores en Trance
- **Asignado a:** Uriel Yáñez
- **Revisor:** Cynthia Granados
- **Rama:** `feature/teacher-enemy-ai`
- **Historia de Usuario:**
  > *Como* jugador,  
  > *quiero* que los profesores controlados por la pulsera patrullen los pasillos y reaccionen si me ven,  
  > *para* tener un reto de sigilo y evasión dinámico en los niveles 2 y 3.
- **Criterios de Aceptación:**
  - [ ] El profesor patrulla entre puntos designados (Waypoints) cambiando de dirección al llegar a un borde o muro.
  - [ ] Cono de visión 2D (Raycast o Trigger frontal) con luz roja/linterna de pulsera.
  - [ ] Si detecta al jugador, entra en estado de alerta/persecución aumentando ligeramente su velocidad.
  - [ ] Si alcanza al jugador, le aplica daño y empuje (knockback).
- **Tareas Técnicas:**
  1. Programar `TeacherAI.cs` con máquina de estados (`Patrol`, `Alert`, `Chase`, `Stunned`, `Freed`).
  2. Implementar comprobación de línea de visión con `Physics2D.Raycast` filtrando por capas.
  3. Configurar animaciones de caminata robótica y alerta en el `Animator`.

---

#### Issue #10: Mecánica de Des-corrupción de Pulseras (Combate Pasivo)
- **Asignado a:** Cynthia Granados
- **Revisor:** Uriel Yáñez
- **Rama:** `feature/bracelet-emp-decorruption`
- **Historia de Usuario:**
  > *Como* jugador,  
  > *quiero* des-corromper a los profesores disparándoles pulsos PEM a sus pulseras,  
  > *para* liberarlos del control de S.I.A. sin lastimarlos y despejar el camino.
- **Criterios de Aceptación:**
  - [ ] Cada profesor cuenta con un contador de resistencia de pulsera (ej. 3 impactos PEM).
  - [ ] Cada impacto produce chispas visuales, retroalimentación sonora y un breve aturdimiento de 0.5s.
  - [ ] Al llegar el contador a 0: la pulsera explota en chispas, el profesor vuelve a la normalidad, reproduce animación de alivio/huida y sale de la escena corriendo.
  - [ ] Otorga puntos al jugador por "Profesor Liberado".
- **Tareas Técnicas:**
  1. Crear componente `SmartBracelet.cs` con variable `braceletHealth` y evento `OnBraceletDestroyed`.
  2. Vincular colisión de `EMPPulse` con `SmartBracelet`.
  3. Instanciar prefab de partículas de cortocircuito y activar animación de huida en el profesor.

---

#### Issue #11: Sistema de HUD Dinámico (Vida, Puntaje, Tiempo e Inventario)
- **Asignado a:** Uriel Yáñez
- **Revisor:** Cynthia Granados
- **Rama:** `feature/hud-system`
- **Historia de Usuario:**
  > *Como* jugador,  
  > *quiero* visualizar permanentemente en pantalla mi estado de salud, piezas recolectadas, puntaje y tiempo,  
  > *para* gestionar mis recursos durante la partida.
- **Criterios de Aceptación:**
  - [ ] Barra de salud / batería del celular que disminuye al recibir daño y se actualiza suavemente.
  - [ ] Contador de puntaje acumulado y temporizador del nivel en formato `MM:SS`.
  - [ ] Iconos de inventario para piezas de crafteo (Cables, Tarjetas de memoria, Llaves obtenidas).
  - [ ] HUD anclado correctamente (`Anchor Presets`) para no deformarse en distintas resoluciones (16:9, 18:9, etc.).
- **Tareas Técnicas:**
  1. Diseñar Canvas de HUD con TextMeshPro y sliders/imágenes en `HUDController.cs`.
  2. Suscribir el HUD a eventos de `PlayerHealth`, `ScoreManager` y `InventoryManager`.
  3. Añadir animación de sacudida o destello rojo al recibir daño.

---

#### Issue #12: Mecánica de Casilleros, Mochilas, Crafteo y Rescate de Compañeros
- **Asignado a:** Cynthia Granados
- **Revisor:** Uriel Yáñez
- **Rama:** `feature/items-crafting-rescue`
- **Historia de Usuario:**
  > *Como* jugador,  
  > *quiero* abrir casilleros, encontrar café curativo, piezas electrónicas y liberar compañeros con llaves,  
  > *para* obtener mejoras de estadísticas y avanzar hacia la fabricación del USB de Reseteo.
- **Criterios de Aceptación:**
  - [ ] Casilleros y mochilas interactuables con la tecla `E` o botón de acción.
  - [ ] Tazas de café restauran un porcentaje de vida/batería.
  - [ ] Cables y tarjetas de memoria se acumulan en el inventario.
  - [ ] Puertas de aulas cerradas requieren llave específica; al abrirlas, los compañeros son liberados automáticamente otorgando una mejora (vida máxima o velocidad) y un diálogo breve.
- **Tareas Técnicas:**
  1. Programar clase base `InteractableObject.cs` e implementar `Locker.cs`, `CollectibleItem.cs`, `DoorRescue.cs`.
  2. Crear `InventoryManager.cs` que contabilice items clave.
  3. Diseñar prefabs interactivos con indicadores flotantes de botón ("Presiona E").

---

#### Issue #13: Sistema de Audio Integral (BGM, SFX y Mezclador)
- **Asignado a:** Cynthia Granados
- **Revisor:** Uriel Yáñez
- **Rama:** `feature/audio-system-sfx`
- **Historia de Usuario:**
  > *Como* jugador,  
  > *quiero* escuchar música inmersiva acorde a la tensión escolar y efectos de sonido para cada acción,  
  > *para* tener una respuesta auditiva satisfactoria en todo momento.
- **Criterios de Aceptación:**
  - [ ] Música de fondo en bucle para Menú Principal y pistas de tensión para los niveles.
  - [ ] Transición suave (Crossfade) al pausar o cambiar de nivel.
  - [ ] Al menos 6 efectos de sonido sincronizados: disparo PEM, impacto/cortocircuito de pulsera, paso/salto, recoger item, abrir puerta y clic en UI.
  - [ ] Uso de `AudioMixer` con canales independientes: *Master*, *Music*, *SFX*.
- **Tareas Técnicas:**
  1. Crear `AudioManager.cs` con métodos `PlayMusic()`, `PlaySFX(string name)`.
  2. Configurar `AudioMixer` de Unity con buses expuestos para control de volumen por opciones.
  3. Asignar archivos de audio libres de derechos y preparar documento de créditos y licencias sonoras.

---

### 🟣 SPRINT 4: Nivel 4 (Jefe Director), Narrativa, Guardado y Opciones (Límite: 13 de Noviembre)

#### Issue #14: Diseño y Ambientación del Nivel 4 (Zona de Servidores y Dirección)
- **Asignado a:** Cynthia Granados
- **Revisor:** Uriel Yáñez
- **Rama:** `feature/level-4-server-room`
- **Historia de Usuario:**
  > *Como* jugador,  
  > *quiero* adentrarme en el piso más peligroso del edificio F (Oficina de Dirección y Cuarto de Servidores),  
  > *para* enfrentar el reto final y conectar el USB de Reseteo.
- **Criterios de Aceptación:**
  - [ ] Mapa con estética tecnológica avanzada: racks de servidores, luces parpadeantes, alarmas y puertas de seguridad blindadas.
  - [ ] Plataformas móviles y zonas con descargas eléctricas intermitentes que añaden dificultad de plataformeo.
  - [ ] Área de arena cerrada para la batalla contra el Jefe Final.
- **Tareas Técnicas:**
  1. Construir el Tilemap del Nivel 4 con tilesets sci-fi/tecnológicos en `Assets/Scenes/Level4_ServerRoom.unity`.
  2. Implementar script `MovingPlatform.cs` y trampas eléctricas con temporizador.
  3. Configurar triggers de bloqueo de puertas para iniciar el encuentro con el jefe.

---

#### Issue #15: Batalla contra el Jefe Final: "El Director" y Pulsera Maestra
- **Asignado a:** Uriel Yáñez
- **Revisor:** Cynthia Granados
- **Rama:** `feature/boss-director-battle`
- **Historia de Usuario:**
  > *Como* jugador,  
  > *quiero* enfrentar al Director en un combate desafiante por fases,  
  > *para* sobrecargar la pulsera maestra y alcanzar el panel central de S.I.A.
- **Criterios de Aceptación:**
  - [ ] El Director posee una barra de sobrecarga en pantalla (vida de jefe).
  - [ ] **Fase 1:** Ataque frontal con onda de choque y telemetría de S.I.A.; vulnerable a impactos PEM tras esquivar sus embestidas.
  - [ ] **Fase 2:** Invoca drones o defensas del servidor y activa barrera de energía que requiere apagar terminales auxiliares.
  - [ ] **Fase 3 (Final):** Sobrecarga final de la pulsera maestra con animación de cortocircuito masivo y liberación del Director.
- **Tareas Técnicas:**
  1. Crear script `DirectorBossAI.cs` con máquina de estados de jefe.
  2. Implementar barra de vida de jefe en el HUD (`BossHealthBarUI.cs`).
  3. Configurar eventos de victoria que habiliten la interacción con el servidor final.

---

#### Issue #16: Sistema de Narrativa Integrada, Diálogos y Profesor Barrón
- **Asignado a:** Cynthia Granados
- **Revisor:** Uriel Yáñez
- **Rama:** `feature/narrative-dialogue-system`
- **Historia de Usuario:**
  > *Como* jugador,  
  > *quiero* recibir mensajes y comunicaciones por radio/pantalla del Profesor Barrón desde el audiovisual,  
  > *para* avanzar en la historia del Edificio F y conocer los objetivos de cada nivel.
- **Criterios de Aceptación:**
  - [ ] Cuadro de diálogo estilo "Walkie-Talkie / Notificación de Celular" con retrato del Profesor Barrón.
  - [ ] Efecto de texto progresivo (typewriter effect) con sonido de tecleo sutil.
  - [ ] Diálogos introductorios en cada nivel y consejos en momentos clave (ej. antes del jefe).
  - [ ] Botón para saltar diálogo o avanzar al siguiente texto.
- **Tareas Técnicas:**
  1. Crear `DialogueManager.cs` utilizando colas (`Queue<string>`) o ScriptableObjects `DialogueData`.
  2. Implementar corrutina de texto máquina de escribir con TextMeshPro.
  3. Diseñar prefab de UI para la caja de mensajes con soporte de retratos de personajes.

---

#### Issue #17: Sistema de Guardado Persistente y Menú de Opciones
- **Asignado a:** Uriel Yáñez
- **Revisor:** Cynthia Granados
- **Rama:** `feature/save-options-system`
- **Historia de Usuario:**
  > *Como* jugador,  
  > *quiero* que mi progreso, niveles desbloqueados y puntajes se guarden automáticamente y poder calibrar el audio y controles,  
  > *para* continuar mi partida cuando lo desee con mi configuración personalizada.
- **Criterios de Aceptación:**
  - [ ] Guardado automático al completar cada nivel (nivel alcanzado, estrellas/puntaje máximo).
  - [ ] Botón "Continuar" en el Menú Principal carga el último nivel desbloqueado.
  - [ ] Menú de opciones funcional con:
    - Sliders de volumen: Música y Efectos (vinculados al `AudioMixer`).
    - Selector de resolución y modo pantalla completa.
    - Calidad gráfica (Baja, Media, Alta).
    - Pantalla de visualización de controles.
- **Tareas Técnicas:**
  1. Crear clase `SaveSystem.cs` utilizando serialización JSON o `PlayerPrefs` protegido.
  2. Implementar `OptionsMenuController.cs` con listeners de eventos en Sliders y Dropdowns.
  3. Cargar las preferencias automáticamente al iniciar el juego (`Awake`).

---

### 🟠 SPRINT 5: Despliegue Multiplataforma, Pruebas y Entrega Final (Límite: 20-27 de Noviembre)

#### Issue #18: Adaptación de Controles Táctiles para Móvil (Android)
- **Asignado a:** Cynthia Granados
- **Revisor:** Uriel Yáñez
- **Rama:** `feature/mobile-touch-controls`
- **Historia de Usuario:**
  > *Como* jugador móvil,  
  > *quiero* disponer de un joystick virtual en pantalla y botones táctiles ergonómicos,  
  > *para* jugar fluidamente en dispositivos Android.
- **Criterios de Aceptación:**
  - [ ] Joystick virtual flotante o fijo para movimiento horizontal.
  - [ ] Botones virtuales para: Salto (`A`), Disparo PEM (`B`), Esquive (`Dash`) y Pausa.
  - [ ] Los controles táctiles solo se muestran cuando la plataforma detectada es móvil (`#if UNITY_ANDROID`).
  - [ ] Respuesta táctil sin demoras de input (multitouch habilitado).
- **Tareas Técnicas:**
  1. Configurar paquete Unity New Input System con `On-Screen Stick` y `On-Screen Button`.
  2. Diseñar sprites de interfaz táctil semitransparentes.
  3. Integrar soporte híbrido para no afectar los controles de teclado en PC/Web.

---

#### Issue #19: Configuración y Generación de Builds en 3 Plataformas (PC, WebGL, Android)
- **Asignado a:** Uriel Yáñez
- **Revisor:** Cynthia Granados
- **Rama:** `feature/multiplatform-builds`
- **Historia de Usuario:**
  > *Como* evaluador y docente,  
  > *quiero* contar con ejecutables funcionales en Consola/PC (.exe), Web (WebGL) y Móvil (APK Android),  
  > *para* verificar la compatibilidad multiplataforma exigida en la rúbrica.
- **Criterios de Aceptación:**
  - [ ] **PC (.exe):** Ejecutable standalone sin caídas de frames ni bugs de resolución.
  - [ ] **Web (WebGL):** Build optimizado listo para publicar en itch.io o GitHub Pages con compresión gzip/brotli.
  - [ ] **Móvil (Android):** Archivo `.apk` firmado en modo debug/release instalado y probado en emulador o dispositivo real.
  - [ ] Iconos de la aplicación y Splash Screen de Unity configurados con el logo del equipo.
- **Tareas Técnicas:**
  1. Configurar `Player Settings` en Unity 6 para cada target de plataforma (Graphics API, Package Name `com.utng.codigodeemergencia`).
  2. Probar compilación WebGL y ajustar límites de memoria WebAssembly.
  3. Generar APK con Android SDK / NDK compatible con Android 10+.

---

#### Issue #20: Pantalla de Créditos y Adaptación Responsiva de GUI
- **Asignado a:** Cynthia Granados
- **Revisor:** Uriel Yáñez
- **Rama:** `feature/credits-ui-responsive`
- **Historia de Usuario:**
  > *Como* docente y jugador,  
  > *quiero* consultar la pantalla de créditos con los autores y las citas de todos los recursos externos usados,  
  > *para* dar cumplimiento formal a los lineamientos éticos y de autoría.
- **Criterios de Aceptación:**
  - [ ] Pantalla accesible desde el menú principal con créditos a: Uriel Yáñez y Cynthia Granados.
  - [ ] Mención explícita a la institución (UTNG), docente asesor, herramientas (Unity 6, VS Code, Git) y licencias de assets gráficos y audios.
  - [ ] Toda la interfaz (Menús, HUD, Popups) escala correctamente sin solapamientos entre relaciones 16:9, 16:10 y pantallas móviles 19.5:9.
- **Tareas Técnicas:**
  1. Crear escena o panel `CreditsView` con scroll automático o estático y botón de regreso.
  2. Auditar todos los `CanvasScaler` del proyecto con modo `Scale With Screen Size` (1920x1080, match 0.5).
  3. Documentar lista de fuentes y enlaces de licencias en la pantalla y en el reporte escrito.

---

#### Issue #21: Pruebas de Calidad (QA), Balanceo de IA y Optimización de Rendimiento
- **Asignado a:** Uriel Yáñez
- **Revisor:** Cynthia Granados
- **Rama:** `feature/qa-polishing-docs`
- **Historia de Usuario:**
  > *Como* equipo de desarrollo,  
  > *queremos* realizar pruebas con compañeros externos y optimizar el rendimiento del juego,  
  > *para* entregar un producto sin caídas de FPS, bugs críticos ni desequilibrios de dificultad.
- **Criterios de Aceptación:**
  - [ ] Ejecución de sesión de prueba externa con al menos 3 compañeros de clase (conforme a sección 5 del GDD).
  - [ ] Registro de bitácora de bugs corregidos (colisiones de tiles, atascos de IA, tiempos de cooldown).
  - [ ] Perfilado con Unity Profiler asegurando 60 FPS estables en PC/Web y 30-60 FPS en móvil.
  - [ ] Sin advertencias críticas (warnings) ni excepciones rojas en la consola de Unity.
- **Tareas Técnicas:**
  1. Realizar pasadas de prueba en los 4 niveles y documentar hallazgos en la bitácora.
  2. Ajustar rangos de visión de profesores y frecuencia de aparición de café y piezas.
  3. Optimizar Sprite Atlases, compresión de texturas y batching de Tilemaps.

---

#### Issue #22: Documentación Final, Manual de Usuario y Preparación de Video Demostrativo
- **Asignado a:** Cynthia Granados
- **Revisor:** Uriel Yáñez
- **Rama:** `feature/final-demo-video-prep`
- **Historia de Usuario:**
  > *Como* equipo de desarrollo,  
  > *queremos* consolidar toda la documentación técnica, manual de usuario y video demostrativo de 2 a 3 minutos,  
  > *para* obtener el 100% en los criterios de entrega final y presentación en clase.
- **Criterios de Aceptación:**
  - [ ] Documento de Diseño (GDD) actualizado con las decisiones finales y justificación de cambios.
  - [ ] Manual de usuario con guía de instalación, requerimientos y controles para PC, Web y Móvil.
  - [ ] Video demostrativo editado (2 a 3 minutos) mostrando los 4 niveles, menús, mecánicas y builds.
  - [ ] Tabla de contribución individual equitativa (50% Uriel / 50% Cynthia) respaldada por el historial de commits y PRs de GitHub.
- **Tareas Técnicas:**
  1. Redactar el documento final en formato PDF/Markdown según la plantilla institucional de la UTNG.
  2. Grabar capturas de video en alta definición de los 4 niveles en ejecución.
  3. Generar release final etiquetado `v1.0.0` en el repositorio de GitHub.

---

## ⚖️ Resumen de Asignaciones Equitativas

| Integrante | Cantidad de Issues | Issues Asignados | Rol Principal |
| :--- | :---: | :--- | :--- |
| **Uriel Yáñez Aguayo** | **11** | #01, #03, #06, #08, #09, #11, #15, #17, #19, #21, + Arquitectura Core | Programación de Sistemas, Físicas, FSM, Builds & Optimización |
| **Cynthia Janeth Granados Estrada** | **11** | #02, #04, #05, #07, #10, #12, #13, #14, #16, #18, #20, #22 | Diseño de Niveles, Animaciones, UI/UX, Audio, Móvil & Video |
| **Total** | **22** | **100% de cobertura de la rúbrica y GDD** | **Revisión cruzada obligatoria en columna In Review** |
