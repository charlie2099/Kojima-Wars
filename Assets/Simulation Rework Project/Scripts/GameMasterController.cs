using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Networking;

public class GameMasterController : NetworkBehaviour
{
    public InputActionManager inputActionManager;
    public float moveSpeed = 5.0f;
    public float rotateSpeed = 5.0f;
    public Vector3 inputVector = default;

    public void Start()
    {
        //if (!IsLocalPlayer) return;

        InputManager.SetInputType(ControlType.GAMEMASTER);

        InputManager.GAMEMASTER.Move.performed += PlayerInput;


    }

    public void Update()
    {
        Movement();
    }

    public void PlayerInput(InputAction.CallbackContext context)
    {
        inputVector = context.ReadValue<Vector3>();
    }


    public void Movement()
    {
        var type = InputManager.GetCurrentControlType();
        if (type == ControlType.GAMEMASTER)
        {
            gameObject.transform.Translate(inputVector * moveSpeed * Time.deltaTime);
        }
    }   
}