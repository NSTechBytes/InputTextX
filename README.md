# InputTextX

**InputTextX** is a [Rainmeter](https://www.rainmeter.net/) plugin that creates a customizable text input overlay. It allows users to capture text input on-the-fly and inject that text into Rainmeter commands with dynamic placeholder replacement. The plugin supports various input types, custom validation, styling options, and configurable actions for enter, escape, and dismissal events.

---

## Features

- **Customizable Appearance:**Configure dimensions, colors, fonts (including custom font files), and borders.
- **Input Types & Validation:**Supports `String`, `Integer`, `Float`, `Letters`, `Alphanumeric`, `Hexadecimal`, `Email`, and a custom mode with allowed characters.
- **Action Handling:**Execute custom Rainmeter commands on key events:

  - **OnEnterAction:** Triggered when the user submits the input.
  - **OnESCAction:** Triggered when the user presses the Escape key.
  - **OnDismissAction:** Triggered when the input overlay loses focus.
- **Dynamic Placeholder Replacement:**Replace the placeholder `$UserInput$` with the current text input before executing commands.`<span style="color: red;"><strong>`Note:`</strong>` Special characters (e.g., double quotes) are automatically escaped to ensure valid command syntax.
- **Logging:**
  Detailed logging can be enabled for debugging. All logs are printed only when the `Logging` key is set to `1`.

---

## Getting Started

### Installation

1. **Build the Plugin:**Compile the C# code into a DLL (e.g., using Visual Studio) and place it into your Rainmeter plugins folder.
2. **Add the Plugin to Your Skin:**Create a Rainmeter skin and add the following measure:

   ```ini
   [MeasureInput]
   Measure=Plugin
   Plugin=InputTextX.dll
   ```
3. **Configure the Plugin:**
   Set the desired configuration keys (see below) in your Rainmeter skin.
   Example:

   ```ini
   [MeasureInput]
   Measure=Plugin
   Plugin=InputTextX.dll
   W=300
   H=40
   SolidColor=255,255,255
   FontColor=0,0,0
   FontSize=12
   Align=Center
   Password=0
   FontStyle=Normal
   FontFace=Segoe UI
   Multiline=0
   AllowScroll=0
   InputLimit=0
   DefaultValue=Enter text here...
   InputType=String
   AllowedChars=
   OnDismissAction=[!Log "Dismissed: $UserInput$"]
   OnEnterAction=[!Log "Submitted: $UserInput$"]
   OnESCAction=[!Log "Cancelled: $UserInput$"]
   InValidAction=[!Log "Invalid input"]
   X=20
   Y=20
   AllowBorder=1
   BorderColor=0,0,0
   BorderThickness=2
   MinValue=-999999
   MaxValue=999999
   TopMost=1
   Logging=1
   ```

---

## Configuration Keys

The following table lists each configuration key along with its description, type, and default value.

<table>
  <tr>
    <th>Key</th>
    <th>Description</th>
    <th>Type</th>
    <th>Default</th>
  </tr>
  <tr>
    <td>W</td>
    <td>Width of the input overlay</td>
    <td>Integer</td>
    <td>300</td>
  </tr>
  <tr>
    <td>H</td>
    <td>Height of the input overlay</td>
    <td>Integer</td>
    <td>40</td>
  </tr>
  <tr>
    <td>SolidColor</td>
    <td>Background color in R,G,B format</td>
    <td>Color</td>
    <td>255,255,255</td>
  </tr>
  <tr>
    <td>FontColor</td>
    <td>Text color in R,G,B format</td>
    <td>Color</td>
    <td>0,0,0</td>
  </tr>
  <tr>
    <td>FontSize</td>
    <td>Font size for text input</td>
    <td>Float</td>
    <td>12</td>
  </tr>
  <tr>
    <td>Align</td>
    <td>Text alignment (`Left`, `Center`, or `Right`)</td>
    <td>String</td>
    <td>Center</td>
  </tr>
  <tr>
    <td>Password</td>
    <td>If set to 1, input will be masked</td>
    <td>Integer</td>
    <td>0</td>
  </tr>
  <tr>
    <td>FontStyle</td>
    <td>Font style (`Normal`, `Bold`, `Italic`, or `BoldItalic`)</td>
    <td>String</td>
    <td>Normal</td>
  </tr>
  <tr>
    <td>FontFace</td>
    <td>Name or path to the font file</td>
    <td>String</td>
    <td>Segoe UI</td>
  </tr>
  <tr>
    <td>Multiline</td>
    <td>If set to 1, allows multiline input</td>
    <td>Integer</td>
    <td>0</td>
  </tr>
  <tr>
    <td>AllowScroll</td>
    <td>If set to 1, enables vertical scroll for multiline input</td>
    <td>Integer</td>
    <td>0</td>
  </tr>
  <tr>
    <td>InputLimit</td>
    <td>Maximum number of characters allowed</td>
    <td>Integer</td>
    <td>0 (no limit)</td>
  </tr>
  <tr>
    <td>DefaultValue</td>
    <td>Initial text in the input box</td>
    <td>String</td>
    <td>Empty</td>
  </tr>
  <tr>
    <td>InputType</td>
    <td>Type of allowed input (see list below)</td>
    <td>String</td>
    <td>String</td>
  </tr>
  <tr>
    <td>AllowedChars</td>
    <td>For InputType=Custom, characters allowed</td>
    <td>String</td>
    <td>Empty</td>
  </tr>
  <tr>
    <td>OnDismissAction</td>
    <td>Rainmeter command executed when the input overlay loses focus</td>
    <td>String</td>
    <td>Empty</td>
  </tr>
  <tr>
    <td>OnEnterAction</td>
    <td>Rainmeter command executed on pressing Enter</td>
    <td>String</td>
    <td>Empty</td>
  </tr>
  <tr>
    <td>OnESCAction</td>
    <td>Rainmeter command executed on pressing Escape</td>
    <td>String</td>
    <td>Empty</td>
  </tr>
  <tr>
    <td>InValidAction</td>
    <td>Command executed when invalid input is detected</td>
    <td>String</td>
    <td>Empty</td>
  </tr>
  <tr>
    <td>X</td>
    <td>Horizontal offset for the overlay relative to the skin</td>
    <td>Integer</td>
    <td>20</td>
  </tr>
  <tr>
    <td>Y</td>
    <td>Vertical offset for the overlay relative to the skin</td>
    <td>Integer</td>
    <td>20</td>
  </tr>
  <tr>
    <td>AllowBorder</td>
    <td>If set to 1, a border will be drawn around the input box</td>
    <td>Integer</td>
    <td>0</td>
  </tr>
  <tr>
    <td>BorderColor</td>
    <td>Border color in R,G,B format</td>
    <td>Color</td>
    <td>0,0,0</td>
  </tr>
  <tr>
    <td>BorderThickness</td>
    <td>Thickness of the border in pixels</td>
    <td>Integer</td>
    <td>2</td>
  </tr>
  <tr>
    <td>MinValue</td>
    <td>Minimum numeric value (for Integer/Float types)</td>
    <td>Double</td>
    <td>double.MinValue</td>
  </tr>
  <tr>
    <td>MaxValue</td>
    <td>Maximum numeric value (for Integer/Float types)</td>
    <td>Double</td>
    <td>double.MaxValue</td>
  </tr>
  <tr>
    <td>TopMost</td>
    <td>If set to 1, the overlay is displayed on top of other windows</td>
    <td>Integer</td>
    <td>1</td>
  </tr>
  <tr>
    <td>Logging</td>
    <td>If set to 1, detailed log messages will be printed to the Rainmeter log</td>
    <td>Integer</td>
    <td>0</td>
  </tr>
</table>

---

## Input Types

The `InputType` key supports the following values:

- **String**: Accepts any text.
- **Integer**: Accepts numeric digits and a leading minus sign.
- **Float**: Accepts numeric digits, a decimal point, and a leading minus sign.
- **Letters**: Accepts only alphabetical characters.
- **Alphanumeric**: Accepts letters and digits.
- **Hexadecimal**: Accepts digits and letters A-F (case insensitive).
- **Email**: Accepts characters typically allowed in email addresses.
- **Custom**: Only allows characters specified in the `AllowedChars` key.

---

## Usage Notes

> **Note:**
> The plugin dynamically replaces the placeholder `$UserInput$` in action keys with the current input text. It also escapes special characters (such as double quotes) to ensure the resulting Rainmeter command is valid.

> **InputTextX** is incompatible with skins set to **Stay Topmost**, or **AlwaysOnTop=2**, as the conflict between the input field, which requires "focus", and the constant attempts by the skin to stay on "top", in front of the input field, will not allow InputTextX to function correctly.

> **Warning:**
> Ensure that your custom commands (especially those using `$UserInput$`) are properly formatted. Improper command syntax may cause unexpected behavior in Rainmeter.

---

## Troubleshooting

- **Crash on Input:**If Rainmeter crashes when submitting input, enable logging by setting `Logging=1` to view the final command strings in the Rainmeter log. Verify that the commands are correctly formed.
- **No Command Execution:**Double-check the action keys (e.g., `OnEnterAction`, `OnESCAction`) in your skin configuration. They must contain valid Rainmeter commands.
- **Positioning Issues:**
  If the overlay does not appear in the expected location, adjust the `X` and `Y` offset values.

---

## Example Skin Configuration

Below is a sample Rainmeter skin configuration that demonstrates how to use **InputTextX**:

```ini
[Rainmeter]
Update=1000

[MeasureInput]
Measure=Plugin
Plugin=InputTextX.dll
W=300
H=40
SolidColor=255,255,255
FontColor=0,0,0
FontSize=12
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
OnDismissAction=[!Log "Dismissed: $UserInput$"]
OnEnterAction=[!Log "Submitted: $UserInput$"]
OnESCAction=[!Log "Cancelled: $UserInput$"]
InValidAction=[!Log "Invalid input"]
X=20
Y=20
AllowBorder=1
BorderColor=0,0,0
BorderThickness=2
MinValue=-999999
MaxValue=999999
TopMost=1
Logging=1
```

---

## License

Distributed under the MIT License. See [LICENSE](LICENSE) for more information.

---

## Contributing

Contributions are welcome! Please fork the repository and create a pull request with your changes.

---

## Contact

For support or questions, please open an issue on GitHub or contact [your.email@example.com](mailto:your.email@example.com).
