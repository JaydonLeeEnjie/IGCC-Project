using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    public enum ActionType { Attack, Defend, Parry, Heal }

    [Header("Input")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private string moveActionName = "Move";
    [SerializeField] private float deadzone = 0.2f;

    [Header("GameObjects")]
    [SerializeField] private GameObject ClockHand;
    [SerializeField] private Transform ClockCenter;     // <- assign (e.g., player or arena center)
    [SerializeField] private Image Healthbar;
    [SerializeField] private GameObject Options;
    [SerializeField] private Enemy enemy;

    [Header("Settings")]
    [SerializeField] private bool useLocalRotation = true;
    [SerializeField] private float zOffset = -90f;  // offset for visual hand pointing
    [SerializeField] private float clockAngleOffsetForBullets = 0f; // extra offset used for bullet spawning, if needed

    [Header("Stats")]
    [SerializeField] private float currentHealth;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentAttack;
    [SerializeField] private bool inCombat;
    [SerializeField] private bool hasBeenHit;
    [SerializeField] private ActionType currentAction;

    [Header("Smoothing")]
    [SerializeField] private float rotationSmoothing = 15f;

    [SerializeField] private GameObject PlayerHitBox;

    private InputAction moveAction;
    private Transform handT;

    private void Start()
    {
        currentHealth = maxHealth;
        inCombat = false;
    }

    private void Awake()
    {
        handT = ClockHand != null ? ClockHand.transform : null;
        if (playerInput != null)
        {
            var actions = playerInput.actions;
            if (actions != null)
            {
                moveAction = actions.FindAction(moveActionName, throwIfNotFound: false);
            }
        }
    }

    private void OnEnable()
    {
        if (moveAction == null && playerInput != null)
            moveAction = playerInput.actions.FindAction(moveActionName, throwIfNotFound: false);
        moveAction?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
    }

    private void Update()
    {
        if (handT != null && moveAction != null && inCombat)
            HandleHandMovement();

        Options.SetActive(!inCombat);
        Healthbar.fillAmount = (maxHealth > 0f) ? (currentHealth / maxHealth) : 0f;
    }

    // Joystick-only rotation (as you had earlier)
    public void HandleHandMovement()
    {
        Gamepad gp = null;
        if (playerInput != null)
        {
            foreach (var dev in playerInput.devices) // never null
            {
                if (dev is Gamepad g) { gp = g; break; }
            }
        }
        if (gp == null) gp = Gamepad.current;
        if (gp == null) return;

        Vector2 stick = gp.leftStick.ReadValue();
        if (stick.sqrMagnitude < deadzone * deadzone) return;

        float angle = Mathf.Atan2(stick.y, stick.x) * Mathf.Rad2Deg + zOffset;
        Quaternion target = Quaternion.Euler(0f, 0f, angle);

        if (useLocalRotation)
            handT.localRotation = SmoothRotate(handT.localRotation, target, rotationSmoothing);
        else
            handT.rotation = SmoothRotate(handT.rotation, target, rotationSmoothing);
    }

    private static Quaternion SmoothRotate(Quaternion current, Quaternion target, float smoothing)
    {
        float t = 1f - Mathf.Exp(-smoothing * Time.deltaTime);
        return Quaternion.Lerp(current, target, t);
    }

    // === UI hooks ===
    public void ChooseAttack() { StartPlayerChoice(ActionType.Attack); }
    public void ChooseDefend() { StartPlayerChoice(ActionType.Defend); }
    public void ChooseParry() { StartPlayerChoice(ActionType.Parry); }
    public void ChooseHeal() { StartPlayerChoice(ActionType.Heal); }

    private void StartPlayerChoice(ActionType type)
    {
        currentAction = type;
        inCombat = true;
        Options.SetActive(false);

        // Kick the enemy’s sequence — choose the first or random
        var sequence = enemy ? enemy.GetFirstSequence() : null; // or enemy.GetRandomSequence();
        if (sequence == null || handT == null || ClockCenter == null)
        {
            // Fail-safe: end combat immediately if we can’t run a sequence
            inCombat = false;
            Options.SetActive(true);
            return;
        }

        enemy.RunAttackSequence(sequence, ClockCenter, clockAngleOffsetForBullets, OnEnemySequenceComplete);
    }

    private void OnEnemySequenceComplete()
    {
        inCombat = false;
        Options.SetActive(true);
    }

    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Max(0f, currentHealth - amount);
        Healthbar.fillAmount = (maxHealth > 0f) ? (currentHealth / maxHealth) : 0f;
        hasBeenHit = true;
        // Optional: hit VFX/SFX, death check, etc.
    }
}
