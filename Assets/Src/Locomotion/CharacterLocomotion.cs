using UnityEngine;

public class CharacterLocomotion : MonoBehaviour
{
    [Range(100f, 300f)][SerializeField]float _movementSpeed = 2.5f;
    [Range(1f, 100f)][SerializeField]float _jumpStrength = 10f;

    [Range(.01f, 1f)][SerializeField]float _groundedThreshold = .25f;
    [Range(.01f, 1f)][SerializeField]float _obscuredThreshold = 1f;
    [Range(.5f, 5f)][SerializeField]float _slideSpeedThreshold = 5f;

    [Range(.0001f, 1f)][SerializeField]float _airborneSpeedModifier = .1f;
    [Range(.0001f, 1f)][SerializeField]float _crouchedSpeedModifier = .25f;
    [Range(.0001f, 1f)][SerializeField]float _aimingSpeedModifier = .5f;
    [Range(.0001f, 10f)][SerializeField]float _slidingSpeedModifier = 2f;

    Entity _entity;

    Rigidbody _rigidbody;

    [SerializeField]CapsuleCollider _collider;
    [SerializeField]GameObject _cameraJig;
    [SerializeField]GameObject _weaponJig;

    [SerializeField]Vector3 _weaponJigIdlePosition;
    [SerializeField]Vector3 _weaponJigIdleEulerAngles;
    [SerializeField]Vector3 _weaponJigObscuredPosition;
    [SerializeField]Vector3 _weaponJigObscuredEulerAngles;

    [SerializeField]Vector3 _aimDownSightsPosition;
    [SerializeField]Vector3 _aimDownSightsEulerAngles;

    Camera _camera;

    [SerializeField]float _standingHeight = 1.8f;
    [SerializeField]float _crouchingHeight = 1f;
    [SerializeField]float _slidingHeight = .75f;

    float _timeSpentSliding = 0f;
    [SerializeField]float _slideTimeScaleFactor = 21f;

    [SerializeField]bool _isCrouching = false;
    [SerializeField]bool _isGrounded = true;
    [SerializeField]bool _isObscured = false;
    [SerializeField]bool _isAiming = false;
    [SerializeField]bool _isSliding = false;

    [SerializeField]WeaponBehaviour _weapon;

    [Header("Misc")]
    [SerializeField]bool _displayDebugRays = false;

    void Awake()
    {
        _entity = this.GetComponent<Entity>();
        _rigidbody = this.GetComponent<Rigidbody>();
        _camera = _cameraJig.GetComponentInChildren<Camera>();

        _collider.height = _standingHeight;

        InitializeEvents();
    }
    void FixedUpdate()
    {
        //if (_rigidbody.velocity.magnitude < .5f)
        //    _rigidbody.velocity = Vector3.zero;

        if (_isSliding)
            _timeSpentSliding += Time.fixedDeltaTime;

        UpdateGroundedStatus();
        UpdateObscuredStatus();
    }

    void InitializeEvents()
    {
        _entity.events.Subscribe(LocalEvent.UpdateMouseInput, (object[] args) => OnMouseInputUpdated((Vector2)args[0]));
        _entity.events.Subscribe(LocalEvent.UpdateMovementInput, (object[] args) => OnMovementInputUpdated((Vector2)args[0]));
        _entity.events.Subscribe(LocalEvent.UpdateCrouchInput, UpdateCrouchingAndSliding);
        _entity.events.Subscribe(LocalEvent.UpdateAimingInput, UpdateAimingStatus);
        _entity.events.Subscribe(LocalEvent.Jump, (object[] args) => Jump());
        _entity.events.Subscribe(LocalEvent.Fire, (object[] args) => Fire());
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
        Vector3 force;

        if (!_isGrounded)
            force = relative * _movementSpeed * _airborneSpeedModifier;
        else
        {
            if(_isCrouching)
                force = relative * _movementSpeed * _crouchedSpeedModifier;
            else if (_isSliding)
                force = relative * (_movementSpeed * _slidingSpeedModifier / (_timeSpentSliding * _slideTimeScaleFactor));
            else
                force = relative * (_isAiming ? _movementSpeed * _aimingSpeedModifier : _movementSpeed);
        }

        force *= Time.fixedDeltaTime;

        _rigidbody.AddForce(force, ForceMode.VelocityChange);
        //_rigidbody.MovePosition(this.transform.position + (relative * _movementSpeed * Time.fixedDeltaTime));
    }
    void Jump()
    {
        if (!_isGrounded)
            return;

        if (_isSliding)
        {
            _isSliding = false;

            UpdateCameraJig();
        }

        _rigidbody.velocity += Vector3.up * _jumpStrength;
    }

