using System.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using static scr_Models;

public class scr_WeaponController : MonoBehaviour
{
    private scr_CharacterController _characterController;
    public ParticleSystem muzzleFlash;
    public scr_Weapon weapon1, weapon2, weapon3;
    public GameObject weaponModel;

    [Header("Settings")]
    public WeaponSettingsModel settings;

    private bool _automatic;
    private float _damage;
    [SerializeField] private float _rateOfFire;
    private float _reloadTime;
    private float _bulletSpread;
    private int _bulletsPerShot;
    private int _clipSize;

    public bool isInitialised;
    public bool isFiring = false;
    public bool canFire = true;
    public Transform cameraRaycastOrigin, gunRaycastOrigin;

    Ray cameraRay, gunRay;
    RaycastHit cameraHitInfo;
    [SerializeField] float accumulatedTime;

    public GameObject bulletHole;
    public TrailRenderer tracerEffect;

    Vector3 newWeaponRotation;
    Vector3 newWeaponRotaionVelocity;

    Vector3 targetWeaponRotation;
    Vector3 targetWeaponRotationVelocity;

    private void Start()
    {
        newWeaponRotation = transform.localRotation.eulerAngles;
        SwitchWeapon(1);

    }

    public void Initialise(scr_CharacterController characterController)
    {
        _characterController = characterController;
        isInitialised = true;
    }

    private void Update()
    {
        if (!isInitialised)
        {
            return;
        }

        targetWeaponRotation.y += settings.swayAmount * _characterController.input_View.x * Time.deltaTime;
        targetWeaponRotation.x += settings.swayAmount * -_characterController.input_View.y * Time.deltaTime;

        targetWeaponRotation.x = Mathf.Clamp(targetWeaponRotation.x, -settings.SwayClampX, settings.SwayClampX);
        targetWeaponRotation.y = Mathf.Clamp(targetWeaponRotation.y, -settings.SwayClampY, settings.SwayClampY);

        targetWeaponRotation = Vector3.SmoothDamp(targetWeaponRotation, Vector3.zero, ref targetWeaponRotationVelocity, settings.swayResetSmoothing);
        newWeaponRotation = Vector3.SmoothDamp(newWeaponRotation, targetWeaponRotation, ref newWeaponRotaionVelocity, settings.swaySmoothing);

        transform.localRotation = Quaternion.Euler(newWeaponRotation);
    }

    public void SwitchWeapon(int weaponSlot)
    {
        scr_Weapon currentWeapon = weapon1;
        switch (weaponSlot)
        {
            case 1:
                currentWeapon = weapon1;
                break;
            case 2:
                currentWeapon = weapon2;
                break;
            case 3:
                currentWeapon = weapon3;
                break;
            default:
                break;
        }
        _damage = currentWeapon.damage;
        _rateOfFire = currentWeapon.rateOfFire;
        _reloadTime = currentWeapon.reloadTime;
        _bulletSpread = currentWeapon.bulletSpread;
        _bulletsPerShot = currentWeapon.bulletsPerShot;
        _clipSize = currentWeapon.clipSize;
    }

    public void StartFiring()
    {

        if (accumulatedTime >= 0.0f)
        {
            accumulatedTime = 0.0f;
            //FireBullet();
        }
        isFiring = true;

    }

    public void UpdateFiring(float deltaTime)
    {
        accumulatedTime += deltaTime;
        float fireInterval = 1.0f / _rateOfFire;
        if (isFiring)
        {
            while (accumulatedTime >= 0.0f)
            {
                FireBullet();
                accumulatedTime -= fireInterval;
            }
        }

    }

    public void FireBullet()
    {
        int i = 0;
        while (i < _bulletsPerShot)
        {
            muzzleFlash.Emit(1);

            cameraRay.origin = cameraRaycastOrigin.position;
            Vector3 direction = cameraRaycastOrigin.forward;
            if (_characterController.playerStance == playerStance.Crouch)
            {
                direction.x += UnityEngine.Random.Range(-_bulletSpread / 2, _bulletSpread / 2);
                direction.y += UnityEngine.Random.Range(-_bulletSpread / 2, _bulletSpread / 2);
                direction.z += UnityEngine.Random.Range(-_bulletSpread / 2, _bulletSpread / 2);
            }
            else
            {
                direction.x += UnityEngine.Random.Range(-_bulletSpread, _bulletSpread);
                direction.y += UnityEngine.Random.Range(-_bulletSpread, _bulletSpread);
                direction.z += UnityEngine.Random.Range(-_bulletSpread, _bulletSpread);
            }


            cameraRay.direction = direction;

            gunRay.origin = transform.position;
            gunRay.direction = cameraRaycastOrigin.forward;


            var tracer = Instantiate(tracerEffect, gunRay.origin, Quaternion.identity);
            tracer.AddPosition(gunRay.origin);

            if (Physics.Raycast(cameraRay, out cameraHitInfo))
            {
                if (cameraHitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    cameraHitInfo.transform.GetComponent<scr_CharacterController>().health -= 20;
                }
                else
                {
                    Instantiate(bulletHole, cameraHitInfo.point, Quaternion.identity);
                }
                tracer.transform.localPosition = cameraHitInfo.point;
            }
            i++;
        }
    }
    public void StopFiring()
    {
        
        isFiring = false;
    }
}
