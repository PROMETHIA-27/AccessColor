using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AccessColor
{
    public static class SettingsBlob
    {
        public static Boolean KeyComboIsPressed(KeyCombo combo)
        {
            return PromethiaInputManager.IsKeyPressed(combo.key) &&
                (combo.isShiftPressed == (PromethiaInputManager.IsKeyPressed(Key.LeftShift) || PromethiaInputManager.IsKeyPressed(Key.RightShift))) &&
                (combo.isCtrlPressed == (PromethiaInputManager.IsKeyPressed(Key.LeftCtrl) || PromethiaInputManager.IsKeyPressed(Key.RightCtrl))) &&
                (combo.isAltPressed == (PromethiaInputManager.IsKeyPressed(Key.LeftAlt) || PromethiaInputManager.IsKeyPressed(Key.RightAlt)));
        }

        public static Dictionary<String, KeyCombo> Shortcuts { get; set; } = new();

        static SettingsBlob()
        {
            Shortcuts["LockGlass"] = new() { key = Key.L, isAltPressed = true };
            Shortcuts["PickColor"] = new() { key = Key.P, isAltPressed = true };
        }

        public struct KeyCombo
        {
            public Key key;
            public Boolean isShiftPressed;
            public Boolean isCtrlPressed;
            public Boolean isAltPressed;
        }
    }
}
