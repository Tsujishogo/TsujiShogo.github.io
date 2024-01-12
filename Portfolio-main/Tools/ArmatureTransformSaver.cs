using UnityEditor;
using UnityEngine;

public class ArmatureTransformSaver : EditorWindow
{
    [MenuItem("Window/Armature Transform Saver")]
    public static void ShowWindow()
    {
        GetWindow<ArmatureTransformSaver>("Armature Transform Saver");
    }

    GameObject copyArmature; // �R�s�[��object
    GameObject destinationArmature; // �R�s�[��object

    void OnGUI() // GUI
    {
        EditorGUILayout.LabelField("���ꂼ��I�����Ď��s�I�R�s�[��͕�������܂��B", EditorStyles.boldLabel);//����


        copyArmature = EditorGUILayout.ObjectField("�R�s�[��", copyArmature,
                     typeof(GameObject), true) as GameObject;

        destinationArmature = EditorGUILayout.ObjectField("�R�s�[��", destinationArmature, 
                     typeof(GameObject), true) as GameObject;

        if (GUILayout.Button("Copy!"))
        {
            CopyRun(copyArmature,destinationArmature);
        }
    }

    public void CopyRun(GameObject from, GameObject to)
    {
        if (copyArmature == null || destinationArmature == null)
        {
            Debug.LogWarning("���w���object������܂�");
            return;
        }
        // �R�s�[��𕡐����ĉ��ɂ��炷
        GameObject duplicateTo = Instantiate(to);
        duplicateTo.transform.position +=  new Vector3(-1, 0, 0);
        //���ꂼ���Armature��T���B�i�K�w����ԏ�̂��́j
        Transform fromT = from.transform.Find("Armature");
        Transform toT = duplicateTo.transform.Find("Armature");

        CopyTransform(fromT, toT);
    }



 
    private void CopyTransform(Transform from, Transform to)
    {
        
        
        // ���O�������ł���΁ATransform�̒l���R�s�[
        if (from.name == to.name)
        {
            from.position = to.position;
            from.rotation = to.rotation;
            from.localScale = to.localScale;
        }

        // A��B�̎q�I�u�W�F�N�g�������čċA�I�ɏ���
        foreach (Transform fromChild in from)
        {
            // �������O�̍ŏ�ʂ̃I�u�W�F�N�g��������
            Transform toChild = null;
            foreach (Transform child in to)
            {
                if (child.name == fromChild.name)
                {
                    toChild = child;
                    break;
                }
            }

            // �ŏ�ʂ̓����I�u�W�F�N�g�����������ꍇ�A�ċA�I�ɏ���
            if (toChild != null)
            {
                CopyTransform(fromChild, toChild);
            }
        }
    }

}
