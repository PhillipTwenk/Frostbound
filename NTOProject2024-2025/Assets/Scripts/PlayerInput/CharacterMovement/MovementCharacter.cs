using UnityEngine;

public class MovementCharacter : MonoBehaviour
{
    [Header("Tutorial")]
    [SerializeField] private TutorialObjective WASDTutorial;
    private bool IsBearMove;
    
    
    private EntityID playerID;
    private Rigidbody _rb;
    private PlayerAnimationController _animationController;
    
    private Vector3 _input; // вектор для управления
    public bool matrix = false; // Преобразование поворота для изометрии
    private Vector3 relative;
    private bool isGrounded;
    private int jumpCount;
    private bool IsInAir;

    private bool IsSceneLoaded;

    private void Start()
    {
        _animationController = GetComponent<PlayerAnimationController>();
        _rb = GetComponent<Rigidbody>();
        IsInAir = false;
    }

    public void InitializePlayer()
    {
        playerID = CurrentPlayersDataControl.WhichPlayerCreate;
        IsSceneLoaded = true;
        Debug.Log("Персонаж готов");
    }

    private void Update()
    {
        if (IsSceneLoaded)
        {
            InputGet();
            Look();   
        }
    }
    private void FixedUpdate()
    {
        if (_input != Vector3.zero && IsSceneLoaded)
        {
            Move();
        }
        else
        {
            // Остановка движения
            _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0); // Обнуляем горизонтальную скорость
        }
    }
    void InputGet()
    {
        _input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (Input.GetButton("Sprint")) 
        {
            playerID.speed = playerID.sprintSpeed;
        }
        else
        {
            playerID.speed = playerID.normalSpeed;
        }

    }
    void Move()
    {
        Vector3 movement = transform.forward * _input.magnitude * playerID.speed;
        _rb.linearVelocity = new Vector3(movement.x, _rb.linearVelocity.y, movement.z); // Сохраняем вертикальную скорость для гравитации

        if (!IsBearMove)
        {
            Utility.Invoke(this, () => WASDTutorial.CheckAndUpdateTutorialState(), 2f);
            IsBearMove = true;
        }
    }


    void Look()
    {
        if (_input != Vector3.zero)
        {
            if (matrix == false) // требуется ли доп поворот
            {
                relative = (transform.position + _input) - transform.position;
            }
            else
            {
                relative = (transform.position + _input.ToIso()) - transform.position;
            }

            var rot = Quaternion.LookRotation(relative, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, playerID.speedTurn * Time.deltaTime);
            _animationController.Run(playerID.speed, true); // включаем анимации 
        }
        else
        {
            _animationController.Run(playerID.speed, false);
        }
    }
    
    //Не используется
    // private void Jump()
    // {
    //     // Проверяем, находится ли игрок на земле
    //     if (Physics.Raycast(transform.position, Vector3.down, 0.5f))
    //     {
    //         isGrounded = true;
    //         IsInAir = false;
    //         _animationController.JumpAnim("end", false);
    //     }
    //     else
    //     {
    //         isGrounded = false;
    //     }
    //     
    //     //Проверка, падает ли игрок
    //     if (Physics.Raycast(transform.position, Vector3.down, 3.3f))
    //     {
    //         IsInAir = false;
    //         _animationController.JumpAnim("end", false);
    //     }
    //     else
    //     {
    //         IsInAir = true;
    //         _animationController.Falling();
    //     }
    //
    //     // Проверяем нажатие пробела
    //     if (Input.GetButtonDown("Jump") && isGrounded && !IsInAir)
    //     {
    //         _animationController.JumpAnim("start", IsInAir);
    //         _rb.AddForce(Vector3.up * playerID.jumpForce, ForceMode.Impulse);
    //     }
    // }
}
