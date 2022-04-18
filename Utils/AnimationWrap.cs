using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animation))]
public class AnimationWrap : MonoBehaviour
{
    private Animation _animation;
    
    public SerializableDictionary<string, List<AnimationClip>> animationDic = new SerializableDictionary<string, List<AnimationClip>>();
    
    void Awake()
    {
        if (_animation == null)
        {
            _animation = GetComponent<Animation>();
            _animation.playAutomatically = false;
        }

        foreach (var keyValue in animationDic)
        {
            foreach (var animationClip in keyValue.Value)
            {
                _animation.AddClip(animationClip, animationClip.name);
            }
        }

        if (animationDic.ContainsKey("test"))
        {
            PlayList("test");
        }
    }

    public void PlayList(string stateStr)
    {
        // Debug.Log($"============ {stateStr}");
        if (_animation == null)
        {
            _animation = GetComponent<Animation>();
        }

        if (_animation.isPlaying)
        {
            _animation.Stop();
        }


        foreach (var clip in animationDic[stateStr])
        {
            // _animation.CrossFadeQueued(clip.name, 0);
            // _animation.wrapMode = clip.wrapMode;
            _animation.PlayQueued(clip.name);
        }
    }

    // private void OnGUI()
    // {
    //     if (GUI.Button(new Rect(0, 0, 100, 50), "第一个Button"))
    //     {
    //         PlayList("test1");
    //     }
    //
    //     if (GUI.Button(new Rect(200, 0, 100, 50), "第二个Button"))
    //     {
    //         PlayList("test2");
    //     }
    // }
}
