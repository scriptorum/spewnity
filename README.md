# spewnity
This is a collection of Unity classes and shaders I've written or assembled primarily for use in game jams. The focus is on 2D, but there are some general purpose classes in here you might like.

### Classes
*SoundManager* organizes your sounds. Each one can consist of multiple variations. Sounds can be played back with custom pitch/pan/volume, and those same characteristics can be randomly altered. SoundManager also manages a pool of AudioSources, so you can just `SoundManager.instance.Play("mysound")`. Supports events.

*TweenManager* organizes your tweens. Each tween consists of a target and changes in position/rotation/scale/color over time. (For color, you'll need a component with a color property, such as SpriteRenderer, Renderer.Material, Text, TextMesh, or TextMeshPro.) Tweens can be ping ponged, reversed, and given custom easing. Multiple tweens can be scheduled into a composite. Supports events. Tweens also don't need a target - you can just tween between two values and get a Change event callback with the value.

*Toolkit* supports many extension methods for lerping, randomizing, integer-based math, string manipulation, and GameObject assistance.

*ObjectPooler* includes a class to make a generic object pool, a GameObjectPool, and a ObjectPooler that manages multiple pools.

*CameraDirector* provides camera assistance: shaking, continuous target following, cutting and dollying.

*ActionQueue* executes a series of commands in order. Great for scripted/timed game events.

*Anim* is an easy to use, lightweight animator.

*Map* is a generic class for manipulating a 2D grid content. Get neighbors, and enumerate over cells in various traversal orders.

*ParticleMonitor* tracks particle birth and death. Supports events. Can self-destruct GameObject when ParticleSystem is finished.

*SceneLoader* triggers the loading of another scene when scene conditions are met: Awake, Start, TriggerEnter2D, MouseDown, Keydown. Scenes can be loaded additively by name or by order. Supports events.

*InputController* manages your inputs in one location. Gives you an event driven model for responding to events.

*CallbackHelper* provides many support functions for testing your code, focused around callbacks. Keep counts of calls, fire events during awake, start, log messages on callbacks, trigger callback from inspector, etc.

*RepositionAttribute* lets your change the order of properties in the inspector. The named properties are pulled to the top.

*Point* is a 2D integer position for working with grids.

*CoroutineHelper* is a MonoBehavior that can run your coroutines for you.

*DontDestroyOnLoad* calls DontDestroyOnLoad() for you.

*EnforceBoundary* is a hard X/Y boundary limiter for any GameObject.

*Parallax* manages parallax movement, moving GameObjects along X and/or Y at a relative speed to the camera.

*VerticalDrawOrder* sets the sortingOrder of the SpriteRenderer relative to the vertical position of the GameObject's transform.

*Letterbox* resizes the camera's viewpoint to maintain the desired aspect ratio. Adds letter/pillar box black bars.

### Credits
 - Some shaders from http://forum.unity3d.com/threads/shaders-for-2d-games.71748/ courtesy Dasinf
 - LineMup from http://wiki.unity3d.com/index.php/LineMup courtesy Matthew J. Collins
 - TransformContextMenu from http://wiki.unity3d.com/index.php/TransformContextMenu courtesy Zach Aikman
 - SoundManager inspired by https://unity3d.com/learn/tutorials/projects/2d-roguelike/audio?playlist=17150 by Matthew Schell
 - Export Package Plus inspired byfrom http://forum.unity3d.com/threads/layers-tags-in-unity-packages.87418/ by Mikael H


