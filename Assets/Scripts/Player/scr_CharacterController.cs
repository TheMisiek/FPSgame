using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Unity.Netcode;
using static scr_Models;

public class scr_CharacterController : NetworkBehaviour
{
    private CharacterController _characterController;
    private DefaultInput _defaultInput;
    private Vector2 _input_Movement;
    [HideInInspector] public Vector2 input_View;
    private Vector3 _newCameraRotation;
    private Vector3 _newPlayerRotation;
    private float _positionRange = 2.5f;

    [Header("References")]
    public Transform cameraHolder;
    public Camera playerCam;
    public GameObject playerBody;
    public scr_WeaponController weapon;

    [Header("Settings")]
    public PlayerSettingsModel playerSettings;
    public float viewClampYMin = -70, viewClampXMin = 80;

    [Header("Gravity")]
    public float gravityAmount;
    public float gravityMin;
    private float _playerGravity;

    public Vector3 jumpingForce;
    private Vector3 _jumpingForceVelocity;

    [Header("Stance")]
    public playerStance playerStance;
    public float playerStanceSmoothing;
    public PlayerStanceDetails playerStandStance, playerCrouchStance;


    private float _cameraHeight;
    private float _cameraHeightVelocity;

    private Vector3 stanceCapsuleCenterVelocity;
    private float stanceCapsuleHeightVelocity;

    private Vector3 _newMovementSpeed;
    private Vector3 _newMovementVelocity;

    [Header("Player Stats")]
    public float health = 100f;






    private void Start()
    {
        if (IsOwner)
        {
            playerCam.enabled = true;
        }
    }
    public override void OnNetworkSpawn()
    {
        transform.position = new Vector3 (UnityEngine.Random.Range(_positionRange, -_positionRange), 0, UnityEngine.Random.Range(_positionRange, -_positionRange));
        transform.rotation = new Quaternion(0, 180, 0, 0);
    }

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();

        _defaultInput = new DefaultInput();
        _defaultInput.Player.Movement.performed += e => _input_Movement = e.ReadValue<Vector2>();
        _defaultInput.Player.View.performed += e => input_View = e.ReadValue<Vector2>();

        _defaultInput.Player.Jump.performed += e => Jump();
        _defaultInput.Player.Crouch.started += e => Crouch();
        _defaultInput.Player.Crouch.canceled += e => Crouch();
        _defaultInput.Player.Fire1.performed += e => weapon.StartFiring();
        _defaultInput.Player.Fire1.canceled += e => weapon.StopFiring();
        _defaultInput.Player.Weapon1.performed += e => weapon.SwitchWeapon(1);
        _defaultInput.Player.Weapon2.performed += e => weapon.SwitchWeapon(2);
        _defaultInput.Player.Weapon3.performed += e => weapon.SwitchWeapon(3);

        _defaultInput.Enable();

        _newCameraRotation = cameraHolder.localRotation.eulerAngles;
        _newPlayerRotation = transform.localRotation.eulerAngles;

        _cameraHeight = cameraHolder.localPosition.y;

        if (weapon)
        {
            weapon.Initialise(this);
        }
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (playerStance == playerStance.Crouch)
        {
            CalculateMovement(playerSettings.crouchForwardSpeed, playerSettings.crouchStrafeSpeed);
        }
        else
        {
            CalculateMovement(playerSettings.walkingForwardSpeed, playerSettings.walkingStrafeSpeed);
        }
        CalculateView();
        CalculateJump();
        CalculateStance();

    }
    private void LateUpdate()
    {
        weapon.UpdateFiring(Time.deltaTime);
    }

    private void CalculateMovement(float forwardSpeed, float strafeSpeed)
    {
        var _verticalSpeed = forwardSpeed * _input_Movement.y * Time.deltaTime;
        var _horizontalSpeed = strafeSpeed * _input_Movement.x * Time.deltaTime;

        _newMovementSpeed = Vector3.SmoothDamp(_newMovementSpeed, new Vector3(_horizontalSpeed, 0, _verticalSpeed), ref _newMovementVelocity, playerSettings.movementSmoothing);
        var movementSpeed = transform.TransformDirection(_newMovementSpeed);

        _playerGravity -= gravityAmount * Time.deltaTime;

        if (_playerGravity > gravityMin)
        {
            _playerGravity -= gravityAmount * Time.deltaTime;
        }
        if (_playerGravity < -0.1f && _characterController.isGrounded)
        {
            _playerGravity = -0.1f;
        }

        movementSpeed.y += _playerGravity;
        movementSpeed += jumpingForce * Time.deltaTime;

        _characterController.Move(movementSpeed);
    }
    private void CalculateView()
    {
        _newPlayerRotation.y += playerSettings.viewXSensitivity * input_View.x * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(_newPlayerRotation);

        _newCameraRotation.x += playerSettings.viewYSensitivity * -input_View.y * Time.deltaTime;
        _newCameraRotation.x = Mathf.Clamp(_newCameraRotation.x, viewClampYMin, viewClampXMin);

        cameraHolder.localRotation = Quaternion.Euler(_newCameraRotation);
    }
    private void CalculateJump()
    {
        jumpingForce = Vector3.SmoothDamp(jumpingForce, Vector3.zero, ref _jumpingForceVelocity, playerSettings.jumpingFalloff);
    }
    private void CalculateStance()
    {
        var _currentStance = playerStandStance;

        if (playerStance == playerStance.Crouch)
        {
            _currentStance = playerCrouchStance;
        }

        _cameraHeight = Mathf.SmoothDamp(cameraHolder.localPosition.y, _currentStance.cameraHeight, ref _cameraHeightVelocity, playerStanceSmoothing);
        cameraHolder.localPosition = new Vector3(cameraHolder.localPosition.x, _cameraHeight, cameraHolder.localPosition.z);

        Vector3 velocity = Vector3.zero;

        _characterController.height = Mathf.SmoothDamp(_characterController.height, _currentStance.stanceCollider.height, ref stanceCapsuleHeightVelocity, playerStanceSmoothing);
        _characterController.center = Vector3.SmoothDamp(_characterController.center, _currentStance.stanceCollider.center, ref stanceCapsuleCenterVelocity, playerStanceSmoothing);
    }
    private void Jump()
    {
        if (!_characterController.isGrounded)
        {
            return;
        }

        jumpingForce = Vector3.up * playerSettings.jumpingHeight;
        _playerGravity = 0;
    }
    private void Crouch()
    {
        if (playerStance == playerStance.Crouch)
        {
            playerStance = playerStance.Stand;
            return;
        }

        playerStance = playerStance.Crouch;
    }
    private void Death()
    {

    }
}