    void UpdateGroundedStatus()
    {
        Ray ray = new Ray(this.transform.position + (Vector3.up * .25f), Vector3.down);

        _isGrounded = Physics.Raycast(ray, _groundedThreshold);
        _rigidbody.drag = !_isGrounded || _isSliding ? 0f : 10f;
    
        if(_displayDebugRays)
            Debug.DrawRay(this.transform.position + (Vector3.up * .25f), Vector3.down * _groundedThreshold, Color.yellow);
    }
    void UpdateCrouchingAndSliding(object[] args)
    {
        //if we either want to crouch or slide
        if ((bool)args[0])
        {
            if (_isSliding)
                return;

            _isSliding = _rigidbody.velocity.magnitude >= _slideSpeedThreshold;

            if (_isSliding)
            {
                if (!_isGrounded)
                    return;

                _timeSpentSliding = Time.fixedDeltaTime;
            }
            else
            {
                _isCrouching = true;
            }
        }
        else
        {
            _isSliding = false;
            _isCrouching = false;
        }

        UpdateCameraJig();
    }
    void UpdateObscuredStatus()
    {
        Ray ray = new Ray(_camera.transform.position, _camera.transform.forward);
    
        _isObscured = Physics.Raycast(ray, _obscuredThreshold, LayerMask.NameToLayer("Player"));

        UpdateWeaponJig();

        if(_displayDebugRays)
            Debug.DrawRay(_camera.transform.position, _camera.transform.forward * _obscuredThreshold, _isObscured ? Color.red : Color.green);
    }
    void UpdateAimingStatus(object[] args)
    {
        _isAiming = (bool)args[0];

        UpdateWeaponJig();
    }

    void UpdateCameraJig()
    {
        if(_isCrouching)
        {
            _cameraJig.transform.localPosition = new Vector3(0f, _crouchingHeight, 0f);
            _collider.height = _crouchingHeight;
        }
        else if (_isSliding)
        {
            _cameraJig.transform.localPosition = new Vector3(0f, _slidingHeight, 0f);
            _collider.height = _slidingHeight;
        }
        else
        {
            _cameraJig.transform.localPosition = new Vector3(0f, _standingHeight, 0f);
            _collider.height = _standingHeight;
        }

        _collider.transform.localPosition = new Vector3(0f, _collider.height / 2, 0f);
    }
    void UpdateWeaponJig()
    {
        if (_isObscured)
        {
            _weaponJig.transform.localPosition = _weaponJigObscuredPosition;
            _weaponJig.transform.localEulerAngles =  _weaponJigObscuredEulerAngles;
        }
        else
        {
            if (_isAiming)
            {
                _weaponJig.transform.localPosition = _aimDownSightsPosition;
                _weaponJig.transform.localEulerAngles = _aimDownSightsEulerAngles;
            }
            else
            {
                _weaponJig.transform.localPosition = _weaponJigIdlePosition;
                _weaponJig.transform.localEulerAngles = _weaponJigIdleEulerAngles;
            }
        }
    }
    void Fire()
    {
        if (_isObscured)
            return;

        _weapon?.Fire(_camera.transform.position, _camera.transform.forward);

        if (_displayDebugRays)
            Debug.DrawRay(_camera.transform.position, _camera.transform.forward * 10000f, Color.red, 5f);
    }
}