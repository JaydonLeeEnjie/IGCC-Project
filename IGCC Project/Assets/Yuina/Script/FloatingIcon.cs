using UnityEngine;

public class FloatingIcon : MonoBehaviour
{
    private Camera mainCam;

    [SerializeField] private Transform target;   // 追従する対象（本や棚）
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

        // 常にカメラに正対（ビルボード）
        transform.rotation = Quaternion.LookRotation(mainCam.transform.forward, Vector3.up);

        // ワールドスケールを固定する
        Vector3 desiredWorldScale = Vector3.one * 1.0f; // サイズ
        transform.localScale = Vector3.one; // まずリセット
        transform.localScale = new Vector3(
            desiredWorldScale.x / transform.lossyScale.x,
            desiredWorldScale.y / transform.lossyScale.y,
            desiredWorldScale.z / transform.lossyScale.z
        );
    }

}
