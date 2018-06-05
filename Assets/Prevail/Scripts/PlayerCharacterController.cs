using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Character))]
public class PlayerCharacterController : NetworkBehaviour
{
    private Character m_Character; // A reference to the ThirdPersonCharacter on the object
    public Transform m_Cam;                  // A reference to the main camera in the scenes transform
    private Vector3 m_CamForward;             // The current forward direction of the camera
    private Vector3 m_Move;
    private bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.

    public Camera cam;

    public PlayerInput input;

    public EquipmentController equipmentController;

    bool canOpenMenu = true;

    private void Awake()
    {
        UIEventHandler.OnGameMenuClosed += UnlockControl;
    }

    private void Start()
    {
        // get the transform of the main camera
        if (Camera.main != null)
        {
            m_Cam = Camera.main.transform;
            cam = Camera.main;
        }
        else
        {
            Debug.LogWarning(
                "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.", gameObject);
            // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
        }

        // get the third person character ( this should never be null due to require component )
        m_Character = GetComponent<Character>();

        //if (localPlayerAuthority == false)
        //{
        //    return;
        //}

        if (!isLocalPlayer)
        {
            //cam.enabled = false;
            return;
        }


        cam.transform.parent.GetComponentInParent<AutoCam>().Target = this.transform;

    }

    private void UnlockControl()
    {
        input.playerControllerInputBlocked = false;
    }





    private void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }
    }


    // Fixed update is called in sync with physics
    private void FixedUpdate()
    {
        if (!isLocalPlayer)
        {
            return;
        }


        if (input.BackButtonInput && !input.playerControllerInputBlocked)
        {
            input.playerControllerInputBlocked = true;
            UIEventHandler.GameMenuOpened();
        }

        // read inputs
        float x = input.LeftStickInput.x;
        float y = -input.LeftStickInput.y;

        bool aButton = input.AButtonInput;
        bool bButton = input.BButtonInput;
        bool xButton = input.XButtonInput;
        bool yButton = input.YButtonInput;

        float rightTrigger = input.RightTrigger;
        float leftTrigger = input.LeftTrigger;

        // calculate move direction to pass to character
        if (m_Cam != null)
        {
            // calculate camera relative direction to move:
            m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
            m_Move = y * m_CamForward + x * m_Cam.right;
        }
        else
        {
            // we use world-relative directions in the case of no main camera
            m_Move = y * Vector3.forward + x * Vector3.right;
        }

        // walk speed multiplier
        m_Move *= 0.5f;

        // pass all parameters to the character control script
        m_Character.ReceiveInput(m_Move, aButton, bButton, xButton, yButton, rightTrigger, leftTrigger, false, false);
        m_Jump = false;
    }
}
