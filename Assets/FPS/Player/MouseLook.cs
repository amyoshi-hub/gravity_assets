using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.CameraControl{
    public class MouseLook : MonoBehaviour
    {
        // ★修正: ターゲット（Player）の参照とTPSカメラのパラメーターを追加
        [Header("ターゲット (Player)"), SerializeField]
        private Transform _target; // カメラが追従・回転するターゲット (Player)

        [Header("TPSカメラ設定")]
        [SerializeField] private float _distance = 5.0f; // ターゲットからの距離
        [SerializeField] private float _height = 2.0f; // ターゲットからの高さ
        [SerializeField] private float _damping = 5.0f; // カメラ移動の追従速度（滑らかさ）

        [Header("マウス感度"), SerializeField]
        private float _mouseSensitivity = 30f;

        [Header("カメラ回転の最小・最大角度 (垂直)")]
        // TPSではFPSより可動域を制限することが多い
        [SerializeField] private float _minXRotation = -30f;
        [SerializeField] private float _maxXRotation = 60f;

        [Header("FoV設定")]
        [SerializeField] private float _normalFOV = 60f; // 通常時のFoV
        [SerializeField] private float _sprintFOV = 70f; // 走るときのFoV
        [SerializeField] private float _fovChangeSpeed = 5f; // FoVが切り替わる速度

        private PlayerMain _playerController;
        private Camera _mainCamera;

        private Vector2 _inputLook;
        private float _xRotation = 0f; // 垂直回転角度 (上下)
        private float _yRotation = 0f; // 水平回転角度 (左右 - TPS用)

        // Raycast関連
        [SerializeField] private float _interactDistance = 3f;
        //private Item _currentItem;
        private bool _isInteracting;

        /// <summary>
        /// Look Action (PlayerInput側から呼ばれる)
        /// </summary>
        public void OnLook(InputAction.CallbackContext context)
        {
            _inputLook = context.ReadValue<Vector2>();
        }

        /// <summary>
        /// Interact Action (Eキーなどの入力がここに来る)
        /// </summary>
        public void OnInteract(InputAction.CallbackContext context)
        {
            _isInteracting = context.performed;
        }

        private void Start()
        {
            // ターゲットが設定されていることを確認
            if (_target == null)
            {
                Debug.LogError("Target (Player) must be assigned in the Inspector for TPS camera.");
                return;
            }

            // ターゲットからPlayerMainを取得
            _playerController = _target.GetComponent<PlayerMain>();
            // Cameraコンポーネントを取得
            _mainCamera = GetComponent<Camera>();

            if (_playerController == null) Debug.LogError("PlayerController not found on target.");
            if (_mainCamera == null) Debug.LogError("Camera component not found on MouseLook object.");

            Cursor.lockState = CursorLockMode.Locked;

            if (_mainCamera != null)
            {
                _mainCamera.fieldOfView = _normalFOV;
            }

            // ★追加: 初期Y回転角をターゲットの初期回転から取得
            _yRotation = _target.eulerAngles.y;
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            // 1. 回転角度の計算
            // TPSカメラでは、マウス入力にTime.deltaTimeを適用して滑らかにする
            float mouseX = _inputLook.x * _mouseSensitivity * Time.deltaTime;
            float mouseY = _inputLook.y * _mouseSensitivity * Time.deltaTime;

            // 水平方向の回転 (Y軸) は累積する (オービタル回転)
            _yRotation += mouseX;

            // 垂直方向の回転 (X軸) は制限を設ける
            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, _minXRotation, _maxXRotation);

            // 最終的な回転のQuaternionを計算 (ワールド空間)
            Quaternion targetRotation = Quaternion.Euler(_xRotation, _yRotation, 0f);

            // 2. カメラの追跡位置の計算

            // ターゲットの中心位置 (プレイヤーの足元 + 設定された高さ)
            Vector3 targetPosition = _target.position + Vector3.up * _height;

            // 目標位置: ターゲットを中心に、回転と距離を考慮した位置
            Vector3 desiredPosition = targetPosition - targetRotation * Vector3.forward * _distance;

            // 3. カメラの移動と回転の適用 (滑らかに追従)

            // 回転を滑らかに適用
            // Slerp: 球面線形補間（回転に適している）
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * _damping);

            // 位置を滑らかに適用
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * _damping);


            // ★追加: ダッシュカメラ処理
            DashCamera();

            // レイキャストのチェック
            //CheckInteractable();
        }


        private void DashCamera()
        {
            if (_mainCamera == null || _playerController == null) return;

            // プレイヤーのisSprintingフラグを取得し、目標FoVを決定
            float targetFOV = _playerController.IsSprinting ? _sprintFOV : _normalFOV;

            // FoVを徐々に目標値に近づける
            _mainCamera.fieldOfView = Mathf.Lerp(
                _mainCamera.fieldOfView,
                targetFOV,
                Time.deltaTime * _fovChangeSpeed
            );
        }

        /*
        private void CheckInteractable()
        {
            RaycastHit hit;
            Item hitItem = null;

            // カメラからレイキャストを発射
            bool isHit = Physics.Raycast(transform.position, transform.forward, out hit, _interactDistance);

            if (isHit)
            {
                hitItem = hit.collider.GetComponent<Item>();
                Debug.DrawRay(transform.position, transform.forward * hit.distance, Color.green, 0.1f);
            }
            else
            {
                Debug.DrawRay(transform.position, transform.forward * _interactDistance, Color.red, 0.1f);
            }

            // --- ハイライト制御とインタラクト処理 ---

            // A) アイテムに当たった場合
            if (hitItem != null)
            {
                if (_currentItem != hitItem)
                {
                    if (_currentItem != null)
                    {
                        _currentItem.Highlight(false);
                    }
                    _currentItem = hitItem;
                    _currentItem.Highlight(true);
                }

                if (_isInteracting)
                {
                    _currentItem.Collect();
                    _currentItem = null;
                }
            }
            // B) アイテムから視線が外れた場合
            else if (_currentItem != null)
            {
                _currentItem.Highlight(false);
                _currentItem = null;
            }

            // Interactフラグは最後にリセット
            if (_isInteracting)
            {
                _isInteracting = false;
            }
        }
        */
        public float getYRotation()
        {
            return _yRotation;
        }
    }
}