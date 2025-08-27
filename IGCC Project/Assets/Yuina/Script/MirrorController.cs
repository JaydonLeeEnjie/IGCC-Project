using UnityEngine;

public class MirrorController : MonoBehaviour
{
    private bool isPlayerNearby = false;


    private void Update()
    {
        if (!isPlayerNearby) return;

        // X�L�[�Ŏ��v����45��
        if (Input.GetKeyDown(KeyCode.X))
        {
            RotateMirror(-45f);
        }

        // Z�L�[�Ŕ����v����45��
        if (Input.GetKeyDown(KeyCode.Z))
        {
            RotateMirror(45f);
        }
    }

    private void RotateMirror(float angle)
    {
        transform.Rotate(0f, 0f, angle); // Z����]
    }

        private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }
}
