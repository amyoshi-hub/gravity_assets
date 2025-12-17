using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Game.CameraControl;

[RequireComponent(typeof(CharacterController))]
public class PlayerMain : MonoBehaviour
{
    // --- 回転設定 ---
    [Header("Rotation")]
    [SerializeField] private float _rotationSpeed = 500f;
    private Quaternion _targetRotation;

    // --- 移動速度設定 ---
    [Header("speed"), SerializeField]
    private float _speed = 3;

    [Header("sprint"), SerializeField]
    private float _sprintSpeed = 20;

    private bool _isSprinting;
    public bool IsSprinting => _isSprinting;

    // --- ジャンプ・重力設定 ---
    [Header("jump"), SerializeField]
    private float _jumpSpeed = 7;

    [Header("gravity"), SerializeField]
    private float _gravity = 15;

    [Header("fall_speed"), SerializeField]
    private float _fallSpeed = 10;

    [Header("inital_fall_speed"), SerializeField]
    private float _initFallSpeed = 2;

    [Header("Attacker")]
    //[SerializeField] private Attacker _attacker;
    [SerializeField] private int mode_range = 3;

    [Header("Current State")]
    [SerializeField] private PlayerState _activeState;

    [Header("Collision")]
    //[SerializeField] private InteractCollision _interactCollision;
    //[SerializeField] private ItemCollector _itemCollector;

    // [Header("VFX")]
    // [SerializeField] private VFXController _vfXController;

    // [Header("animator")]
    // [SerializeField] private Animator _animation;

    private int attack_mode = 0;

    // --- コンポーネント・変数 ---
    private Transform _transform;
    private Transform _mainCameraTransform;
    private Game.CameraControl.MouseLook _mouseLook;

    
    public CharacterController _characterController;

    public Vector2 InputMove;
    private float _verticalVelocity;
    private bool _isGroundedPrev;

    private void Awake()
    {
        _transform = transform;

        // ★修正2: CharacterControllerをこのオブジェクトから取得
        _characterController = GetComponent<CharacterController>();

        // MouseLookはMain Cameraから取得
        if (Camera.main != null)
        {
            _mouseLook = Camera.main.GetComponent<Game.CameraControl.MouseLook>();

            _mainCameraTransform = Camera.main.transform;
            Debug.Log("Success: Main Camera Transform acquired.");

            if (_mouseLook == null)
            {
                Debug.LogError("Error: MouseLook component not found on Main Camera. Check MouseLook script attachment.");
            }
        }
        else
        {
            Debug.LogError("Error: Main Camera is not found. Please ensure your scene has a Camera tagged as 'MainCamera'. Character movement will be disabled.");
        }

        if (_characterController == null)
        {
            Debug.LogError("Error: CharacterController not found. Please ensure it is attached to the player GameObject.");
        }

        Cursor.lockState = CursorLockMode.Locked;
        _targetRotation = _transform.rotation;
        //Menu.SetActive(false);
        //HUD.SetActive(true);
        //Bag.SetActive(false);
    }

    private void Start()
    {
        TransitionToState(new PlayerNormalState());
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        InputMove = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _isSprinting = true;
        }
        else if (context.canceled)
        {
            _isSprinting = false;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        // CharacterControllerが接地しているときのみジャンプを許可 (isGroundedがfalseの代わり)
        if (_characterController != null && _characterController.isGrounded)
        {
            _verticalVelocity = _jumpSpeed;
            //_animation.SetTrigger("jump");
        }
    }

    public void RKEY(InputAction.CallbackContext context)
    {
        if (!context.started)
        {
            return;
        }
        attack_mode += 1;
        attack_mode = attack_mode % mode_range;
        Debug.Log("attack_mode:" + attack_mode);
        //_vfXController.PlayModeSwitchVFX();
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (attack_mode == 0)
        {
            if (context.performed)
            {
                //_attacker.SetFire(true);
            }
            if (context.canceled)
            {
                //_attacker.SetFire(false);
            }
        }
        else if (attack_mode == 1)
        {
            //_attacker.MeleeAttack();
            // _animation.SetTrigger("flyAttack");

        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        CarRigid car = FindAnyObjectByType<CarRigid>();
        if(car != null)
        {
            TransitionToState(new PlayerRideCarState(car));
        }
    }


    private void Update()
    {
        // CharacterControllerがない、またはカメラがない場合は移動・回転をスキップ
        if (_characterController == null || _mainCameraTransform == null) return;
        
        _activeState?.UpdateState(this);
        
    }

    public void TransitionToState(PlayerState newState)
    {
        _activeState?.ExitState(this);
        _activeState = newState;
        _activeState.EnterState(this);
    }

    public void HandleMovement()
    {
        // var isGrounded = true; // ← この仮の接地フラグをCharacterControllerのものに戻す
        var isGrounded = _characterController.isGrounded;

        if (isGrounded)
        {
            if (_verticalVelocity < -0.1f)
            {
                _verticalVelocity = -_initFallSpeed;
            }
            // 接地している間はジャンプキーが押されない限り、重力の影響はリセット
            if (_verticalVelocity < 0)
            {
                _verticalVelocity = -_initFallSpeed;
            }
        }

        if (!isGrounded)
        {
            _verticalVelocity -= _gravity * Time.deltaTime;

            if (_verticalVelocity < -_fallSpeed)
                _verticalVelocity = -_fallSpeed;
        }

        _isGroundedPrev = isGrounded;

        float currentSpeed = _isSprinting ? _sprintSpeed : _speed;

        Vector3 camForward = _mainCameraTransform.forward;
        Vector3 camRight = _mainCameraTransform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 desiredHorizontalMove = camForward * InputMove.y + camRight * InputMove.x;

        Vector3 horizontalMoveVelocity;

        if (desiredHorizontalMove.sqrMagnitude > 0.01f)
        {
            horizontalMoveVelocity = desiredHorizontalMove.normalized * currentSpeed;
        }
        else
        {
            horizontalMoveVelocity = Vector3.zero;
        }

        // if (horizontalMoveVelocity.sqrMagnitude > 0.01f) { ... } // デバッグログは省略

        var worldMoveVelocity = new Vector3(
            horizontalMoveVelocity.x,
            _verticalVelocity,
            horizontalMoveVelocity.z
        );

        var moveDelta = worldMoveVelocity * Time.deltaTime;
        _characterController.Move(moveDelta);
    }

    
    public void HandleRotate()
    {
        if (_mouseLook == null) return;

        // プレイヤーが移動入力を与えている場合のみ、目標回転をカメラの向きに更新する
        if (InputMove.sqrMagnitude > 0.01f)
        {
            // float targetYRotation = _mouseLook.GetYRotation(); // エラーCS1061
            float targetYRotation = _mouseLook.getYRotation(); // ★修正: メソッド名を小文字で呼び出し
            _targetRotation = Quaternion.Euler(0f, targetYRotation, 0f);
        }

        // _targetRotationへ滑らかに回転させる
        _transform.rotation = Quaternion.RotateTowards(
            _transform.rotation,
            _targetRotation,
            _rotationSpeed * Time.deltaTime
        );
    }

    public void TeleportPlayer(Vector3 newPosition)
    {
        if (_characterController != null)
        {
            // CharacterControllerを使用している場合、直接Transformを操作せず、
            // enabledを一時的に false にして Move メソッドで位置をリセットするのが一般的
            _characterController.enabled = false;
            _transform.position = newPosition;
            _characterController.enabled = true;
        }
        else
        {
            _transform.position = newPosition;
        }

        _verticalVelocity = -_initFallSpeed;
        Debug.Log($"Player teleported to: {newPosition}");
    }
}