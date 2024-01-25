using UnityEngine;

/// <summary>
/// �K�C�h���C�������N���X
/// </summary>
public class GuideLine : MonoBehaviour
{
    [Header("�K�C�h���C���������̈ʒu")]
    [SerializeField] Transform _handTransform;
    [Header("������̈ʒu")]
    [SerializeField] Transform _swordTransform;
    [Header("�x�W�G�Ȑ����_�ʒu")]
    [SerializeField] Vector3 _controlPoint;
    [Header("�x�W�G�Ȑ��̐���_�̐�(�����قǂȂ߂炩)")]
    [SerializeField] int _curvePosCount = 5;
    [Header("�Ȑ��ɂ���LineRenderer")]
    [SerializeField] LineRenderer _lineRenderer;

    void Start()
    {
        _lineRenderer.positionCount = _curvePosCount; // ����_������
    }

    void Update()
    {
        Vector3 controlPos = _handTransform.position + (_swordTransform.position - _handTransform.position) * 0.5f + _controlPoint; // ����_�̈ʒu

        for (int i = 0; i < _curvePosCount; i++) // ����_���Ƀx�W�G�Ȑ��̌v�Z
        {
            float t = i / (float)(_curvePosCount - 1); 
            Vector3 point = SampleCurve(_handTransform.position, _swordTransform.position,controlPos, t);
            _lineRenderer.SetPosition(i, point); // �v�Z���ʂ𔽉f
        }

        // �ȉ��R�[�h�̎Q�l https://developer.oculus.com/blog/teleport-curves-with-the-gear-vr-controller/
        Vector3 SampleCurve(Vector3 start, Vector3 end, Vector3 control, float t)
        {
            // Interpolate along line S0: control - start;
            Vector3 Q0 = Vector3.Lerp(start, control, t);
            // Interpolate along line S1: S1 = end - control;
            Vector3 Q1 = Vector3.Lerp(control, end, t);
            // Interpolate along line S2: Q1 - Q0
            Vector3 Q2 = Vector3.Lerp(Q0, Q1, t);
            return Q2; // Q2 is a point on the curve at time t
        }

    }
}
