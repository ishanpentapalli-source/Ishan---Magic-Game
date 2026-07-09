using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        Idle,
        Walk,
        Run,
        Jump,
        Fall,
        Dash,
        Attacking,
        CastSpell
    }

    [Header("References")]
    public Camera playerCamera;
    public Transform cameraTarget;
    public Transform lockOnTarget;

    [Header("Modules")]
    public PlayerStats stats;
    public PlayerInput input;
    public PlayerMovement movement;
    public PlayerRotation rotation;
    public PlayerJump jump;
    public PlayerDash dash;
    public PlayerCombat combat;
    public PlayerAnimator playerAnimator;
    public GroundChecker groundChecker;

    public CharacterController CharacterController { get; private set; }
    public bool LockOn { get; private set; }
    public PlayerState State { get; private set; }

    private Animator animator;
    private Vector3 lastPlanarVelocity;
    public void Reset()
    {
        EnsureModules();
    }

    private void Awake()
    {
        EnsureModules();
        animator = playerAnimator != null ? playerAnimator.animator : GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        input.Capture;

        if (input.LockOnPressed)
        {
            LockOn = !LockOn;
        }

        groundChecker.Tick();
        combat.Tick(input, stats, animator);

        Transform cameraTransform = playerCamera != null ? playerCamera.transform : null;
        Vector3 desiredDirection = movement.GetCameraRelativeDirection(input.Move, cameraTransform);
        bool actionLocked = combat.IsActionLocked;
        bool canMove = !actionLocked && !dash.IsDashing;
        bool sprinting = input.SprintHeld && input.Move.sqrMagnitude > 0.1f && !actionLocked;

        Vector3 planarVelocity = movement.Tick(input.move, cameraTransform, sprinting, canMove, stats);
        Vector3 dashVelocity = dash.Tick(input.DashPressed, desiredDirection, !actionLocked, stats);
        float verticalVelocity = jump.Tick(groundChecker.IsGrounded, input.JumpPressed, !actionLocked && !dash.IsDashing, stats);

        Vector3 frameVelocity = planarVelocity + dashVelocity + Vector3.up * verticalVelocity;
        CharacterController.Move(frameVelocity * Time.deltaTime);

        Vector3 rotateVelocity = dash.IsDashing ? dashVelocity : planarVelocity;
        rotation.Tick(rotateVelocity, lockOnTarget, LockOn, !actionLocked || LockOn, stats);

        lastPlanarVelocity = planarVelocity + dashVelocity;
        UpdateState(sprinting);
        PlayerAnimator.Tick(lastPlanarVelocity, groundChecker.IsGrounded, sprinting, LockOn, dash.IsDashing, stats.sprintSpeed);
    }

    private void OnAnimatorMove()
    {
       if (stats == null || !stats.useRootMotion || animator == null || CharacterController == null)
        {
            return;
        }

        Vector3 delta = animator.deltaPosition * stats.rootMotionScale;
        delta.y = 0f;
        CharacterController.Move(Delta);
    }

    private void EnsureModules()
    {
        CharacterController = GetComponent<CharacterController>();
        stats = GetOrAdd(stats);
        input = GetOrAdd(input);
        movement = GetOrAdd(movement);
        rotation = GetOrAdd(rotation);
        jump = GetOrAdd(jump);
        dash = GetOrAdd(dash);
        combat = GetOrAdd(combat);
        playerAnimator = GetOrAdd(playerAnimator);
        groundChecker = GetOrAdd(groundChecker);

        if (groundChecker.characterController == null)
        {
            groundChecker.characterController = CharacterController;
        }

        if (playerAnimator.animator == null)
        {
            playerAnimator.animator = GetComponentInChildren<Animator>();
        }

        if (combat.spellController == null)
        {
            combat.spellController = GetComponent<SpellController>();
        }

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private T GetOrAdd<T>(T current) where T : Component
    {
        if (current != null)
        {
            return current;
        }

        T existing = GetComponent<T>();
        return existing != null ? existing : gameObject.AddComponent<T>();
    }

    private void UpdateState(bool sprinting)
    {
        if (combat.IsActionLocked)
        {
            State = combat.CurrentAction == PlayerCombat.ActionKind.Attack ? PlayerState.Attack : PlayerState.CastSpell;
        }
        else if (dash.IsDashing)
        {
            State = PlayerState.Dash;
        }
        else if (!groundChecker.IsGrounded)
        {
            State = jump.VerticalVelocity > 0f ? PlayerState.Jump : PlayerState.Fall;
        }
        else if (lastPlanarVelocity.sqrMagnitude > 0.05f)
        {
            State = sprinting ? PlayerState.Run : PlayerState.Walk;
        }
        else
        {
            State = PlayerState.Idle;
        }
    }
}
