using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{

    [Header("Player Movement")]
    public float moveSpeed = 5.0f;
    public float jumpForce = 5.0f;

    //
    [Header("Camera Settings")]
    public Camera firstPersonCamera;
    public Camera thirdPersonCamera;
    public float mouseSenesitivity = 2.0f;


    public float radius = 5.0f;
    public float minRadius = 1.0f;
    public float maxRadius = 10.0f;

    public float yMinLimit = -90;
    public float yMaxLimit = 90;

    private float theta = 0.0f;
    private float phi = 0.0f;
    private float targetVericalRotation = 0;
    private float verticalRoatationSpeed = 240f;

    //내부 변수들
    private bool isFirstPerson = true;    //1인칭 모드인지 여부
    private bool isGrounded;      //플레이어가 땅에 있는지 여부
    private Rigidbody rb;         //플레이어의 Rigidbody
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();  //RigidBody 컴포넌트를 가져온다.

        Cursor.lockState = CursorLockMode.Locked;
        SetupCameras();
        SetActiveCamera();
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleJump();
        HandleCameraToggle();
    }

    void SetActiveCamera()
    {
        firstPersonCamera.gameObject.SetActive(isFirstPerson);
        thirdPersonCamera.gameObject.SetActive(!isFirstPerson);
    }

    void SetupCameras()
    {
        firstPersonCamera.transform.localPosition = new Vector3(0.0f, 0.6f, 0.0f); 
        firstPersonCamera.transform.localRotation = Quaternion.identity;
    }

    void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSenesitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSenesitivity;

        //수평 회전 (theta 값)
        theta += mouseX;
        theta = Mathf.Repeat(theta, 360.0f);    //각도값이 360을 넘지않도록 조정 (0~360)

        //수직 회전 처리
        targetVericalRotation -= mouseY;
        targetVericalRotation = Mathf.Clamp(targetVericalRotation, yMinLimit, yMaxLimit);
        phi = Mathf.MoveTowards(phi, targetVericalRotation, verticalRoatationSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Euler(0.0f, theta, 0.0f);

        if (isFirstPerson)
        {
            firstPersonCamera.transform.localRotation = Quaternion.Euler(phi, 0.0f, 0.0f);
        }
        else
        {

            float x = radius * Mathf.Sin(Mathf.Deg2Rad * phi) * Mathf.Cos(Mathf.Deg2Rad * theta);
            float y = radius * Mathf.Cos (Mathf.Deg2Rad * phi);
            float z = radius * Mathf.Sin(Mathf.Deg2Rad * phi) * Mathf.Sin (Mathf.Deg2Rad * theta);

            thirdPersonCamera.transform.position = transform.position + new Vector3(x, y, z);
            thirdPersonCamera.transform.LookAt(transform);

            radius = Mathf.Clamp(radius - Input.GetAxis("Mouse ScrollWheel") * 5, minRadius, maxRadius);
        }

    }


    void HandleCameraToggle()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            isFirstPerson = !isFirstPerson;
            SetActiveCamera();
        }
                
    }

    void HandleJump()
    {
        //점프 버튼을 누르고 땅에 있을때
        if(Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }


    void HandleMovement()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        if(!isFirstPerson)
        {
            Vector3 cameraForward = thirdPersonCamera.transform.forward;
            cameraForward.y= 0.0f;
            cameraForward.Normalize();

            Vector3 cameraRight = thirdPersonCamera.transform.right;
            cameraRight.y= 0.0f;
            cameraRight.Normalize();
            
            Vector3 movement = cameraRight * moveHorizontal + cameraForward * moveVertical;
            rb.MovePosition(rb.position + movement * moveSpeed * Time.deltaTime);
        }

        else
        {
            //캐릭터 기준으로 이동
            Vector3 movement = transform.right * moveHorizontal + transform.forward * moveVertical;
            rb.MovePosition(rb.position + movement * moveSpeed * Time.deltaTime);   //물리 기반 이동
        }


        
    }

    private void OnCollisionStay(Collision collision)
    {
        isGrounded = true;       //충돌 중이면 플레이어는 땅에 있다. 
    }
}
