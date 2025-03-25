# InputTextX Plugin

**InputTextX** is a Rainmeter plugin that provides an on-screen input field with customizable properties. It supports custom fonts (TTF and OTF), dynamic colors, border options, topmost behavior, and more. The plugin’s position is computed only once when started, and it can be fully reset on command.

## Features

- **Customizable Appearance:**  
  Set the input field's width, height, font size, font face, string style (Normal, Bold, Italic, BoldItalic), solid background color, and font color.
  
- **Border Control:**  
  Choose to show or hide the border around the input field.
  
- **TopMost Behavior:**  
  Control whether the input field always remains on top or not. If the skin is topmost, the plugin can force the input field to remain visible.
  
- **Dynamic Properties:**  
  Many properties are dynamically reloadable via Rainmeter variables.
  
- **Password Mode:**  
  Mask input characters with '*' when in password mode.
  
- **Event Actions:**  
  Execute Rainmeter bangs on text change, dismiss, invalid input, Escape, and Enter.

- **Full Reset on Stop:**  
  Fully disposes and resets the plugin when stopped.

## Documentation

| Option             | Type          | Default          | Description                                                                                                                |
|--------------------|---------------|------------------|----------------------------------------------------------------------------------------------------------------------------|
| **InitialText**    | String        | (empty)          | The text to display initially in the input field.                                                                        |
| **OnTextChangedAction** | String  | (empty)          | Rainmeter bang to execute when the text changes.                                                                         |
| **OnDismissAction**| String        | (empty)          | Rainmeter bang to execute when the input field is dismissed.                                                             |
| **OnInvalidAction**| String        | (empty)          | Rainmeter bang to execute when an invalid character is entered.                                                          |
| **OnESCAction**    | String        | (empty)          | Rainmeter bang to execute when the Escape key is pressed.                                                                |
| **OnEnterAction**  | String        | (empty)          | Rainmeter bang to execute when Enter is pressed.                                                                         |
| **Width**          | Integer       | 300              | Width of the input field (in pixels).                                                                                    |
| **Height**         | Integer       | 50               | Height of the input field (in pixels).                                                                                   |
| **FontSize**       | Integer       | 12               | Font size of the text.                                                                                                   |
| **FontColor**      | RGB String    | "0,0,0"          | Color of the text, in R,G,B format.                                                                                      |
| **StringAlign**    | String        | "Left"           | Alignment of the text: "Left", "Center", or "Right".                                                                     |
| **Password**       | Boolean (0/1) | 0                | When set to 1, the input field masks characters with '*'.                                                              |
| **StringStyle**    | String        | "Normal"         | Style of the font: "Normal", "Bold", "Italic", or "BoldItalic".                                                          |
| **FontFace**       | String        | "Segoe UI"       | Name of the font family (e.g., "Ubuntu"). The plugin will try to load the corresponding TTF/OTF from the Fonts folder.     |
| **SolidColor**     | RGB String    | "255,255,255"    | The solid background color of the input field, in R,G,B format.                                                          |
| **Multiline**      | Boolean (0/1) | 0                | If set to 1, the input field supports multiple lines.                                                                    |
| **InputType**      | String        | "String"         | Specifies the input type: "String", "Integer", "Float", "Letters", "Alphanumeric", "Hexadecimal", "Email", or "Custom".   |
| **UnFocusDismiss** | Boolean (0/1) | 0                | If set to 1, the input field dismisses when it loses focus.                                                              |
| **InputLimit**     | Integer       | 0                | Maximum number of characters allowed (0 = no limit).                                                                     |
| **X**              | Integer       | 0                | Horizontal offset (in pixels) added to the skin’s current X position.                                                    |
| **Y**              | Integer       | 0                | Vertical offset (in pixels) added to the skin’s current Y position.                                                      |
| **InputTopMost**   | String        | (empty)          | "1" forces the input field to be always on top, "0" forces it not to be topmost (unless the skin is topmost).             |
| **TopMostOffsetY** | Integer       | 0                | Additional vertical offset (in pixels) when InputTopMost is "1". Can be negative to adjust upward.                         |
| **Border**         | String        | "1"              | "1" shows a border around the input field; "0" hides the border.                                                         |
| **WindowTopMost**  | String        | "0"              | (Optional) Indicates whether the skin itself is topmost. Used by the plugin to decide whether to force the input field on top.|

## Installation

1. **Compile the Plugin:**  
   Use Visual Studio (or your preferred IDE) to compile the project as a DLL named **InputTextX.dll**.

2. **Copy to Plugins Folder:**  
   Place the compiled **InputTextX.dll** into your Rainmeter Plugins folder (usually `C:\Users\<username>\Documents\Rainmeter\Plugins\`).

3. **Configure Your Skin:**  
   Create or update your Rainmeter skin INI file to include a measure using the plugin, and define your desired options (see the configuration table above).

## Example Skin Configuration

```ini
[Rainmeter]
Update=50
DynamicVariables=1

[MeasureInput]
Measure=Plugin
Plugin=InputTextX.dll
InitialText=Your initial text
OnTextChangedAction=[!Log "Text changed"]
OnDismissAction=[!Log "Dismissed"]
OnInvalidAction=[!Log "Invalid input"]
OnESCAction=[!Log "ESC pressed"]
OnEnterAction=[!Log "Entered: [MeasureInput:GetString]"]
Width=400
Height=60
FontSize=14
FontColor=255,0,0
StringAlign=Center
Password=1
StringStyle=BoldItalic
FontFace=Ubuntu
SolidColor=255,255,255
Multiline=0
InputType=String
UnFocusDismiss=0
InputLimit=20
X=20
Y=20
InputTopMost=0
TopMostOffsetY=-20
Border=1
WindowTopMost=1
```

## Usage

- **Starting the Input Field:**  
  Use the Rainmeter bang command:  
  `!CommandMeasure MeasureInput "Start"`  
  This computes the position only once and displays the input field with the specified options.

- **Stopping/Resetting the Plugin:**  
  Use the Rainmeter bang command:  
  `!CommandMeasure MeasureInput "Stop"`  
  This fully resets the plugin (disposing and reinitializing the input field).

## Notes & Warnings

- **Dynamic Variables:**  
  Many options (colors, font properties, etc.) update dynamically when the skin is reloaded. However, the input field’s position is computed only once on start. To update the position, you must stop and restart the plugin.

- **TopMost Behavior:**  
  The plugin tries to honor the **InputTopMost** setting. If set to "0" but the skin is topmost (determined by the variable "WindowTopMost"), the plugin forces the input field on top. Ensure that your skin defines "WindowTopMost" correctly if you rely on this behavior.

- **Border Option:**  
  Set **Border=1** to show a border around the input field or **Border=0** to hide it.

- **Font Loading:**  
  The plugin attempts to load custom fonts from the folder defined by Rainmeter’s `#@#` variable combined with a "Fonts" subfolder. If the custom font file is not found, it falls back to a system font and ultimately to "Segoe UI". Make sure your fonts are placed in the correct folder.

- **Performance:**  
  Setting a low Update interval (e.g., Update=50) may cause issues if the skin forces frequent reloads. The plugin minimizes flickering by computing the position only once, but heavy dynamic changes could still affect performance.

- **Disposal & Reset:**  
  When stopping the plugin, it fully disposes of the input field. Ensure you use the Stop command (`!CommandMeasure MeasureInput "Stop"`) to reset the plugin correctly.
