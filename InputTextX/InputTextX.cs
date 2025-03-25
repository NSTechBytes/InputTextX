using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Text.RegularExpressions;
using Rainmeter;

namespace InputTextX
{
    // Define an enumeration for the allowed input types.
    public enum InputTypeOption
    {
        String,
        Integer,
        Float,
        Letters,
        Alphanumeric,
        Hexadecimal,
        Email,
        Custom
    }

    internal class Measure
    {
        private API _api;
        private static Thread inputThread;
        private static InputOverlay inputOverlay;
        private string currentText = "";
        private int unFocusDismiss = 1; // 1 = dismiss on unfocus; 0 = do not dismiss

        // Existing properties read from the measure.
        private int inputWidth = 300;
        private int inputHeight = 40;
        private Color solidColor = Color.White;
        private Color fontColor = Color.Black;
        private float fontSize = 12f;

        // New appearance properties.
        private HorizontalAlignment textAlign = HorizontalAlignment.Center;
        private bool isPassword = false;
        private FontStyle fontStyleParam = FontStyle.Regular;
        private string fontFace = "Segoe UI"; // default or custom font family name

        // New behavioral properties.
        private bool multiline = false;
        private int allowScroll = 0; // 1 = allow scrollbars in multiline
        private int inputLimit = 0;  // 0 means no limit
        private string defaultValue = "";

        // New input filtering properties.
        private InputTypeOption inputType = InputTypeOption.String;
        private string allowedChars = ""; // used if inputType==Custom

        // New action parameters.
        private string onDismissAction = "";
        private string onEnterAction = "";
        private string onESCAction = "";
        private string onInvalidAction = "";

        // New offset parameters.
        private int offsetX = 0;
        private int offsetY = 0;

        // New border parameters.
        private int allowBorder = 0;
        private Color borderColor = Color.Black;
        private int borderThickness = 2;

        // New numeric range parameters.
        private double minValue = double.MinValue;
        private double maxValue = double.MaxValue;

        // New TopMost parameter.
        private int topMost = 1;

        internal void Reload(API api, ref double maxValueOut)
        {
            _api = api;
            _api.Log(API.LogType.Notice, "Reloading measure...");
            unFocusDismiss = api.ReadInt("UnFocusDismiss", 1);

            inputWidth = api.ReadInt("W", 300);
            inputHeight = api.ReadInt("H", 40);
            string solidColorStr = api.ReadString("SolidColor", "255,255,255");
            solidColor = ParseColor(solidColorStr, Color.White);
            string fontColorStr = api.ReadString("FontColor", "0,0,0");
            fontColor = ParseColor(fontColorStr, Color.Black);
            fontSize = api.ReadInt("FontSize", 12);

            string alignStr = api.ReadString("Align", "Center");
            switch (alignStr.ToLower())
            {
                case "left": textAlign = HorizontalAlignment.Left; break;
                case "right": textAlign = HorizontalAlignment.Right; break;
                default: textAlign = HorizontalAlignment.Center; break;
            }

            isPassword = api.ReadInt("Password", 0) == 1;
            string styleStr = api.ReadString("FontStyle", "Normal");
            switch (styleStr.ToLower())
            {
                case "bolditalic": fontStyleParam = FontStyle.Bold | FontStyle.Italic; break;
                case "bold": fontStyleParam = FontStyle.Bold; break;
                case "italic": fontStyleParam = FontStyle.Italic; break;
                default: fontStyleParam = FontStyle.Regular; break;
            }
            fontFace = api.ReadString("FontFace", "Segoe UI");
            if (!Path.IsPathRooted(fontFace))
                fontFace = GetFontPathForFamily(fontFace, fontStyleParam);

            multiline = api.ReadInt("Multiline", 0) == 1;
            allowScroll = api.ReadInt("AllowScroll", 0);
            inputLimit = api.ReadInt("InputLimit", 0);
            defaultValue = api.ReadString("DefaultValue", "");

            string inputTypeStr = api.ReadString("InputType", "String");
            if (!Enum.TryParse(inputTypeStr, true, out inputType))
                inputType = InputTypeOption.String;
            allowedChars = api.ReadString("AllowedChars", "");

            onDismissAction = api.ReadString("OnDismissAction", "");
            onEnterAction = api.ReadString("OnEnterAction", "");
            onESCAction = api.ReadString("OnESCAction", "");
            onInvalidAction = api.ReadString("InValidAction", "");

            offsetX = api.ReadInt("X", 0);
            offsetY = api.ReadInt("Y", 0);

            allowBorder = api.ReadInt("AllowBorder", 0);
            borderColor = ParseColor(api.ReadString("BorderColor", "0,0,0"), Color.Black);
            borderThickness = api.ReadInt("BorderThickness", 2);

            minValue = api.ReadDouble("MinValue", double.MinValue);
            maxValue = api.ReadDouble("MaxValue", double.MaxValue);

            topMost = api.ReadInt("TopMost", 1);

            _api.Log(API.LogType.Notice, $"Reload complete. Input dimensions: {inputWidth}x{inputHeight}");
        }

