using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace CodigoDeEmergencia.Core
{
    public enum InputActionId
    {
        MoveLeft = 0,
        MoveRight = 1,
        ClimbUp = 2,
        ClimbDown = 3,
        Jump = 4,
        Dash = 5,
        Shoot = 6,
        Melee = 7,
        Pause = 8
    }

    /// <summary>
    /// Proveedor unificado de entrada compatible con Unity 6 (New Input System y Legacy).
    /// Soporta asignación personalizada de una sola tecla o botón de mouse por acción sin duplicados,
    /// además de Gamepad.
    /// </summary>
    public static class GameInput
    {
        private const string BindPrefPrefix = "CDE_Bind_";
        public const int ActionCount = 9;

        private static readonly KeyCode[] DefaultBindings = new KeyCode[]
        {
            KeyCode.A,          // MoveLeft
            KeyCode.D,          // MoveRight
            KeyCode.W,          // ClimbUp
            KeyCode.S,          // ClimbDown
            KeyCode.Space,      // Jump
            KeyCode.LeftShift,  // Dash
            KeyCode.Mouse0,     // Shoot
            KeyCode.Mouse1,     // Melee
            KeyCode.P           // Pause
        };

        private static readonly KeyCode[] currentBindings = new KeyCode[ActionCount];
        private static bool bindingsLoaded = false;

#if ENABLE_INPUT_SYSTEM
        private struct KeyMapPair
        {
            public KeyCode keyCode;
            public Key inputKey;
        }

        private static readonly KeyMapPair[] KeyMap = new KeyMapPair[]
        {
            new KeyMapPair { keyCode = KeyCode.A, inputKey = Key.A },
            new KeyMapPair { keyCode = KeyCode.B, inputKey = Key.B },
            new KeyMapPair { keyCode = KeyCode.C, inputKey = Key.C },
            new KeyMapPair { keyCode = KeyCode.D, inputKey = Key.D },
            new KeyMapPair { keyCode = KeyCode.E, inputKey = Key.E },
            new KeyMapPair { keyCode = KeyCode.F, inputKey = Key.F },
            new KeyMapPair { keyCode = KeyCode.G, inputKey = Key.G },
            new KeyMapPair { keyCode = KeyCode.H, inputKey = Key.H },
            new KeyMapPair { keyCode = KeyCode.I, inputKey = Key.I },
            new KeyMapPair { keyCode = KeyCode.J, inputKey = Key.J },
            new KeyMapPair { keyCode = KeyCode.K, inputKey = Key.K },
            new KeyMapPair { keyCode = KeyCode.L, inputKey = Key.L },
            new KeyMapPair { keyCode = KeyCode.M, inputKey = Key.M },
            new KeyMapPair { keyCode = KeyCode.N, inputKey = Key.N },
            new KeyMapPair { keyCode = KeyCode.O, inputKey = Key.O },
            new KeyMapPair { keyCode = KeyCode.P, inputKey = Key.P },
            new KeyMapPair { keyCode = KeyCode.Q, inputKey = Key.Q },
            new KeyMapPair { keyCode = KeyCode.R, inputKey = Key.R },
            new KeyMapPair { keyCode = KeyCode.S, inputKey = Key.S },
            new KeyMapPair { keyCode = KeyCode.T, inputKey = Key.T },
            new KeyMapPair { keyCode = KeyCode.U, inputKey = Key.U },
            new KeyMapPair { keyCode = KeyCode.V, inputKey = Key.V },
            new KeyMapPair { keyCode = KeyCode.W, inputKey = Key.W },
            new KeyMapPair { keyCode = KeyCode.X, inputKey = Key.X },
            new KeyMapPair { keyCode = KeyCode.Y, inputKey = Key.Y },
            new KeyMapPair { keyCode = KeyCode.Z, inputKey = Key.Z },
            new KeyMapPair { keyCode = KeyCode.Alpha0, inputKey = Key.Digit0 },
            new KeyMapPair { keyCode = KeyCode.Alpha1, inputKey = Key.Digit1 },
            new KeyMapPair { keyCode = KeyCode.Alpha2, inputKey = Key.Digit2 },
            new KeyMapPair { keyCode = KeyCode.Alpha3, inputKey = Key.Digit3 },
            new KeyMapPair { keyCode = KeyCode.Alpha4, inputKey = Key.Digit4 },
            new KeyMapPair { keyCode = KeyCode.Alpha5, inputKey = Key.Digit5 },
            new KeyMapPair { keyCode = KeyCode.Alpha6, inputKey = Key.Digit6 },
            new KeyMapPair { keyCode = KeyCode.Alpha7, inputKey = Key.Digit7 },
            new KeyMapPair { keyCode = KeyCode.Alpha8, inputKey = Key.Digit8 },
            new KeyMapPair { keyCode = KeyCode.Alpha9, inputKey = Key.Digit9 },
            new KeyMapPair { keyCode = KeyCode.Space, inputKey = Key.Space },
            new KeyMapPair { keyCode = KeyCode.LeftShift, inputKey = Key.LeftShift },
            new KeyMapPair { keyCode = KeyCode.RightShift, inputKey = Key.RightShift },
            new KeyMapPair { keyCode = KeyCode.LeftControl, inputKey = Key.LeftCtrl },
            new KeyMapPair { keyCode = KeyCode.RightControl, inputKey = Key.RightCtrl },
            new KeyMapPair { keyCode = KeyCode.LeftAlt, inputKey = Key.LeftAlt },
            new KeyMapPair { keyCode = KeyCode.RightAlt, inputKey = Key.RightAlt },
            new KeyMapPair { keyCode = KeyCode.Tab, inputKey = Key.Tab },
            new KeyMapPair { keyCode = KeyCode.Return, inputKey = Key.Enter },
            new KeyMapPair { keyCode = KeyCode.Backspace, inputKey = Key.Backspace },
            new KeyMapPair { keyCode = KeyCode.UpArrow, inputKey = Key.UpArrow },
            new KeyMapPair { keyCode = KeyCode.DownArrow, inputKey = Key.DownArrow },
            new KeyMapPair { keyCode = KeyCode.LeftArrow, inputKey = Key.LeftArrow },
            new KeyMapPair { keyCode = KeyCode.RightArrow, inputKey = Key.RightArrow },
            new KeyMapPair { keyCode = KeyCode.Keypad0, inputKey = Key.Numpad0 },
            new KeyMapPair { keyCode = KeyCode.Keypad1, inputKey = Key.Numpad1 },
            new KeyMapPair { keyCode = KeyCode.Keypad2, inputKey = Key.Numpad2 },
            new KeyMapPair { keyCode = KeyCode.Keypad3, inputKey = Key.Numpad3 },
            new KeyMapPair { keyCode = KeyCode.Keypad4, inputKey = Key.Numpad4 },
            new KeyMapPair { keyCode = KeyCode.Keypad5, inputKey = Key.Numpad5 },
            new KeyMapPair { keyCode = KeyCode.Keypad6, inputKey = Key.Numpad6 },
            new KeyMapPair { keyCode = KeyCode.Keypad7, inputKey = Key.Numpad7 },
            new KeyMapPair { keyCode = KeyCode.Keypad8, inputKey = Key.Numpad8 },
            new KeyMapPair { keyCode = KeyCode.Keypad9, inputKey = Key.Numpad9 },
            new KeyMapPair { keyCode = KeyCode.KeypadEnter, inputKey = Key.NumpadEnter },
            new KeyMapPair { keyCode = KeyCode.F1, inputKey = Key.F1 },
            new KeyMapPair { keyCode = KeyCode.F2, inputKey = Key.F2 },
            new KeyMapPair { keyCode = KeyCode.F3, inputKey = Key.F3 },
            new KeyMapPair { keyCode = KeyCode.F4, inputKey = Key.F4 },
            new KeyMapPair { keyCode = KeyCode.F5, inputKey = Key.F5 },
            new KeyMapPair { keyCode = KeyCode.F6, inputKey = Key.F6 },
            new KeyMapPair { keyCode = KeyCode.F7, inputKey = Key.F7 },
            new KeyMapPair { keyCode = KeyCode.F8, inputKey = Key.F8 },
            new KeyMapPair { keyCode = KeyCode.F9, inputKey = Key.F9 },
            new KeyMapPair { keyCode = KeyCode.F10, inputKey = Key.F10 },
            new KeyMapPair { keyCode = KeyCode.F11, inputKey = Key.F11 },
            new KeyMapPair { keyCode = KeyCode.F12, inputKey = Key.F12 },
            new KeyMapPair { keyCode = KeyCode.Insert, inputKey = Key.Insert },
            new KeyMapPair { keyCode = KeyCode.Delete, inputKey = Key.Delete },
            new KeyMapPair { keyCode = KeyCode.Home, inputKey = Key.Home },
            new KeyMapPair { keyCode = KeyCode.End, inputKey = Key.End },
            new KeyMapPair { keyCode = KeyCode.PageUp, inputKey = Key.PageUp },
            new KeyMapPair { keyCode = KeyCode.PageDown, inputKey = Key.PageDown },
            new KeyMapPair { keyCode = KeyCode.Comma, inputKey = Key.Comma },
            new KeyMapPair { keyCode = KeyCode.Period, inputKey = Key.Period },
            new KeyMapPair { keyCode = KeyCode.Slash, inputKey = Key.Slash },
            new KeyMapPair { keyCode = KeyCode.Semicolon, inputKey = Key.Semicolon },
            new KeyMapPair { keyCode = KeyCode.Minus, inputKey = Key.Minus },
            new KeyMapPair { keyCode = KeyCode.Equals, inputKey = Key.Equals },
            new KeyMapPair { keyCode = KeyCode.LeftBracket, inputKey = Key.LeftBracket },
            new KeyMapPair { keyCode = KeyCode.RightBracket, inputKey = Key.RightBracket }
        };

        private static Dictionary<KeyCode, Key> keyCodeLookup;

        private static void EnsureKeyLookup()
        {
            if (keyCodeLookup != null) return;
            keyCodeLookup = new Dictionary<KeyCode, Key>(KeyMap.Length);
            for (int i = 0; i < KeyMap.Length; i++)
            {
                keyCodeLookup[KeyMap[i].keyCode] = KeyMap[i].inputKey;
            }
        }
#endif

        public static void LoadBindings()
        {
            bindingsLoaded = true;
            for (int i = 0; i < ActionCount; i++)
            {
                InputActionId act = (InputActionId)i;
                int saved = PlayerPrefs.GetInt(BindPrefPrefix + act, (int)DefaultBindings[i]);
                KeyCode code = (KeyCode)saved;
                currentBindings[i] = (code == KeyCode.None || code == KeyCode.Escape) ? DefaultBindings[i] : code;
            }

            // Resolver posibles duplicados heredados si se modificaron preferencias manualmente
            HashSet<KeyCode> used = new HashSet<KeyCode>();
            for (int i = 0; i < ActionCount; i++)
            {
                if (!used.Add(currentBindings[i]))
                {
                    currentBindings[i] = DefaultBindings[i];
                    used.Add(currentBindings[i]);
                }
            }
        }

        private static void EnsureBindingsLoaded()
        {
            if (!bindingsLoaded) LoadBindings();
        }

        public static KeyCode GetBinding(InputActionId action)
        {
            EnsureBindingsLoaded();
            return currentBindings[(int)action];
        }

        public static string GetBindingDisplayName(InputActionId action)
        {
            return FormatKeyCodeSpanish(GetBinding(action));
        }

        public static string GetActionDisplayName(InputActionId action)
        {
            switch (action)
            {
                case InputActionId.MoveLeft: return "Mover Izquierda";
                case InputActionId.MoveRight: return "Mover Derecha";
                case InputActionId.ClimbUp: return "Subir Escalera";
                case InputActionId.ClimbDown: return "Bajar Escalera";
                case InputActionId.Jump: return "Saltar / Planear";
                case InputActionId.Dash: return "Dash / Esquive";
                case InputActionId.Shoot: return "Disparo Pulso PEM";
                case InputActionId.Melee: return "Ataque Katana";
                case InputActionId.Pause: return "Menú de Pausa";
                default: return action.ToString();
            }
        }

        public static bool TryAssignBinding(InputActionId action, KeyCode newCode, out InputActionId conflictAction)
        {
            EnsureBindingsLoaded();
            conflictAction = action;
            if (newCode == KeyCode.None || newCode == KeyCode.Escape) return false;

            for (int i = 0; i < ActionCount; i++)
            {
                if (i == (int)action) continue;
                if (currentBindings[i] == newCode)
                {
                    conflictAction = (InputActionId)i;
                    return false;
                }
            }

            currentBindings[(int)action] = newCode;
            PlayerPrefs.SetInt(BindPrefPrefix + action, (int)newCode);
            PlayerPrefs.Save();
            return true;
        }

        public static void ResetDefaultBindings()
        {
            bindingsLoaded = true;
            for (int i = 0; i < ActionCount; i++)
            {
                currentBindings[i] = DefaultBindings[i];
                PlayerPrefs.SetInt(BindPrefPrefix + (InputActionId)i, (int)DefaultBindings[i]);
            }
            PlayerPrefs.Save();
        }

        public static string FormatKeyCodeSpanish(KeyCode code)
        {
            switch (code)
            {
                case KeyCode.Mouse0: return "CLIC IZQ";
                case KeyCode.Mouse1: return "CLIC DER";
                case KeyCode.Mouse2: return "CLIC MED";
                case KeyCode.Space: return "ESPACIO";
                case KeyCode.LeftShift: return "SHIFT IZQ";
                case KeyCode.RightShift: return "SHIFT DER";
                case KeyCode.LeftControl: return "CTRL IZQ";
                case KeyCode.RightControl: return "CTRL DER";
                case KeyCode.LeftAlt: return "ALT IZQ";
                case KeyCode.RightAlt: return "ALT DER";
                case KeyCode.Return: return "ENTER";
                case KeyCode.KeypadEnter: return "NUM ENTER";
                case KeyCode.Backspace: return "RETROCESO";
                case KeyCode.UpArrow: return "FLECHA ↑";
                case KeyCode.DownArrow: return "FLECHA ↓";
                case KeyCode.LeftArrow: return "FLECHA ←";
                case KeyCode.RightArrow: return "FLECHA →";
                case KeyCode.Escape: return "ESC";
                default:
                    string s = code.ToString();
                    if (s.StartsWith("Alpha")) return s.Substring(5);
                    if (s.StartsWith("Keypad")) return "NUM " + s.Substring(6);
                    return s.ToUpperInvariant();
            }
        }

        /// <summary>
        /// Captura cualquier tecla del teclado o botón del mouse presionado este frame para remapeo.
        /// Escape cancela la captura.
        /// </summary>
        public static bool CaptureAnyBindingThisFrame(out KeyCode captured, out bool cancelled)
        {
            captured = KeyCode.None;
            cancelled = false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    cancelled = true;
                    return false;
                }

                for (int i = 0; i < KeyMap.Length; i++)
                {
                    KeyControl ctrl = Keyboard.current[KeyMap[i].inputKey];
                    if (ctrl != null && ctrl.wasPressedThisFrame)
                    {
                        captured = KeyMap[i].keyCode;
                        return true;
                    }
                }
            }

            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame) { captured = KeyCode.Mouse0; return true; }
                if (Mouse.current.rightButton.wasPressedThisFrame) { captured = KeyCode.Mouse1; return true; }
                if (Mouse.current.middleButton.wasPressedThisFrame) { captured = KeyCode.Mouse2; return true; }
            }
            return false;
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                cancelled = true;
                return false;
            }
            if (Input.GetMouseButtonDown(0)) { captured = KeyCode.Mouse0; return true; }
            if (Input.GetMouseButtonDown(1)) { captured = KeyCode.Mouse1; return true; }
            if (Input.GetMouseButtonDown(2)) { captured = KeyCode.Mouse2; return true; }
            foreach (KeyCode k in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (k != KeyCode.None && k != KeyCode.Escape && (int)k < (int)KeyCode.Mouse0 && Input.GetKeyDown(k))
                {
                    captured = k;
                    return true;
                }
            }
            return false;
