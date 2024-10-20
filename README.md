# Blockdraw

An application to draw images using custom items in Trackmania 2.

### Usage

1. Unzip colors.zip and move the contents into your Items folder (by default in `C:\Users\foo\Documents\Maniaplanet\Items` on Windows), so as to have e.g `Items/colors/blue1.Item.Gbx`.

2. Launch Blockdraw and enter the path to the map you wish to modify, e.g `C:\Users\foo\Documents\Maniaplanet\My Maps\test.Map.Gbx`.

3. Select the image you would like to add to the map. Keep in mind that time taken to load maps as well as performance starts to degrade between 50-100K blocks, and at >100K maps may not load at all.

4. Save and open the map in the map editor. Changes only get written when you click save. You may use the 'Wipe' button to remove ALL items from the `colors` folder from the map.

__If you get an error saying "Error while loading Map: Missing items", make sure you have unzipped colors.zip in the right folder.__


### Notes

This application works by using a set of pre-generated items, each 4x4 squares (with only one side textured for performance). The colors are therefore limited to those found in the default materials in TM2 Stadium.

A future update will allow you to substitute blocks of your choice for the ones provided by default.

Each pixel in the given image (if not transparent) is converted to the nearest color available in the default items and placed on the map at the corresponding location.

__Drawing Options__ 

* Position determines the starting position of the drawing. It uses block coordinates (E.g default map is 32x40x32), so if you wanted to center your horizontal drawing, you might use 16,2,16. Values of Y < 2 may be under the ground for reasons that aren't clear to me yet, so that is the minimum recommended.

* Orientation determines whether your drawing is horizontal (e.g along the ground) or vertical.

* Per block rotation applies a rotation to every item placed. For instance if you are drawing vertically, by default the items will be hard to see from the ground as they are parallel to the ground and their underside has no texture. Setting a rotation of X:90 Y:0 Z:90 will make them perpendicular with the ground.

Note: There is no option yet to rotate the overall image. 