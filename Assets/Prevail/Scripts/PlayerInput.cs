using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [Header("XBOX Gamepad Mapping")]
    //[SerializeField]
    string leftSkickXAxisS = "Left Stick X Axis";
    //[SerializeField]
    string leftStickYAxisS = "Left Stick Y Axis";
    //[SerializeField]
    string rightStickXAxisS = "Right Stick X Axis";
    //[SerializeField]
    string rightStickYAxisS = "Right Stick Y Axis";
    //[SerializeField]
    string dPadXAxisS = "D-Pad X Axis";
    //[SerializeField]
    string dPadYAxisS = "D-Pad Y Axis";
    //[SerializeField]
    string triggersS = "Triggers";
    //[SerializeField]
    string leftTriggerS = "Left Trigger";
    //[SerializeField]
    string rightTriggerS = "Right Trigger";
    //[SerializeField]
    string aButtonS = "A Button";
    //[SerializeField]
    string bButtonS = "B Button";
    //[SerializeField]
    string xButtonS = "X Button";
    //[SerializeField]
    string yButtonS = "Y Button";
    //[SerializeField]
    string leftBumperS = "Left Bumper";
    //[SerializeField]
    string rightBumperS = "Right Bumper";
    //[SerializeField]
    string backButtonS = "Back Button";
    //[SerializeField]
    string startButtonS = "Start Button";
    //[SerializeField]
    string leftClickS = "Left Stick Click";
    //[SerializeField]
    string rightClickS = "Right Stick Click";

    public bool playerControllerInputBlocked;


    //axis
    [SerializeField] Vector2 leftStick;
    [SerializeField] Vector2 rightStick;
    [SerializeField] float leftTrigger;
    [SerializeField] float rightTrigger;

    // buttons
    [SerializeField] bool aButton;
    [SerializeField] bool bButton;
    [SerializeField] bool xButton;
    [SerializeField] bool yButton;

    // bumpers
    [SerializeField] bool leftBumper;
    [SerializeField] bool rightBumper;

    //start back
    [SerializeField] bool backButton;
    [SerializeField] bool startButton;

    //clicks
    [SerializeField] bool leftClick;
    [SerializeField] bool rightClick;

    // Properties -----------------------------------------
    //-----------------------------------------------------
    public Vector2 LeftStickInput
    {
        get
        {
            if (playerControllerInputBlocked)
            {
                return Vector2.zero;
            }
            return leftStick;
        }
    }
    public Vector2 RightStickIput
    {
        get
        {
            if (playerControllerInputBlocked)
            {
                return Vector2.zero;
            }
            return rightStick;
        }
    }
    public float LeftTrigger
    {
        get
        {
            if (playerControllerInputBlocked)
            {
                return 0;
            }

            return leftTrigger;
        }
    }
    public float RightTrigger
    {
        get
        {
            if (playerControllerInputBlocked)
            {
                return 0;
            }
            return rightTrigger;
        }
    }
    public bool AButtonInput
    {
        get
        {
            return aButton && !playerControllerInputBlocked;
        }
    }
    public bool BButtonInput
    {
        get
        {
            return bButton && !playerControllerInputBlocked;
        }
    }
    public bool XButtonInput
    {
        get
        {
            return xButton && !playerControllerInputBlocked;
        }
    }
    public bool YButtonInput
    {
        get
        {
            return yButton && !playerControllerInputBlocked;
        }
    }
    public bool LeftBumperInput
    {
        get
        {
            return leftBumper && !playerControllerInputBlocked;
        }
    }
    public bool RightBumperInput
    {
        get
        {
            return rightBumper && !playerControllerInputBlocked;
        }
    }
    public bool BackButtonInput
    {
        get
        {
            return backButton && !playerControllerInputBlocked;
        }
    }
    public bool StartButtonInput
    {
        get
        {
            return startButton;
        }
    }
    public bool LeftClickInput
    {
        get
        {
            return leftClick;
        }
    }
    public bool RightClickInput
    {
        get
        {
            return rightClick;
        }
    }

    private void Update()
    {
        leftStick.Set(Input.GetAxis(leftSkickXAxisS), Input.GetAxis(leftStickYAxisS));
        rightStick.Set(Input.GetAxis(rightStickXAxisS), Input.GetAxis(rightStickYAxisS));

        leftTrigger = Input.GetAxis(leftTriggerS);
        rightTrigger = Input.GetAxis(rightTriggerS);

        aButton = Input.GetButtonDown(aButtonS);
        bButton = Input.GetButtonDown(bButtonS);
        xButton = Input.GetButtonDown(xButtonS);
        yButton = Input.GetButtonDown(yButtonS);

        leftBumper = Input.GetButtonDown(leftBumperS);
        rightBumper = Input.GetButtonDown(rightBumperS);

        backButton = Input.GetButtonDown(backButtonS);
        startButton = Input.GetButtonDown(startButtonS);

        leftClick = Input.GetButtonDown(leftClickS);
        rightClick = Input.GetButtonDown(rightClickS);
    }
}
