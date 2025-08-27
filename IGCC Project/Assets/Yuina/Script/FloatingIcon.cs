using UnityEngine;

public class FloatingIcon : MonoBehaviour
{
    private Camera mainCam;

    [SerializeField] private Transform target;   // �Ǐ]����Ώہi�{��I�j
    [SerializeField] private Vector3 offset = new Vector3(0, 0, 0); // ����ɏo���ʒu(�C���X�y�N�^�[�Őݒ�)

    void Start()
    {
        mainCam = Camera.main;

        // target �����ݒ�Ȃ�e�I�u�W�F�N�g�������Q��
        if (target == null && transform.parent != null)
        {
            target = transform.parent;
        }
    }

    void LateUpdate()
    {
        if (mainCam == null || target == null) return;

        // ��� target �̏�ɕ\��
        transform.position = target.position + offset;

        // ��ɃJ�����ɐ��΁i�r���{�[�h�j
        transform.rotation = Quaternion.LookRotation(mainCam.transform.forward, Vector3.up);

        // ���[���h�X�P�[�����Œ肷��
        Vector3 desiredWorldScale = Vector3.one * 1.0f; // �T�C�Y
        transform.localScale = Vector3.one; // �܂����Z�b�g
        transform.localScale = new Vector3(
            desiredWorldScale.x / transform.lossyScale.x,
            desiredWorldScale.y / transform.lossyScale.y,
            desiredWorldScale.z / transform.lossyScale.z
        );
    }

}
