using UnityEngine.InputSystem;

// Unity InputSystem V1.4.4 does not support initialising the C# generated classes with the InputActionAsset in the project.
// It builds a new one from json. This means the InputActionAsset in the project, can never be accessed with the wrapper through code.
// Only the new one built when the C# class is constructed.
// This class (LinkedInputActionAssets) is a workaround that holds a reference to two InputActionAssets.
// Using the methods in this class to enable and disable maps, it keeps them both in sync.
// This means dragging and dropping references to InputActions and accessing them through code through the InteractionSystem will yield the same results.

// There is a TODO line in the InputSystem V1.4.4 to add this functionality.
// There is also a github pull request with the feature that has been sitting there since Oct 2021: https://github.com/Unity-Technologies/InputSystem/pull/1414

namespace InteractionSystemToolsAndAssets
{
    /// <summary>
    /// Links two InputActionAssets together, keeping the enabled/disabled maps in sync. Used by the <see cref="InteractionSystem"/>. 
    /// <para>Typically one InputActionAsset is the one generated by code, and the other, Resources.Load()</para>
    /// <para>This is a workaround for the InputSystem not allowing the C# classes generated by InputActionAssets to be initialised with the source asset as a reference.</para>
    /// More info in class file.
    /// </summary>
    public class LinkedInputActionAssets
    {
        private InputActionAsset _asset1;
        private InputActionAsset _asset2;

        public InputActionAsset CodeAsset => _asset1;
        public InputActionAsset ResourceAsset => _asset2;

        public LinkedInputActionAssets(InputActionAsset codeAsset, InputActionAsset resourceAsset)
        {
            _asset1 = codeAsset;
            _asset2 = resourceAsset;
        }

        public void EnableAllMaps()
        {
            _asset1.Enable();
            _asset2.Enable();
        }

        public void EnableMap(InputActionMap map)
        {
            _asset1.FindActionMap(map.name)?.Enable();
            _asset2.FindActionMap(map.name)?.Enable();
        }
        public void DisableMap(InputActionMap map)
        {
            _asset1.FindActionMap(map.name)?.Disable();
            _asset2.FindActionMap(map.name)?.Disable();
        }

        public bool ContainsAsset(InputActionAsset asset)
        {
            if (_asset1 == asset) return true;
            if (_asset2 == asset) return true;
            return false;
        }
        public bool ContainsMap(InputActionMap map)
        {
            if (_asset1.FindActionMap(map.id) != null) return true;
            if (_asset2.FindActionMap(map.id) != null) return true;
            return false;
        }
        public bool ContainsAction(InputAction action)
        {
            if (_asset1.FindAction(action.id) != null) return true;
            if (_asset2.FindAction(action.id) != null) return true;
            return false;
        }
    }
}