#else
            return false;
#endif
        }

        private static bool IsBindingPressed(KeyCode code)
        {
#if ENABLE_INPUT_SYSTEM
            if (code == KeyCode.Mouse0) return Mouse.current != null && Mouse.current.leftButton.isPressed;
            if (code == KeyCode.Mouse1) return Mouse.current != null && Mouse.current.rightButton.isPressed;
            if (code == KeyCode.Mouse2) return Mouse.current != null && Mouse.current.middleButton.isPressed;
            EnsureKeyLookup();
            if (Keyboard.current != null && keyCodeLookup.TryGetValue(code, out Key key))
            {
                KeyControl ctrl = Keyboard.current[key];
                return ctrl != null && ctrl.isPressed;
            }
            return false;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(code);
#else
            return false;
#endif
        }

        private static bool WasBindingPressedThisFrame(KeyCode code)
        {
#if ENABLE_INPUT_SYSTEM
            if (code == KeyCode.Mouse0) return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            if (code == KeyCode.Mouse1) return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
            if (code == KeyCode.Mouse2) return Mouse.current != null && Mouse.current.middleButton.wasPressedThisFrame;
            EnsureKeyLookup();
            if (Keyboard.current != null && keyCodeLookup.TryGetValue(code, out Key key))
            {
                KeyControl ctrl = Keyboard.current[key];
                return ctrl != null && ctrl.wasPressedThisFrame;
            }
            return false;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(code);
#else
            return false;
#endif
        }

        private static bool WasBindingReleasedThisFrame(KeyCode code)
        {
#if ENABLE_INPUT_SYSTEM
            if (code == KeyCode.Mouse0) return Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;
            if (code == KeyCode.Mouse1) return Mouse.current != null && Mouse.current.rightButton.wasReleasedThisFrame;
            if (code == KeyCode.Mouse2) return Mouse.current != null && Mouse.current.middleButton.wasReleasedThisFrame;
            EnsureKeyLookup();
            if (Keyboard.current != null && keyCodeLookup.TryGetValue(code, out Key key))
            {
                KeyControl ctrl = Keyboard.current[key];
                return ctrl != null && ctrl.wasReleasedThisFrame;
            }
            return false;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyUp(code);
#else
            return false;
#endif
        }

        /// <summary>
        /// Eje de movimiento horizontal (-1 = izquierda, +1 = derecha).
        /// </summary>
        public static float GetHorizontal()
        {
            float move = 0f;
            if (IsBindingPressed(GetBinding(InputActionId.MoveLeft))) move -= 1f;
            if (IsBindingPressed(GetBinding(InputActionId.MoveRight))) move += 1f;

#if ENABLE_INPUT_SYSTEM
            if (Gamepad.current != null)
            {
                float stickX = Gamepad.current.leftStick.x.ReadValue();
                if (Mathf.Abs(stickX) > 0.15f) move = stickX;
                if (Gamepad.current.dpad.left.isPressed) move = -1f;
                if (Gamepad.current.dpad.right.isPressed) move = 1f;
            }
#endif
            return move;
        }

        /// <summary>
        /// Eje de movimiento vertical (-1 = abajo, +1 = arriba) para escaleras.
        /// </summary>
        public static float GetVertical()
        {
            float vert = 0f;
            if (IsBindingPressed(GetBinding(InputActionId.ClimbDown))) vert -= 1f;
            if (IsBindingPressed(GetBinding(InputActionId.ClimbUp))) vert += 1f;

#if ENABLE_INPUT_SYSTEM
            if (Gamepad.current != null)
            {
                float stickY = Gamepad.current.leftStick.y.ReadValue();
                if (Mathf.Abs(stickY) > 0.15f) vert = stickY;
                if (Gamepad.current.dpad.down.isPressed) vert = -1f;
                if (Gamepad.current.dpad.up.isPressed) vert = 1f;
            }
#endif
            return vert;
        }

        public static bool GetJumpDown()
        {
            bool jump = WasBindingPressedThisFrame(GetBinding(InputActionId.Jump));
#if ENABLE_INPUT_SYSTEM
            if (Gamepad.current != null) jump |= Gamepad.current.buttonSouth.wasPressedThisFrame;
#endif
            return jump;
        }

        public static bool GetJumpSpaceOnlyDown()
        {
            KeyCode jumpBind = GetBinding(InputActionId.Jump);
            bool jump = (jumpBind < KeyCode.Mouse0 && WasBindingPressedThisFrame(jumpBind));
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null) jump |= Keyboard.current.spaceKey.wasPressedThisFrame;
            if (Gamepad.current != null) jump |= Gamepad.current.buttonSouth.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            jump |= Input.GetKeyDown(KeyCode.Space);
