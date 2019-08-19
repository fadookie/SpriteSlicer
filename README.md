# Unity Sprite Slice Export Tool
## Background
I wanted a way to quickly slice up spritesheet rips for a game I am working on. [Unity's built-in spritesheet editor](https://docs.unity3d.com/Manual/SpriteEditor.html) has some fantastic tools to slice spritesheets automatically. However, at this time of writing there is no built-in way to export the sprites as individual images. This editor script is intended to bridge that gap, turning Unity into an automatic spritesheet-slicing machine.

## Installation
This works as a standalone Unity project, or you may copy the `Scripts` folder into an existing project.

## Usage
Once you have some spritesheets made in the Editor, select them and then click the `SpriteTextureSliceExporter` -> `Export Slices` item from the main menu.

Pro tip: turn the "Max Size" all the way up (to `8192`) in the import settings for the texture to avoid Unity automatically scaling your textures down. If your sheet is larger than 8192 x 8192, split it into smaller sheets using an image editor first.

Now, choose a folder to export images to.

Note: I have not tested this on Atlases, only spritesheets of type `Sprite (2D and UI)`.