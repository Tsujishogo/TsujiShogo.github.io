using UnityEngine;
using UnityEditor;

public class BlendShapeToAnimation : EditorWindow
{
    SkinnedMeshRenderer targetRenderer; //�Ώ�Mesh
    string animationFileName = "NewAnimation";�@// ��������t�@�C����
    int ignoreCountTop = 0; // �ォ�疳�����鐔��
    int ignoreCountBottom = 0; // �����疳�����鐔��
    bool ignoreZeroValues = false; // �l��0��BlendShape��animation�ɔ��f���邩�ǂ���
    string outputFolderPath = "Assets/Animations";�@// �ۑ���p�X

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
            GenerateAnimation(); //���s�֐�
        }
    }

    private void GenerateAnimation()
    {
        if (targetRenderer == null) // null�`�F�b�N
        {
            Debug.LogError("�Ώۂ�SkinMeshRenderer���w�肵�Ă�");
            return;
        }

        //�Ώۂ�BlendShape�����J�E���g
        int blendShapeCount = targetRenderer.sharedMesh.blendShapeCount; 
        EditorCurveBinding[] curveBindings = new EditorCurveBinding[blendShapeCount];

        var animationClip = new AnimationClip(); // animation�t�@�C���������Ė��O������
        animationClip.name = animationFileName;


        for (int i = 0; i < blendShapeCount; i++)
        {
            if (i < ignoreCountTop || i >= blendShapeCount - ignoreCountBottom)// ��O�ݒ蕪���O
            {
                continue;
            }
            // �Ώۂ�BlendShape�̖��O�ƒl���擾
            string blendShapeName = targetRenderer.sharedMesh.GetBlendShapeName(i);
            var blendShapeValue = targetRenderer.GetBlendShapeWeight(i);

            if (ignoreZeroValues && Mathf.Approximately(blendShapeValue, 0f))// 0�̒l���擾���Ȃ��ݒ�̏ꍇ�Ƃ΂�
            {
                continue;
            }

            // blendshape��ۑ����邽�߂̃v���p�e�B�擾
            EditorCurveBinding curveBinding = new EditorCurveBinding
            {
                path = targetRenderer.gameObject.name,
                propertyName = "blendShapes." + blendShapeName,
                type = typeof(SkinnedMeshRenderer)
            };

            // animation�t�@�C����0�t���[���ɕۑ�
            AnimationUtility.SetEditorCurve(animationClip, curveBinding, new AnimationCurve(new Keyframe(0f, blendShapeValue)));

            curveBindings[i] = curveBinding;
        }


        if (!string.IsNullOrEmpty(outputFolderPath))// �t�@�C���ۑ�
        {
            var fileName = animationFileName + ".anim";
            var filePath = System.IO.Path.Combine(outputFolderPath, fileName);
            AssetDatabase.CreateAsset(animationClip, filePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}