#endif
            return jump;
        }

        public static bool GetConfirmDialogueDown()
        {
#if ENABLE_INPUT_SYSTEM
            bool confirmed = false;
            if (Keyboard.current != null)
            {
                confirmed |= Keyboard.current.spaceKey.wasPressedThisFrame ||
                             Keyboard.current.enterKey.wasPressedThisFrame ||
                             Keyboard.current.numpadEnterKey.wasPressedThisFrame ||
                             Keyboard.current.eKey.wasPressedThisFrame;
            }
            if (Gamepad.current != null)
            {
                confirmed |= Gamepad.current.buttonSouth.wasPressedThisFrame ||
                             Gamepad.current.startButton.wasPressedThisFrame;
            }
            if (Mouse.current != null)
            {
                confirmed |= Mouse.current.leftButton.wasPressedThisFrame;
            }
            return confirmed;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetMouseButtonDown(0);
#else
            return false;
#endif
        }

        public static bool GetJumpHeld()
        {
            bool jump = IsBindingPressed(GetBinding(InputActionId.Jump));
#if ENABLE_INPUT_SYSTEM
            if (Gamepad.current != null) jump |= Gamepad.current.buttonSouth.isPressed;
#endif
            return jump;
        }

        public static bool GetDashDown()
        {
            bool dash = WasBindingPressedThisFrame(GetBinding(InputActionId.Dash));
#if ENABLE_INPUT_SYSTEM
            if (Gamepad.current != null) dash |= Gamepad.current.buttonEast.wasPressedThisFrame || Gamepad.current.rightShoulder.wasPressedThisFrame;
#endif
            return dash;
        }

        public static bool GetShootDown()
        {
            bool shoot = WasBindingPressedThisFrame(GetBinding(InputActionId.Shoot));
#if ENABLE_INPUT_SYSTEM
            if (Gamepad.current != null) shoot |= Gamepad.current.buttonWest.wasPressedThisFrame || Gamepad.current.rightTrigger.wasPressedThisFrame;
#endif
            return shoot;
        }

        public static bool GetShootHeld()
        {
            bool shoot = IsBindingPressed(GetBinding(InputActionId.Shoot));
#if ENABLE_INPUT_SYSTEM
            if (Gamepad.current != null) shoot |= Gamepad.current.buttonWest.isPressed || Gamepad.current.rightTrigger.isPressed;
#endif
            return shoot;
        }

        public static bool GetShootUp()
        {
            bool shoot = WasBindingReleasedThisFrame(GetBinding(InputActionId.Shoot));
#if ENABLE_INPUT_SYSTEM
            if (Gamepad.current != null) shoot |= Gamepad.current.buttonWest.wasReleasedThisFrame || Gamepad.current.rightTrigger.wasReleasedThisFrame;
#endif
            return shoot;
        }

        public static bool GetShoot() => GetShootHeld();

        public static bool GetMeleeDown()
        {
            bool melee = WasBindingPressedThisFrame(GetBinding(InputActionId.Melee));
#if ENABLE_INPUT_SYSTEM
            if (Gamepad.current != null) melee |= Gamepad.current.buttonNorth.wasPressedThisFrame || Gamepad.current.leftShoulder.wasPressedThisFrame;
#endif
            return melee;
        }

        public static bool GetRestartDown()
        {
#if ENABLE_INPUT_SYSTEM
            bool restart = Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
            if (Gamepad.current != null) restart |= Gamepad.current.selectButton.wasPressedThisFrame;
            return restart;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.R);
