using UnityEngine;

public class MouseLock : MonoBehaviour
{
    public float mouseSensitivity;
    public GameObject recoilPivot;
    private float mouseX;
    private float mouseY;
    private float horAngel;
    public float verAngel;
    public float verOffset;
    private float horOffset;
    public float verCurrent;
    private float horCurrent;
    private float timeToPosition;
    private bool fired;

    void Start()
    {

    }

    void Update()
    {
        if (Time.timeScale == 0f)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
        
        if (verOffset > 0 && mouseY < 0)
        {
            verOffset += mouseY * mouseSensitivity * Time.deltaTime;
        }
        else
        {
            verAngel += mouseY * mouseSensitivity * Time.deltaTime;
            verAngel = Mathf.Clamp(verAngel, -70, 70);
        }

        if (horOffset * mouseX < 0)
        {
            horOffset += mouseX * mouseSensitivity * Time.deltaTime;
        }
        else
        {
            horAngel += mouseX * mouseSensitivity * Time.deltaTime;
        }

        if (fired)
        {
            verCurrent = Mathf.Lerp(verCurrent, verOffset, 10 * Time.deltaTime);
            horCurrent = Mathf.Lerp(horCurrent, horOffset, 10 * Time.deltaTime);

            timeToPosition -= Time.deltaTime;

            if (timeToPosition < 0)
            {
                fired = false;
            }
        }
        else
        {
            if (Mathf.Abs(verCurrent) > 0.1)
            {
                verCurrent = Mathf.Lerp(verCurrent, 0, 3* Time.deltaTime);
                verOffset = 0;
            }
            else
            {
                verCurrent = 0;
                verOffset = 0;
            }

            if (Mathf.Abs(horCurrent) > 0.5)
            {
                horCurrent = Mathf.Lerp(horCurrent, 0, 3 * Time.deltaTime);
                horOffset = horCurrent;
            }
            else
            {
                horCurrent = 0;
                horOffset = 0;
            }
        }

        transform.localRotation = Quaternion.Euler(0, horAngel + horCurrent, 0);
        recoilPivot.transform.localRotation = Quaternion.Euler(Mathf.Clamp(-(verAngel + verCurrent), -70, 70), 0, 0);
    }

    public void AddRecoil(float horRecoil, float verRecoil, float time)
    {
        verOffset += verRecoil;
        horOffset += horRecoil;
        timeToPosition = time;
        fired = true;
    }
}
