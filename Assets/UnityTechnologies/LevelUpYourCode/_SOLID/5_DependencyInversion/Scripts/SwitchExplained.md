# DIP in Unity: Using Interfaces in the Inspector with a Switch Example

Ever wanted to make a script that can interact with different kinds of objects without needing to know their specific
concrete types beforehand? This `Switch` script demonstrates a common method to achieve this in Unity.

## The Code (Switch.cs)

First, let's look at the full `Switch.cs` script:

```csharp
using UnityEngine;

namespace DesignPatterns.DIP
{
    /// <summary>
    /// A Switch component that can toggle the state of an ISwitchable client. This class demonstrates
    /// the Dependency Inversion Principle by depending on an abstraction (ISwitchable) rather than concrete implementations.
    /// </summary>
    public class Switch : MonoBehaviour
    {
        // Unity's serialization system does not directly support interfaces. Work around this limitation
        // by using a serialized reference to a MonoBehaviour that implements ISwitchable.

        [SerializeField] private MonoBehaviour m_ClientBehavior;
        private ISwitchable m_Client => m_ClientBehavior as ISwitchable;

        void Awake()
        {
            if (m_ClientBehavior == null)
            {
                Debug.LogError($"ClientBehavior not assigned on Switch '{gameObject.name}'. Disabling.", this);
                enabled = false;
                return;
            }

            if (m_ClientBehavior is ISwitchable) return; 

            Debug.LogError($"ClientBehavior on Switch '{gameObject.name}' does not implement ISwitchable. Disabling.", this);
            enabled = false;
        }

        /// <summary>
        /// Toggles the active state of the associated ISwitchable client.
        /// If the client is active, it deactivates it; otherwise, it activates it.
        /// </summary>
        public void Toggle()
        {
            if (!enabled) 
            {
                return;
            }

            if (m_Client.IsActive)
            {
                m_Client.Deactivate();
            }
            else
            {
                m_Client.Activate();
            }
        }

        /// <summary>
        /// Validates that the assigned client behavior implements the ISwitchable interface.
        /// Logs a warning if it doesn't. This is useful for editor-time validation.
        /// </summary>
        void OnValidate()
        {
            if (m_ClientBehavior is not null and not ISwitchable)
            {
                Debug.LogWarning(
                    $"Assigned ClientBehavior on '{gameObject.name}' must implement ISwitchable.", this);
            }
        }
    }

    /// <summary>
    /// Defines the contract for objects that can be switched on or off.
    /// This interface allows the Switch to interact with various types of objects
    /// (e.g., doors, traps, lights) in a uniform way.
    /// </summary>
    public interface ISwitchable
    {
        bool IsActive { get; }
        void Activate();
        void Deactivate();
    }
}
```

## How It Works

1. **The Problem:** You want a `Switch` that can alter the state of an entity—be it a `Door`, a `Trap`, a `Light`, or
   another type of object. It is inefficient to write a separate `Switch` implementation for each distinct type.

2. **The Solution (Interfaces):**
    * We define an `ISwitchable` "contract" (an interface). Any component that is intended to be switchable (such as a
      `Door` or `Trap` script) must implement this interface, which mandates an `IsActive` property, an `Activate()`
      method, and a `Deactivate()` method.
    * Example:
     ```csharp
     // In your Door.cs
     public class Door : MonoBehaviour, ISwitchable
     {
         private bool m_IsOpen = false;
         public bool IsActive => m_IsOpen;

         public void Activate() {
             m_IsOpen = true;
             Debug.Log("Door Opened!");
             // Add door opening logic
         }

         public void Deactivate() {
             m_IsOpen = false;
             Debug.Log("Door Closed!");
             // Add door closing logic
         }
     }
     ```

3. **The Inspector Trick:**
    * Unity's Inspector cannot directly display a field for an interface type like "ISwitchable".
    * Therefore, in `Switch.cs`, we declare `[SerializeField] private MonoBehaviour m_ClientBehavior;`. This creates a
      slot in the Inspector where any component deriving from `MonoBehaviour` (i.e., any script) attached to a
      GameObject can be assigned.
    * When you assign your `Door` component (which is a `MonoBehaviour` and also implements `ISwitchable`) to this slot,
      `m_ClientBehavior` then references that `Door` component instance.

