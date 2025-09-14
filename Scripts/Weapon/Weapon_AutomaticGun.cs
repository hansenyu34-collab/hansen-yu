using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;


public class Weapon_AutomaticGun : Weapon
{
    public PlayerController.MovementState state;
    private PlayerController playerController;
    private ShootingEffect effect;
    private MouseLock mouse;
    private Enemy damagedEnemy;


    [Header("Weapon Components Position")]
    public Camera gunCamera;
    public Vector3 aimPosition;
    public Vector3 normalPosition;
    public Transform shootPoint;
    public Transform bulletShootPoint;
    public Transform castingBulletSpawnPoint;
    private Camera mainCamera;


    [Header("Weapon Statistics")]
    public float range;
    public float autoRate;
    public float singleRate;
    public float fireRate;
    public float originalSpread;
    public float spreadFactor;
    [Tooltip("Magzine Size")] public int bulletMag;
    [Tooltip("Current Bullets Left in the Magzine")] public int currentBullets;
    [Tooltip("Total Amount of Ammo")] public int bulletsLeft;
    private float targetMainCameraFOV;
    private float targetGunCameraFOV;
    private float currentMainCameraFOV;
    private float currentGunCameraFOV;
    public enum ShootMode
    {
        Auto,
        Single,
    }
    public bool onlyOneMode;
    public ShootMode shootingMode;
    public float minDamage;
    public float maxDamage;
    public float horRecoil;
    public float verRecoil;
    public float damageFallOffFactor;
    private float fireTimer;
    private float knifeAttackTimer;
    private float bulletForce;
    private bool gunShootInput;
    private string shootModeName;



    [Header("Weapon State")]
    public bool isSilencer;
    public bool isAiming;
    private bool isReloading;

    [Header("Art")]
    public Light muzzleFlashLight;
    public ParticleSystem muzzleParticle;
    public ParticleSystem sparkParticle;
    public Transform bulletPrefab;
    public Transform castingPrefab;
    public SoundClips soundClips;
    [HideInInspector] public Animator animator;
    private float lightDuration;
    private int minSparkEmission = 1;
    private int maxSparkEmission = 7;
    private AudioSource mainAudioSource;

    [Header("UI")]
    public Image[] crossQuarterImages;
    public TMP_Text ammoTextUI;
    public TMP_Text shootingModeTextUI;
    private float crossExpandDegree;
    private float maxExpandDegree;
    private float currentExpandDegree;
    private bool hitEnemy;
    private float currentAlpha;

