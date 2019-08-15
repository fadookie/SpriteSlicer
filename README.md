# Unity Sprite Slice Export Tool
## Background
I wanted a way to quickly slice up spritesheet rips for a game I am working on. [Unity's built-in spritesheet editor](https://docs.unity3d.com/Manual/SpriteEditor.html) has some fantastic tools to slice spritesheets automatically. However, at this time of writing there is no built-in way to export the sprites as individual images. This editor script is intended to bridge that gap, turning Unity into an automatic spritesheet-slicing machine.

## Installation
This works as a standalone Unity project, or you may copy the `Scripts` folder into an existing project.

## Usage
Once you have some spritesheets made in the Editor, select them and then click the `SpriteTextureSliceExporter` -> `Export Slices` item from the main menu.

Now, choose a folder to export images to.

Note: I have not tested this on Atlases, only spritesheets of type `Sprite (2D and UI)`.