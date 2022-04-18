using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class AnimationWrap : SerializedMonoBehaviour
{
    private Animation _animation;
    
    [DictionaryDrawerSettings(KeyLabel = "状态名", ValueLabel = "AnimationClip")]
    public Dictionary<string, List<AnimationClip>> animationDic = new Dictionary<string, List<AnimationClip>>();
    
    
    void Awake()
    {
        _animation = GetComponent<Animation>();
        _animation.wrapMode = WrapMode.Default;
        
        // PlayList("test1");
    }

    public void PlayList(string stateStr)
    {
        if (_animation.isPlaying)
        {
            _animation.Stop();
            foreach (AnimationState state in _animation)
            {
                _animation.RemoveClip(state.clip.name);
            }
        }


        foreach (var clip in animationDic[stateStr])
        {
            _animation.AddClip(clip, clip.name);
            _animation.PlayQueued(clip.name);
        }
    }
}
