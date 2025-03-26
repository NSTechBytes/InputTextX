using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Rainmeter;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace InputTextX
{


    public class InputOverlay : Form
    {
        internal static class NativeMethods
        {
            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool SetWindowPos(
                IntPtr hWnd, IntPtr hWndInsertAfter,
                int X, int Y, int cx, int cy, uint uFlags);
        }
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOACTIVATE = 0x0010;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        public event EventHandler<string> TextSubmitted;
        private TextBox textBox;
        private API _api;
        private readonly int unFocusDismiss;
        private int logging; // local logging flag

        // Appearance and behavior.
        private int inputWidth;
        private int inputHeight;
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

        // Text parameters.
        private int _inputLimit;
        private string _defaultValue;
        private string _fontFace;
        private PrivateFontCollection _pfc = null;

        // Final position.
        private int posX;
        private int posY;

        // Border and numeric range.
        private int allowBorder;
        private Color _borderColor;
        private int _borderThickness;
        private double _minValue, _maxValue;

        // TopMost parameter.
        private int _topMost;

        // Skin window handle.
        private IntPtr skinWindow;

        // SolidColor for the input box background.
        private Color solidColor;

        // Flag to prevent duplicate dismiss actions.
        private bool _isClosing = false;

        // Override CreateParams to reduce flickering.
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }

        public InputOverlay(
            API api,
            int unFocusDismiss,
            int inputWidth, int inputHeight,
            Color inputFontColor, float inputFontSize,
            HorizontalAlignment textAlign, bool isPassword, FontStyle textFontStyle, bool multiline,
            int allowScroll, InputTypeOption inputType, string allowedChars,
            string onDismissAction, string onEnterAction, string onESCAction, string onInvalidAction,
            int inputLimit, string defaultValue, string fontFace,
            int posX, int posY,
            int allowBorder,
            Color borderColor, int borderThickness,
            double minValue, double maxValue,
            int topMost,
            IntPtr skinWindow,
            Color solidColor,
            int logging) // new parameter for logging
        {
            _api = api;
            this.unFocusDismiss = unFocusDismiss;
            this.inputWidth = inputWidth;
            this.inputHeight = inputHeight;
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

            _inputLimit = inputLimit;
            _defaultValue = defaultValue;
            _fontFace = fontFace;
            this.posX = posX;
            this.posY = posY;

            this.allowBorder = allowBorder;
            _borderColor = borderColor;
            _borderThickness = borderThickness;
            _minValue = minValue;
            _maxValue = maxValue;

            _topMost = topMost;
            this.skinWindow = skinWindow;
            this.solidColor = solidColor;
            this.logging = logging;

            if (skinWindow != IntPtr.Zero)
            {
                NativeMethods.EnableWindow(skinWindow, false);
            }

            // Pre-configure the form: enable double buffering and suspend layout.
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            this.SuspendLayout();

            // Set form properties and position.
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.Bounds = new Rectangle(posX, posY, inputWidth, inputHeight);
            this.Opacity = 0; // Start fully transparent.

            if (skinWindow != IntPtr.Zero)
            {
                NativeMethods.SetWindowPos(skinWindow, this.Handle, 0, 0, 0, 0,
                    SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
            }

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
                    BackColor = solidColor,
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
                    BackColor = solidColor,
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
            textBox.TextChanged += (sender, e) =>
            {
                TextSubmitted?.Invoke(this, textBox.Text);
            };

            this.ResumeLayout(false);
        }

        // Helper method to escape double quotes in user input.
        private string EscapeCommandArgument(string input)
        {
            if (input == null)
                return string.Empty;
            return input.Replace("\"", "\\\"");
        }

        // New helper: case-insensitive replacement for "$UserInput$"
        private string ReplacePlaceholder(string command, string replacement)
        {
            if (string.IsNullOrEmpty(command))
                return command;
            return Regex.Replace(command, Regex.Escape("$UserInput$"), replacement, RegexOptions.IgnoreCase);
        }

        private bool ValidateChar(char ch)
        {
            if (char.IsControl(ch))
                return true;
            switch (inputType)
            {
                case InputTypeOption.String:
                    return true;
                case InputTypeOption.Integer:
                    if (char.IsDigit(ch))
                        return true;
                    if (ch == '-' && textBox.SelectionStart == 0 && !textBox.Text.Contains("-"))
                        return true;
                    return false;
                case InputTypeOption.Float:
                    if (char.IsDigit(ch))
                        return true;
                    if (ch == '-' && textBox.SelectionStart == 0 && !textBox.Text.Contains("-"))
                        return true;
                    if (ch == '.' && !textBox.Text.Contains("."))
                        return true;
                    return false;
                case InputTypeOption.Letters:
                    return char.IsLetter(ch);
                case InputTypeOption.Alphanumeric:
                    return char.IsLetterOrDigit(ch);
                case InputTypeOption.Hexadecimal:
                    if (char.IsDigit(ch))
                        return true;
                    ch = char.ToUpper(ch);
                    return (ch >= 'A' && ch <= 'F');
                case InputTypeOption.Email:
                    if (char.IsLetterOrDigit(ch))
                        return true;
                    if ("@.-_+".IndexOf(ch) >= 0)
                        return true;
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
                if (logging == 1)
                    _api.Log(API.LogType.Notice, $"Invalid character detected: {e.KeyChar}");
                try
                {
                    _api.Execute(_onInvalidAction);
                }
                catch (Exception ex)
                {
                    if (logging == 1)
                        _api.Log(API.LogType.Error, "Error executing OnInvalidAction: " + ex.Message);
                }
                e.Handled = true;
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                // Use ReplacePlaceholder for case-insensitive replacement.
                string command = ReplacePlaceholder(_onESCAction, EscapeCommandArgument(textBox.Text));
                if (!string.IsNullOrWhiteSpace(command))
                {
                    if (logging == 1)
                        _api.Log(API.LogType.Notice, "Final ESC command: " + command);
                    try { _api.Execute(command); }
                    catch (Exception ex)
                    {
                        if (logging == 1)
                            _api.Log(API.LogType.Error, "Error executing ESC command: " + ex.Message);
                    }
                }
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
                            if (logging == 1)
                                _api.Log(API.LogType.Notice, "Integer value out of range.");
                            try { _api.Execute(_onInvalidAction); }
                            catch (Exception ex)
                            {
                                if (logging == 1)
                                    _api.Log(API.LogType.Error, "Error executing OnInvalidAction for integer: " + ex.Message);
                            }
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
                            if (logging == 1)
                                _api.Log(API.LogType.Notice, "Float value out of range.");
                            try { _api.Execute(_onInvalidAction); }
                            catch (Exception ex)
                            {
                                if (logging == 1)
                                    _api.Log(API.LogType.Error, "Error executing OnInvalidAction for float: " + ex.Message);
                            }
                            e.Handled = true;
                            return;
                        }
                    }
                }
                string enterCommand = ReplacePlaceholder(_onEnterAction, EscapeCommandArgument(textBox.Text));
                if (!string.IsNullOrWhiteSpace(enterCommand))
                {
                    if (logging == 1)
                        _api.Log(API.LogType.Notice, "Final Enter command: " + enterCommand);
                    try { _api.Execute(enterCommand); }
                    catch (Exception ex)
                    {
                        if (logging == 1)
                            _api.Log(API.LogType.Error, "Error executing Enter command: " + ex.Message);
                    }
                }
                e.Handled = true;
                CloseInput();
            }
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            if (!_isClosing && unFocusDismiss == 1)
            {
                string command = ReplacePlaceholder(_onDismissAction, EscapeCommandArgument(textBox.Text));
                if (!string.IsNullOrWhiteSpace(command))
                {
                    if (logging == 1)
                        _api.Log(API.LogType.Notice, "Final Dismiss command: " + command);
                    try { _api.Execute(command); }
                    catch (Exception ex)
                    {
                        if (logging == 1)
                            _api.Log(API.LogType.Error, "Error executing Dismiss command: " + ex.Message);
                    }
                }
                CloseInput();
            }
        }

        private void CloseInput()
        {
            if (_isClosing)
                return;
            _isClosing = true;
            if (logging == 1)
                _api.Log(API.LogType.Notice, "Closing input overlay.");
            if (skinWindow != IntPtr.Zero)
            {
                NativeMethods.EnableWindow(skinWindow, true);
            }
            try { this.Close(); }
            catch (Exception ex)
            {
                if (logging == 1)
                    _api.Log(API.LogType.Error, "Error closing input overlay: " + ex.Message);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.Opacity = 1; 
        }
    }
}
