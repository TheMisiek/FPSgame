using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public static class scr_Models
{
    #region - Player -

    public enum playerStance
    {
        Stand,
        Crouch
    }

    [Serializable]
    public class PlayerSettingsModel
    {
        [Header("View Settings")]
        public float viewXSensitivity;
        public float viewYSensitivity;

        //public bool ViewXInverted;
        //public bool ViewYInverted;

        [Header("Movement")]
        public float walkingForwardSpeed;
        public float walkingStrafeSpeed;
        public float crouchForwardSpeed;
        public float crouchStrafeSpeed;

        public float movementSmoothing;

        [Header("Jumping")]
        public float jumpingHeight;
        public float jumpingFalloff;


    }

    [Serializable]
    public class PlayerStanceDetails
    {
        public float cameraHeight;
        public CapsuleCollider stanceCollider;
    }

    #endregion

    #region - Weapons -

    [Serializable]
    public class WeaponSettingsModel
    {
        [Header("Sway")]
        public float swayAmount;
        public float swaySmoothing;
        public float swayResetSmoothing;
        public float SwayClampX;
        public float SwayClampY;
    }
    [Serializable]
    public class WeaponStats
    {
        public bool automatic;
        public float damage;
        public float rateOfFire;
        public float reloadTime;
        public float bulletSpread;
        public int bulletsPerShot;
        public int clipSize;
    }

    #endregion
}