using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using Rainmeter;
using static InputTextX.InputOverlay;

namespace InputTextX
{
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
        private int unFocusDismiss = 1;

        // Properties read from the measure.
        private int inputWidth = 300;
        private int inputHeight = 40;
        private Color solidColor = Color.White;   // Input box background.
        private Color fontColor = Color.Black;
        private float fontSize = 12f;
        private HorizontalAlignment textAlign = HorizontalAlignment.Center;
        private bool isPassword = false;
        private FontStyle fontStyleParam = FontStyle.Regular;
        private string fontFace = "Segoe UI";

        // Behavioral properties.
        private bool multiline = false;
        private int allowScroll = 0;
        private int inputLimit = 0;
        private string defaultValue = "";

        // Input filtering.
        private InputTypeOption inputType = InputTypeOption.String;
        private string allowedChars = "";

        // Action parameters.
        private string onDismissAction = "";
        private string onEnterAction = "";
        private string onESCAction = "";
        private string onInvalidAction = "";

        // Offset parameters.
        private int offsetX = 20;
        private int offsetY = 20;

        // Border parameters.
        private int allowBorder = 0;
        private Color borderColor = Color.Black;
        private int borderThickness = 2;

        // Numeric range.
        private double minValue = double.MinValue;
        private double maxValue = double.MaxValue;

        // TopMost parameter.
        private int topMost = 1;

        // Logging flag.
        private int logging = 0;

        // Skin window handle.
        public IntPtr skinWindow = IntPtr.Zero;

