#Willow

Willow is Unity asset for working with instantiating objects on surfaces in Editor.
For initialize Willow you need to open Tools -> Willow -> Prefab Brush. To start working you need to enable it in Editor window.

All surfaces that you want to spawn objects on must contain WillowTerrainSettings component and any type of collider.
Note: Further in the documentation, GameObject with WillowTerrainSettings is replaced by Terrain for convenience

There are 3 actions you can do:
<details>
  <summary>Place</summary>
  <p>
	Instantiating objects on the surface.
	There are three types of detecting surfaces to spawn on:
		- Only on Terrain
		- Only on Objects
		- Both on Terrain and Objects
  </p>
</details>
- Erase
- Exchange

<details>
  <summary>FAQ</summary>
  <p>
	<h3>I cant spawn objects, what may be wrong?</h3>
	1. Make sure Willow is enabled.
	2. Make sure your surface that you want to spawn objects on contains WillowTerrainSettings component. It is important!
	3. Make sure you have at least one active spawnable object with setted Object.
	4. Make sure parameter Placement Type of global settings is OnlyOnTerrain or OnTerrainAndObjects, otherwise it is only possible to spawn objects on other objects.
	5. Make sure brush size is setted above 0.
  </p>
</details>