# CircuitSimulator

A public version of circuit simulator project

This project uses Windows Forms, so is only available for Windows

This repo does not include any images required for this project due to copyright reasons. The program will still work without the images

The images for the diagram mode will be added, the images for the drawing mode must be supplied by the user

This uses re-implemented data structures from [my data structures library](https://www.nuget.org/packages/Anarchie.DataStructsLib/1.0.0), as this was part of the project


# Installation instructions

## From source

### Dependencies

.NET SDK [Download Here](https://dotnet.microsoft.com/en-us/download)

This project is designed for .NET 6, but should work with newer versions, although it has not been tested

### Clone repository

Clone the repository with

```bash
git clone https://github.com/anarchie347/CircuitSimulator.git
```
Alternatively download as Zip and extract

### Build

1. In a terminal, navigate to the project directory

2. Restore the dependencies:

```bash
dotnet restore
```

3. Build in release mode

```bash
dotnet build --configuration Release
```

4. Execute

The program is locatated at:

```
/path/to/project/bin/release/net6.0-windows/CircuitSimulator.exe
```

Note: If using a different version of .NET, then the the directory will use the corresponding verasion number

5. Images

  This step is optional but will improve the appearance of the program
  
  Copy the `img` folder from this repository into the same directory as the program executable.

  If you wish to add images for the drawing mode, , inside the `img` folder, create a folder called `draw` and place the images in there. The images must have the `.jpg` file extension. For the names, refer to the names used in the diagram images folder, they are case sensitive


7. Shortcut

You may wish to create a shortcut to the application to add to your desktop or start menu

# Additional information

Troubleshooting: IF you encounter issues, please refer to [Github isses section](https://github.com/anarchie347/CircuitSimulator/issues).

Contributing: Contributions are welcome.

License: This project is licensed under the MIT License. See [LICENSE](https://github.com/anarchie347/CircuitSimulator/blob/main/LICENSE.txt) for more details.
