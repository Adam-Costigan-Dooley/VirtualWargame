using UnityEngine;
using UnityEngine.InputSystem;

public static class InputHelper
{

    public static bool InteractPressedThisFrame()
    {
        bool keyboardPressed =
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame;

        bool mousePressed = false;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            float x = Mouse.current.position.ReadValue().x;
            float w = Screen.width;
            if (w > 0f && x >= w / 3f && x <= 2f * w / 3f)
            {
                mousePressed = true;
            }
        }

        bool touchPressed = false;
        if (Touchscreen.current != null)
        {
            float w = Screen.width;
            if (w > 0f)
            {
                foreach (var t in Touchscreen.current.touches)
                {
                    if (!t.press.wasPressedThisFrame) continue;

                    float x = t.position.ReadValue().x;
                    if (x >= w / 3f && x <= 2f * w / 3f)
                    {
                        touchPressed = true;
                        break;
                    }
                }
            }
        }

        return keyboardPressed || mousePressed || touchPressed;
    }

    public static float GetTouchHorizontal()
    {
        float move = 0f;
        float w = Screen.width;
        if (w <= 0f) return 0f;

        if (Touchscreen.current != null)
        {
            foreach (var t in Touchscreen.current.touches)
            {
                if (!t.press.isPressed) continue;

                float x = t.position.ReadValue().x;

                if (x < w / 3f)      move = -1f;
                else if (x > 2f * w / 3f) move = 1f;
            }
        }
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            float x = Mouse.current.position.ReadValue().x;

            if (x < w / 3f)      move = -1f;
            else if (x > 2f * w / 3f) move = 1f;
        }

        return move;
    }
}
