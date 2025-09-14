using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Threading;

public class Shotgun : Weapon
{
    public PlayerController.MovementState state;
    private PlayerController playerController;
    private ShootingEffect effect;
    private MouseLock mouse;

    [Header("Weapon Components Position")]
    public Camera gunCamera;
    public Transform shootPoint;
    public Vector3 aimPosition;
    public Vector3 normalPosition;
    public Transform castingBulletSpawnPoint;
    private Camera mainCamera;


    [Header("Weapon Statistics")]
    public float range;
    public float fireRate;
    public float originalSpread;
    public float spreadFactor;
    [Tooltip("Magzine Size")] public int bulletMag;
    [Tooltip("Current Bullets Left in the Magzine")] public int currentBullets;
    [Tooltip("Total Amount of Ammo")] public int bulletsLeft;
    private float mainFOVVelocity;
    private float gunFOVVelocity;
    public float minDamage;
    public float maxDamage;
    public float horRecoil;
    public float verRecoil;
    public float damageFallOffFactor;
    private float fireTimer;
    private float knifeAttackTimer;
    private float bulletForce;
    private string shootModeName;
    private int shotGunFragment;


    [Header("Weapon State")]
    public bool isSilencer;
    public bool isAiming;
    private bool isReloading;
    public bool hitEnemy;

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
    private float currentAlpha;

    [Header("UI")]
    public Image[] crossQuarterImages;
    public TMP_Text ammoTextUI;
    public TMP_Text shootingModeTextUI;
    private float crossExpandDegree;
    private float maxExpandDegree;
    private float currentExpandDegree;


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
        shootModeName = "Single";
        shotGunFragment = 8;
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

        if (Input.GetMouseButtonDown(2)) KnifeAttack();

        animator.SetBool("Run", playerController.isRunning);
        animator.SetBool("Walk", playerController.isWalking);

        if (isReloading || animator.GetCurrentAnimatorStateInfo(0).IsName("Fire") || animator.GetCurrentAnimatorStateInfo(0).IsName("Draw"))
        {
            mainCamera.fieldOfView = 40f;
            gunCamera.fieldOfView = 50f;
        }

        isReloading =   animator.GetCurrentAnimatorStateInfo(0).IsName("ReloadOpen") ||
                        animator.GetCurrentAnimatorStateInfo(0).IsName("ReloadInsert 1") ||
                        animator.GetCurrentAnimatorStateInfo(0).IsName("ReloadInsert 2") ||
                        animator.GetCurrentAnimatorStateInfo(0).IsName("ReloadInsert 3") ||
                        animator.GetCurrentAnimatorStateInfo(0).IsName("ReloadInsert 4") ||
                        animator.GetCurrentAnimatorStateInfo(0).IsName("ReloadInsert 5") ||
                        animator.GetCurrentAnimatorStateInfo(0).IsName("ReloadInsert 6") ||
                        animator.GetCurrentAnimatorStateInfo(0).IsName("ReloadClose");

        if ((animator.GetCurrentAnimatorStateInfo(0).IsName("ReloadInsert 1") ||
             animator.GetCurrentAnimatorStateInfo(0).IsName("ReloadInsert 2") ||
             animator.GetCurrentAnimatorStateInfo(0).IsName("ReloadInsert 3") ||
             animator.GetCurrentAnimatorStateInfo(0).IsName("ReloadInsert 4") ||
             animator.GetCurrentAnimatorStateInfo(0).IsName("ReloadInsert 5") ||
             animator.GetCurrentAnimatorStateInfo(0).IsName("ReloadInsert 6")) &&
             currentBullets == bulletMag
            )
        {
            animator.Play("ReloadClose");
            isReloading = false;
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

                transform.localPosition = aimPosition;
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

                transform.localPosition = normalPosition;
            }
        }

        animator.SetBool("Aim", isAiming);
  
        if (isAiming)
        {
            spreadFactor = 0.003f;
        }
        else if (playerController.isShootRunning)
        {
            spreadFactor = originalSpread * 1.2f;
        }
        else if (playerController.isShootWalking)
        {
            spreadFactor = originalSpread * 1.1f;
        }
        else if (!playerController.isGrounded)
        {
            spreadFactor = originalSpread * 1.5f;
        }
        else
        {
            spreadFactor = originalSpread;
        }


        if (Input.GetKeyDown(KeyCode.T))
        {
            animator.SetTrigger("Inspect");
        }

        if (Input.GetMouseButtonDown(0))
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

        if (Input.GetMouseButtonDown(2))
        {
            KnifeAttack();
        }

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
        mainAudioSource.PlayOneShot(isSilencer ? soundClips.silentShootSound : soundClips.shootingSound);

        if (!isAiming)
        {
            animator.CrossFadeInFixedTime("Fire", 0.1f);
        }
        else
        {
            animator.CrossFadeInFixedTime("AimFire", 0.1f);
        }

        Instantiate(castingPrefab, castingBulletSpawnPoint.transform.position, castingBulletSpawnPoint.transform.rotation);

        fireTimer = 0f;
        currentBullets--;
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
        mainCamera.fieldOfView = Mathf.SmoothDamp(20f, 40f, ref mainFOVVelocity, 2f);
        gunCamera.fieldOfView = Mathf.SmoothDamp(20f, 50f, ref gunFOVVelocity, 2f);
    }

    public override void AimOut()
    {
        mainAudioSource.clip = soundClips.aimSound;
        mainAudioSource.Play();
        mainCamera.fieldOfView = Mathf.SmoothDamp(40f, 20f, ref mainFOVVelocity, 2f);
        gunCamera.fieldOfView = Mathf.SmoothDamp(50f, 20f, ref gunFOVVelocity, 2f);
    }


    public override void DoReloadAnimation()
    {
        if (currentBullets == bulletMag) return;
        animator.SetTrigger("ShotGunReload");
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

    public void ShotGunReload()
    {
        if (bulletsLeft <= 0) return;

        if (currentBullets < bulletMag)
        {
            currentBullets++;
            bulletsLeft--;
        }
        else
        {
            animator.Play("ReloadClose");
            return;
        }
    }

    public void ActuallyFire()
    {
        for (int i = 0; i <= shotGunFragment; i++)
        {
            Vector3 shootDirection = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f)).direction;

            shootDirection = shootDirection + shootPoint.TransformDirection(new Vector3(Random.Range(-spreadFactor, spreadFactor), Random.Range(-spreadFactor, spreadFactor)));

            RaycastHit hit;

            int mask = ~LayerMask.GetMask("Detection");

            if (Physics.Raycast(shootPoint.position, shootDirection, out hit, range, mask))
            {
                if (hit.transform.tag == "Enemy")
                {
                    float damage = Random.Range(minDamage, maxDamage) - (Vector3.Distance(shootPoint.position, hit.point) / damageFallOffFactor);
                    damage = Mathf.Clamp(damage, 0, maxDamage);
                    Enemy damagedEnemy = hit.transform.gameObject.GetComponentInParent<Enemy>();

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
                    Debug.Log(damage);
                }

                Debug.Log("hit" + hit.transform.gameObject.name);
            }

            Transform bullet;

            bullet = Instantiate(bulletPrefab, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));


            mouse.AddRecoil(horRecoil, verRecoil, fireRate);


            bullet.GetComponent<Rigidbody>().linearVelocity = shootDirection * bulletForce;
        }
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
                Enemy damagedEnemy = hit.transform.GetComponentInParent<Enemy>();

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

}
