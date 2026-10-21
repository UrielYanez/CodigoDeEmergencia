using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CodigoDeEmergencia.Core
{
    /// <summary>
    /// Proveedor unificado de entrada compatible con Unity 6 (New Input System y Legacy).
    /// Detecta automáticamente el sistema activo en Project Settings sin provocar excepciones:
    /// - Soporta Teclado (WASD, Flechas, Espacio, Shift, J, Ctrl).
    /// - Soporta Ratón (Clic Izquierdo).
    /// - Soporta Gamepad / Controles (Stick Izquierdo, D-Pad, Botones A, B, X, Gatillos).
    /// </summary>
    public static class GameInput
    {
        /// <summary>
        /// Eje de movimiento horizontal (-1 = izquierda, +1 = derecha).
        /// </summary>
        public static float GetHorizontal()
        {
#if ENABLE_INPUT_SYSTEM
            float move = 0f;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) move -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) move += 1f;
            }

            if (Gamepad.current != null)
            {
                float stickX = Gamepad.current.leftStick.x.ReadValue();
                if (Mathf.Abs(stickX) > 0.15f) move = stickX;
                if (Gamepad.current.dpad.left.isPressed) move = -1f;
                if (Gamepad.current.dpad.right.isPressed) move = 1f;
            }

            return move;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetAxisRaw("Horizontal");
#else
            return 0f;
#endif
        }

        /// <summary>
        /// Eje de movimiento vertical (-1 = abajo, +1 = arriba) para escaleras.
        /// </summary>
        public static float GetVertical()
        {
#if ENABLE_INPUT_SYSTEM
            float vert = 0f;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vert -= 1f;
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vert += 1f;
            }

            if (Gamepad.current != null)
            {
                float stickY = Gamepad.current.leftStick.y.ReadValue();
                if (Mathf.Abs(stickY) > 0.15f) vert = stickY;
                if (Gamepad.current.dpad.down.isPressed) vert = -1f;
                if (Gamepad.current.dpad.up.isPressed) vert = 1f;
            }

            return vert;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetAxisRaw("Vertical");
#else
            return 0f;
#endif
        }

        /// <summary>
        /// Detecta la pulsación inicial del salto.
        /// </summary>
        public static bool GetJumpDown()
        {
#if ENABLE_INPUT_SYSTEM
            bool jump = false;

            if (Keyboard.current != null)
            {
                jump |= Keyboard.current.spaceKey.wasPressedThisFrame ||
                        Keyboard.current.wKey.wasPressedThisFrame ||
                        Keyboard.current.upArrowKey.wasPressedThisFrame;
            }

            if (Gamepad.current != null)
            {
                jump |= Gamepad.current.buttonSouth.wasPressedThisFrame; // A (Xbox) / Cruz (PS)
            }

            return jump;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space);
#else
            return false;
#endif
        }

        /// <summary>
        /// Detecta pulsación de salto únicamente con Espacio o botón A del Gamepad (usado al estar en una escalera para que W/Arriba sirva para trepar).
        /// </summary>
        public static bool GetJumpSpaceOnlyDown()
        {
#if ENABLE_INPUT_SYSTEM
            bool jump = false;
            if (Keyboard.current != null) jump |= Keyboard.current.spaceKey.wasPressedThisFrame;
            if (Gamepad.current != null) jump |= Gamepad.current.buttonSouth.wasPressedThisFrame;
            return jump;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Space);
#else
            return false;
#endif
        }

        /// <summary>
        /// Detecta si el botón de salto continúa presionado (para salto variable).
        /// </summary>
        public static bool GetJumpHeld()
        {
#if ENABLE_INPUT_SYSTEM
            bool jump = false;

            if (Keyboard.current != null)
            {
                jump |= Keyboard.current.spaceKey.isPressed ||
                        Keyboard.current.wKey.isPressed ||
                        Keyboard.current.upArrowKey.isPressed;
            }

            if (Gamepad.current != null)
            {
                jump |= Gamepad.current.buttonSouth.isPressed;
            }

            return jump;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetButton("Jump") || Input.GetKey(KeyCode.Space);
#else
            return false;
#endif
        }

        /// <summary>
        /// Detecta la pulsación de esquive (Dash).
        /// </summary>
        public static bool GetDashDown()
        {
#if ENABLE_INPUT_SYSTEM
            bool dash = false;

            if (Keyboard.current != null)
            {
                dash |= Keyboard.current.leftShiftKey.wasPressedThisFrame ||
                        Keyboard.current.rightShiftKey.wasPressedThisFrame ||
                        Keyboard.current.kKey.wasPressedThisFrame;
            }

            if (Gamepad.current != null)
            {
                dash |= Gamepad.current.buttonEast.wasPressedThisFrame ||
                        Gamepad.current.rightShoulder.wasPressedThisFrame; // B / Círculo / RB
            }

            return dash;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.LeftShift) || Input.GetButtonDown("Fire3");
