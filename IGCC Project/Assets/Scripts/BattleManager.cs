using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    public enum ActionType { Attack, Defend, Parry, Heal, None }
    private enum ArrowDir { Up, Down, Left, Right }

    [Header("Input")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private string moveActionName = "Move";
    [SerializeField] private float deadzone = 0.2f;

    [Header("GameObjects")]
    [SerializeField] private GameObject ClockHand;
    [SerializeField] private Transform ClockCenter;
    [SerializeField] private Image Healthbar;
    [SerializeField] private GameObject Options;
    [SerializeField] private Enemy enemy;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip HitSound;

    [Header("QTE UI")]
    [SerializeField] private GameObject QTE;
    [SerializeField] private List<Image> ArrowList = new List<Image>(); // slots to show sequence (use first N)
    [SerializeField] private float QTETimeLimit = 3f;
    [SerializeField] private Sprite LeftArrow;
    [SerializeField] private Sprite RightArrow;
    [SerializeField] private Sprite UpArrow;
    [SerializeField] private Sprite DownArrow;
    [SerializeField] private Image QTETimeImage;
    [SerializeField] private int QTELengthMin = 3;
    [SerializeField] private int QTELengthMax = 5;
    [SerializeField] private Color qteDefaultColor = Color.white;
    [SerializeField] private Color qteHitColor = Color.black;

    [Header("Settings")]
    [SerializeField] private bool useLocalRotation = true;
    [SerializeField] private float zOffset = -90f;
    [SerializeField] private float clockAngleOffsetForBullets = 0f;

    [Header("Stats")]
    [SerializeField] private float currentHealth;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentAttack;
    [SerializeField] public bool inCombat;
    [SerializeField] private bool hasBeenHit;
    [SerializeField] private ActionType currentAction;

    [Header("DangerZone")]
    [SerializeField] private List<Sprite> numberSprites = new List<Sprite>();
    [SerializeField] private GameObject dangerZone;
    [SerializeField] private List<float> numberAngles = new List<float>();
    [SerializeField] private Image numberSprite;
    [SerializeField] private DangerZone dangerZoneScript; // assign the component on 'dangerZone' GO



    [Header("Smoothing")]
    [SerializeField] private float rotationSmoothing = 15f;

    [SerializeField] private GameObject PlayerHitBox;

    private InputAction moveAction;
    private Transform handT;

    private void Start()
    {
        currentHealth = maxHealth;
        inCombat = false;
        if (QTE) QTE.SetActive(false);
    }

    private void Awake()
    {
        handT = ClockHand != null ? ClockHand.transform : null;
        if (playerInput != null)
        {
            var actions = playerInput.actions;
            if (actions != null)
                moveAction = actions.FindAction(moveActionName, throwIfNotFound: false);
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

        Options.SetActive(!inCombat && !enemy.isDead && currentHealth > 0);
        Healthbar.fillAmount = (maxHealth > 0f) ? (currentHealth / maxHealth) : 0f;
    }

    // Joystick-only rotation (as before)
    public void HandleHandMovement()
    {
        Gamepad gp = null;
        if (playerInput != null)
        {
            foreach (var dev in playerInput.devices)
                if (dev is Gamepad g) { gp = g; break; }
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
        if (enemy == null || enemy.currentHealth <= 0f)
        {
            FreezeBattle();
            return;
        }

        inCombat = true;
        Options.SetActive(false);

        StartCoroutine(QTE_Then_EnemyAttack(type));
    }

    private IEnumerator QTE_Then_EnemyAttack(ActionType chosen)
    {
        currentAction = chosen;

        bool qteSuccess = false;
        yield return StartCoroutine(RunQTE(result => qteSuccess = result));

        if (!qteSuccess) currentAction = ActionType.None;

        if (qteSuccess && currentAction == ActionType.Attack)
            enemy?.TakeDamage(currentAttack);


        if (enemy == null || enemy.currentHealth <= 0f)
        {
            FreezeBattle();
            yield break;
        }

        var sequence = enemy ? enemy.GetNextSequence() : null;
        if (sequence == null || handT == null || ClockCenter == null)
        {
            inCombat = false;
            Options.SetActive(true);
            yield break;
        }

        enemy.RunAttackSequence(sequence, ClockCenter, clockAngleOffsetForBullets, OnEnemySequenceComplete);
    }


    private void OnEnemySequenceComplete()
    {
        inCombat = false;
        Options.SetActive(true);

        if (currentAction == ActionType.Parry && !hasBeenHit)
        {
            enemy.TakeDamage(currentAttack * 3f);
        }
        else if (currentAction == ActionType.Heal && !hasBeenHit)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + 20);
            Healthbar.fillAmount = (maxHealth > 0f) ? (currentHealth / maxHealth) : 0f;
        }

        hasBeenHit = false;
    }

    public void TakeDamage(float amount)
    {
        if (currentAction == ActionType.Defend)
            currentHealth = Mathf.Max(0f, currentHealth - (amount / 2f));
        else if (currentAction == ActionType.Heal)
            currentHealth = Mathf.Max(0f, currentHealth - (amount * 2f));
        else
            currentHealth = Mathf.Max(0f, currentHealth - amount);

        Healthbar.fillAmount = (maxHealth > 0f) ? (currentHealth / maxHealth) : 0f;
        hasBeenHit = true;
        audioSource.PlayOneShot(HitSound);

        if (currentHealth <= 0f)
        {
            FreezeBattle();
            return;
        }
    }

    // =========================
    // QTE IMPLEMENTATION BELOW
    // =========================

    private IEnumerator RunQTE(System.Action<bool> finished)
    {
        if (QTE == null || ArrowList == null || ArrowList.Count == 0)
        {
            finished?.Invoke(false);
            yield break;
        }

        // Build random sequence length (clamped to available UI slots)
        int len = Random.Range(QTELengthMin, QTELengthMax + 1);
        len = Mathf.Clamp(len, 1, ArrowList.Count);

        var seq = new List<ArrowDir>(len);
        for (int i = 0; i < len; i++)
            seq.Add((ArrowDir)Random.Range(0, 4)); // Up,Down,Left,Right

        // Setup UI
        QTE.SetActive(true);
        for (int i = 0; i < ArrowList.Count; i++)
        {
            bool active = i < len;
            ArrowList[i].gameObject.SetActive(active);
            if (active)
            {
                ArrowList[i].sprite = SpriteFor(seq[i]);
                ArrowList[i].color = qteDefaultColor;
            }
        }

        // Reset and show the timer bar
        if (QTETimeImage)
        {
            QTETimeImage.fillAmount = 0f;
            QTETimeImage.enabled = true;
        }

        float elapsed = 0f;
        int index = 0;
        bool failed = false;

        // Loop until all matched or time is up (or fail)
        while (elapsed < QTETimeLimit && index < len && !failed)
        {
            // Keyboard-only inputs
            if (Keyboard.current != null)
            {
                int nextIndex = index;

                if (Keyboard.current.upArrowKey.wasPressedThisFrame) nextIndex = ProcessKey(seq, index, ArrowDir.Up);
                else if (Keyboard.current.downArrowKey.wasPressedThisFrame) nextIndex = ProcessKey(seq, index, ArrowDir.Down);
                else if (Keyboard.current.leftArrowKey.wasPressedThisFrame) nextIndex = ProcessKey(seq, index, ArrowDir.Left);
                else if (Keyboard.current.rightArrowKey.wasPressedThisFrame) nextIndex = ProcessKey(seq, index, ArrowDir.Right);

                if (nextIndex == -1) // wrong key = hard fail
                {
                    failed = true;
                    break;
                }

                index = nextIndex;
            }

            // advance time + update fill
            elapsed += Time.unscaledDeltaTime; // use deltaTime if you want timescale-aware
            if (QTETimeImage)
                QTETimeImage.fillAmount = Mathf.Clamp01(elapsed / QTETimeLimit);

            yield return null;
        }

        bool success = !failed && (index >= len);

        // Optionally snap to full bar on timeout
        if (QTETimeImage && !success)
            QTETimeImage.fillAmount = 1f;

        QTE.SetActive(false);

        finished?.Invoke(success);
        yield break;
    }


    private int ProcessKey(List<ArrowDir> seq, int currentIndex, ArrowDir pressed)
    {
        if (seq[currentIndex] == pressed)
        {
            ArrowList[currentIndex].color = qteHitColor; // black on correct
            return currentIndex + 1;
        }
        // wrong key -> fail this QTE immediately
        return -1;
    }

    private Sprite SpriteFor(ArrowDir dir)
    {
        switch (dir)
        {
            case ArrowDir.Up: return UpArrow;
            case ArrowDir.Down: return DownArrow;
            case ArrowDir.Left: return LeftArrow;
            case ArrowDir.Right: return RightArrow;
            default: return null;
        }
    }

    public void TriggerDangerZone(float preDelay, float activeDuration, float dps)
    {
        StartCoroutine(DangerZoneRoutine(preDelay, activeDuration, dps));
    }

    private IEnumerator DangerZoneRoutine(float preDelay, float activeDuration, float dps)
    {
        // Pick 1..12 inclusive
        int n = Random.Range(1, 13);
        int idx = n - 1;

        // Show number sprite
        if (numberSprite != null)
        {
            if (numberSprites != null && idx >= 0 && idx < numberSprites.Count && numberSprites[idx] != null)
                numberSprite.sprite = numberSprites[idx];
            numberSprite.gameObject.SetActive(true);
        }

        // Wait pre-delay
        yield return new WaitForSeconds(preDelay);

        // Hide number, show zone at mapped angle
        if (numberSprite != null) numberSprite.gameObject.SetActive(false);

        if (dangerZone != null)
        {
            float zAngle;
            if (numberAngles != null && idx >= 0 && idx < numberAngles.Count)
                zAngle = numberAngles[idx];
            else
                zAngle = idx * 30f; // fallback: 12 slices around the clock

            dangerZone.transform.rotation = Quaternion.Euler(0f, 0f, zAngle);

            // configure DPS and activate
            if (!dangerZoneScript) dangerZoneScript = dangerZone.GetComponent<DangerZone>();
            if (dangerZoneScript) dangerZoneScript.Configure(dps);

            dangerZone.SetActive(true);

            yield return new WaitForSeconds(activeDuration);

            dangerZone.SetActive(false);
        }
    }

    private void FreezeBattle()
    {
        // Stop all coroutines in the BattleManager (QTE, timers, etc.)
        StopAllCoroutines();

        // Tell the enemy to stop its own attack sequence
        if (enemy != null)
        {
            enemy.StopAllCoroutines();   // kills any coroutines started in Enemy

        }

        inCombat = false;
        Options.SetActive(false);

        Debug.Log("[BattleManager] Battle frozen.");
    }
    
    public void Win()
    {
        // Placeholder: fill this in later with win screen, level progression, etc.
        Debug.Log("You Win!");
    }
}
