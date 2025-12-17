using UnityEngine;

public class CarRigid : MonoBehaviour
{

    [SerializeField] private GameObject car; //�Ԃ̃��f��
    
    [SerializeField] private float moveSpeed = 10; //speed
    [SerializeField] private float maxSpeed = 10; //speed

    [SerializeField] private int sripValue = 10; //スリップの強さ
    [SerializeField] private float RotateSpeed = 0.5f; //回転の強さ
    [SerializeField] private float startRotate = 8f; //回転し始めるスピード

    [SerializeField] private float shakeIntensity = 8f; //start hweel shake




    [Header("Grip Setting")]
    [SerializeField] private float maxGrip = 10; //静止時のグリップ
    [SerializeField] private float kineticGrip = 5; //滑っているときのグリップ

    [SerializeField] private GameObject FrontRotateOrign;
    [SerializeField] private GameObject Seat;


    private bool srip;

    private float Dforce1 = 0;
    private float Dforce2 = 0;
    [SerializeField] float forceThreadhold = 0.5f;
    private float moveVector;


    private Rigidbody rBody;


    void Awake()
    {
        rBody = GetComponent<Rigidbody>();
        if (rBody == null)
        {
            Debug.LogError("CarRigid: Rigidbodyが見つかりません！同じGameObjectについているか確認してください。");
        }
        rBody.centerOfMass = FrontRotateOrign.transform.localPosition;
    }

    void Start()
    {
    }

    public void Forward()
    {
        if(Mathf.Abs(Dforce2) - Mathf.Abs(Dforce1) > forceThreadhold)
        {
            Srip();
        }
    }

    void Srip()
    {
        srip = true;
    }

    void Rotate(int handle_rotate)
    {
        Vector3 localVel = transform.InverseTransformDirection(rBody.linearVelocity);
        float forwardSpeed = Mathf.Abs(localVel.z); //速度の絶対値
        


        //ホイールスピン
        if (srip && forwardSpeed < moveSpeed)
        {
            //見た目だけの揺れ
            float sripShake = (Random.Range(-1, shakeIntensity) * sripValue);

            // 視覚的な演出（モデルだけを小刻みに震わせる）
            car.transform.localRotation = Quaternion.Euler(0, sripShake * 8f, 0);
        }

        float speedMod = Mathf.Clamp01(forwardSpeed / startRotate);
        speedMod = Mathf.Pow(speedMod, 2);
        float y_angle_move = handle_rotate * RotateSpeed * speedMod;
        
        //car.transform.Rotate(0, y_angle_move, 0); //直接制御
        
        //物理的な制御
        Quaternion deltaRotation = Quaternion.Euler(new Vector3(0, y_angle_move, 0));
        rBody.MoveRotation(rBody.rotation * deltaRotation);

    }

    void ApplyDriveForce(float input) // input: -1 (���) ���� 1 (�O�i)
    {
        if (rBody == null) return;
        float driveInput = -input;

        float currentVel = rBody.linearVelocity.magnitude;
        float driveTorque = driveInput * moveSpeed;

        if(!srip && currentVel < 2f && driveTorque > maxGrip)
        {
            srip = true;
            Debug.Log("ホイールスピン開始");
        }

        float speedFactor = Mathf.Clamp01(1f - (currentVel / maxSpeed));

        rBody.AddForce(transform.forward * driveInput * moveSpeed * speedFactor, ForceMode.Acceleration);
    }

    void FixedUpdate()
    {
        Vector3 localVel = transform.InverseTransformDirection(rBody.linearVelocity);

        // 2. 横方向（左右）の速度を打ち消す力を計算
        // スリップ中はわざとこの力を弱めることでドリフトさせる
        float sideGrip = srip ? 0.05f : 1.0f;
        float lateralImpulse = -localVel.x * sideGrip;
        rBody.AddRelativeForce(Vector3.right * lateralImpulse, ForceMode.VelocityChange);

        if (srip && localVel.magnitude < 5f) srip = false;
    }

    public void Drive(Vector2 input)
    {
        // 前進・後退
        if (Mathf.Abs(input.y) > 0.01f)
        {
            ApplyDriveForce(input.y);
        }

        // 旋回（ハンドル操作）
        if (Mathf.Abs(input.x) > 0.01f)
        {
            Rotate(input.x > 0 ? 1 : -1); // 簡易的なハンドル操作
        }
    }


    public Vector3 GetDriverSeatLocal()
    {
        return Seat.transform.localPosition;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
