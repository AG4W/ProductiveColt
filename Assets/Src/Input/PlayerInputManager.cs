using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    [SerializeField]Vector2 _lookSensitivity = new Vector2(2f, 2f);
    [SerializeField]bool _invertY = false;

    Entity _entity;

    Vector2 _mouseInput;
    Vector2 _movementInput;

    void Awake()
    {
        _entity = this.GetComponent<Entity>();
        _mouseInput = new Vector2();

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        //get inputs
        _mouseInput.x = Input.GetAxisRaw("Mouse X") * _lookSensitivity.x;
        _mouseInput.y = Input.GetAxisRaw("Mouse Y") * _lookSensitivity.y;

        _movementInput.x = Input.GetAxisRaw("Horizontal");
        _movementInput.y = Input.GetAxisRaw("Vertical");
        _movementInput.Normalize();

        if (!_invertY)
            _mouseInput.y *= -1f;

        if (Input.GetKeyDown(KeyCode.T))
            GlobalEventBus.Raise(GlobalEvent.ToggleSlowMotion);
        if (Input.GetKeyDown(KeyCode.Space))
            _entity.events.Raise(LocalEvent.Jump);

        _entity.events.Raise(LocalEvent.Fire, Input.GetKey(KeyCode.Mouse0));
        _entity.events.Raise(LocalEvent.UpdateAimingInput, Input.GetKey(KeyCode.Mouse1));

        _entity.events.Raise(LocalEvent.UpdateCrouchInput, Input.GetKey(KeyCode.LeftControl));
    }
    void FixedUpdate()
    {
        _entity.events.Raise(LocalEvent.UpdateMouseInput, _mouseInput);
        _entity.events.Raise(LocalEvent.UpdateMovementInput, _movementInput);
    }
}
