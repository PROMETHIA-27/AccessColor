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
            _ = PlainTextSerialization.LoadToDictionary(
                "shortcutSettings",
                Shortcuts,
                (name) => name,
                (combo) =>
                {
                    var parts = combo.Split('-');
                    return new()
                    {
                        key = (Key)Int32.Parse(parts[0]),
                        isShiftPressed = Boolean.Parse(parts[1]),
                        isCtrlPressed = Boolean.Parse(parts[2]),
                        isAltPressed = Boolean.Parse(parts[3])
                    };
                },
                AppContext.BaseDirectory + "\\shortcutSettings.txt"
            );



            AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
            {
                _ = PlainTextSerialization.SaveFromDictionary(
                    Shortcuts,
                    "shortcutSettings",
                    (name) => name,
                    (combo) => $"{(Int32)combo.key:D}-{combo.isShiftPressed}-{combo.isCtrlPressed}-{combo.isAltPressed}"
                );
            };
        }

        public static void Wake() { }

        public struct KeyCombo
        {
            public Key key;
            public Boolean isShiftPressed;
            public Boolean isCtrlPressed;
            public Boolean isAltPressed;
        }
    }
}
