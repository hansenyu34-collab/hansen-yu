using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Interfaces;

public class Inventory : MonoBehaviour
{

    public List<GameObject> weapons = new List<GameObject>();
    public int currentWeaponID;

    void Start()
    {
        currentWeaponID = 0;
    }


    void Update()
    {
        ChangeCurrentWeaponID();
    }

    public void ChangeCurrentWeaponID()
    {
        if (currentWeaponID == 0)
        {
            ChangeWeapon(currentWeaponID);
        }
        
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            ChangeWeapon(currentWeaponID + 1);
        }
        else if ((Input.GetAxis("Mouse ScrollWheel") > 0))
        {
            ChangeWeapon(currentWeaponID - 1);
        }

        for (int i = 0; i <10;  i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                int num = 0;
                if (i == 0)
                {
                    num = 10;
                }
                else
                {
                    num = i - 1;
                }
                if (num < weapons.Count)
                {
                    ChangeWeapon(num);
                }
            }
        }
    }


    public void ChangeWeapon(int weaponID)
    {
        if (weapons.Count == 0) return;

       
        if (weaponID > weapons.Max(weapons.IndexOf))
        {
            weaponID = weapons.Min(weapons.IndexOf);
        }
        else if (weaponID < weapons.Min(weapons.IndexOf))
        {
            weaponID = weapons.Max(weapons.IndexOf);
        }


            currentWeaponID = weaponID;

        for (int i = 0; i < weapons.Count; i++)
        {
            if (weaponID == i)
            {
                weapons[i].gameObject.SetActive(true);
            }
            else
            {
                weapons[i].gameObject.SetActive(false);
            }
        }

    }

    public void AddWeapon(GameObject weapon)
    {
        if (weapons.Contains(weapon))
        {
            print("Max Ammo");
            return;
        }
        else
        {
            if (weapons.Count < 10)
            {
                weapons.Add(weapon);
                ChangeWeapon(weapons.IndexOf(weapon));
            }
        }
    }

    public void ThrowWeapon(GameObject weapon)
    {
        if (!weapons.Contains(weapon) || weapons.Count == 0)
        {
            print("Don't have the weapon");
            return;
        }
        else
        {
            weapons.Remove(weapon);
            ChangeWeapon(currentWeaponID - 1);
            weapon.gameObject.SetActive(false);

        }
    }
}



