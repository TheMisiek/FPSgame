using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon")]
public class scr_Weapon : ScriptableObject
{
    public GameObject weaponPrefab;
    public float damage;
    public float rateOfFire;
    public float reloadTime;
    public float bulletSpread;
    public int bulletsPerShot;
    public int clipSize;
}
