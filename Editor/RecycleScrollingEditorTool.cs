using UnityEditor;
using UnityEngine;

namespace Hzeff.Editor
{
    [ExecuteInEditMode]
    public static class RecycleScrollingEditorTool
    {
        const string PrefabName = "Recyclable Scroll View";
        const string PrefabPath = "Packages/Recyclable Scroll Rect/Runtime/Prefabs/Recyclable Scroll View.prefab";

        [MenuItem("GameObject/UI/Recyclable Scroll View")]
        private static void CreateRecyclableScrollView()
        {
            GameObject selected = Selection.activeGameObject;

            //If selected isn't a UI gameobject then find a Canvas
            if (!selected || !(selected.transform is RectTransform))
            {
                selected = GameObject.FindObjectOfType<Canvas>().gameObject;
            }

            if (!selected) return;

            GameObject asset = AssetDatabase.LoadAssetAtPath(PrefabPath, typeof(GameObject)) as GameObject;

            GameObject item = Object.Instantiate(asset, selected.transform, true);
            item.name = PrefabName;

            item.transform.localPosition = Vector3.zero;
            item.transform.localScale = Vector3.one;
            item.transform.localRotation = Quaternion.identity;

            item.GetComponent<RectTransform>().sizeDelta = selected.GetComponent<RectTransform>().sizeDelta;

            Selection.activeGameObject = item;
            Undo.RegisterCreatedObjectUndo(item, $"Create {PrefabName}");
        }
    }
}