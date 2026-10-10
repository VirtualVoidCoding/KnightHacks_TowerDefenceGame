using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerMovement2_5D : MonoBehaviour
{
    
    private Vector2 PlayerMovementInput;
    private Vector3 Velocity;
    private float xRot;

    [SerializeField] private Transform PlayerCamera;
    [SerializeField] private CharacterController Controller;
    [SerializeField] private InputActionAsset InputSystem;
    [Space]
    [SerializeField] private float Speed;

    private void Update()
    {
        PlayerMovementInput = InputSystem["Move"].ReadValue<Vector2>();

        MovePlayer();
    }

    private void MovePlayer()
    {
        Vector3 MoveVector = new Vector3(PlayerMovementInput.x, 0f, PlayerMovementInput.y) * Speed * Time.deltaTime;

        if (InputSystem != null)
        {
            if (InputSystem["Jump"].triggered)
            {
                Velocity.y = 1f * Speed;
            }
            else if (InputSystem["Crouch"].triggered)
            {
                Velocity.y = -1f * Speed;
            }
        }
        else
        {
            //Error log if InputSystem is not assigned
            Velocity.y = 0f;
            string errorMessage = "InputSystem is not assigned in the inspector. Please assign it to the PlayerMovement2_5D script.";
            Debug.LogError(errorMessage);
        }

        Controller.Move(MoveVector * Speed * Time.deltaTime);
        Controller.Move(Velocity * Time.deltaTime);

        Velocity.y = 0f;
    }

}
