# NOLiveryPlus
A BepInEx mod and unity editor tool combo for making Nuclear option Liveries with full control over the material

# LiveryPlus Creation Guide

## Unity project Setup (same as the offical livery tool)

1. **Install Unity Hub**: [Unity Hub Download](https://unity.com/download)

2. **Install Unity 2022.3.6f1**: [Unity Editor 2022.3.6f1](https://unity.com/releases/editor/archive)

3. **Download the Modified Unity Project** ZIP file

4. **Open Project**: Launch Unity Hub and open the downloaded project.

## Create an Assetbundle

1. **Import & Apply texture**:

	* Drag your livery texture into the project's `Assets` folder.
	* Double click on the `COIN_skin_matte_gray_b` prefab object, on the right you will find the inspector window with a MeshRenderer component.
	* In the Meshrender component, replace the `Materials` field with your own livery texture.
	* Scroll down a bit to find the Materials settings.
	* Add your texture to `Surface Inputs` -> `Base Map`
	* To add emissions - enable the `Emission` checkbox, add your texture to `Emission Map`, and set the colour to white. (or mess with the material however you like, im not your mom. fun stuff i've found include making it transparent or adding the texture to `Height Map` to make the texture move with the camera for a neat effect. you can probably do some real funky stuff with all the different fields)

2. **IMPORTANT**
	* Rename the prefab to have the **exact** same name as the livery you're trying to update. the name is used as a key to find the correct ingame livery to overwrite.

3. **Assign AssetBundle**:

	* Select the prefab in the editor.
	* In the inspector (lower-right), under 'AssetBundle', click the dropdown and select `New…`.
		(If you don't see it, it might be hidden, in the lower edge of the inspector window is a bar `Example ============`, click on it)
	* Name it something, preferably same to the livery name.
	* Only 1 prefab per bundle is allowed.

4. **Build & Export**:

	* In Unity's top bar menu, click `liveryplus -> Build AssetBundle`.
	* Locate your file in the unity project folder: LiveryPlus/Assets/AssetBundles/<yourliveryfilename>.liveryplus

Your liverybundle file is now ready. Put it in the same folder as the bepinex mod `liveryplus.dll`

# CREDITS
	* Nikkorap - Unity project and bepinex mod
	
	* Offiry - Sample livery texture, testing, liveries
	[offiry's 0th SW Revoker](https://steamcommunity.com/sharedfiles/filedetails/?id=3452644234)
	[offiry's 0th SW Vortex](https://steamcommunity.com/sharedfiles/filedetails/?id=3452644393)
	[offiry's 0th SW Ifrit](https://steamcommunity.com/sharedfiles/filedetails/?id=3452644584)
	
	* Littlesneez - testing, liveries
	["Air Tactical Orchestra" FS-12 Revoker](https://steamcommunity.com/sharedfiles/filedetails/?id=3472977541)
	["Circuit" EW-25 Medusa](https://steamcommunity.com/sharedfiles/filedetails/?id=3494005532)
	["Sneeze" KR-67 Ifrit](https://steamcommunity.com/sharedfiles/filedetails/?id=3465210615)
	["CIT" KR-67 Ifrit](https://steamcommunity.com/sharedfiles/filedetails/?id=3465210309)