        internal double Update() => currentText.Length;
        internal string GetString() => currentText;

        internal void ExecuteCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
                return;

            if (command.Equals("Start", StringComparison.InvariantCultureIgnoreCase))
            {
                _api.Log(API.LogType.Notice, "ExecuteCommand: Starting input overlay...");
                if (inputThread == null || !inputThread.IsAlive)
                {
                    inputThread = new Thread(() =>
                    {
                        int baseX = int.Parse(_api.ReplaceVariables("#CURRENTCONFIGX#"));
                        int baseY = int.Parse(_api.ReplaceVariables("#CURRENTCONFIGY#"));
                        _api.Log(API.LogType.Notice, $"Base coordinates: {baseX},{baseY}");
                        _api.Log(API.LogType.Notice, $"Offset: {offsetX},{offsetY}");

                        string overlayWidthStr = _api.ReplaceVariables("#CURRENTCONFIGWIDTH#");
                        string overlayHeightStr = _api.ReplaceVariables("#CURRENTCONFIGHEIGHT#");
                        int overlayWidth = 400, overlayHeight = 300;
                        int.TryParse(overlayWidthStr, out overlayWidth);
                        int.TryParse(overlayHeightStr, out overlayHeight);

                        int overlayX = baseX;
                        int overlayY = baseY;
                        _api.Log(API.LogType.Notice, $"Overlay bounds: {overlayX},{overlayY} {overlayWidth}x{overlayHeight}");

                        Action<string> execCallback = (action) =>
                        {
                            if (!string.IsNullOrEmpty(action))
                                _api.Log(API.LogType.Notice, "Executing action: " + action);
                        };

                        inputOverlay = new InputOverlay(
                            unFocusDismiss,
                            overlayX, overlayY, overlayWidth, overlayHeight,
                            inputWidth, inputHeight, solidColor, fontColor, fontSize,
                            textAlign, isPassword, fontStyleParam, multiline,
                            allowScroll, inputType, allowedChars,
                            onDismissAction, onEnterAction, onESCAction, onInvalidAction,
                            execCallback,
                            inputLimit, defaultValue, fontFace,
                            offsetX, offsetY,
                            allowBorder,
                            borderColor, borderThickness,
                            minValue, maxValue,
                            topMost);
                        inputOverlay.TextSubmitted += (s, text) =>
                        {
                            currentText = text;
                            _api.Log(API.LogType.Notice, "Text submitted: " + text);
                        };
                        Application.Run(inputOverlay);
                    });
                    inputThread.SetApartmentState(ApartmentState.STA);
                    inputThread.IsBackground = true;
                    inputThread.Start();
                }
            }
        }

        internal void Unload()
        {
            try
            {
                if (inputOverlay != null && !inputOverlay.IsDisposed)
                    inputOverlay.Invoke(new Action(() => inputOverlay.Close()));
            }
            catch (Exception ex)
            {
                _api.Log(API.LogType.Notice, "Error during Unload: " + ex.Message);
            }
        }

        private Color ParseColor(string colorStr, Color defaultColor)
        {
            try
            {
                string[] parts = colorStr.Split(',');
                if (parts.Length >= 3)
                {
                    int r = int.Parse(parts[0].Trim());
                    int g = int.Parse(parts[1].Trim());
                    int b = int.Parse(parts[2].Trim());
                    return Color.FromArgb(r, g, b);
                }
            }
            catch { }
            return defaultColor;
        }

