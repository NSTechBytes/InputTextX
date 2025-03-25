# InputTextX

**InputTextX** is a Rainmeter plugin that provides an interactive, customizable on‐screen text input overlay. It allows Rainmeter skins to collect user input with flexible appearance, filtering, and behavior options.

---

## Features

- **Multiple Input Types:**
  - Supports _String_, _Integer_, _Float_, _Letters_, _Alphanumeric_, _Hexadecimal_, _Email_, and _Custom_ allowed characters.
- **Customizable Appearance:**
  - Define dimensions, colors (background, font, border), font size, style, and face.
  - Optionally enable a custom border with configurable color and thickness.
- **Input Filtering & Validation:**
  - Filters invalid characters based on the chosen input type.
  - Supports numeric range validation using `MinValue` and `MaxValue`.
- **Behavioral Options:**
  - Multiline support with optional scrollbars.
  - Define a character input limit and a default text value.
  - Configurable actions on submission, dismissal, ESC key, and invalid input.
- **Positioning & Z‑Order Control:**
  - Configure offset relative to the skin.
  - Control TopMost behavior to determine whether the overlay appears above all windows.

---

## Installation

1. **Download the Plugin:**Clone or download the latest release from the [GitHub Releases](#) page.
2. **Extract Files:**Place the extracted files into your Rainmeter `Plugins` folder (typically `C:\Users\<YourUser>\Documents\Rainmeter\Plugins`).
3. **Configure Your Skin:**Include the plugin in your Rainmeter skin configuration (see [Usage Example](#usage-example) below).
4. **Restart Rainmeter:**
   Reload your skins or restart Rainmeter for the plugin to initialize.

---

## Configuration Parameters

Below is a table of all configurable parameters available in InputTextX:

| **Parameter**       | **Type** | **Default**   | **Description**                                                                                                                                                                                                           |
| ------------------------- | -------------- | ------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **W**               | Integer        | `300`             | Width of the input box.                                                                                                                                                                                                         |
| **H**               | Integer        | `40`              | Height of the input box.                                                                                                                                                                                                        |
| **SolidColor**      | String         | `"255,255,255"`   | Background color (RGB). If provided with an alpha value (e.g.`"255,255,255,255"`), the alpha is ignored.                                                                                                                      |
| **FontColor**       | String         | `"0,0,0"`         | Font color in RGB.                                                                                                                                                                                                              |
| **FontSize**        | Integer        | `12`              | Size of the input text.                                                                                                                                                                                                         |
| **Align**           | String         | `"Center"`        | Text alignment:`Left`, `Center`, or `Right`.                                                                                                                                                                              |
| **Password**        | Integer        | `0`               | Set to `1` to mask the input for password entry.                                                                                                                                                                              |
| **FontStyle**       | String         | `"Normal"`        | Font style:`Normal`, `Bold`, `Italic`, or `BoldItalic`.                                                                                                                                                                 |
| **FontFace**        | String         | `"Segoe UI"`      | Font family name or a full path to a custom font file.                                                                                                                                                                          |
| **Multiline**       | Integer        | `0`               | Set to `1` to enable multiline input.                                                                                                                                                                                         |
| **AllowScroll**     | Integer        | `0`               | Enable scrollbars in multiline mode by setting to `1`.                                                                                                                                                                        |
| **InputLimit**      | Integer        | `0`               | Maximum number of characters (0 for no limit).                                                                                                                                                                                  |
| **DefaultValue**    | String         | `""`              | Default text displayed in the input box.                                                                                                                                                                                        |
| **InputType**       | String         | `"String"`        | Allowed input type. Options:`String`, `Integer`, `Float`, `Letters`, `Alphanumeric`, `Hexadecimal`, `Email`, `Custom`.                                                                                          |
| **AllowedChars**    | String         | `""`              | Custom allowed characters if `InputType` is set to `Custom`.                                                                                                                                                                |
| **OnDismissAction** | String         | `""`              | Action executed when the input overlay is dismissed.                                                                                                                                                                            |
| **OnEnterAction**   | String         | `""`              | Action executed when the input is submitted.                                                                                                                                                                                    |
| **OnESCAction**     | String         | `""`              | Action executed when the ESC key is pressed.                                                                                                                                                                                    |
| **InValidAction**   | String         | `""`              | Action executed when invalid input is detected.                                                                                                                                                                                 |
| **X**               | Integer        | `0`               | Horizontal offset relative to the Rainmeter skin's base position.                                                                                                                                                               |
| **Y**               | Integer        | `0`               | Vertical offset relative to the Rainmeter skin's base position.                                                                                                                                                                 |
| **AllowBorder**     | Integer        | `0`               | Set to `1` to enable a custom border.                                                                                                                                                                                         |
| **BorderColor**     | String         | `"0,0,0"`         | Color of the border in RGB.                                                                                                                                                                                                     |
| **BorderThickness** | Integer        | `2`               | Thickness of the border.                                                                                                                                                                                                        |
| **MinValue**        | Double         | `double.MinValue` | Minimum value allowed for numeric inputs.                                                                                                                                                                                       |
| **MaxValue**        | Double         | `double.MaxValue` | Maximum value allowed for numeric inputs.                                                                                                                                                                                       |
| **TopMost**         | Integer        | `1`               | Set to `1` to force the input overlay above all windows, or `0` to allow normal window behavior.                                                                                                                            |
| UnFocusDismiss            | Integer        | `1`               | **1:** The input overlay is dismissed when it loses focus (clicking outside will close it).<br />**0:** The input overlay remains active, and clicks are redirected to the input box, preventing <br />dismissal. |

---

## Usage Example

Below is an example configuration snippet for a Rainmeter skin using InputTextX:

```ini
[Rainmeter]
Update=1000

[Metadata]
Name=InputTextX Example
Author=Your Name

[Variables]
CURRENTCONFIGX=100
CURRENTCONFIGY=100
CURRENTCONFIGWIDTH=800
CURRENTCONFIGHEIGHT=600

[MeasureInputText]
Measure=Plugin
Plugin=InputTextX.dll
W=300
H=40
SolidColor=255,255,255
FontColor=0,0,0
FontSize=14
Align=Center
Password=0
FontStyle=Normal
FontFace=Segoe UI
Multiline=0
AllowScroll=0
InputLimit=0
DefaultValue=Type here...
InputType=String
AllowedChars=
OnDismissAction=
OnEnterAction=!Log "Input Submitted"
OnESCAction=!Log "Input Cancelled"
InValidAction=!Log "Invalid Input"
X=0
Y=0
AllowBorder=1
BorderColor=0,0,0
BorderThickness=2
MinValue=-100
MaxValue=100
TopMost=1
```

---

## Detailed Documentation

### Overview

InputTextX provides a Rainmeter skin with an input overlay that can capture user text input. It leverages Windows Forms to create an overlay and a text box which can be customized extensively through configuration parameters. The plugin supports a range of input types, filters invalid characters, and validates numeric inputs against a defined range.

### Input Filtering and Validation

- **InputType:**Determines what kind of characters are allowed. For instance, setting it to `Integer` restricts input to digits (with an optional leading minus).
- **AllowedChars:**When using `Custom` as the input type, only the characters defined in this parameter are permitted.
- **Numeric Range:**
  When the input type is `Integer` or `Float`, the plugin validates that the input falls within the range defined by `MinValue` and `MaxValue`.

### Appearance and Behavior

- **Custom Border:**By setting `AllowBorder` to 1, you can draw a border around the input box. Configure the border’s appearance using `BorderColor` and `BorderThickness`. The plugin uses a container panel with padding to display only the border rim.
- **TopMost Option:**
  The `TopMost` parameter controls whether the input overlay should appear above all other windows (including Rainmeter skins). Setting `TopMost` to 1 forces the input overlay to be topmost.

### Logging

The plugin logs key events and configuration details to help with debugging. Check the Rainmeter log for messages prefixed with “InputOverlay” or “InputTextX” for details.

---

## Warnings and Limitations

- **Operating System:**InputTextX is designed for Windows. It uses Win32 API calls and Windows Forms.
- **Skin Compatibility:**If the Rainmeter skin is set to TopMost, the plugin may need the `TopMost` parameter to be configured appropriately to maintain overlay visibility.
- **Input Focus Behavior:**When `UnFocusDismiss` is set to 0, mouse clicks on the overlay are intercepted to redirect focus to the input box. This may not work perfectly in all multi-monitor or complex skin scenarios.
- **Performance:**
  Using multiple overlay windows can impact performance on lower-end systems.

---

## Contribution

Contributions, bug fixes, and feature requests are welcome. Please fork the repository and submit a pull request or open an issue.

---

## License

This project is licensed under the MIT License.

---

## Contact

For questions, issues, or further discussion, please open an issue on the GitHub repository or contact the maintainer at [YourEmail@example.com].

---

Feel free to adapt this README to better fit your project details and style.
