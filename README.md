# Tilt-Brush-QV-Pen
Quest compatible VRC Prefab

Requirements: Udon#, VRChat SDK3

![tilt brush vrc](https://user-images.githubusercontent.com/93958928/147859838-33bf47ad-c2dd-45b9-86ff-62b5f7acf142.gif)


 ** Check off Allow 'Unsafe Code' in the Player tab of the Project Settings

![player settings](https://user-images.githubusercontent.com/93958928/147859545-5fe32b22-21ef-440e-82a9-e13523fb6cbb.PNG)

<i>
note: The VelvetInk WaveForm and Bloom brush shaders have different include paths than the other tilt brush shaders.

<b>If you're testing new tiltbrush shaders from the tilt brush unity sdk /b>, make sure the the Brush.cgic file is set to the direct path
  
#include "Assets/Tilt Brushes by Smew/UnitySDK/Assets/TiltBrush/Assets/Shaders/Include/Brush.cginc"

..Same goes for other include errors
  
There are more VRC/Quest compatible shaders in here, I just haven't tested them all. I know the Ink splatter and toon works 
but the toon doesn't generate the 3D toon objects yet
  
</i>

![VRChat_1920x1080_2022-01-01_02-24-13 659](https://user-images.githubusercontent.com/93958928/147859577-f3d01a11-a4c2-4adf-ab95-df3d3eb74314.png)
