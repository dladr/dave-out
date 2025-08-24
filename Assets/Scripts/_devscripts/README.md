# _devscripts - Unity Development Utilities

This folder contains reusable utility scripts for Unity projects. These scripts are designed to be generic and useful across multiple projects.

## Folder Structure

### 📁 **Core**
Essential utilities and extensions that form the foundation of most Unity projects.

- **`MonoBehaviourExtra.cs`** - Extended MonoBehaviour with coroutine helpers and lerp utilities
- **`SingletonManager.cs`** - Modern service locator pattern for managing singletons and tagged objects
- **`Extensions.cs`** - Useful extension methods for Color, Vector3, Collections, Transform, etc.
- **`ConversionExtensions.cs`** - Type conversion utilities and enum parsing
- **`HelperFunctions.cs`** - Static utility functions for common operations
- **`ObjectPool.cs`** - Simple object pooling system for performance optimization

### 📁 **Audio**
Audio-related utilities and managers.

- **`ClipPlayer.cs`** - Advanced audio clip player with randomization, queuing, and timer functionality
- **`MusicManager.cs`** - Priority-based music manager that handles persistence and conflicts
- **`MixerAutoAssign.cs`** - Automatically assigns unassigned AudioSources to Master mixer group

### 📁 **UI** 
User interface and visual effect components.

- **`SceneLoader.cs`** - Generic scene loader with fade support, auto-loading, and validation
- **`BlinkComponent.cs`** - Simple component to make UI elements blink on/off
- **`SimpleFader.cs`** - Screen fader for scene transitions and cinematic effects

### 📁 **Components**
General-purpose MonoBehaviour components for GameObjects.

- **`FollowObject.cs`** - Advanced object following with delay, selective axis, and offset support
- **`RandomRotation.cs`** - Applies random rotation on Awake

### 📁 **Utilities**
Helper utilities and convenience tools.

- **`UnityUIHelper.cs`** - Solutions for common Unity UI and camera positioning challenges

### 📁 **Rendering**
Graphics and rendering utilities.

- **`DynamicLineRenderer.cs`** - Creates dynamic lines between transforms with performance options

## Usage Notes

### Dependencies
- **Odin Inspector** - Many scripts use Odin attributes for better inspector experience
- **Unity 2022.3+** - Scripts use modern Unity APIs

### Getting Started
1. Import these scripts into your Unity project
2. Install Odin Inspector from the Asset Store
3. Start using the utilities in your scenes!

### Common Patterns

**Service Locator:**
```csharp
// Register a service
SingletonManager.Register<MyService>(myServiceInstance);

// Get a service
var myService = SingletonManager.Get<MyService>();

// Get a GameObject by tag (cached)
var player = SingletonManager.GetTag("Player");
```

**Extended MonoBehaviour:**
```csharp
public class MyScript : MonoBehaviourExtra
{
    void Start()
    {
        // Delayed invoke
        Invoke(() => Debug.Log("Hello!"), 2f);
        
        // Lerp with callback
        LerpCoroutine(value => transform.position = Vector3.up * value, 0f, 5f, 2f);
    }
}
```

**Extensions:**
```csharp
// Color utilities
myRenderer.color = myRenderer.color.SetAlpha(0.5f);

// Vector3 utilities  
transform.position = transform.position.Add(y: 2f);

// Collection utilities
var randomItem = myList.GetRandom();
```

**UI Canvas Solutions:**
```csharp
// Create a non-interfering canvas using Screen Space Camera mode
var canvas = UnityUIHelper.CreateNonInterferingCanvas("Game UI", Camera.main);

// Create a separate UI camera for complete separation
var uiCamera = UnityUIHelper.CreateUICameraSetup("UI Camera");
var canvas = UnityUIHelper.CreateNonInterferingCanvas("Game UI", uiCamera);

// Alternative: World space canvas positioned away from game
var canvas = UnityUIHelper.CreateWorldSpaceUICanvas(new Vector3(-50, 0, 0));
```

**Scene Fading:**
```csharp
// Simple screen fading (now with better canvas handling)
var fader = FindObjectOfType<SimpleFader>();
fader.FadeOut(1f); // Fade to black over 1 second
fader.FadeIn(0.5f); // Fade from black over 0.5 seconds
```

**Object Pooling:**
```csharp
// Setup an ObjectPool component in your scene
public class BulletSpawner : MonoBehaviour
{
    public ObjectPool bulletPool;
    
    void FireBullet()
    {
        var bullet = bulletPool.GetPooledObject();
        if (bullet != null)
        {
            bullet.transform.position = firePoint.position;
            // Configure bullet...
            
            // Bullet will auto-return to pool when destroyed or after delay
            bullet.GetComponent<PooledObject>().ReturnToPoolAfterDelay(5f);
        }
    }
}
```

## Contributing
When adding new scripts to this template:
1. Follow the existing folder structure
2. Use Odin Inspector attributes for better UX
3. Include comprehensive tooltips and documentation
4. Add validation methods where helpful
5. Follow C# naming conventions
