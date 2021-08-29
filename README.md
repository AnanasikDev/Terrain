## Willow

Willow is **Unity** asset for working with instantiating objects on surfaces in **Editor**.<br>
For initialize Willow you need to open ***Tools -> Willow -> Prefab Brush***. To start working you need to enable it in Editor window.<br>

All surfaces that you want to spawn objects on must contain ***WillowTerrainSettings*** component and any type of collider.<br>
Note: Further in the documentation, GameObject with WillowTerrainSettings is replaced by Terrain for *convenience*<br>

There are 3 actions you can do:

<details>
<summary><strong>Place</strong></summary>
Instantiating objects on the surface.
There are three types of detecting surfaces to spawn on:
	
- Only on Terrain
- Only on Objects
- Both on Terrain and Objects
	
</details>

<details>
<summary><strong>Erase</strong></summary>
	
Erasing (removing) objects that have been spawned by *Willow*<br>
It does not destroy objects, but hide them. It is possible to enable all erased objects in last session with ***Tools -> Willow -> Enable Destroyed Objects***<br>
Further you can simply disable them by using ***Tools -> Willow -> Disable Destroyed Objects***<br>

</details>

<details>
  <summary><strong>Exchange</strong></summary>
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

<summary><strong>FAQ</strong></summary>
<br>
	
<details>
<summary><strong>What is the minimum Unity version to use Willow?</strong></summary>
<br>
- Your project should be on Unity 2019.4.23f and higher.
</details>

<details><summary><strong>I want to get it, how do I import it into my project?</strong></summary>
<br>
- Instruction:

1. Download last [Release](https://github.com/AnanasikDev/Willow/releases)
1. Go to your Unity project, go to Assets -> Import Package -> Custom Package and select the .unitypackage file you just downloaded
1. Click Import in the following window and wait for import.

[Unity Instruction](https://unity3d.com/quick-guide-to-unity-asset-store#:~:text=Click%20the%20Go%20to%20My,assets%20you%20have%20already%20chosen.&text=Another%20way%20to%20import%20assets,your%20asset%20on%20your%20computer.)
	
</details>

<details><summary><strong>I cant spawn objects, what may be wrong?</strong></summary>
<br>
	
1. Make sure Willow is enabled.<br>
1. Make sure your surface that you want to spawn objects on contains WillowTerrainSettings component. It is important!<br>
1. Make sure you have at least one active spawnable object with setted Object.<br>
1. Make sure parameter Placement Type of global settings is OnlyOnTerrain or OnTerrainAndObjects, otherwise it is only possible to spawn objects on other objects.<br>
1. Make sure brush size is setted above 0.<br>

</details>

<details><summary><strong>I cant see brush, what should I do?</strong></summary>
<br>
- This is a temporary bug I am trying to fix. You need to close Willow window and open it again.

</details>
	
</details>