    private void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
        mainAudioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;
        effect = GetComponentInParent<ShootingEffect>();
        mouse = GetComponentInParent<MouseLock>();
    }

    private void Start()
    {
        muzzleFlashLight.enabled = false;
        bulletsLeft = bulletMag * 10;
        currentBullets = bulletMag;
        lightDuration = 0.02f;
        bulletForce = 1500f;
        currentMainCameraFOV = 40f;
        currentGunCameraFOV = 50f;
        UpdateAmmoUI();
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        if (playerController.playerIsDead)
        {
            mainAudioSource.Pause();
            return;
        }

        ShootingMode();

        UpdateAmmoUI();

        ShootingEffectUI();

        hitEnemy = false;

        if (fireTimer < fireRate)
        {
            fireTimer += Time.deltaTime;
        }

        if (knifeAttackTimer < playerController.knifeAttackRate)
        {
            knifeAttackTimer += Time.deltaTime;
        }

        animator.SetBool("Run", playerController.isRunning);
        animator.SetBool("Walk", playerController.isWalking);

        if (isReloading || animator.GetCurrentAnimatorStateInfo(0).IsName("Fire") || animator.GetCurrentAnimatorStateInfo(0).IsName("Draw"))
        {
            mainCamera.fieldOfView = 40f;
            gunCamera.fieldOfView = 50f;
        }

        isReloading = animator.GetCurrentAnimatorStateInfo(0).IsName("Reload") ||
                      animator.GetCurrentAnimatorStateInfo(0).IsName("ReloadNoAmmo");

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("AimFire"))
        {
            transform.localPosition = aimPosition;
        }

        if (Input.GetKeyDown(KeyCode.R) && currentBullets < bulletMag && bulletsLeft > 0 && !isReloading)
        {
            DoReloadAnimation();
        }

        if (Input.GetMouseButton(1) && !isReloading)
        {
            if (!isAiming)
            {
                isAiming = true;
                for (int i = 0; i < crossQuarterImages.Length; i++)
                {
                    crossQuarterImages[i].gameObject.SetActive(false);
                }
                effect.swayAmount = 0.002f;
                effect.maxSwayAmount = 0.004f;
                effect.rotationStep = 0.002f;
                effect.maxRotationStep = 0.004f;
            }
        }
        else
        {
            if (isAiming)
            {
                isAiming = false;
                for (int i = 0; i < crossQuarterImages.Length; i++)
                {
                    crossQuarterImages[i].gameObject.SetActive(true);
                }

                effect.swayAmount = 0.04f;
                effect.maxSwayAmount = 0.06f;
                effect.rotationStep = 0.04f;
                effect.maxRotationStep = 0.06f;
            }
        }


        ChangingFOV();

        animator.SetBool("Aim", isAiming);

        if (isAiming)
        {
            spreadFactor = 0.002f;
        }
        else if (playerController.isShootRunning)
        {
            spreadFactor = originalSpread * 3;
        }
        else if (playerController.isShootWalking)
        {
            spreadFactor = originalSpread * 2;
        }
        else if (!playerController.isGrounded)
        {
            spreadFactor = originalSpread * 4f;
        }
        else
        {
            spreadFactor = originalSpread;
        }


        if (Input.GetKeyDown(KeyCode.T))
        {
            animator.SetTrigger("Inspect");
        }

        if (gunShootInput)
        {
            if (currentBullets > 0)
            {
                GunFire();
            }
            else
            {
                KnifeAttack();
            }
        }

        if (Input.GetMouseButtonDown(2)) KnifeAttack();

        ChangingEnemyDetectionRange();
    }

    public IEnumerator MuzzleFlashLight()
    {
        muzzleFlashLight.enabled = true;
        yield return new WaitForSeconds(lightDuration);
        muzzleFlashLight.enabled = false;
    }

    public void UpdateAmmoUI()
    {
        ammoTextUI.text = currentBullets + "/" + bulletsLeft;
        shootingModeTextUI.text = shootModeName;
    }

    public override void GunFire()
    {
        if (fireTimer < fireRate || currentBullets <= 0 || animator.GetCurrentAnimatorStateInfo(0).IsName("Draw") || isReloading) return;

        StartCoroutine(MuzzleFlashLight());
        muzzleParticle.Emit(2);
        sparkParticle.Emit(Random.Range(minSparkEmission, maxSparkEmission));
        StartCoroutine(Shoot_Cross());
        
        if(damagedEnemy == null || damagedEnemy.enemyHealth > 0 || damagedEnemy.isDead)
        {
            mainAudioSource.PlayOneShot(isSilencer ? soundClips.silentShootSound : soundClips.shootingSound);
        }



        if (!isAiming)
        {
            animator.CrossFadeInFixedTime("Fire", 0.1f);
        }
        else
        {
            animator.CrossFadeInFixedTime("AimFire", 0.1f);
        }

        Vector3 shootDirection = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f)).direction;

        shootDirection = shootDirection + shootPoint.TransformDirection(new Vector3(Random.Range(-spreadFactor, spreadFactor), Random.Range(-spreadFactor, spreadFactor)));

        RaycastHit hit;

        int mask = ~LayerMask.GetMask("Detection");

        if (Physics.Raycast(shootPoint.position, shootDirection, out hit, range, mask))
        {
            Debug.Log(hit.transform.name);
            
            if (hit.transform.tag == "Enemy")
            {

                float damage = Random.Range(minDamage, maxDamage) - (Vector3.Distance(shootPoint.position, hit.point) / damageFallOffFactor);
                damage = Mathf.Clamp(damage, 0, maxDamage);
                
                damagedEnemy = hit.transform.gameObject.GetComponentInParent<Enemy>();

                if (!damagedEnemy.isDead)
                {
                    damagedEnemy.Health(damage);
                    hitEnemy = true;

                    if (damagedEnemy.enemyHealth <= 0)
                    {
                        foreach (Image img in playerController.shootingEffectUI)
                        {
                            img.color = Color.red;
                        }

                        mainAudioSource.PlayOneShot(playerController.shootKillSound);
                    }
                    else
                    {
                        foreach (Image img in playerController.shootingEffectUI)
                        {
                            img.color = Color.white;
                        }
                    }
                }                     
            }
        }

        Transform bullet;

        bullet = (Transform)Instantiate(bulletPrefab, bulletShootPoint.transform.position, bulletShootPoint.transform.rotation);

        bullet.GetComponent<Rigidbody>().linearVelocity = shootDirection * bulletForce;

        Instantiate(castingPrefab, castingBulletSpawnPoint.transform.position, castingBulletSpawnPoint.transform.rotation);

        mouse.AddRecoil(Random.Range(-horRecoil, horRecoil), verRecoil, fireRate);
        effect.shakeTimer = fireRate / 2f;

        fireTimer = 0f;
        currentBullets--;
        UpdateAmmoUI();
    }


    public void ExpandCross(float add)
    {
        crossQuarterImages[0].transform.localPosition += new Vector3(-add, 0, 0);
        crossQuarterImages[1].transform.localPosition += new Vector3(add, 0, 0);
        crossQuarterImages[2].transform.localPosition += new Vector3(0, add, 0);
        crossQuarterImages[3].transform.localPosition += new Vector3(0, -add, 0);

        currentExpandDegree += add;
        currentExpandDegree = Mathf.Clamp(currentExpandDegree, 0, maxExpandDegree);
    }

    public IEnumerator Shoot_Cross()
    {
        crossQuarterImages[0].transform.localPosition += new Vector3(-15, 0, 0);
        crossQuarterImages[1].transform.localPosition += new Vector3(15, 0, 0);
        crossQuarterImages[2].transform.localPosition += new Vector3(0, 15, 0);
        crossQuarterImages[3].transform.localPosition += new Vector3(0, -15, 0);

        yield return new WaitForSeconds(0.03f);

        crossQuarterImages[0].transform.localPosition += new Vector3(15, 0, 0);
        crossQuarterImages[1].transform.localPosition += new Vector3(-15, 0, 0);
        crossQuarterImages[2].transform.localPosition += new Vector3(0, -15, 0);
        crossQuarterImages[3].transform.localPosition += new Vector3(0, 15, 0);
    }


    public override void AimIn()
    {
        mainAudioSource.clip = soundClips.aimSound;
        mainAudioSource.Play();
        transform.localPosition = aimPosition;
    }

    public override void AimOut()
    {
        mainAudioSource.clip = soundClips.aimSound;
        mainAudioSource.Play();
        transform.localPosition = normalPosition;
    }


    public override void DoReloadAnimation()
    {

        if (currentBullets > 0 && bulletsLeft > 0)
        {
            animator.Play("Reload");
            mainAudioSource.clip = soundClips.reloadSound;
            mainAudioSource.Play();
        }
        if (currentBullets == 0 && bulletsLeft > 0)
        {
            animator.Play("ReloadNoAmmo", 0, 0);
            mainAudioSource.clip = soundClips.reloadSoundNoAmmo;
            mainAudioSource.Play();
        }
    }

    public override void ExpandingCrossUpdate(float expandDegree)
    {
        if (currentExpandDegree < expandDegree - 5)
        {
            ExpandCross(150 * Time.deltaTime * 2);
        }
        else if (currentExpandDegree > expandDegree + 5)
        {
            ExpandCross(-300 * Time.deltaTime * 2);
        }
    }

    public override void Reload()
    {
        if (bulletsLeft <= 0) return;
        int bulletsToLoad = bulletMag - currentBullets;
        int bulletsToReduce = bulletsLeft >= bulletsToLoad ? bulletsToLoad : bulletsLeft;
        bulletsLeft -= bulletsToReduce;
        currentBullets += bulletsToReduce;
    }

    public void ShootingMode()
    {
        if (!onlyOneMode)
        {
            if (Input.GetKeyDown(KeyCode.X) && shootingMode == ShootMode.Auto)
            {
                shootingMode = ShootMode.Single;
            }
            else if (Input.GetKeyDown(KeyCode.X) && shootingMode == ShootMode.Single)
            {
                shootingMode = ShootMode.Auto;
            }
        }

        switch (shootingMode)
        {
            case ShootMode.Auto:
                gunShootInput = Input.GetMouseButton(0);
                fireRate = autoRate;
                shootModeName = "Auto";
                break;
            case ShootMode.Single:
                gunShootInput = Input.GetMouseButtonDown(0);
                fireRate = singleRate;
                shootModeName = "Single";
                break;
        }
    }

    public void ChangingFOV()
    {
        targetMainCameraFOV = isAiming && !animator.GetCurrentAnimatorStateInfo(0).IsName("KnifeAttack0") ? 20f : 40f;
        targetGunCameraFOV = isAiming && !animator.GetCurrentAnimatorStateInfo(0).IsName("KnifeAttack0") ? 20f : 50f;

        currentMainCameraFOV = Mathf.Lerp(currentMainCameraFOV, targetMainCameraFOV, 10 * Time.deltaTime);
        currentGunCameraFOV = Mathf.Lerp(currentGunCameraFOV, targetGunCameraFOV, 10 * Time.deltaTime);

        Camera.main.fieldOfView = currentMainCameraFOV;
        gunCamera.fieldOfView = currentGunCameraFOV;
    }

    public void KnifeAttack()
    {
        if (knifeAttackTimer < playerController.knifeAttackRate) return;

        animator.Play("KnifeAttack0");

        mainAudioSource.PlayOneShot(playerController.knifeAttack);

        Invoke("InstantiateEffect", 0.1f);

        RaycastHit hit;

        int mask = ~LayerMask.GetMask("Detection");

        if (Physics.Raycast(
                Camera.main.transform.position,
                Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f)).direction,
                out hit,
                playerController.knifeAttackRange,
                mask))
        {
            Vector3 offsetPos = hit.point + hit.normal * 0.01f;
            Quaternion rot = Quaternion.LookRotation(-hit.normal);
            GameObject mark = Instantiate(playerController.knifeSlashMark, offsetPos, rot);

            Destroy(mark, 2f);

            if (hit.transform.CompareTag("Enemy"))
            {
                damagedEnemy = hit.transform.GetComponentInParent<Enemy>();

                if (!damagedEnemy.isDead)
                {
                    damagedEnemy.Health(Random.Range(playerController.minKnifeAttackDamage, playerController.maxKnifeAttackDamage));
                    hitEnemy = true;

                    if (damagedEnemy.enemyHealth <= 0)
                    {
                        foreach (Image img in playerController.shootingEffectUI)
                        {
                            img.color = Color.red;
                        }

                        playerController.audioSource[1].PlayOneShot(playerController.knifeKillSound);
                    }
                    else
                    {
                        foreach (Image img in playerController.shootingEffectUI)
                        {
                            img.color = Color.white;
                        }
                    }
                }              
            }
        }


        knifeAttackTimer = 0;
    }

    public void InstantiateEffect()
    {
        ParticleSystem effect = Instantiate(playerController.knifeAttackEffect, playerController.knifeAttackEffectPivot.transform.position, playerController.knifeAttackEffectPivot.transform.rotation);
        effect.transform.SetParent(playerController.knifeAttackEffectPivot);
    }

    public void ShootingEffectUI()
    {
        if (hitEnemy)
        {
            currentAlpha = 1f;
        }

        currentAlpha = Mathf.Lerp(currentAlpha, 0f, 5 * Time.deltaTime);

        foreach (Image img in playerController.shootingEffectUI)
        {
            Color c = img.color;
            c.a = currentAlpha;
            img.color = c;
        }
    }

    public void ChangingEnemyDetectionRange()
    {
        if (damagedEnemy != null)
        {
            Detection detection = damagedEnemy.GetComponentInChildren<Detection>();

            if (damagedEnemy.attackList.Count == 0)
            {
                if (hitEnemy)
                {
                    detection.boxCollider.size = detection.idleDetectionRange * 4f;
                }
                else
                {
                    detection.boxCollider.size = detection.idleDetectionRange;
                }
            }
        }
    }
}
