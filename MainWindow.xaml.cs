using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AccessColor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow? Instance { get; set; }
        private Boolean _closing;
        public MainWindow()
        {
            this.InitializeComponent();

            var dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\AccessColor\\";
            if (!Directory.Exists(dir))
                _ = Directory.CreateDirectory(dir);

            if (!ColorNamer.LoadColorData())
                throw new Exception("Error loading color data!");

            Instance = this;

            GlassImage.Source = writeBmp;
            
            //Give it a second to start, hopefully prevent any weird glitches with launching threads before init is complete
            this._screenReaderTimer = new(this.ReadScreenAndSend, null, 1000, Timeout.Infinite);

            PromethiaInputManager.KeyPressEvent += this.LockGlassInputCallback;
            PromethiaInputManager.KeyPressEvent += this.PickColorInputCallback;

            this.SettingsWin.Closing += (sender, args) =>
            {
                if (!_closing)
                    args.Cancel = true;
                this.SettingsWin.Visibility = Visibility.Hidden;
            };
        }

        readonly Timer _screenReaderTimer;
        volatile Boolean _disposingTimer;
        volatile Boolean _glassLocked;
        volatile Boolean _pickColor;
        Bitmap screenBmp = new(1, 1);
        readonly WriteableBitmap writeBmp = new(1, 1, 100, 100, PixelFormats.Bgra32, BitmapPalettes.Halftone256);
        private void ReadScreenAndSend(Object? state) // Actually works on multiple monitors, I didn't even have to do anything
        {
            //This should be the only access to _zoomScale in this function!
            //Later accesses could introduce a race condition
            Int32Rect zoomedRect = PromethiaInputManager.GetMousePos().ToDrawingPoint().ToZoomedSlice(_zoomScale); 

            screenBmp = new(zoomedRect.Width, zoomedRect.Height);

            using (var gfx = Graphics.FromImage(screenBmp))
                gfx.CopyFromScreen(zoomedRect.StartToDrawingPoint(), System.Drawing.Point.Empty, zoomedRect.SizeToDrawingSize());

            Byte[] bmpBuffer;
            BitmapData bmpData;

            bmpData = screenBmp.LockBits(new System.Drawing.Rectangle(0, 0, screenBmp.Width, screenBmp.Height), ImageLockMode.ReadOnly, screenBmp.PixelFormat);

            var numbytes = bmpData.Stride * screenBmp.Height;
            bmpBuffer = new Byte[numbytes];
            IntPtr ptr = bmpData.Scan0;
            Marshal.Copy(ptr, bmpBuffer, 0, numbytes);

            screenBmp.UnlockBits(bmpData);

            Int32Rect rect = new(0, 0, zoomedRect.Width, zoomedRect.Height);
            this.Dispatcher.Invoke(() => 
            {   
                if (!_glassLocked)
                {   // This is awful for GC but there is no convenient way to resize a writeableBitmap, so I can't even pool. WriteableBitmapEx might have something.
                    var newSource = new WriteableBitmap(zoomedRect.Width, zoomedRect.Height, 100, 100, PixelFormats.Bgra32, BitmapPalettes.Halftone256);
                    newSource.WritePixels(rect, bmpBuffer, 4 * zoomedRect.Width, 0);
                    GlassImage.Source = newSource;
                }
                
                if (!_pickColor)
                    ColorRect.Fill = new SolidColorBrush(bmpBuffer.ReadPixelInfoMedia(4 * zoomedRect.Width, zoomedRect.Width / 2, zoomedRect.Height / 2));
                else 
                {
                    _pickColor = false;
                    this.PickColor(bmpBuffer.ReadPixelInfo(4 * zoomedRect.Width, zoomedRect.Width / 2, zoomedRect.Height / 2));
                }
            });

            if (!this._disposingTimer)
                _ = _screenReaderTimer.Change(33, Timeout.Infinite);
        }

        private void Window_Closing(Object sender, CancelEventArgs e)
        {
            using (ManualResetEvent disposeHandle = new(false))
            {
                _disposingTimer = true;
                _ = this._screenReaderTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _screenReaderTimer.Dispose();
            }
            _closing = true;
            this.SettingsWin.Close();
        }

        private volatile Int32 _zoomScale;
        private void ZoomSlider_ValueChanged(Object sender, RoutedPropertyChangedEventArgs<Double> e)
        {
            _zoomScale = (Int32)e.NewValue;
        }

        public SettingsWindow SettingsWin { get; } = new();
        private void SettingsButton_Click(Object sender, RoutedEventArgs e)
        {
            this.SettingsWin.Show();
        }

        private void LockGlassInputCallback(Key key)
        {
            if (SettingsBlob.KeyComboIsPressed(SettingsBlob.Shortcuts["LockGlass"]))
            {
                _glassLocked = !_glassLocked;
            }
        }

        private void PickColorInputCallback(Key key)
        {
            if (SettingsBlob.KeyComboIsPressed(SettingsBlob.Shortcuts["PickColor"]))
            {
                _pickColor = true;
            }
        }

        private void PickColor(System.Drawing.Color col)
        {
            NameTextBox.Text = $"Name: {ColorNamer.GetClosestColorName(col.To32Bit())}";
            RGBTextBox.Text = $"RGB: {col.R}, {col.G}, {col.B}";
            //Converts to hex and removes first 2 chars (Both ff, for alpha 255)
            HexTextBox.Text = "Hex: " + col.To32Bit().ToString("X")[2..^0];
        }
    }
}
