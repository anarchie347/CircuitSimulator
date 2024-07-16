# CircuitSimulator

A public version of circuit simulator project

This project uses Windows Forms, so is only available for Windows

This repo does not include any images required for this project due to copyright reasons. The program will still work without the images

The images for the diagram mode will be added, the images for the drawing mode must be supplied by the user

This uses re-implemented data structures from [my data structures library](https://www.nuget.org/packages/Anarchie.DataStructsLib/1.0.0), as this was part of the project


# Installation instructions

## From compiled

1. Download the latest release from the releases tab on the right

2. Extract the zip file

3. The file `CircuitSimulator.exe` can then be run

4. Refer to [Post Installation](#post-installation)

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

Refer to [Post Installation](#post-installation)

# Post Installation

1. Images

This step is optional but will improve the appearance of the program
  
Copy the `img` folder from this repository into the same directory as the program executable.

If you wish to add images for the drawing mode, , inside the `img` folder, create a folder called `draw` and place the images in there. The images must have the `.jpg` file extension. For the names, refer to the names used in the diagram images folder, they are case sensitive.

If images are not present in the `draw` folder, there will be error messages on startup


2. Shortcut

You may wish to create a shortcut to the application to add to your desktop or start menu



# Usage and features

If when you load the program, you receive many pop-ups informing you of missing images, this can be fixed by creating image files in the paths specified ([see images step of post installation](#post-installation))

## Components 

To use the program, you can drag and drop components from the left sidebar.

To connect components, left click and release on the connector, then left click and release on the other connector to form a wire

To delete a component, press the red bin button on the component when you hover over it

Some components (e.g fuses and switches) have an extra button for extra functionality (such as reconnecting a fuse or toggling a switch)

Some components (e.g. diodes) have boxes at the top of them to indicate state, such as the diode allowing current through

Lots of components have extra properties which can be edited by right clicking on them

## Top Menu Bar

The `Clear` button clears all components

The `Load` button brings up a dialog box to select a circuit to either load or delete. IF a circuit doesnt appear, make sure the `circuits.db` file in the prorgams directory is the one that contains the circuit. Refer to [Saves and databases](#saves-and-databases)

The `Save` button brings up a dialog box to select a save to overwrite, or to create a new save, where you will be prompted for a circuit name and author name. The program does not auto-save, so to update the circuit, open the save dialog and overwrite the save with the new one

The `Diagram` button opens a window and displays a generated circuit diagram of your circuit. If the diagram doesnt look right, move around components in your drawing as the diagram uses locations and orientations from the drawing. To use a different set of symbols, replace thje images in `img/diagram` directory

The `Detailed Diagram` button does the same as the `Diagram` button, except components have captions showing their properties. If there are visual issues, try moving around components in your drawing and try again.

The `Environment` button opens a dialogue box to edit the environment. This controls the environment dependent components (e.g. the thermistor and LDR). It is global to all components in the circuit.

The `Simulate` button simulates the circuit, and displays appropriate values on ammeters and voltmeters, as well as displaying on output components that they are powered. It does not live update, so you will need to press simulate again if you make changes.

## Saves and databases

This program stores circuits in a database called `circuits.db` stored in the same directory as the executable. To export saves to another device, or another place where the program is, just move this database file. The program will always use a file called `circuits.db` in its directory, this cannot be changed. If one doesnt exist, then it will be created.

2 different database files cannot be merged or opened simulataneously by the same instance of the program.

To remove saves from a database, open either the `Save` or `Load` dialog and press the bin icon

The environment information is saved as part of the save file

# Additional information

Troubleshooting: IF you encounter issues, please refer to [Github isses section](https://github.com/anarchie347/CircuitSimulator/issues).

Contributing: Contributions are welcome.

License: This project is licensed under the MIT License. See [LICENSE](https://github.com/anarchie347/CircuitSimulator/blob/main/LICENSE.txt) for more details.
