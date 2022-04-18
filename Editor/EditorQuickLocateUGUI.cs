#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

[InitializeOnLoad]
public class EditorQuickLocateUGUI
{
    static List<RaycastResult> raycastResults = new List<RaycastResult>();
    static Vector2 oldPosition = Vector2.zero;
    static int index = 0;
    static EditorUpdateHelper()
    {
        EditorApplication.update += Update;
    }

    static void Update()
    {
        if (!EditorApplication.isPlaying)
            return;
        PingObject();
        PingUIObject();
    }
    //选中3D对象
    static void PingObject()
    {
        //左Ctrl加左键
        if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftControl))
        {
            //Input.mousePosition与Touch.position相同？？？
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                EditorGUIUtility.PingObject(hitInfo.transform);
            }
        }
    }

    //选中UI对象
    static void PingUIObject()
    {
        //左Ctrl加右键
        if (Input.GetMouseButtonDown(1) && Input.GetKey(KeyCode.LeftControl))
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            if (oldPosition != pointerEventData.position)
            {
                oldPosition = pointerEventData.position;
                index = 0;
            }
            else
            {
                index++;
            }
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);
            if (raycastResults.Count > 0)
            {
                if (index >= raycastResults.Count)
                    index = 0;

                EditorGUIUtility.PingObject(raycastResults[index].gameObject);
            }
        }
    }
}
#endif
