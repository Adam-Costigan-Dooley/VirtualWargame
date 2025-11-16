using UnityEngine;
using UnityEngine.InputSystem;

public class TouchInputHandler : MonoBehaviour
{
    public static TouchInputHandler Instance { get; private set; }

    public bool LeftHeld  { get; private set; }
    public bool RightHeld { get; private set; }
    public bool MiddleTappedThisFrame { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        LeftHeld  = false;
        RightHeld = false;
        MiddleTappedThisFrame = false;

        if (Touchscreen.current == null)
            return;

        float screenWidth = Screen.width;
        if (screenWidth <= 0f) return;

        foreach (var touchControl in Touchscreen.current.touches)
        {
            if (!touchControl.press.isPressed)
                continue;

            var pos = touchControl.position.ReadValue();
            float xNorm = pos.x / screenWidth;

            if (xNorm < 1f / 3f)
            {
                LeftHeld = true;
            }
            else if (xNorm > 2f / 3f)
            {
                RightHeld = true;
            }
            else
            {
                if (touchControl.press.wasPressedThisFrame)
                {
                    MiddleTappedThisFrame = true;
                }
            }
        }
    }
}