#else
            return false;
#endif
        }

        /// <summary>
        /// Detecta el inicio de la pulsación de disparo (para comenzar a cargar).
        /// </summary>
        public static bool GetShootDown()
        {
#if ENABLE_INPUT_SYSTEM
            bool shoot = false;
            if (Mouse.current != null) shoot |= Mouse.current.leftButton.wasPressedThisFrame;
            if (Keyboard.current != null) shoot |= Keyboard.current.jKey.wasPressedThisFrame || Keyboard.current.leftCtrlKey.wasPressedThisFrame;
            if (Gamepad.current != null) shoot |= Gamepad.current.buttonWest.wasPressedThisFrame || Gamepad.current.rightTrigger.wasPressedThisFrame;
            return shoot;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.J);
#else
            return false;
#endif
        }

        /// <summary>
        /// Detecta si se mantiene presionado el botón de disparo (para acumular carga).
        /// </summary>
        public static bool GetShootHeld()
        {
#if ENABLE_INPUT_SYSTEM
            bool shoot = false;
            if (Mouse.current != null) shoot |= Mouse.current.leftButton.isPressed;
            if (Keyboard.current != null) shoot |= Keyboard.current.jKey.isPressed || Keyboard.current.leftCtrlKey.isPressed;
            if (Gamepad.current != null) shoot |= Gamepad.current.buttonWest.isPressed || Gamepad.current.rightTrigger.isPressed;
            return shoot;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetButton("Fire1") || Input.GetKey(KeyCode.J);
#else
            return false;
#endif
        }

        /// <summary>
        /// Detecta cuando se suelta el botón de disparo (para liberar el pulso cargado).
        /// </summary>
        public static bool GetShootUp()
        {
#if ENABLE_INPUT_SYSTEM
            bool shoot = false;
            if (Mouse.current != null) shoot |= Mouse.current.leftButton.wasReleasedThisFrame;
            if (Keyboard.current != null) shoot |= Keyboard.current.jKey.wasReleasedThisFrame || Keyboard.current.leftCtrlKey.wasReleasedThisFrame;
            if (Gamepad.current != null) shoot |= Gamepad.current.buttonWest.wasReleasedThisFrame || Gamepad.current.rightTrigger.wasReleasedThisFrame;
            return shoot;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetButtonUp("Fire1") || Input.GetKeyUp(KeyCode.J);
#else
            return false;
#endif
        }

        /// <summary>
        /// Detecta disparo continuo o pulsado.
        /// </summary>
        public static bool GetShoot() => GetShootHeld();

        /// <summary>
        /// Detecta ataque cuerpo a cuerpo (Katana / Electro-Espada): Clic Derecho, U, X, E o F.
        /// </summary>
        public static bool GetMeleeDown()
        {
#if ENABLE_INPUT_SYSTEM
            bool melee = false;
            if (Mouse.current != null) melee |= Mouse.current.rightButton.wasPressedThisFrame;
            if (Keyboard.current != null)
            {
                melee |= Keyboard.current.uKey.wasPressedThisFrame ||
                         Keyboard.current.xKey.wasPressedThisFrame ||
                         Keyboard.current.eKey.wasPressedThisFrame ||
                         Keyboard.current.fKey.wasPressedThisFrame;
            }
            if (Gamepad.current != null)
            {
                melee |= Gamepad.current.buttonNorth.wasPressedThisFrame || Gamepad.current.leftShoulder.wasPressedThisFrame;
            }
            return melee;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.U) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.E);
#else
            return false;
#endif
        }

        /// <summary>
        /// Detecta la tecla de reinicio o reintento rápido (R o botón Select del Gamepad).
        /// </summary>
        public static bool GetRestartDown()
        {
#if ENABLE_INPUT_SYSTEM
            bool restart = false;
            if (Keyboard.current != null)
            {
                restart |= Keyboard.current.rKey.wasPressedThisFrame;
            }
            if (Gamepad.current != null)
            {
                restart |= Gamepad.current.selectButton.wasPressedThisFrame;
            }
            return restart;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.R);
#else
            return false;
#endif
        }
    }
}
