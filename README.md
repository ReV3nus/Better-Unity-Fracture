# Better Unity Fracture

## Planning and Design Doc

### Introduction

I have implemented serveral kinds of fire and water, which are two kinds of the fundatmental elements in WuXing. So this time I want to focus on the Earth element, which could be presented as dirt and rock.

When I think of these objects, I think of their textures and interactive performance, which means the Fracture.

But in most of the approaches in video games, these effects are implemented by pre-fragmentation with Voronoi, which means they cannot act precisely to the real-time interactions. 

Therefore, I want to implement a real-time interactive fracture effect with realisitic physical reacts and stylized effects.

#### Goal

In this final project, I want to implement the real-time interactive fracture with:
- 2 different hitting types, the point strucking and the plane cutting.
- Physically and realistically impact and shattering of objects.
- Real-time, high-efficiency computational simulation methods.
- Some stylized rendering effect to enhance the effect.

### Techniques:
- [Voronoi](https://en.wikipedia.org/wiki/Voronoi_diagram)
- [Unity Fracture](https://github.com/ElasticSea/unity-fracture)
- [OpenFracture](https://github.com/dgreenheck/OpenFracture)
- [Basic Fracture Mechanics](https://www.youtube.com/watch?v=jJMSvgcZaGA)

### Timeline:
- Week 1: Basic fracture and advanced attempts
- Week 2: Advanced fracture
- Week 3: Interactions and effects
- Week 4: Stylization and optimization

## Milestone 1

In this week, I mainly focused on the basic implementation of initial fracture.

At first, I was learning some basic fracture mechanics to understand theoretical methods and searching for practical implementations in Unity.

After that, I tried to use [Blast](https://docs.omniverse.nvidia.com/kit/docs/blast-sdk/latest/index.html#blast-sdk-documentation) in PhysX by Nvidia. But because it is a C++ library, so I need to compile it to .dlls and import them in Unity. But after some attempts, it seems to be more difficult than I thought because when I was writing some wrappers I found that some core functions I need have no API interfaces in Blast, and I cannot compile more extra Dlls because of outdated environmental issues.

Therefore, I utilized some wrapper logics from [Unity Fracture](https://github.com/ElasticSea/unity-fracture) and a [Blast Forum](https://discussions.unity.com/t/nvidia-blast/665733). 

By the way, actually I do considered about writing my own version of 3D voronoi but I'm afraid it will cost my many times and Blast have done many optimizations to make it run rany fast on CPUs.

At the end of the stage, I am able to fracture any objects in my Unity project into designated pieces.

<img width="1111" height="715" alt="image" src="https://github.com/user-attachments/assets/b7a1a1c9-0973-422f-b8ca-a27deae0e3da" />

## Milestone 2

In this two weeks, I mainly focused on the whole framework of fracture using the newest Blast and Unity.

First work I've done is that I fully reimplemented the wrappers in Unity, as well as the interfaces in the newest Blast. Later I'll put relevant code and introduction in another repository Unity Blast. Besides, I also implemented some extra extensions of the original Blast, including custom fracture, neightbor fetch, crack export and so on. I've already used some optimizations in these extensions so I think they won't took much cost.

Additionaly, using my newest wrapper and extensions, I am able to implement some simple crack effects in Unity currently. Now I have implemented some primary Light FX from cracks and particle systems.

<img width="852" height="683" alt="image" src="https://github.com/user-attachments/assets/89bfad15-90e8-4c86-b4a4-162b305e6f43" />

<img width="863" height="555" alt="image" src="https://github.com/user-attachments/assets/3dda2557-8bd2-411a-b1e5-78088361acec" />

However, there's still a lot of optimizations to do and I haven't arrange my project properly yet, so I won't upload my code until I finished the whole structure.

In the next and the last week, I'm going to finish all of this and present a better animation and FXs.



