using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// used to turn on and off the enhanced vision manager
/// this woud likely be on a player script but I didn't want to touch the script 
/// </summary>
public class VisionToggleInput : MonoBehaviour
{
    // the key used to turn on / off the vision enhancement mode, if no specification assumes v(for vision)
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

    /// <summary>
    /// turns on the VisionManager
    /// </summary>
    /// <param name="ctx"></param>
    private void OnTogglePerformed(InputAction.CallbackContext ctx)
    {
        if (VisionManager.Instance != null)
            VisionManager.Instance.Toggle();
    }
}
