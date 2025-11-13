using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SideBlockChecker))]
public class SideBlockCheckerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var checker = (SideBlockChecker)target;

        EditorGUILayout.Space();

        // targetObject フィールドの表示＆変更受付
        checker.targetObject = (GameObject)EditorGUILayout.ObjectField("Target Object", checker.targetObject, typeof(GameObject), true);

        if (checker.targetObject != null)
        {
            // targetObjectにアタッチされているMonoBehaviourを全部取得
            var components = checker.targetObject.GetComponents<MonoBehaviour>();

            string[] options = new string[components.Length];
            int currentIndex = -1;

            for (int i = 0; i < components.Length; i++)
            {
                options[i] = components[i].GetType().Name;
                if (components[i] == checker.targetScript)
                    currentIndex = i;
            }

            int selected = EditorGUILayout.Popup("Target Script", currentIndex, options);

            if (selected >= 0 && selected < components.Length)
            {
                if (checker.targetScript != components[selected])
                {
                    Undo.RecordObject(checker, "Change Target Script");
                    checker.targetScript = components[selected];
                    EditorUtility.SetDirty(checker);
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Target Object をセットしてください。", MessageType.Info);
            checker.targetScript = null;
        }
    }
}