4. **Making it Usable:**
    * The line `private ISwitchable m_Client => m_ClientBehavior as ISwitchable;` is an effective mechanism. It
      attempts to cast the `MonoBehaviour` reference assigned in the Inspector (`m_ClientBehavior`) to the
      `ISwitchable` interface type.
    * If the assigned component (e.g., your `Door` script) correctly implements `ISwitchable`, the cast succeeds, and
      `m_Client` will hold a valid reference to it as an `ISwitchable`. Consequently, the `Toggle()` method can invoke
      `m_Client.Activate()` or `m_Client.Deactivate()` without needing to know the concrete type of the client, provided
      it adheres to the `ISwitchable` contract.

## A Note for Server-Side C# Developers: Unity\'s Approach to Dependencies

If you\'re coming from a background in server-side .NET development, you might be used to injecting dependencies through
constructors and validating them there. Here\'s how Unity\'s common practices differ for `MonoBehaviour` components like
our `Switch`:

* **No Constructor Injection for Inspector Fields:** While `MonoBehaviour`s can have constructors, Unity doesn\'t use
  them to inject dependencies that you assign in the Inspector (like `m_ClientBehavior`). Unity manages the creation
  and serialization of these objects.
* **`Awake()` as the Initializer:** The `Awake()` method is the first Unity-specific lifecycle method called after a
  `MonoBehaviour` is instantiated and its serialized fields (like those set in the Inspector) are populated. This makes
  `Awake()` the idiomatic place for runtime initialization and critical dependency validation, similar to how you might
  use a constructor for validation in other .NET contexts. If `m_ClientBehavior` isn\'t set up correctly, `Awake()` in
  our `Switch` logs an error and disables the component.
* **`OnValidate()` for Editor-Time Checks:** `OnValidate()` is a Unity editor-specific callback that runs when a script
  is loaded or a value is changed in the Inspector. It provides immediate feedback *during design time*, helping you
  catch configuration errors before even running the game. This is a proactive way to ensure dependencies are correctly
  assigned.
* **DI Frameworks as an Alternative:** For more complex projects or for those who prefer a more traditional DI approach,
  Unity does support dedicated Dependency Injection frameworks (e.g., VContainer, Zenject). These can provide features
  like constructor injection for `MonoBehaviour`s, but they introduce their own setup and are typically adopted for
  larger-scale applications. The pattern shown in this `Switch` example is a fundamental and common way to handle
  dependencies directly within Unity.

## Wiring It Up in the Unity Editor

1. Create a C# script, say `Door.cs`, and make sure it inherits from `MonoBehaviour` and implements `ISwitchable` (like
   the example above).
2. Create another script, say `Trap.cs`, also implementing `ISwitchable`.
3. In your scene, create a GameObject (e.g., an empty GameObject or a 3D model for a switch). Attach the `Switch.cs`
   script to it.
4. Create a GameObject for your door. Attach the `Door.cs` script to it.
5. Create a GameObject for your trap. Attach the `Trap.cs` script to it.
6. Select the GameObject that has the `Switch.cs` script. In the Inspector, you'll see a field for the `Switch`
   component. It will be labeled "Client Behavior" (Unity automatically formats `m_ClientBehavior` for better
   readability).
7. Drag the GameObject that has your `Door.cs` script from the Hierarchy into the "Client Behavior" slot on the
   `Switch` component.
    * Now, when `Toggle()` is called on this `Switch` instance, it will activate/deactivate that specific `Door`.
8. If you had another `Switch` instance and assigned your `Trap` GameObject to its "Client Behavior" slot, that switch
   would control the trap.

**Why this is advantageous (even if you don't prioritize SOLID principles):**

This setup allows for the reuse of the `Switch` script for many different types of objects without modifying the
`Switch` script itself. You merely specify *which* `MonoBehaviour` (that implements `ISwitchable`) it should interact
with via the Inspector. If you later develop a new `ISwitchable` entity, such as a `LaserBarrier`, your existing
`Switch` script is already equipped to work with it.

The `OnValidate` method in `Switch.cs` is a beneficial feature: it checks in the editor if the component assigned to
   the "Client Behavior" slot correctly implements `ISwitchable`. If not, it will display a warning in the console,
   assisting in the early detection of configuration errors.
