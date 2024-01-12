using UnityEngine;
using UnityEditor;

public class BlendShapeToAnimation : EditorWindow
{
    SkinnedMeshRenderer targetRenderer; //対象Mesh
    string animationFileName = "NewAnimation";　// 生成するファイル名
    int ignoreCountTop = 0; // 上から無視する数量
    int ignoreCountBottom = 0; // 下から無視する数量
    bool ignoreZeroValues = false; // 値が0のBlendShapeをanimationに反映するかどうか
    string outputFolderPath = "Assets/Animations";　// 保存先パス

    [MenuItem("Window/BlendShape Animation Generator")]
    static void OpenWindow()
    {
        var window = GetWindow<BlendShapeToAnimation>();
        window.titleContent = new GUIContent("BlendShape Animation");
        window.Show();
    }

    void OnGUI()
    {
        targetRenderer = EditorGUILayout.ObjectField("Target Renderer", targetRenderer, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;
        animationFileName = EditorGUILayout.TextField("Animation File Name", animationFileName);

        ignoreCountTop = EditorGUILayout.IntField("Ignore Count (Top)", ignoreCountTop);
        ignoreCountBottom = EditorGUILayout.IntField("Ignore Count (Bottom)", ignoreCountBottom);
        ignoreZeroValues = EditorGUILayout.Toggle("Ignore Zero Values", ignoreZeroValues);
        outputFolderPath = EditorGUILayout.TextField("Save Folder Path", outputFolderPath);
        if (GUILayout.Button("Generate Animation"))
        {
            GenerateAnimation(); //実行関数
        }
    }

    private void GenerateAnimation()
    {
        if (targetRenderer == null) // nullチェック
        {
            Debug.LogError("対象のSkinMeshRendererを指定してね");
            return;
        }

        //対象のBlendShape数をカウント
        int blendShapeCount = targetRenderer.sharedMesh.blendShapeCount; 
        EditorCurveBinding[] curveBindings = new EditorCurveBinding[blendShapeCount];

        var animationClip = new AnimationClip(); // animationファイル生成して名前をつける
        animationClip.name = animationFileName;


        for (int i = 0; i < blendShapeCount; i++)
        {
            if (i < ignoreCountTop || i >= blendShapeCount - ignoreCountBottom)// 例外設定分除外
            {
                continue;
            }
            // 対象のBlendShapeの名前と値を取得
            string blendShapeName = targetRenderer.sharedMesh.GetBlendShapeName(i);
            var blendShapeValue = targetRenderer.GetBlendShapeWeight(i);

            if (ignoreZeroValues && Mathf.Approximately(blendShapeValue, 0f))// 0の値を取得しない設定の場合とばす
            {
                continue;
            }

            // blendshapeを保存するためのプロパティ取得
            EditorCurveBinding curveBinding = new EditorCurveBinding
            {
                path = targetRenderer.gameObject.name,
                propertyName = "blendShapes." + blendShapeName,
                type = typeof(SkinnedMeshRenderer)
            };

            // animationファイルの0フレームに保存
            AnimationUtility.SetEditorCurve(animationClip, curveBinding, new AnimationCurve(new Keyframe(0f, blendShapeValue)));

            curveBindings[i] = curveBinding;
        }


        if (!string.IsNullOrEmpty(outputFolderPath))// ファイル保存
        {
            var fileName = animationFileName + ".anim";
            var filePath = System.IO.Path.Combine(outputFolderPath, fileName);
            AssetDatabase.CreateAsset(animationClip, filePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}