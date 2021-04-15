using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AccessColor
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow() => this.InitializeComponent();

        private Button? activeShortcutButton;
        private String? activeShortcutName;
        private void LockGlassButton_Click(Object sender, RoutedEventArgs e)
        {
            this.activeShortcutButton = LockGlassButton;
            this.activeShortcutName = "LockGlass";
            PromethiaInputManager.KeyPressEvent += this.ChangeShortcutCallback;

            InstructionLabel.Visibility = Visibility.Visible;
        }

        private void PickColorButton_Click(Object sender, RoutedEventArgs e)
        {
            this.activeShortcutButton = PickColorButton;
            this.activeShortcutName = "PickColor";
            PromethiaInputManager.KeyPressEvent += this.ChangeShortcutCallback;

            InstructionLabel.Visibility = Visibility.Visible;
        }

        private void ChangeShortcutCallback(Key key)
        {
            if (key is Key.LeftAlt or Key.RightAlt or Key.LeftShift or Key.RightShift or Key.LeftCtrl or Key.RightCtrl)
                return;

            SettingsBlob.KeyCombo newShortcut = new()
            {
                key = key,
                isShiftPressed = PromethiaInputManager.IsKeyPressed(Key.LeftShift) || PromethiaInputManager.IsKeyPressed(Key.RightShift),
                isCtrlPressed = PromethiaInputManager.IsKeyPressed(Key.LeftCtrl) || PromethiaInputManager.IsKeyPressed(Key.RightCtrl),
                isAltPressed = PromethiaInputManager.IsKeyPressed(Key.LeftAlt) || PromethiaInputManager.IsKeyPressed(Key.RightAlt)
            };

            SettingsBlob.Shortcuts[activeShortcutName!] = newShortcut;

            activeShortcutButton!.Content =
                (newShortcut.isShiftPressed ? "Shift + " : "") +
                (newShortcut.isCtrlPressed ? "Ctrl + " : "") +
                (newShortcut.isAltPressed ? "Alt + " : "") +
                key.ToString();

            PromethiaInputManager.KeyPressEvent -= this.ChangeShortcutCallback;

            InstructionLabel.Visibility = Visibility.Hidden;
        }

        private void OpenColorsButton_Click(Object sender, RoutedEventArgs e)
        {
            _ = System.Diagnostics.Process.Start("notepad.exe", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\AccessColor\\colorSpace.txt");
        }

        private void ReloadNamesButton_Click(Object sender, RoutedEventArgs e)
        {
            _ = ColorNamer.LoadColorData();
        }
    }
}
