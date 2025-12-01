# Better Unity Fracture

## Introduction

**Better Unity Fracture** provides multiple fracturing methods in Unity. Users can use preprocessing or real-time fracturing methods to cut and shatter almost any mesh geometry, along with several interesting configurable effects.

In the sample scene, the objects are fractured by a "BlastPattern" fracturing method during runtime after being hit. After that the special effects will be played and fractured chunks will appear and dissolve in few seconds.



https://github.com/user-attachments/assets/76b35e49-44a8-4ea8-a3d0-c1e9cfea53ad


https://github.com/user-attachments/assets/0a000796-72d3-43be-86cc-5e9a64369e69


https://github.com/user-attachments/assets/d105580c-980c-47b0-b435-15e90b42f6e7

## Requirement

- Unity 6+ with URP rendering pipeline (other versions might work after some modifications)
- DLL library for the corresponding operating system (will be explained in detail later)

## How to use

- **Main Scripts Structure**:
    - *BetterFracture.cs* : The main script used to call and execute the fracture method.
    - *FracturedObject.cs* : Fracturing process handling script mounted on the shattered object
    - *FractureFX.cs, ChunkCollision.cs, ChunkFX.cs* : Fracturing special effects handling script
    - *NvBlast...Wrapper.cs* : Wrappers to use `Blast` library functions.

- To create your own fracture scene, you should first put the *BUF_FX_SYSTEMS.prefab* into your current scene. This prefab contains all the special effects and other configurable params in it or its child prefabs.
- For the target GameObject you wish to fracture, make sure it has a readable mesh structure and uvs.
- Mount and use *BetterFracture.cs* to fracture objects. This script can be used in two ways. You can either mount it to the target object you want to fracture or mount it to a controller object and specify a target object. Don't forget to drag the *BUF_FX_SYSTEMS.prefab* in your scene to the script.
- If you want to modify the effects or add your owns, modify the *FractureFX.cs* script or the *BUF_FX_SYSTEMS.prefab* in your scene.

## Known Issues

- Dissolve FX: Imcomplete shader graph. This can lead to a sudden change in the material's appearance. And it also does not support multiple material displays, so internal textures may not work in special effects.
- Optimizations: The load on the fracturing algorithm itself can cause stuttering in the early stages of crushing. This is a drawback of real-time fracturing methods.

## Other Details

This project is also a usage example of [Unity Blast](https://github.com/ReV3nus/Unity-Blast). A detailed introduction of main functions in `Blast` and wrappers will be provided in that project. Some of the textures and particle effects are modified from [Unity Particle Pack](https://assetstore.unity.com/packages/vfx/particles/particle-pack-127325).

Only the x64 version *.dll* is contained in this repository. To get your compiled libraries of corresponding operating system, you can also check [Unity Blast](https://github.com/ReV3nus/Unity-Blast) for details instructions.

The general working logic is as follows:
- Initialize voronoi sites and fracture using given meshes and configurations.
- Calculate cracks and start playing special effects
- After a period of time, proceed to the next stage and generate chunk meshes with some other special effects.
- At last, start dissolve animation and clear all chunks and meshes.


https://github.com/user-attachments/assets/912eca1b-0412-443f-b754-7e83a3c7d4bf



### Future works:

More fracturing methods. More optimizations.
