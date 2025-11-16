using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private PlayerControls _controls;
    private Rigidbody2D _rb;
    private float _moveInputX;

    private void Awake()
    {
        _controls = new PlayerControls();
        _rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        _controls.Player.Enable();
    }

    private void OnDisable()
    {
        _controls.Player.Disable();
    }

    private void Update()
    {
        float inputX = 0f;

        if (_controls.Player.MoveLeft.IsPressed())
            inputX -= 1f;

        if (_controls.Player.MoveRight.IsPressed())
            inputX += 1f;

        float touchMove = InputHelper.GetTouchHorizontal();
        if (Mathf.Abs(touchMove) > 0.01f)
        {
            inputX = touchMove;
        }

        _moveInputX = inputX;

        if (_moveInputX > 0.1f)
        {
            var scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        else if (_moveInputX < -0.1f)
        {
            var scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    private void FixedUpdate()
    {
        var velocity = _rb.linearVelocity;
        velocity.x = _moveInputX * moveSpeed;
        velocity.y = 0f;
        _rb.linearVelocity = velocity;
    }
}