        private string GetFontPathForFamily(string fontFamily, FontStyle style)
        {
            string fontsFolder = Path.Combine(_api.ReplaceVariables("#@#"), "Fonts");
            if (!Directory.Exists(fontsFolder))
                return fontFamily;

            var files = new System.Collections.Generic.List<string>();
            files.AddRange(Directory.GetFiles(fontsFolder, "*.ttf"));
            files.AddRange(Directory.GetFiles(fontsFolder, "*.otf"));

            var candidates = new System.Collections.Generic.List<string>();
            foreach (string file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                if (fileName.StartsWith(fontFamily, StringComparison.OrdinalIgnoreCase))
                    candidates.Add(file);
            }
            if (candidates.Count == 0)
                return fontFamily;

            string styleStr = "";
            if ((style & FontStyle.Bold) == FontStyle.Bold && (style & FontStyle.Italic) == FontStyle.Italic)
                styleStr = "BoldItalic";
            else if ((style & FontStyle.Bold) == FontStyle.Bold)
                styleStr = "Bold";
            else if ((style & FontStyle.Italic) == FontStyle.Italic)
                styleStr = "Italic";

            if (!string.IsNullOrEmpty(styleStr))
            {
                foreach (string candidate in candidates)
                {
                    string candidateName = Path.GetFileNameWithoutExtension(candidate);
                    if (candidateName.IndexOf(styleStr, StringComparison.OrdinalIgnoreCase) >= 0)
                        return candidate;
                }
            }
            foreach (string candidate in candidates)
            {
                string candidateName = Path.GetFileNameWithoutExtension(candidate);
                if (candidateName.IndexOf("Regular", StringComparison.OrdinalIgnoreCase) >= 0)
                    return candidate;
            }
            return candidates[0];
        }
    }

    public class InputOverlay : Form
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOACTIVATE = 0x0010;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        public event EventHandler<string> TextSubmitted;
        private TextBox textBox;
        private Form overlayForm;
        private readonly int unFocusDismiss;

        // Appearance and behavior.
        private int inputWidth;
        private int inputHeight;
        private Color inputBackColor;
        private Color inputFontColor;
        private float inputFontSize;
        private HorizontalAlignment textAlign;
        private bool isPassword;
        private FontStyle textFontStyle;
        private bool multiline;
        private int allowScroll;
        private InputTypeOption inputType;
        private string allowedChars;

        // Action parameters.
        private string _onDismissAction;
        private string _onEnterAction;
        private string _onESCAction;
        private string _onInvalidAction;
        private Action<string> _executeActionCallback;

        // Text parameters.
        private int _inputLimit;
        private string _defaultValue;
        private string _fontFace;
        private PrivateFontCollection _pfc = null;

        // Offset parameters.
        private int _offsetX;
        private int _offsetY;

        // Border and numeric range.
        private int allowBorder;
        private Color _borderColor;
        private int _borderThickness;
        private double _minValue, _maxValue;

        // TopMost parameter.
        private int _topMost;

        public InputOverlay(
            int unFocusDismiss,
            int overlayX, int overlayY, int overlayWidth, int overlayHeight,
            int inputWidth, int inputHeight, Color inputBackColor, Color inputFontColor, float inputFontSize,
            HorizontalAlignment textAlign, bool isPassword, FontStyle textFontStyle, bool multiline,
            int allowScroll, InputTypeOption inputType, string allowedChars,
            string onDismissAction, string onEnterAction, string onESCAction, string onInvalidAction,
            Action<string> executeActionCallback,
            int inputLimit, string defaultValue, string fontFace,
            int offsetX, int offsetY,
            int allowBorder,
            Color borderColor, int borderThickness,
            double minValue, double maxValue,
            int topMost)
        {
            this.unFocusDismiss = unFocusDismiss;
            this.inputWidth = inputWidth;
            this.inputHeight = inputHeight;
            this.inputBackColor = inputBackColor;
            this.inputFontColor = inputFontColor;
            this.inputFontSize = inputFontSize;
            this.textAlign = textAlign;
            this.isPassword = isPassword;
            this.textFontStyle = textFontStyle;
            this.multiline = multiline;
            this.allowScroll = allowScroll;
            this.inputType = inputType;
            this.allowedChars = allowedChars;

            _onDismissAction = onDismissAction;
            _onEnterAction = onEnterAction;
            _onESCAction = onESCAction;
            _onInvalidAction = onInvalidAction;
            _executeActionCallback = executeActionCallback;

            _inputLimit = inputLimit;
            _defaultValue = defaultValue;
            _fontFace = fontFace;
            _offsetX = offsetX;
            _offsetY = offsetY;

            this.allowBorder = allowBorder;
            _borderColor = borderColor;
            _borderThickness = borderThickness;
            _minValue = minValue;
            _maxValue = maxValue;

            _topMost = topMost;

            // Create overlay form.
            overlayForm = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
                BackColor = Color.Black,
                Opacity = 0.01,
                TopMost = (_topMost == 1),
                Bounds = new Rectangle(overlayX, overlayY, overlayWidth, overlayHeight)
            };

            if (unFocusDismiss == 0)
            {
                overlayForm.MouseDown += (s, e) =>
                {
                    this.Activate();
                    if (textBox != null)
                        textBox.Focus();
                };
            }

            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;
            this.ShowInTaskbar = false;
            this.TopMost = (_topMost == 1);
            int inputX = overlayX + _offsetX;
            int inputY = overlayY + _offsetY;
            this.Bounds = new Rectangle(inputX, inputY, inputWidth, inputHeight);

            Control inputControl = null;
            if (allowBorder == 1 && _borderThickness > 0)
            {
                Panel borderPanel = new Panel
                {
                    BackColor = _borderColor,
                    Location = new Point(0, 0),
                    Size = new Size(inputWidth, inputHeight),
                    Padding = new Padding(_borderThickness)
                };

                textBox = new TextBox
                {
                    BorderStyle = BorderStyle.None,
                    Dock = DockStyle.Fill,
                    ForeColor = inputFontColor,
                    TextAlign = textAlign,
                    BackColor = inputBackColor,
                    UseSystemPasswordChar = isPassword,
                    Multiline = multiline,
                    AutoSize = false,
                    ScrollBars = (multiline && allowScroll == 1) ? ScrollBars.Vertical : ScrollBars.None
                };
                if (_inputLimit > 0)
                    textBox.MaxLength = _inputLimit;
                if (!string.IsNullOrEmpty(_defaultValue))
                    textBox.Text = _defaultValue;
                borderPanel.Controls.Add(textBox);
                inputControl = borderPanel;
            }
            else
            {
                textBox = new TextBox
                {
                    BorderStyle = BorderStyle.None,
                    ForeColor = inputFontColor,
                    TextAlign = textAlign,
                    BackColor = inputBackColor,
                    UseSystemPasswordChar = isPassword,
                    Multiline = multiline,
                    AutoSize = false,
                    ScrollBars = (multiline && allowScroll == 1) ? ScrollBars.Vertical : ScrollBars.None
                };
                textBox.SetBounds(0, 0, inputWidth, inputHeight);
                if (_inputLimit > 0)
                    textBox.MaxLength = _inputLimit;
                if (!string.IsNullOrEmpty(_defaultValue))
                    textBox.Text = _defaultValue;
                inputControl = textBox;
            }
            this.Controls.Add(inputControl);

            Font fontToUse = null;
            try
            {
                if (!string.IsNullOrEmpty(_fontFace) && File.Exists(_fontFace))
                {
                    _pfc = new PrivateFontCollection();
                    _pfc.AddFontFile(_fontFace);
                    fontToUse = new Font(_pfc.Families[0], inputFontSize, textFontStyle);
                }
                else
                {
                    fontToUse = new Font(_fontFace, inputFontSize, textFontStyle);
                }
            }
            catch
            {
                fontToUse = new Font("Segoe UI", inputFontSize, textFontStyle);
            }
            textBox.Font = fontToUse;

            textBox.KeyDown += TextBox_KeyDown;
            textBox.KeyPress += TextBox_KeyPress;

            overlayForm.Show();
            this.Show();
            this.DesktopLocation = new Point(inputX, inputY);

            // Set window Z-order based on _topMost value.
            if (_topMost == 1)
            {
                SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
                SetWindowPos(overlayForm.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
                _executeActionCallback?.Invoke("InputOverlay: Windows set to TOPMOST.");
            }
            else
            {
                SetWindowPos(this.Handle, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
                SetWindowPos(overlayForm.Handle, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
                _executeActionCallback?.Invoke("InputOverlay: Windows set to NOT TOPMOST.");
            }
        }

        private bool ValidateChar(char ch)
        {
            if (char.IsControl(ch))
                return true;
            switch (inputType)
            {
                case InputTypeOption.String: return true;
                case InputTypeOption.Integer:
                    if (char.IsDigit(ch)) return true;
                    if (ch == '-' && textBox.SelectionStart == 0 && !textBox.Text.Contains("-"))
                        return true;
                    return false;
                case InputTypeOption.Float:
                    if (char.IsDigit(ch)) return true;
                    if (ch == '-' && textBox.SelectionStart == 0 && !textBox.Text.Contains("-"))
                        return true;
                    if (ch == '.' && !textBox.Text.Contains("."))
                        return true;
                    return false;
                case InputTypeOption.Letters: return char.IsLetter(ch);
                case InputTypeOption.Alphanumeric: return char.IsLetterOrDigit(ch);
                case InputTypeOption.Hexadecimal:
                    if (char.IsDigit(ch)) return true;
                    ch = char.ToUpper(ch);
                    return (ch >= 'A' && ch <= 'F');
                case InputTypeOption.Email:
                    if (char.IsLetterOrDigit(ch)) return true;
                    if ("@.-_+".IndexOf(ch) >= 0) return true;
                    return false;
                case InputTypeOption.Custom:
                    if (!string.IsNullOrEmpty(allowedChars))
                        return allowedChars.IndexOf(ch) >= 0;
                    else
                        return true;
                default:
                    return true;
            }
        }

        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!ValidateChar(e.KeyChar))
            {
                _executeActionCallback?.Invoke(_onInvalidAction);
                e.Handled = true;
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                _executeActionCallback?.Invoke(_onESCAction);
                e.Handled = true;
                CloseInput();
                return;
            }

            if (!multiline && e.KeyCode == Keys.Enter ||
                (multiline && e.Control && e.KeyCode == Keys.Enter))
            {
                if (inputType == InputTypeOption.Integer)
                {
                    if (int.TryParse(textBox.Text, out int val))
                    {
                        if (val < _minValue || val > _maxValue)
                        {
                            _executeActionCallback?.Invoke(_onInvalidAction);
                            e.Handled = true;
                            return;
                        }
                    }
                }
                else if (inputType == InputTypeOption.Float)
                {
                    if (double.TryParse(textBox.Text, out double val))
                    {
                        if (val < _minValue || val > _maxValue)
                        {
                            _executeActionCallback?.Invoke(_onInvalidAction);
                            e.Handled = true;
                            return;
                        }
                    }
                }
                _executeActionCallback?.Invoke(_onEnterAction);
                TextSubmitted?.Invoke(this, textBox.Text);
                e.Handled = true;
                CloseInput();
            }
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            if (unFocusDismiss == 1 && overlayForm != null && overlayForm.Visible)
            {
                _executeActionCallback?.Invoke(_onDismissAction);
                CloseInput();
            }
        }

        private void CloseInput()
        {
            try { overlayForm?.Close(); } catch { }
            this.Close();
        }
    }

    public static class Plugin
    {
        private static IntPtr lastStringPtr = IntPtr.Zero;
        [DllExport]
        public static void Initialize(ref IntPtr data, IntPtr rm)
        {
            data = GCHandle.ToIntPtr(GCHandle.Alloc(new Measure()));
        }
        [DllExport]
        public static void Finalize(IntPtr data)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            measure.Unload();
            GCHandle.FromIntPtr(data).Free();
            if (lastStringPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(lastStringPtr);
                lastStringPtr = IntPtr.Zero;
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
            string s = measure.GetString();
            if (lastStringPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(lastStringPtr);
                lastStringPtr = IntPtr.Zero;
            }
            lastStringPtr = Marshal.StringToHGlobalUni(s);
            return lastStringPtr;
        }
        [DllExport]
        public static void ExecuteBang(IntPtr data, [MarshalAs(UnmanagedType.LPWStr)] string args)
        {
            Measure measure = (Measure)GCHandle.FromIntPtr(data).Target;
            measure.ExecuteCommand(args);
        }
    }
}
