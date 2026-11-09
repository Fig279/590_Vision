using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Optional add-on: toggles vision from its own Input Action, so no existing
/// controller script or Input Actions asset needs to be edited.
/// Add this to the player (or any object) and untick "Use Built In Toggle Key"
/// on the VisionManager. Rebind or add bindings (e.g. a gamepad button) in the Inspector.
/// </summary>
public class VisionToggleInput : MonoBehaviour
{
    [SerializeField]
    private InputAction toggleAction = new InputAction("ToggleVision", InputActionType.Button, "<Keyboard>/v");

    private void OnEnable()
    {
        toggleAction.performed += OnTogglePerformed;
        toggleAction.Enable();
    }

    private void OnDisable()
    {
        toggleAction.performed -= OnTogglePerformed;
        toggleAction.Disable();
    }

    private void OnTogglePerformed(InputAction.CallbackContext ctx)
    {
        if (VisionManager.Instance != null)
            VisionManager.Instance.Toggle();
    }
}
