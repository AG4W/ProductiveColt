using UnityEngine;

public class CharacterLocomotion : MonoBehaviour
{
    [Range(1f, 10f)][SerializeField]float _movementSpeed = 100f;
    [Range(1f, 100f)][SerializeField]float _jumpStrength = 100f;

    [SerializeField]float _groundedThreshold = .1f;

    Entity _entity;

    Rigidbody _rigidbody;
    Camera _camera;

    [SerializeField]bool _isGrounded = true;

    void Awake()
    {
        _entity = this.GetComponent<Entity>();
        _rigidbody = this.GetComponent<Rigidbody>();
        _camera = this.GetComponentInChildren<Camera>();

        InitializeEvents();
    }

    void InitializeEvents()
    {
        _entity.events.Subscribe(LocalEvent.UpdateMouseInput, (object[] args) => OnMouseInputUpdated((Vector2)args[0]));
        _entity.events.Subscribe(LocalEvent.UpdateMovementInput, (object[] args) => OnMovementInputUpdated((Vector2)args[0]));
        _entity.events.Subscribe(LocalEvent.Jump, (object[] args) => Jump());
    }

    void FixedUpdate()
    {
        if (_rigidbody.velocity.magnitude < .5f)
            _rigidbody.velocity = Vector3.zero;

        UpdateGroundedStatus();
    }

    void OnMouseInputUpdated(Vector2 input)
    {
        _rigidbody.rotation = Quaternion.AngleAxis(input.x, Vector3.up) * _rigidbody.rotation;

        if(_camera != null)
            _camera.transform.rotation = Quaternion.AngleAxis(input.y, _rigidbody.transform.right) * _camera.transform.rotation;
    }
    void OnMovementInputUpdated(Vector2 input)
    {
        Vector3 relative = (this.transform.forward * input.y) + (this.transform.right * input.x);

        if (_isGrounded)
            _rigidbody.AddForce(relative * _movementSpeed, ForceMode.VelocityChange);
            //_rigidbody.MovePosition(this.transform.position + (relative * _movementSpeed * Time.fixedDeltaTime));
    }
    void Jump()
    {
        if (!_isGrounded)
            return;

        _rigidbody.AddForce(Vector3.up * _jumpStrength, ForceMode.VelocityChange);
    }

    void UpdateGroundedStatus()
    {
        Ray ray = new Ray(this.transform.position + (Vector3.up * .25f), Vector3.down);

        _isGrounded = Physics.Raycast(ray, _groundedThreshold);
        _rigidbody.drag = _isGrounded ? 10f : 0f;

        Debug.DrawRay(this.transform.position + (Vector3.up * .25f), Vector3.down * _groundedThreshold, Color.yellow);
    }
}