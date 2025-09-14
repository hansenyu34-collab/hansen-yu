using Cinemachine;
using UnityEngine;

public class ShootingEffect : MonoBehaviour
{
    private Weapon_AutomaticGun weapon;
    private Shotgun shotGun;
    public Transform leaningPivot;
    public Transform swayPivot;
    public Transform mainShakePivot;
    public Transform gunShakePivot;

    [Header("Sway")]
    public float swayAmount;
    public float maxSwayAmount;
    public float rotationStep;
    public float maxRotationStep;
    public float swaySmooth;
    public float swaySmoothRot;
    private Vector3 target;
    private Vector3 swayEulerRot;

    [Header("Lean")]
    public float leanAngel;
    public float leanSmoothing;
    public float currentLean;
    private float leanVelocity;
    private float targetLean;

    [Header("CameraShake")]
    public float shakeTimer = 0f;
    public float shakeSmoothness;
    public float mainShakeAmount;
    public float gunShakeAmount;
    private Vector3 mainCurrentShake;
    private Vector3 mainStartPosition;
    private Vector3 gunCurrentShake;
    private Vector3 gunStartPosition;




    void Awake()
    {

    }

    private void Start()
    {
        mainStartPosition = mainShakePivot.localPosition;
        gunStartPosition = gunShakePivot.localPosition;
    }

    void Update()
    {
        Sway();
        Leaning();
        MainShake();
    }

    public void Sway()
    {
        Vector3 invertLook3 = new Vector3(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * -swayAmount;

        invertLook3.x = Mathf.Clamp(invertLook3.x, -maxSwayAmount, maxSwayAmount);
        invertLook3.y = Mathf.Clamp(invertLook3.y, -maxSwayAmount, maxSwayAmount);

        target = invertLook3;

        Vector2 invertLook2 = new Vector3(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * -rotationStep;
        invertLook2.x = Mathf.Clamp(invertLook2.x, -maxRotationStep, maxRotationStep);
        invertLook2.y = Mathf.Clamp(invertLook2.y, -maxRotationStep, maxRotationStep);

        swayEulerRot = new Vector3(invertLook2.y, invertLook2.x, invertLook2.x);

        swayPivot.transform.localRotation = Quaternion.Slerp(swayPivot.transform.localRotation, Quaternion.Euler(swayEulerRot), Time.deltaTime * swaySmooth);
        swayPivot.transform.localPosition = Vector3.Lerp(swayPivot.transform.localPosition, target, Time.deltaTime * swaySmoothRot);
    }

    private void Leaning()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (targetLean == 0)
            {
                targetLean = leanAngel;
            }
            else
            {
                targetLean = 0;
            }

        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (targetLean == 0)
            {
                targetLean = -leanAngel;
            }
            else
            {
                targetLean = 0;
            }
        }

        currentLean = Mathf.SmoothDamp(currentLean, targetLean, ref leanVelocity, leanSmoothing);

        leaningPivot.localRotation = Quaternion.Euler(new Vector3(0, 0, currentLean));
    }

    public void MainShake()
    {
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;

            Vector3 mainTargetOffset = Random.insideUnitSphere * mainShakeAmount;
            mainCurrentShake = Vector3.Lerp(mainCurrentShake, mainTargetOffset, shakeSmoothness * Time.deltaTime);
            mainShakePivot.localPosition = mainStartPosition + mainCurrentShake;

            Vector3 gunTargetOffset = Random.insideUnitSphere * gunShakeAmount;
            gunCurrentShake = Vector3.Lerp(gunCurrentShake, gunTargetOffset, shakeSmoothness * Time.deltaTime);
            gunShakePivot.localPosition = gunStartPosition + gunCurrentShake;
        }
        else
        {
            mainShakePivot.localPosition = mainStartPosition;
            gunShakePivot.localPosition = gunStartPosition;
        }
    }
}