        internal void Reload(API api, ref double maxValueOut)
        {
            _api = api;
            if (api.ReadInt("Logging", 0) == 1)
                _api.Log(API.LogType.Notice, "Reloading measure...");

            unFocusDismiss = api.ReadInt("UnFocusDismiss", 1);
            inputWidth = api.ReadInt("W", 300);
            inputHeight = api.ReadInt("H", 40);
            string solidColorStr = api.ReadString("SolidColor", "255,255,255");
            solidColor = ParseColor(solidColorStr, Color.White);
            fontColor = ParseColor(api.ReadString("FontColor", "0,0,0"), Color.Black);
            fontSize = api.ReadInt("FontSize", 12);

            string alignStr = api.ReadString("Align", "Center");
            switch (alignStr.ToLower())
            {
                case "left":
                    textAlign = HorizontalAlignment.Left;
                    break;
                case "right":
                    textAlign = HorizontalAlignment.Right;
                    break;
                default:
                    textAlign = HorizontalAlignment.Center;
                    break;
            }

            isPassword = api.ReadInt("Password", 0) == 1;
            string styleStr = api.ReadString("FontStyle", "Normal");
            switch (styleStr.ToLower())
            {
                case "bolditalic":
                    fontStyleParam = FontStyle.Bold | FontStyle.Italic;
                    break;
                case "bold":
                    fontStyleParam = FontStyle.Bold;
                    break;
                case "italic":
                    fontStyleParam = FontStyle.Italic;
                    break;
                default:
                    fontStyleParam = FontStyle.Regular;
                    break;
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

            offsetX = api.ReadInt("X", 20);
            offsetY = api.ReadInt("Y", 20);

            allowBorder = api.ReadInt("AllowBorder", 0);
            borderColor = ParseColor(api.ReadString("BorderColor", "0,0,0"), Color.Black);
            borderThickness = api.ReadInt("BorderThickness", 2);

            minValue = api.ReadDouble("MinValue", double.MinValue);
            maxValue = api.ReadDouble("MaxValue", double.MaxValue);

            topMost = api.ReadInt("TopMost", 1);
            logging = api.ReadInt("Logging", 0);

            _api.Execute("[!SetOption InputText_X DynamicVariables 1]");

            if (logging == 1)
                _api.Log(API.LogType.Notice, $"Reload complete. Input dimensions: {inputWidth}x{inputHeight}, SolidColor: {solidColor}, TopMost: {topMost}, Logging: {logging}");
        }

        internal double Update() => currentText.Length;
        internal string GetString() => currentText;

        internal void ExecuteCommand(string command)
        {
            if (logging == 1)
                _api.Log(API.LogType.Notice, $"ExecuteCommand received: {command}");
            if (string.IsNullOrEmpty(command))
                return;

            if (command.Equals("Stop", StringComparison.InvariantCultureIgnoreCase))
            {
                if (logging == 1)
                    _api.Log(API.LogType.Notice, "Stop command received.");
                if (inputOverlay != null && !inputOverlay.IsDisposed)
                {
                    inputOverlay.Invoke(new Action(() => inputOverlay.Close()));
                    if (logging == 1)
                        _api.Log(API.LogType.Notice, "Input overlay stopped.");
                }
                else if (logging == 1)
                    _api.Log(API.LogType.Notice, "No active input overlay to stop.");
                return;
            }

            if (command.Equals("Start", StringComparison.InvariantCultureIgnoreCase))
            {
                if (logging == 1)
                    _api.Log(API.LogType.Notice, "Start command received.");
                if (inputThread == null || !inputThread.IsAlive)
                {
                    inputThread = new Thread(() =>
                    {
                        try
                        {
                            int skinX = int.Parse(_api.ReplaceVariables("#CURRENTCONFIGX#"));
                            int skinY = int.Parse(_api.ReplaceVariables("#CURRENTCONFIGY#"));
                            int posX = skinX + offsetX;
                            int posY = skinY + offsetY;
                            if (logging == 1)
                                _api.Log(API.LogType.Notice, $"Calculated position: {posX}, {posY}");

                            // Pass the logging flag into InputOverlay.
                            inputOverlay = new InputOverlay(
                                _api,
                                unFocusDismiss,
                                inputWidth, inputHeight,
                                fontColor, fontSize,
                                textAlign, isPassword, fontStyleParam, multiline,
                                allowScroll, inputType, allowedChars,
                                onDismissAction, onEnterAction, onESCAction, onInvalidAction,
                                inputLimit, defaultValue, fontFace,
                                posX, posY,
                                allowBorder,
                                borderColor, borderThickness,
                                minValue, maxValue,
                                topMost,
                                skinWindow,
                                solidColor,
                                logging);

                            inputOverlay.TextSubmitted += (s, text) =>
                            {
                                currentText = text;
                                if (logging == 1)
                                    _api.Log(API.LogType.Notice, "Text updated: " + text);
                            };

                            try
                            {
                                int repSkinX = int.Parse(_api.ReplaceVariables("#CURRENTCONFIGX#"));
                                int repSkinY = int.Parse(_api.ReplaceVariables("#CURRENTCONFIGY#"));
                                inputOverlay.Location = new Point(repSkinX + offsetX, repSkinY + offsetY);
                            }
                            catch (Exception ex)
                            {
                                if (logging == 1)
                                    _api.Log(API.LogType.Error, "Error updating overlay location: " + ex.Message);
                            }

                            if (logging == 1)
                                _api.Log(API.LogType.Notice, "Running InputOverlay application loop.");
                            Application.Run(inputOverlay);
                        }
                        catch (Exception ex)
                        {
                            if (logging == 1)
                                _api.Log(API.LogType.Error, "Error in Start thread: " + ex.Message);
                        }
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
                if (logging == 1)
                    _api.Log(API.LogType.Notice, "Unloading plugin...");
                if (inputOverlay != null && !inputOverlay.IsDisposed)
                    inputOverlay.Invoke(new Action(() => inputOverlay.Close()));
                if (skinWindow != IntPtr.Zero)
                    NativeMethods.EnableWindow(skinWindow, true);
                if (logging == 1)
                    _api.Log(API.LogType.Notice, "Plugin unloaded successfully.");
            }
            catch (Exception ex)
            {
                if (logging == 1)
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

}
