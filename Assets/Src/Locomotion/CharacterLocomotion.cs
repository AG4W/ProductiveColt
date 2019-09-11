using System.Collections;
using UnityEngine;

public class CharacterLocomotion : MonoBehaviour
{
    [Range(100f, 2000f)][SerializeField]float _movementSpeed = 100f;
    [Range(100f, 1000f)][SerializeField]float _jumpStrength = 100f;

    Entity _entity;

    Rigidbody _rigidbody;
    Camera _camera;

    bool _isJumping = false;

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
        if (_rigidbody.velocity.magnitude < .25f)
            _rigidbody.velocity = Vector3.zero;
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

        if (!_isJumping)
            _rigidbody.AddForce(relative * _movementSpeed, ForceMode.Force);
            //_rigidbody.MovePosition(this.transform.position + (relative * _movementSpeed * Time.fixedDeltaTime));
    }
    void Jump()
    {
        _rigidbody.AddForce((_rigidbody.velocity * _jumpStrength) + (Vector3.up * _jumpStrength), ForceMode.Impulse);
        _isJumping = true;

        StartCoroutine(JumpCooldown());
    }

    IEnumerator JumpCooldown()
    {
        yield return new WaitForSeconds(1f);
        _isJumping = false;
    }
}