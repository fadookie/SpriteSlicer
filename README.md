# Unity Sprite Slice Export Tool
## Background
I wanted a way to quickly slice up spritesheet rips for a game I am working on. [Unity's built-in spritesheet editor](https://docs.unity3d.com/Manual/SpriteEditor.html) has some fantastic tools to slice spritesheets automatically. However, at this time of writing there is no built-in way to export the sprites as individual images. This editor script is intended to bridge that gap, turning Unity into an automatic spritesheet-slicing machine.

## Installation
This works as a standalone Unity project, or you may copy the `Scripts` folder into an existing project.

## Usage

### Pre-Processing
You will get the best results if you pre-process your spritesheet by deleting any unwanted graphics along with the background color if any, and save it as an alpha-transparent PNG. This will help Unity's automatic slicing algorithm, as well as usually being what you want in order to use the sprite in a modern game engine. If you are looking for tools to help do this, I recommend [Pixen](https://pixenapp.com/) (macOS), [Paint.NET](https://www.getpaint.net/) (free, Windows), or [GIMP](https://www.gimp.org/) (free, all platforms).


### Required Import Settings
Each texture you want to use should have the following import settings:

| Property | Value | Explanation |
| ------------- | ------------- |  ------------- |
| `Texture Type`  | `Sprite (2D and UI)`  | This script is only designed to export sliced sprite sheets. |
| `Sprite Mode` | `Multiple`  | " |
| `Advanced` → `Read/Write Enabled` | ☑️  | Required for exporter to read texture. |
| `Default` → `Max Size` | `8192` (Or highest available)  | Prevents automatic downsampling of large textures. If your sheet is larger than this (8192 x 8192,) split it into smaller sheets using an image editor first. |

### Export Steps
1. Slice your spritesheets using the [Sprite Editor](https://docs.unity3d.com/Manual/SpriteEditor.html).
	* I recommend using the automatic slicing feature and then tweaking if needed.
3. From the main menu bar, click `SpriteTextureSliceExporter` → `Export Slices`.
	* I wanted this to be in the context menu as well but couldn't get it working. If you can figure this out please send a PR! 
4. Choose a folder to export images to.

Note: I have not tested this on atlases, only spritesheets attached to individual sprite assets as described above.