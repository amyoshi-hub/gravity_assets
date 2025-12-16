using UnityEngine;

public class CarRigid : MonoBehaviour
{

    [SerializeField] private GameObject car; //�Ԃ̃��f��
    
    [SerializeField] private int moveSpeed = 10; //speed
    [SerializeField] private int resistance = 10; //�����ɑ΂����R��
    [SerializeField] private int friction = 10; //
    [SerializeField] private int sripValue = 10; //������̃X���b�v

    private bool srip;

    private float Dforce1 = 0;
    private float Dforce2 = 0;
    [SerializeField] float forceThreadhold = 0.5f;
    private float moveVector;


    private Rigidbody rBody;

    public void Forward()
    {
        if(Mathf.Abs(Dforce1) - Mathf.Abs(Dforce2) > forceThreadhold)
        {
            Srip();
        }
            Friction();
    
    
    }

    void Srip()
    {
        srip = true;
    }
    void Rotate(int handle_rotate)
    {
        if (srip)
        {
            float y_angle_change = sripValue * Mathf.Cos(Time.time);
            car.transform.Rotate(0, y_angle_change * Time.fixedDeltaTime, 0);
        }

    }

    void Resistance()
    {
        Rotate(0);
    }

    void Friction()
    {

    }

    void ApplyDriveForce(float input) // input: -1 (���) ���� 1 (�O�i)
    {
        // �����A��R�A���C���l��������ŁARigidbody�ɗ͂�������
        rBody.AddForce(transform.forward * input * moveSpeed * Time.fixedDeltaTime, ForceMode.Acceleration);
    }

    void FixedUpdate()
    {
        if (rBody.linearVelocity.magnitude > 0.01f)
        {
            // ��R�i��C��R�ȂǁB���x�ɔ�Ⴕ�Č���j
            rBody.AddForce(-rBody.linearVelocity * resistance * Time.fixedDeltaTime);

            // ���C�i��ɎԂ̑��x�ɋt�炤�j
            rBody.AddForce(-rBody.linearVelocity.normalized * friction * Time.fixedDeltaTime);
        }
        Forward();

        ApplyDriveForce(1);
    }



    //test�p
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
