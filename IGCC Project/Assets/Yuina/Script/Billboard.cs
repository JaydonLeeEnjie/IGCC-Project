using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Billboard : MonoBehaviour
{
    private Camera mainCam;

    [SerializeField] private Transform target;   // 追従する対象
    [SerializeField] private Vector3 offset = new Vector3(0, 0, 0); // 頭上に出す位置(インスペクターで設定)


    void Start()
    {
        mainCam = Camera.main;

        // target が未設定なら親オブジェクトを自動参照
        if (target == null && transform.parent != null)
        {
            target = transform.parent;
        }

    }

    void LateUpdate()
    {
        if (mainCam == null || target == null) return;

        // 常に target の上に表示
        transform.position = target.position + offset;

        // 常にカメラ方向を向く
        transform.forward = mainCam.transform.forward;
    }
}
