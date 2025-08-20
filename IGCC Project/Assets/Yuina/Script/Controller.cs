using UnityEngine;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour
{
    private float m_speed = 5.0f;
    private InputAction m_moveAction;
    private SpriteRenderer m_spriteRenderer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_moveAction = InputSystem.actions.FindAction("Move");
        // ‰Šú‰»
        m_moveAction.Enable();

        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        var moveValue = m_moveAction.ReadValue<Vector2>();
        var move = new Vector3(moveValue.x, 0f, moveValue.y) * m_speed * Time.deltaTime;
        transform.Translate(move);

        // •ûŒü“]Š·
        if (moveValue.x > 0.01f)    // ‰E‚É“ü—Í‚ª‚ ‚é‚Æ‚«
        {
            m_spriteRenderer.flipX = true;
        }
        else if (moveValue.x < -0.01f) // ¶‚É“ü—Í‚ª‚ ‚é‚Æ‚«
        {
            m_spriteRenderer.flipX = false;
        }
    }
}
