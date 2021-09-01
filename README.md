## Willow

<details>
<summary><b>What is it?</b></summary>

Willow is **Unity** asset for working with custom instantiating objects on surfaces in **Editor**.<br>
For initialize Willow you need to open ***Tools -> Willow -> Prefab Brush***. To start working you need to enable it in Editor window.<br>
*Note: If you want to set path to **Willow Tools** you can set **WillowGlobalConfig.Path***

</details>

<details>
<summary><b>Global settings</b></summary>


***Brush density***: static amount of spawned object per click.

***Randomize, Randomization %***: If Randomize is enabled then brush density will be randomized as Randomization % value.

***Brush size***: static abstract size of brush. You can change it with hotkeys: *F + Scroll Wheel*

***Brush shape***: shape of brush. Spawnes objects evenly on shape.
	
***Brush mode***: brush surface detection mode. If it is static then brush will be used only on static normal. If mode is AsNormal then brush rotates as normal the cursor points on. First mode is useful for trees planting; Second may be used for planting moss onto walls etc.

***Fill brush***: if turned on it will spawn *inside* shape, otherwise only on border.
	
***Index objects***: if turned on Willow will index names.
	
***Auto-save***: if turned on it saves all Willow settings in a file whenever you work with objects.<br>
*Note: it does not save automatically if you change settings.*



</details>

All surfaces that you want to spawn objects on must contain ***WillowTerrainSettings*** component and any type of collider.<br>
*Note: Further in the documentation, GameObject with WillowTerrainSettings is replaced by Terrain for **convenience***<br>

There are 3 actions you can do:

<details>
<summary><b>Place</b></summary>	
Instantiating objects on the surface.<br>
There are three types of detecting surfaces to spawn on:
	
- Only on Terrain
- Only on Objects
- Both on Terrain and Objects
	
</details>

<details>
<summary><b>Erase</b></summary>
	
Erasing (removing) objects that have been spawned by *Willow*<br>
It does not destroy objects, but hide them. It is possible to enable all erased objects in last session with ***Tools -> Willow -> Enable Destroyed Objects***<br>
Further you can simply disable them by using ***Tools -> Willow -> Disable Destroyed Objects***<br>

Also it supports *smoothness* - the rarity of applying to object. More smoothnes less chance object will be exchanged by new one
	
</details>

<details>
  <summary><b>Exchange</b></summary>
This mode allows you to recalculate some parameters, like color, rotation, scale and other. It can be used to get next random rotation, scale, etc.<br>
Also it may change object to another one from spawnable objects list.<br>
You can exchange:<br>

- Position
- Rotation
- Scale
- Parent
- Color

Also it supports *smoothness* - the rarity of applying to object. More smoothnes less chance object will be exchanged by new one

</details>

Willow ***saves*** your configuration and spawned objects, so it will read from file when you re-import scripts or re-enter Unity.
The file is on the path *root/WillowSaveFile.txt*. You can change it in WillowFileManager.path.

<hr>

<details>

<summary><b>FAQ</b></summary>
<br>
	
<details>
<summary><b>What is the minimum Unity version to use Willow?</b></summary>
<br>
- Your project should be on Unity 2019.4.23f and higher.
</details>

<details><summary><b>I want to get it, how do I import it into my project?</b></summary>
<br>
- Instruction:

1. Download last [Release](https://github.com/AnanasikDev/Willow/releases)
1. Go to your Unity project, go to Assets -> Import Package -> Custom Package and select the .unitypackage file you just downloaded
1. Click Import in the following window and wait for import.

[Unity Instruction](https://unity3d.com/quick-guide-to-unity-asset-store#:~:text=Click%20the%20Go%20to%20My,assets%20you%20have%20already%20chosen.&text=Another%20way%20to%20import%20assets,your%20asset%20on%20your%20computer.)
	
</details>

<details><summary><b>I cant spawn objects, what may be wrong?</b></summary>
<br>
	
1. Make sure Willow is enabled.<br>
1. Make sure your surface that you want to spawn objects on contains WillowTerrainSettings component. It is important!<br>
1. Make sure you have at least one active spawnable object with setted Object.<br>
1. Make sure parameter Placement Type of global settings is OnlyOnTerrain or OnTerrainAndObjects, otherwise it is only possible to spawn objects on other objects.<br>
1. Make sure brush size is setted above 0.<br>

</details>

<details><summary><b>I cant see brush, what should I do?</b></summary>
<br>
- This is a temporary bug I am trying to fix. You need to close Willow window and open it again.

</details>
	
</details>
