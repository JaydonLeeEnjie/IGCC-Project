using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Billboard : MonoBehaviour
{
    private Camera mainCam;

    [SerializeField] private Transform target;   // �Ǐ]����Ώ�
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

        // ��ɃJ��������������
        transform.forward = mainCam.transform.forward;
    }
}
