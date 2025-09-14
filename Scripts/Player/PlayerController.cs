using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    private Vector3 moveDirection;


    [Header("Player Information")]
    public float Speed;
    public float walkSpeed;
    public float runSpeed;
    public float shootRunningSpeed;
    public float shootWalkingSpeed;
    public float aimRunningSpeed;
    public float aimWalkingSpeed;
    public enum MovementState
    {
        walking,
        running,
        aimWalking,
        aimRunning,
        shootWalking,
        shootRunning,
        idle,
        
    }
    public float jumpForce;
    public float fallForce;
    public float playerHealth;
    public float minKnifeAttackDamage;
    public float maxKnifeAttackDamage;
    public float knifeAttackRange;
    public float knifeAttackRate;

    [Header("Keybinding")]
    public KeyCode runInputName = KeyCode.LeftShift;
    public KeyCode jumpInputName = KeyCode.Space;

    [Header("Player State")]
    public bool isRunning;
    public bool isWalking;
    public bool isJumping;
    public bool isAimRunning;
    public bool isAimWalking;
    public bool isShootWalking;
    public bool isShootRunning;
    public bool isIdling;
    public bool canBeHurt;
    public bool noBodyCanHurt;
    private CollisionFlags collisionFlags;
    public MovementState State;
    public bool isGrounded;
    public bool playerIsDead;
    private bool isDamage;

    [Header("Dash")]
    public float dashSpeed;
    public float dashAmount;
    public float dashDuration;
    public float playerStamina;
    public float staminaConsumption;
    public float staminaRecoverySpeed;
    private float dashDurationTimer = 0;
    private float fallingTimer;

    [Header("Healing")]
    public float healingAmount;
    public int bottleAmount;

    [Header("Respawn")]
    public Transform respawnPoint;

    [Header("Art")]
    public AudioSource[] audioSource;
    public AudioClip walkingSound;
    public AudioClip runningSound;
    public AudioClip dashSound;
    public AudioClip healingSound;
    public AudioClip knifeAttack;
    public ParticleSystem knifeAttackEffect;
    public Transform knifeAttackEffectPivot;
    public Image hurtImage;
    public GameObject knifeSlashMark;
    public AudioClip knifeKillSound;
    public AudioClip shootKillSound;
    private Color flashColor = new Color(1f, 0f, 0f, 0.6f);
    private Color clearColor = Color.clear;

    [Header("UI")]
    public Slider healthSlider;
    public Slider healthSliderVFX;
    public Slider staminaSlider;
    public Slider staminaSliderVFX;
    public TMP_Text healingBottleAmountUI;
    public Image[] shootingEffectUI;

    private Inventory inventory;
    private Weapon_AutomaticGun gun;
    private Shotgun Shotgun;
    private  GameObject currentWeaponObject;
    public Transform spawnPoint;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponents<AudioSource>();
        inventory = GetComponentInChildren<Inventory>();
        
        jumpForce = 0f;
        fallForce = 10f;
        
        walkSpeed = 10f;
        runSpeed = 15f;
        aimRunningSpeed = 10;
        aimWalkingSpeed = 5;
        shootRunningSpeed =12;   
        shootWalkingSpeed = 7;

        healthSlider.minValue = 0f;
        healthSlider.maxValue = playerHealth;
        healthSlider.value = playerHealth;
        healthSliderVFX.minValue = 0f;
        healthSliderVFX.maxValue = playerHealth;
        healthSliderVFX.value = playerHealth;
        
        staminaSlider.minValue = 0f;
        staminaSlider.maxValue = playerStamina;
        staminaSlider.value = playerStamina;
        staminaSliderVFX.minValue = 0f;
        staminaSliderVFX.maxValue = playerStamina;
        staminaSliderVFX.value = playerStamina;

        playerIsDead = false;
    }


    void Update()
    {

        if (isDamage)
        {
            hurtImage.color = flashColor;
        }
        else
        {
            hurtImage.color = Color.Lerp(hurtImage.color, Color.clear, Time.deltaTime);
        }

        DeadAction();

        playerHealth = Mathf.Clamp(playerHealth, healthSlider.minValue, healthSlider.maxValue);
        healthSlider.value = Mathf.Lerp(healthSlider.value, playerHealth, 8 * Time.deltaTime);
        healthSliderVFX.value = Mathf.Lerp(healthSliderVFX.value, playerHealth, 3 * Time.deltaTime);
        staminaSliderVFX.value = Mathf.Lerp(staminaSliderVFX.value, playerStamina, 3 * Time.deltaTime);

        isDamage = false;

        currentWeaponObject = inventory.weapons[inventory.currentWeaponID];
        if (inventory.currentWeaponID >= 0 && inventory.currentWeaponID < inventory.weapons.Count)
        {
            gun = currentWeaponObject.GetComponentInChildren<Weapon_AutomaticGun>();
        }

        Moving();
        Jump();
        PlayerFootSoundSet();
        Dash();
        Healing();
        FallingDetection();
    }

    public void Moving()
    {

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        isIdling = Mathf.Abs(h) == 0 && Mathf.Abs(v) == 0;
        isAimRunning = gun.isAiming && Input.GetKey(KeyCode.LeftShift) && !isIdling && isGrounded;
        isAimWalking = gun.isAiming && !isIdling && !isRunning && isGrounded;
        isShootRunning = gun.animator.GetCurrentAnimatorStateInfo(0).IsName("Fire") && Input.GetKey(runInputName) && !isIdling && isGrounded;
        isShootWalking = gun.animator.GetCurrentAnimatorStateInfo(0).IsName("Fire") && !isIdling && !isRunning && isGrounded;
        isRunning = Input.GetKey(runInputName) && !isIdling && isGrounded && !isAimRunning && !isShootRunning;
        isWalking = !isIdling && !isRunning && isGrounded && !isAimWalking && !isShootWalking;
       

        if (isShootRunning)
            State = MovementState.shootRunning;
        else if (isShootWalking)
            State = MovementState.shootWalking;
        else if (isAimRunning)
            State = MovementState.aimRunning;
        else if (isAimWalking)
            State = MovementState.aimWalking;
        else if (isRunning)
            State = MovementState.running;
        else if (isWalking)
            State = MovementState.walking;
        else if (isIdling)
            State = MovementState.idle;



        switch (State)
        {
            case MovementState.shootRunning:
                Speed = shootRunningSpeed;
                break;
            case MovementState.shootWalking:
                Speed = shootWalkingSpeed;
                break;
            case MovementState.aimRunning:
                Speed = aimRunningSpeed;
                break;
            case MovementState.aimWalking:
                Speed = aimWalkingSpeed;
                break;
            case MovementState.running:
                Speed = runSpeed;
                break;
            case MovementState.walking:
                Speed = walkSpeed;
                break;
            case MovementState.idle:
                Speed = 0f;
                break;
        }

        moveDirection = (transform.right * h + transform.forward * v).normalized;
        characterController.Move(moveDirection * Speed * Time.deltaTime);
    }

    private void Jump()
    {

        if (Input.GetKey(jumpInputName) && isGrounded)
        {
            isGrounded = false;
            jumpForce = 7f;
        }
        else if (!Input.GetKey(jumpInputName) && isGrounded)
        {
            isGrounded = false;
        }

        if (!isGrounded)
        {
            jumpForce -= fallForce * Time.deltaTime;
            Vector3 Jump = new Vector3(0, jumpForce * Time.deltaTime, 0);
            collisionFlags = characterController.Move(Jump);
        }

        if (collisionFlags == CollisionFlags.Below)
        {
            isGrounded = true;
            jumpForce = -2f;
        }

    }

    private void PlayerFootSoundSet()
    {

        if (isGrounded && moveDirection.sqrMagnitude > 0)
        {
            AudioClip targetClip = isRunning ? runningSound : walkingSound;

            if (audioSource[0].clip != targetClip)
            {
                audioSource[0].clip = targetClip;
                audioSource[0].Play();
            }

            if (!audioSource[0].isPlaying)
            {
                audioSource[0].Play();
            }
        }
        else
        {
            audioSource[0].Stop();
        }
    }

    public void PickUpWeapon(GameObject weapon)
    {

        if (inventory.weapons.Contains(weapon))
        {
            weapon.GetComponent<Weapon_AutomaticGun>().bulletsLeft = weapon.GetComponent<Weapon_AutomaticGun>().bulletMag * 5;
            weapon.GetComponent<Weapon_AutomaticGun>().UpdateAmmoUI();
        }
        else
        {
            inventory.AddWeapon(weapon);
        }
    }


    public void PlayerHealth(float damage)
    {
        if (!canBeHurt || noBodyCanHurt) return;

        playerHealth -= damage;
        isDamage = true;
        if (playerHealth <= 0)
        {
            playerIsDead = true;
        }
    }    

    public void Dash()
    {
        playerStamina += Time.deltaTime * staminaRecoverySpeed;
        playerStamina = Mathf.Clamp(playerStamina, staminaSlider.minValue, staminaSlider.maxValue);
        staminaSlider.value = playerStamina;

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (playerStamina <= staminaConsumption) return;

            audioSource[1].clip = dashSound;
            audioSource[1].Play();
            dashDurationTimer = dashDuration;
            playerStamina -= staminaConsumption;
        }

        if (dashDurationTimer > 0)
        {

            if (!isIdling)
            {
                characterController.Move(moveDirection * dashSpeed * Time.deltaTime);
            }
            else
            {
                characterController.Move(transform.forward * dashSpeed * Time.deltaTime);
            }

            dashDurationTimer -= Time.deltaTime;

            gameObject.layer = LayerMask.NameToLayer("NoCollision");
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Player");
        }

            canBeHurt = dashDurationTimer <= 0;
    }

    public void Healing()
    {
        healingBottleAmountUI.text = bottleAmount.ToString();

        if (Input.GetKeyDown(KeyCode.V))
        {
            if (bottleAmount <= 0 || playerHealth == healthSlider.maxValue) return;

            audioSource[1].clip = healingSound;
            audioSource[1].Play();
            bottleAmount--;
            playerHealth += healingAmount;

        }
    }

    public void DeadAction()
    {
        if (playerIsDead)
        {
            audioSource[0].Pause();
            audioSource[1].Pause();
            Time.timeScale = 0f;
            return;
        }
    }

    public void Respawn()
    {
        characterController.enabled = false;
        transform.position = respawnPoint.transform.position;
        playerHealth = healthSlider.maxValue;
        playerStamina = staminaSlider.maxValue;
        fallingTimer = 0f;
        playerIsDead = false;
        Time.timeScale = 1f;
        characterController.enabled = true;
    }

    public void FallingDetection()
    {
        fallingTimer += Time.deltaTime;

        if (isGrounded) fallingTimer = 0f;  

        if (fallingTimer > 4f) playerIsDead = true;
    }
}
