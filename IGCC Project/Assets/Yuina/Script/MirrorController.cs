using UnityEngine;

public class MirrorController : MonoBehaviour
{
    private bool isPlayerNearby = false;


    private void Update()
    {
        if (!isPlayerNearby) return;

        // Xキーで時計回りに45°
        if (Input.GetKeyDown(KeyCode.X))
        {
            RotateMirror(-45f);
        }

        // Zキーで反時計回りに45°
        if (Input.GetKeyDown(KeyCode.Z))
        {
            RotateMirror(45f);
        }
    }

    private void RotateMirror(float angle)
    {
        transform.Rotate(0f, 0f, angle); // Z軸回転
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
