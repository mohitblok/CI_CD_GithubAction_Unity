using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStateMachine : StateMachine
{
    public Vector3 Velocity;
    public float MovementSpeed = 5f;
    public float JumpForce = 5f;
    public float LookRotationDampFactor = 10f;
    public Transform MainCamera;
    public Animator Animator;
    public CharacterController Controller;

    public static CharacterStateMachine instance = null;

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); }
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {

        CharacterMoveState characterMoveState = new CharacterMoveState(this);
        if (characterMoveState == null) {
            Debug.Log("charcater move state is null");
        }


        SwitchState(characterMoveState);
    }
}