#else
            return false;
#endif
        }

        public static bool GetPauseDown()
        {
            bool pause = WasBindingPressedThisFrame(GetBinding(InputActionId.Pause));
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null) pause |= Keyboard.current.escapeKey.wasPressedThisFrame;
            if (Gamepad.current != null) pause |= Gamepad.current.startButton.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            pause |= Input.GetKeyDown(KeyCode.Escape);
#endif
            return pause;
        }

        public static Vector2 GetMouseScreenPosition()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null) return Mouse.current.position.ReadValue();
            return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.mousePosition;
#else
            return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
#endif
        }

        public static Vector3 GetMouseWorldPosition(Vector3 fallbackOrigin)
        {
            Camera cam = Camera.main;
            if (cam == null) return fallbackOrigin + Vector3.right;

            Vector2 screenPos = GetMouseScreenPosition();
            float zDist = Mathf.Abs(cam.transform.position.z);
            Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zDist));
            worldPos.z = 0f;
            return worldPos;
        }
    }

    /// <summary>
    /// Configuraciones persistentes y en tiempo real de Video, Jugabilidad, Dificultad y Nickname.
    /// </summary>
    public static class GameSettings
    {
        private const string KeyNickname = "CDE_PlayerNickname";
        private const string KeyDifficulty = "CDE_Difficulty";
        private const string KeyBrightness = "CDE_Brightness";
        private const string KeyCameraZoom = "CDE_CameraZoom";
        private const string KeyScreenEffects = "CDE_ScreenEffects";
        private const string KeyEnemyBars = "CDE_ShowEnemyBars";
        private const string KeyHudScale = "CDE_HudScale";
        private const string KeyShowFps = "CDE_ShowFps";
        private const string KeyFpsLimit = "CDE_FpsLimit";
        private const string KeyResolution = "CDE_ResolutionIdx";
        private const string KeyVSync = "CDE_VSync";

        private static bool loaded = false;
        private static string playerNickname = "Agente";
        private static int difficulty = 1;       // 0 = Asistido, 1 = Normal, 2 = Difícil
        private static int brightness = 1;       // 0 = Oscuro, 1 = Normal, 2 = Brillante
        private static int cameraZoom = 1;       // 0 = Cercana, 1 = Normal, 2 = Amplia
        private static int screenEffects = 2;    // 0 = Desactivada, 1 = Suave, 2 = Completa
        private static bool showEnemyBars = true;
        private static int hudScale = 1;         // 0 = Compacto, 1 = Normal, 2 = Grande
        private static bool showFpsCounter = false;
        private static int fpsLimitIndex = 1;    // 0 = 30, 1 = 60, 2 = 120, 3 = Sin límite
        private static int resolutionIndex = 3;  // 0 = 1280x720, 1 = 1600x900, 2 = 1920x1080, 3 = Nativa
        private static bool vSyncEnabled = true;

        public static void LoadAll()
        {
            loaded = true;
            playerNickname = SanitizeNickname(PlayerPrefs.GetString(KeyNickname, "Agente"), fallback: "Agente");
            difficulty = Mathf.Clamp(PlayerPrefs.GetInt(KeyDifficulty, 1), 0, 2);
            brightness = Mathf.Clamp(PlayerPrefs.GetInt(KeyBrightness, 1), 0, 2);
            cameraZoom = Mathf.Clamp(PlayerPrefs.GetInt(KeyCameraZoom, 1), 0, 2);
            screenEffects = Mathf.Clamp(PlayerPrefs.GetInt(KeyScreenEffects, 2), 0, 2);
            showEnemyBars = PlayerPrefs.GetInt(KeyEnemyBars, 1) == 1;
            hudScale = Mathf.Clamp(PlayerPrefs.GetInt(KeyHudScale, 1), 0, 2);
            showFpsCounter = PlayerPrefs.GetInt(KeyShowFps, 0) == 1;
            fpsLimitIndex = Mathf.Clamp(PlayerPrefs.GetInt(KeyFpsLimit, 1), 0, 3);
            resolutionIndex = Mathf.Clamp(PlayerPrefs.GetInt(KeyResolution, 3), 0, 3);
            vSyncEnabled = PlayerPrefs.GetInt(KeyVSync, 1) > 0;
        }

        private static void EnsureLoaded()
        {
            if (!loaded) LoadAll();
        }

        public static string SanitizeNickname(string raw, string fallback = "")
        {
            if (string.IsNullOrWhiteSpace(raw)) return fallback;
            string cleaned = raw.Replace("<", "").Replace(">", "").Trim();
            if (cleaned.Length > 16) cleaned = cleaned.Substring(0, 16).Trim();
            return cleaned.Length < 2 ? fallback : cleaned;
        }

        public static string PlayerNickname
        {
            get { EnsureLoaded(); return playerNickname; }
            set
            {
                EnsureLoaded();
                playerNickname = SanitizeNickname(value, fallback: "Agente");
                PlayerPrefs.SetString(KeyNickname, playerNickname);
                PlayerPrefs.Save();
            }
        }

        public static int Difficulty
        {
            get { EnsureLoaded(); return difficulty; }
            set { EnsureLoaded(); difficulty = Mathf.Clamp(value, 0, 2); PlayerPrefs.SetInt(KeyDifficulty, difficulty); PlayerPrefs.Save(); }
        }

        public static float PlayerDamageMultiplier => Difficulty == 0 ? 0.65f : (Difficulty == 2 ? 1.40f : 1.0f);
        public static int StartingLives => Difficulty == 0 ? 5 : (Difficulty == 2 ? 2 : 3);
        public static int CheckpointHealAmount => Difficulty == 0 ? 70 : (Difficulty == 2 ? 25 : 45);
        public static float InvulnerabilityDuration => Difficulty == 0 ? 1.60f : (Difficulty == 2 ? 0.85f : 1.20f);
        public static float EnemyHealthDifficultyMultiplier => Difficulty == 0 ? 0.85f : (Difficulty == 2 ? 1.20f : 1.0f);
        public static float EnemySpawnIntervalMultiplier => Difficulty == 0 ? 1.20f : (Difficulty == 2 ? 0.82f : 1.0f);
        public static string DifficultyDisplayName => Difficulty == 0 ? "ASISTIDO" : (Difficulty == 2 ? "DIFÍCIL" : "NORMAL");
        public static string DifficultySummary
        {
            get
            {
                if (Difficulty == 0) return "Daño recibido -35% • 5 Vidas iniciales • Checkpoints: 100% HP o +1 Vida • Drones -15% vida";
                if (Difficulty == 2) return "Daño recibido +40% • 2 Vidas iniciales • Checkpoints: 100% HP o +1 Vida • Drones +20% vida";
                return "Daño recibido estándar • 3 Vidas iniciales • Checkpoints: 100% HP o +1 Vida • Balance original";
            }
        }

        public static int BrightnessLevel
        {
            get { EnsureLoaded(); return brightness; }
            set { EnsureLoaded(); brightness = Mathf.Clamp(value, 0, 2); PlayerPrefs.SetInt(KeyBrightness, brightness); PlayerPrefs.Save(); }
        }
        public static float BrightnessMultiplier => BrightnessLevel == 0 ? 0.72f : (BrightnessLevel == 2 ? 1.38f : 1.0f);
        public static string BrightnessDisplayName => BrightnessLevel == 0 ? "OSCURO" : (BrightnessLevel == 2 ? "BRILLANTE" : "NORMAL");

        public static int CameraZoomLevel
        {
            get { EnsureLoaded(); return cameraZoom; }
            set { EnsureLoaded(); cameraZoom = Mathf.Clamp(value, 0, 2); PlayerPrefs.SetInt(KeyCameraZoom, cameraZoom); PlayerPrefs.Save(); }
        }
        public static float TargetCameraOrthoSize => CameraZoomLevel == 0 ? 5.4f : (CameraZoomLevel == 2 ? 7.6f : 6.4f);
        public static string CameraZoomDisplayName => CameraZoomLevel == 0 ? "CERCANA" : (CameraZoomLevel == 2 ? "AMPLIA" : "NORMAL");

        public static int ScreenEffectsLevel
        {
            get { EnsureLoaded(); return screenEffects; }
            set { EnsureLoaded(); screenEffects = Mathf.Clamp(value, 0, 2); PlayerPrefs.SetInt(KeyScreenEffects, screenEffects); PlayerPrefs.Save(); }
        }
        public static float ScreenShakeScale => ScreenEffectsLevel == 0 ? 0f : (ScreenEffectsLevel == 1 ? 0.45f : 1.0f);
        public static float ScreenFlashScale => ScreenEffectsLevel == 0 ? 0f : (ScreenEffectsLevel == 1 ? 0.40f : 1.0f);
        public static string ScreenEffectsDisplayName => ScreenEffectsLevel == 0 ? "DESACTIVADOS" : (ScreenEffectsLevel == 1 ? "SUAVES" : "COMPLETOS");

        public static bool ShowEnemyHealthBars
        {
            get { EnsureLoaded(); return showEnemyBars; }
            set { EnsureLoaded(); showEnemyBars = value; PlayerPrefs.SetInt(KeyEnemyBars, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static int HudScaleLevel
        {
            get { EnsureLoaded(); return hudScale; }
            set { EnsureLoaded(); hudScale = Mathf.Clamp(value, 0, 2); PlayerPrefs.SetInt(KeyHudScale, hudScale); PlayerPrefs.Save(); }
        }
        public static float HudScaleFactor => HudScaleLevel == 0 ? 0.85f : (HudScaleLevel == 2 ? 1.18f : 1.0f);
        public static string HudScaleDisplayName => HudScaleLevel == 0 ? "COMPACTO" : (HudScaleLevel == 2 ? "GRANDE" : "NORMAL");

        public static bool ShowFpsCounter
        {
            get { EnsureLoaded(); return showFpsCounter; }
            set { EnsureLoaded(); showFpsCounter = value; PlayerPrefs.SetInt(KeyShowFps, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static int FpsLimitIndex
        {
            get { EnsureLoaded(); return fpsLimitIndex; }
            set { EnsureLoaded(); fpsLimitIndex = Mathf.Clamp(value, 0, 3); PlayerPrefs.SetInt(KeyFpsLimit, fpsLimitIndex); PlayerPrefs.Save(); ApplySystemVideoSettings(); }
        }
        public static int TargetFrameRate => FpsLimitIndex == 0 ? 30 : (FpsLimitIndex == 2 ? 120 : (FpsLimitIndex == 3 ? -1 : 60));
        public static string FpsLimitDisplayName => FpsLimitIndex == 0 ? "30 FPS" : (FpsLimitIndex == 2 ? "120 FPS" : (FpsLimitIndex == 3 ? "SIN LÍMITE" : "60 FPS"));

        public static bool VSyncEnabled
        {
            get { EnsureLoaded(); return vSyncEnabled; }
            set { EnsureLoaded(); vSyncEnabled = value; PlayerPrefs.SetInt(KeyVSync, value ? 1 : 0); PlayerPrefs.Save(); ApplySystemVideoSettings(); }
        }

        public static int ResolutionIndex
        {
            get { EnsureLoaded(); return resolutionIndex; }
            set { EnsureLoaded(); resolutionIndex = Mathf.Clamp(value, 0, 3); PlayerPrefs.SetInt(KeyResolution, resolutionIndex); PlayerPrefs.Save(); }
        }
        public static string ResolutionDisplayName => ResolutionIndex == 0 ? "1280×720" : (ResolutionIndex == 1 ? "1600×900" : (ResolutionIndex == 2 ? "1920×1080" : "NATIVA"));

        public static void ApplySystemVideoSettings()
        {
            EnsureLoaded();
            QualitySettings.vSyncCount = vSyncEnabled ? 1 : 0;
            Application.targetFrameRate = TargetFrameRate;
        }

        public static void ApplyResolution(bool fullscreen)
        {
            EnsureLoaded();
            int w, h;
            switch (resolutionIndex)
            {
                case 0: w = 1280; h = 720; break;
                case 1: w = 1600; h = 900; break;
                case 2: w = 1920; h = 1080; break;
                default:
                    w = Display.main != null ? Display.main.systemWidth : Screen.currentResolution.width;
                    h = Display.main != null ? Display.main.systemHeight : Screen.currentResolution.height;
                    break;
            }
            FullScreenMode mode = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            Screen.SetResolution(Mathf.Max(800, w), Mathf.Max(600, h), mode);
        }
    }
}
