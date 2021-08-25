## Willow

Willow is Unity asset for working with instantiating objects on surfaces in Editor.<br>
For initialize Willow you need to open Tools -> Willow -> Prefab Brush. To start working you need to enable it in Editor window.<br>

All surfaces that you want to spawn objects on must contain WillowTerrainSettings component and any type of collider.<br>
Note: Further in the documentation, GameObject with WillowTerrainSettings is replaced by Terrain for convenience<br>

There are 3 actions you can do:
<details>
  <summary>Place</summary>
	Instantiating objects on the surface.
	There are three types of detecting surfaces to spawn on:
		- Only on Terrain
		- Only on Objects
		- Both on Terrain and Objects
</details>	
- Erase
- Exchange

<hr>

<details>

## <summary><strong>FAQ</strong></summary>
  
#### What is the minimum Unity version to use Willow?

Your project should be on Unity 2019.4.23f and higher.

#### I want to get it, how do I import it into my project?

Instruction:
1. Download last [Release](https://github.com/AnanasikDev/Willow/releases)
1. Go to your Unity project, go to Assets/Import Package/Custom Package and select the .unitypackage file you just downloaded
1. Click Import in the following window and wait for import.

#### I cant spawn objects, what may be wrong?

1. Make sure Willow is enabled.<br>
1. Make sure your surface that you want to spawn objects on contains WillowTerrainSettings component. It is important!<br>
1. Make sure you have at least one active spawnable object with setted Object.<br>
1. Make sure parameter Placement Type of global settings is OnlyOnTerrain or OnTerrainAndObjects, otherwise it is only possible to spawn objects on other objects.<br>
1. Make sure brush size is setted above 0.<br>

#### I cant see brush, what should I do?

This is a temporary bug I am trying to fix. You need to close Willow window and open it again.

</details>