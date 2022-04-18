using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorHotkey
{
    public static bool IsPlaying = false;
    
    [MenuItem("TTT/HotKey/暂停 _F2")]
    public static void Switch()
    {
        IsPlaying = !IsPlaying;
        if (IsPlaying)
        {
            Play();
        }
        else
        {
            Pause();
        }
    }

    public static void Play()
    {
        EditorApplication.isPaused = false;
    }

    public static void Pause()
    {
        EditorApplication.isPaused = true;
    }
    
}