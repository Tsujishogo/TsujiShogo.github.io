using UnityEngine;

/// <summary>
/// ガイドライン生成クラス
/// </summary>
public class GuideLine : MonoBehaviour
{
    [Header("ガイドライン生成元の位置")]
    [SerializeField] Transform _handTransform;
    [Header("発生先の位置")]
    [SerializeField] Transform _swordTransform;
    [Header("ベジエ曲線制点位置")]
    [SerializeField] Vector3 _controlPoint;
    [Header("ベジエ曲線の制御点の数(多いほどなめらか)")]
    [SerializeField] int _curvePosCount = 5;
    [Header("曲線にするLineRenderer")]
    [SerializeField] LineRenderer _lineRenderer;

    void Start()
    {
        _lineRenderer.positionCount = _curvePosCount; // 制御点初期化
    }

    void Update()
    {
        Vector3 controlPos = _handTransform.position + (_swordTransform.position - _handTransform.position) * 0.5f + _controlPoint; // 制御点の位置

        for (int i = 0; i < _curvePosCount; i++) // 制御点毎にベジエ曲線の計算
        {
            float t = i / (float)(_curvePosCount - 1); 
            Vector3 point = SampleCurve(_handTransform.position, _swordTransform.position,controlPos, t);
            _lineRenderer.SetPosition(i, point); // 計算結果を反映
        }

        // 以下コードの参考 https://developer.oculus.com/blog/teleport-curves-with-the-gear-vr-controller/
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
