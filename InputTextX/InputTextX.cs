using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Rainmeter;

namespace InputTextX
{
    public class InputTextXForm : Form
    {
        public TextBox InputBox;

        public InputTextXForm()
        {
            // Use manual positioning.
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.BackColor = Color.White; // Will be overridden by SolidColor.
            this.Opacity = 1.0; // Fully opaque.
            this.Width = 300;
            this.Height = 50;
            // Default TopMost; will be updated by InputTopMost.
            this.TopMost = false;
            this.ShowInTaskbar = false;

            InputBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12),
                // Default border style; can be updated based on Border option.
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = HorizontalAlignment.Left,
                UseSystemPasswordChar = false
            };
            this.Controls.Add(InputBox);
        }
    }

    public class Measure
    {
        public API _api;  // Made public so helper methods can access if needed.
        private InputTextXForm _inputForm;
        private string _initialText;
        private string _onTextChangedAction;
        private string _onDismissAction;
        private string _onInvalidAction;
        private string _onESCAction;
        private string _onEnterAction;

        private int _width = 300;
        private int _height = 50;
        private int _fontSize = 12;
        private Color _fontColor = Color.Black;
        private HorizontalAlignment _stringAlign = HorizontalAlignment.Left;
        private bool _password = false;
        // StringStyle option (Normal, Bold, Italic, BoldItalic)
        private FontStyle _stringStyle = FontStyle.Regular;
        private bool _multiline = false;
        private bool _unfocusDismiss = false;
        private int _inputLimit = 0;
        private string _inputType = "string";
        private string _allowedChars = "";
        private bool _initialSet = false;

        // FontFace option (e.g., "Ubuntu")
        private string _fontFace = "Segoe UI";
        // X and Y offsets.
        private int _offsetX = 0;
        private int _offsetY = 0;
        // SolidColor for background.
        private Color _solidColor = Color.White;
        // InputTopMost option:
        // "1" forces the input field to be topmost;
        // "0" forces it not to be topmost;
        // empty means do not override.
        private string _inputTopMost = "";
        // TopMostOffsetY to adjust Y when InputTopMost is "1"
        private int _topMostOffsetY = 0;
        // Border option: "1" shows border, "0" hides border.
        private string _border = "1";
        // Flag to compute position only once.
        private bool _positionSet = false;

        // Import SetForegroundWindow as public static.
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public Measure()
        {
            _inputForm = new InputTextXForm();
        }

        public void Dispose()
        {
            if (_inputForm != null)
            {
                _inputForm.Close();
                _inputForm.Dispose();
                _inputForm = null;
            }
        }

        // Helper method to read the Draggable value from the INI file.
        private string ReadDraggableValue()
        {
            try
            {
                string settingsPath = _api.ReplaceVariables("#SETTINGSPATH#");
                string iniFilePath = Path.Combine(settingsPath, "Rainmeter.ini");
                if (File.Exists(iniFilePath))
                {
                    string[] lines = File.ReadAllLines(iniFilePath);
                    bool inSection = false;
                    string currentConfig = _api.ReplaceVariables("#CURRENTCONFIG#");
                    foreach (string line in lines)
                    {
                        string trimmed = line.Trim();
                        if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                        {
                            inSection = string.Equals(trimmed, "[" + currentConfig + "]", StringComparison.OrdinalIgnoreCase);
                        }
                        else if (inSection && trimmed.StartsWith("Draggable=", StringComparison.OrdinalIgnoreCase))
                        {
                            string[] parts = trimmed.Split('=');
                            if (parts.Length > 1)
                            {
                                return parts[1].Trim();
                            }
                        }
                    }
                }
            }
            catch { }
            return "1"; // default draggable value if not found.
        }

        // Public method to restore the draggable state.
        public void RestoreDraggable()
        {
            string draggableValue = ReadDraggableValue();
            _api.Execute("[!Draggable " + draggableValue + "]");
        }

        public void Reload(API api, ref double maxValue)
        {
            _api = api;

            // Read initial text and actions.
            _initialText = _api.ReadString("InitialText", "");
            _onTextChangedAction = _api.ReadString("OnTextChangedAction", "");
            _onDismissAction = _api.ReadString("OnDismissAction", "");
            _onInvalidAction = _api.ReadString("OnInvalidAction", "");
            _onESCAction = _api.ReadString("OnESCAction", "");
            _onEnterAction = _api.ReadString("OnEnterAction", "");

            _width = _api.ReadInt("Width", 300);
            _height = _api.ReadInt("Height", 50);
            _fontSize = _api.ReadInt("FontSize", 12);
            string fontColorString = _api.ReadString("FontColor", "0,0,0");
            _fontColor = ParseColor(fontColorString, Color.Black);

            string alignString = _api.ReadString("StringAlign", "Left").ToLowerInvariant();
            switch (alignString)
            {
                case "center":
                    _stringAlign = HorizontalAlignment.Center;
                    break;
                case "right":
                    _stringAlign = HorizontalAlignment.Right;
                    break;
                default:
                    _stringAlign = HorizontalAlignment.Left;
                    break;
            }

            string passwordStr = _api.ReadString("Password", "0");
            _password = (passwordStr.Equals("1") || passwordStr.Equals("true", StringComparison.OrdinalIgnoreCase));

            // Read StringStyle option.
            string styleStr = _api.ReadString("StringStyle", "Normal").ToLowerInvariant();
            switch (styleStr)
            {
                case "bold":
                    _stringStyle = FontStyle.Bold;
                    break;
                case "italic":
                    _stringStyle = FontStyle.Italic;
                    break;
                case "bolditalic":
                    _stringStyle = FontStyle.Bold | FontStyle.Italic;
                    break;
                default:
                    _stringStyle = FontStyle.Regular;
                    break;
            }

            // Read FontFace option.
            _fontFace = _api.ReadString("FontFace", "Segoe UI");

            // Read offset values.
            _offsetX = _api.ReadInt("X", 0);
            _offsetY = _api.ReadInt("Y", 0);

            // Read TopMostOffsetY option.
            _topMostOffsetY = _api.ReadInt("TopMostOffsetY", 0);

            // Read SolidColor option ("R,G,B"). Default is white.
            string solidColorString = _api.ReadString("SolidColor", "255,255,255");
            _solidColor = ParseColor(solidColorString, Color.White);

            // Read InputTopMost option.
            _inputTopMost = _api.ReadString("InputTopMost", "");

            // Read Border option.
            _border = _api.ReadString("Border", "1");

            _multiline = _api.ReadInt("Multiline", 0) == 1;
            _inputForm.InputBox.Multiline = _multiline;
            _unfocusDismiss = _api.ReadInt("UnFocusDismiss", 0) == 1;
            _inputForm.Deactivate -= InputForm_Deactivate;
            _inputForm.Deactivate += InputForm_Deactivate;

            _inputLimit = _api.ReadInt("InputLimit", 0);
            _inputForm.InputBox.MaxLength = (_inputLimit > 0) ? _inputLimit : 0;

            _inputType = _api.ReadString("InputType", "String").ToLowerInvariant();
            if (_inputType == "custom")
            {
                _allowedChars = _api.ReadString("AllowedChars", "");
            }

            _inputForm.Width = _width;
            _inputForm.Height = _height;

            // Set solid background color.
            _inputForm.BackColor = _solidColor;
            _inputForm.InputBox.BackColor = _solidColor;
            _inputForm.InputBox.BorderStyle = (_border == "1") ? BorderStyle.FixedSingle : BorderStyle.None;
            _inputForm.Opacity = 1.0;

            // Set initial text only if not already set.
            if (!_initialSet && !string.IsNullOrEmpty(_initialText) && string.IsNullOrEmpty(_inputForm.InputBox.Text))
            {
                _inputForm.InputBox.Text = _initialText;
                _initialSet = true;
            }

            // --- Font Creation using FontFace ---
            Font newFont = null;
            try
            {
                string basePath = _api.ReplaceVariables("#@#");
                string fontsFolder = Path.Combine(basePath, "Fonts");

                string candidate = "";
                if (_stringStyle == FontStyle.Regular)
                {
                    candidate = _fontFace + ".ttf";
                    if (!File.Exists(Path.Combine(fontsFolder, candidate)))
                        candidate = _fontFace + "-Regular.ttf";
                    if (!File.Exists(Path.Combine(fontsFolder, candidate)))
                    {
                        candidate = _fontFace + ".otf";
                        if (!File.Exists(Path.Combine(fontsFolder, candidate)))
                            candidate = _fontFace + "-Regular.otf";
                    }
                }
                else if (_stringStyle == FontStyle.Bold)
                {
                    candidate = _fontFace + "-Bold.ttf";
                    if (!File.Exists(Path.Combine(fontsFolder, candidate)))
                        candidate = _fontFace + "-Bold.otf";
                }
                else if (_stringStyle == FontStyle.Italic)
                {
                    candidate = _fontFace + "-Italic.ttf";
                    if (!File.Exists(Path.Combine(fontsFolder, candidate)))
                        candidate = _fontFace + "-Italic.otf";
                }
                else if (_stringStyle == (FontStyle.Bold | FontStyle.Italic))
                {
                    candidate = _fontFace + "-BoldItalic.ttf";
                    if (!File.Exists(Path.Combine(fontsFolder, candidate)))
                        candidate = _fontFace + "-BoldItalic.otf";
                }

                string fontFilePath = Path.Combine(fontsFolder, candidate);
                if (File.Exists(fontFilePath))
                {
                    PrivateFontCollection pfc = new PrivateFontCollection();
                    pfc.AddFontFile(fontFilePath);
                    newFont = new Font(pfc.Families[0], _fontSize, _stringStyle);
                }
                else
                {
                    try
                    {
                        newFont = new Font(_fontFace, _fontSize, _stringStyle);
                    }
                    catch
                    {
                        newFont = new Font("Segoe UI", _fontSize, _stringStyle);
                    }
                }
            }
            catch
            {
                newFont = new Font("Segoe UI", _fontSize, _stringStyle);
            }
            _inputForm.InputBox.Font = newFont;
            // --- End Font Creation ---

            _inputForm.InputBox.ForeColor = _fontColor;
            _inputForm.InputBox.TextAlign = _stringAlign;

            if (_password)
            {
                _inputForm.InputBox.UseSystemPasswordChar = false;
                _inputForm.InputBox.PasswordChar = '*';
            }
            else
            {
                _inputForm.InputBox.PasswordChar = '\0';
            }

            _inputForm.InputBox.TextChanged -= InputBox_TextChanged;
            _inputForm.InputBox.TextChanged += InputBox_TextChanged;
            _inputForm.InputBox.KeyPress -= InputBox_KeyPress;
            _inputForm.InputBox.KeyPress += InputBox_KeyPress;
            _inputForm.InputBox.KeyDown -= InputBox_KeyDown;
            _inputForm.InputBox.KeyDown += InputBox_KeyDown;

            // --- TopMost handling ---
            if (!string.IsNullOrEmpty(_inputTopMost))
            {
                if (_inputTopMost == "1")
                {
                    _inputForm.TopMost = true;
                }
                else if (_inputTopMost == "0")
                {
                    _inputForm.TopMost = false;
                    _inputForm.BringToFront();
                    _inputForm.Activate();
                    SetForegroundWindow(_inputForm.Handle);
                }
            }
        }

        private Color ParseColor(string colorString, Color defaultColor)
        {
            try
            {
                string[] parts = colorString.Split(',');
                if (parts.Length >= 3)
                {
                    int r = int.Parse(parts[0]);
                    int g = int.Parse(parts[1]);
                    int b = int.Parse(parts[2]);
                    return Color.FromArgb(r, g, b);
                }
            }
            catch { }
            return defaultColor;
        }

        private void InputBox_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_onTextChangedAction))
            {
                _api.Execute(_onTextChangedAction);
            }
        }

        private void InputBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
                return;
            TextBox tb = sender as TextBox;
            bool valid = true;
            switch (_inputType)
            {
                case "integer":
                    if (!char.IsDigit(e.KeyChar) && e.KeyChar != '-')
                        valid = false;
                    else if (e.KeyChar == '-' && tb.SelectionStart != 0)
                        valid = false;
                    break;
                case "float":
                    if (!char.IsDigit(e.KeyChar) && e.KeyChar != '-' && e.KeyChar != '.')
                        valid = false;
                    else if (e.KeyChar == '-' && (tb.SelectionStart != 0 || tb.Text.Contains("-")))
                        valid = false;
                    else if (e.KeyChar == '.' && tb.Text.Contains("."))
                        valid = false;
                    break;
                case "letters":
                    if (!char.IsLetter(e.KeyChar))
                        valid = false;
                    break;
                case "alphanumeric":
                    if (!char.IsLetterOrDigit(e.KeyChar))
                        valid = false;
                    break;
                case "hexadecimal":
                    if (!char.IsDigit(e.KeyChar) && !"abcdefABCDEF".Contains(e.KeyChar.ToString()))
                        valid = false;
                    break;
                case "email":
                    if (!char.IsLetterOrDigit(e.KeyChar) && e.KeyChar != '@' && e.KeyChar != '.' && e.KeyChar != '_' && e.KeyChar != '-')
                        valid = false;
                    break;
                case "custom":
                    if (!string.IsNullOrEmpty(_allowedChars) && !_allowedChars.Contains(e.KeyChar.ToString()))
                        valid = false;
                    break;
                default:
                    break;
            }
            if (!valid)
            {
                e.Handled = true;
                if (!string.IsNullOrEmpty(_onInvalidAction))
                {
                    _api.Execute(_onInvalidAction);
                }
            }
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (!string.IsNullOrEmpty(_onESCAction))
                {
                    _api.Execute(_onESCAction);
                }
                ResetState();
                e.Handled = true;
                return;
            }
            if (e.KeyCode == Keys.Enter)
            {
                if (!_multiline || (_multiline && e.Control))
                {
                    if (!string.IsNullOrEmpty(_onEnterAction))
                    {
                        _api.Execute(_onEnterAction);
                    }
                    ResetState();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private void InputForm_Deactivate(object sender, EventArgs e)
        {
            IntPtr fg = GetForegroundWindow();
            if (fg == _inputForm.Handle || fg == _inputForm.InputBox.Handle)
            {
                return;
            }
            if (_unfocusDismiss && _inputForm.Visible)
            {
                _inputForm.Hide();
                if (!string.IsNullOrEmpty(_onDismissAction))
                {
                    _api.Execute(_onDismissAction);
                }
                ResetState();
            }
        }

        public void ResetFull()
        {
            if (_inputForm != null)
            {
                _inputForm.Close();
                _inputForm.Dispose();
            }
            _inputForm = new InputTextXForm();
            _positionSet = false;
            _initialSet = false;
        }

        public void ResetState()
        {
            ResetFull();
        }

        public void StartForm()
        {
            if (!_positionSet)
            {
                try
                {
                    int skinX = int.Parse(_api.ReplaceVariables("#CURRENTCONFIGX#"));
                    int skinY = int.Parse(_api.ReplaceVariables("#CURRENTCONFIGY#"));
                    int finalY = skinY + _offsetY;
                    if (_inputTopMost == "1")
                    {
                        finalY += _topMostOffsetY;
                    }
                    _inputForm.Location = new Point(skinX + _offsetX, finalY);
                    _positionSet = true;
                }
                catch { }
            }
            if (!string.IsNullOrEmpty(_inputTopMost))
            {
                if (_inputTopMost == "1")
                {
                    _inputForm.TopMost = true;
                }
                else if (_inputTopMost == "0")
                {
                    _inputForm.TopMost = false;
                    _inputForm.BringToFront();
                    _inputForm.Activate();
                    SetForegroundWindow(_inputForm.Handle);
                }
            }
            // On start, disable dragging.
            _api.Execute("[!Draggable 0]");
            if (!_inputForm.Visible)
            {
                _inputForm.Show();
            }
        }

        public double Update()
        {
            return 0.0;
        }

        public string GetInputText()
        {
            return _inputForm?.InputBox.Text ?? "";
        }
    }

    public static class Plugin
    {
        private static IntPtr stringBuffer = IntPtr.Zero;

        [DllExport]
        public static void Initialize(ref IntPtr data, IntPtr rm)
        {
            data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure()));
        }

        [DllExport]
        public static void Finalize(IntPtr data)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            measure.Dispose();
            GCHandle.FromIntPtr(data).Free();
            if (stringBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(stringBuffer);
                stringBuffer = IntPtr.Zero;
            }
        }

        [DllExport]
        public static void Reload(IntPtr data, IntPtr rm, ref double maxValue)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            measure.Reload(new API(rm), ref maxValue);
        }

        [DllExport]
        public static double Update(IntPtr data)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            return measure.Update();
        }

        [DllExport]
        public static IntPtr GetString(IntPtr data)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            string result = measure.GetInputText();
            if (stringBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(stringBuffer);
            }
            stringBuffer = Marshal.StringToHGlobalUni(result);
            return stringBuffer;
        }

        [DllExport]
        public static void ExecuteBang(IntPtr data, [MarshalAs(UnmanagedType.LPWStr)] string command)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            if (string.Equals(command, "Start", StringComparison.OrdinalIgnoreCase))
            {
                measure.StartForm();
            }
            else if (string.Equals(command, "Stop", StringComparison.OrdinalIgnoreCase))
            {
                measure.ResetState();
                measure.RestoreDraggable();
            }
        }
    }
}
