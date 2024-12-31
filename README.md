# Graphorama

**Graphorama** is a function plotting application built using C# and WPF, utilizing the OxyPlot library for plotting mathematical functions. The app supports real-time graphing, multiple functions, and provides interactive features like zoom, graph clearing, and limit adjustments. 

## Features
https://cloud-1ssjxizmq-hack-club-bot.vercel.app/02025-01-01_01-32-43.mp4 (video demo)
- Plot mathematical functions with support for a variety of operations
- Real-time graphing for dynamic function plotting
- Clear and reset graph functionality
- Adjustable axis limits
- Supports standard mathematical functions (sin, cos, tan, log, etc.)
- Input validation for safe expression evaluation
- Real-time speed control for faster/slower plotting

## Getting Started

### Prerequisites

To run Graphorama, you'll need:
- Visual Studio 2019 or later (for compiling and running the code)
- .NET Framework (compatible version based on the project setup)

### Installation

1. **Clone the repository** to your local machine:

    ```bash
    git clone https://github.com/UltimaCodes/Graphorama.git
    ```

2. **Open the project** in Visual Studio.
   
3. **Build and run** the project using Visual Studio (use the "Start" button or press `F5`).

   OR
   
1. Extract all files from the latest release zip file to a folder on your computer.
   
3. Double-click `Graphorama.exe` to launch the application.

### Running the Application

Once the application is running, you will see:

- A **graphing area** where mathematical functions will be plotted.
- An **input box** to enter the function you wish to plot.
- A **list** of previously entered functions.
- **Buttons** for adding, clearing, and interacting with the graph.

### Inputting Functions

You can input standard mathematical functions and operations. Here's how:

#### Basic Functions:
- **Addition**: `2x + 3`
- **Subtraction**: `x - 5`
- **Multiplication**: `2 * x`
- **Division**: `x / 2`

#### Trigonometric Functions:
- **Sine**: `sin(x)`
- **Cosine**: `cos(x)`
- **Tangent**: `tan(x)`
  
#### Inverse Trigonometric Functions:
- **Arcsin (inverse sine)**: `asin(x)` → type as `ArcSin(x)`
- **Arccos (inverse cosine)**: `acos(x)` → type as `ArcCos(x)`
- **Arctan (inverse tangent)**: `atan(x)` → type as `ArcTan(x)`

#### Other Supported Functions:
- **Square Root**: `sqrt(x)`
- **Logarithm**: `log(x)`
- **Exponential**: `exp(x)`

### Using Real-Time Plotting

1. **Enable Real-Time Plotting**: 
   - Click on the **"Real-Time: Off"** button to toggle real-time graphing.
   - When enabled, the function will be plotted dynamically as you input it.
   - You can control the speed of real-time plotting using the **Real-Time Speed** input field (values between 1 and 256).

2. **Input and Plot Function**: 
   - Type your function in the input box (e.g., `sin(x)`).
   - Click **Add Expression** to plot the function on the graph.

### Modifying Graph and Axis Limits

- **Adjust X-axis Limits**: Enter your desired **X Min** and **X Max** values in the respective input boxes and click **Update Limits**.
- **Zoom Reset**: Click the **Reset Zoom** button to reset the graph zoom to default.

### Removing Functions

- To remove a function from the graph, select it from the list and press the **Delete** or **Backspace** key.

### Error Handling

- If you input an invalid function, an error message will be shown prompting you to correct the equation.
- Ensure proper syntax, such as matching parentheses and valid operators.

## Example Inputs

- Plot a sine wave: `sin(x)`
- Plot an inverse sine function: `ArcSin(x)`
- Plot a logarithmic function: `log(x)`
- Plot `2x + 3`: `2x + 3`

## Contributing

Feel free to fork the repository and submit pull requests with any improvements or fixes.

1. Fork the repository
2. Create a new branch (`git checkout -b feature-name`)
3. Commit your changes (`git commit -am 'Add new feature'`)
4. Push to the branch (`git push origin feature-name`)
5. Open a pull request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
