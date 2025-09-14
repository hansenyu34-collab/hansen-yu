using UnityEngine;

public class PickUpWeapons : MonoBehaviour
{

    public int itemID;
    private GameObject weaponModel;
    private float rotateSpeed;

    void Start()
    {
        rotateSpeed = 100f;
    }


    void Update()
    {
        transform.eulerAngles += new Vector3(0, rotateSpeed * Time.deltaTime, 0); 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            PlayerController player = other.GetComponent<PlayerController>();
            weaponModel = GameObject.Find("---Player---/LeanPivot/RecoilPivot/Inventory/").gameObject.transform.GetChild(itemID).gameObject;
            player.PickUpWeapon(weaponModel);
            Destroy(gameObject);
        }
    }
}
