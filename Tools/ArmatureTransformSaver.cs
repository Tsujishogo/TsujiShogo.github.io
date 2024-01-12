using UnityEditor;
using UnityEngine;

public class ArmatureTransformSaver : EditorWindow
{
    [MenuItem("Window/Armature Transform Saver")]
    public static void ShowWindow()
    {
        GetWindow<ArmatureTransformSaver>("Armature Transform Saver");
    }

    GameObject copyArmature; // コピー元object
    GameObject destinationArmature; // コピー先object

    void OnGUI() // GUI
    {
        EditorGUILayout.LabelField("それぞれ選択して実行！コピー先は複製されます。", EditorStyles.boldLabel);//説明


        copyArmature = EditorGUILayout.ObjectField("コピー先", copyArmature,
                     typeof(GameObject), true) as GameObject;

        destinationArmature = EditorGUILayout.ObjectField("コピー元", destinationArmature, 
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
            Debug.LogWarning("未指定のobjectがあります");
            return;
        }
        // コピー先を複製して横にずらす
        GameObject duplicateTo = Instantiate(to);
        duplicateTo.transform.position +=  new Vector3(-1, 0, 0);
        //それぞれのArmatureを探す。（階層が一番上のもの）
        Transform fromT = from.transform.Find("Armature");
        Transform toT = duplicateTo.transform.Find("Armature");

        CopyTransform(fromT, toT);
    }



 
    private void CopyTransform(Transform from, Transform to)
    {
        
        
        // 名前が同じであれば、Transformの値をコピー
        if (from.name == to.name)
        {
            from.position = to.position;
            from.rotation = to.rotation;
            from.localScale = to.localScale;
        }

        // AとBの子オブジェクトを見つけて再帰的に処理
        foreach (Transform fromChild in from)
        {
            // 同じ名前の最上位のオブジェクトを見つける
            Transform toChild = null;
            foreach (Transform child in to)
            {
                if (child.name == fromChild.name)
                {
                    toChild = child;
                    break;
                }
            }

            // 最上位の同名オブジェクトが見つかった場合、再帰的に処理
            if (toChild != null)
            {
                CopyTransform(fromChild, toChild);
            }
        }
    }

}
