using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class Animator2Animation : Editor
{
    static AnimationClip GetLegecyAnimationClip(AnimationClip animClip1, string newNameSuffix)
    {
        var unitypath = AssetDatabase.GetAssetPath(animClip1);
        var desFilePath = Path.GetDirectoryName(unitypath) +
                          "/" + Path.GetFileNameWithoutExtension(unitypath) +
                          newNameSuffix + ".anim";

        return AssetDatabase.LoadAssetAtPath<AnimationClip>(desFilePath);
    }

    [MenuItem("TTT/Rendering/Animator2Animation", false)]
    public static void AnimatorToAnimation()
    {
        Object[] objs = Selection.objects;
        foreach (Object objone in objs)
        {
            // i++;
            // EditorUtility.DisplayProgressBar("替换进度", $"{objone.name}", i / (float)count);
            GameObject obj = objone as GameObject;

            var animators = obj.GetComponentsInChildren<Animator>(true);
            var newNameSuffix = "_Legacy";
            foreach (var animator in animators)
            {
                var childObj = animator.gameObject;
                var clips = animator.runtimeAnimatorController.animationClips;
                
                Animation animation = childObj.AddComponent<Animation>();
                // animation.clip = GetLegecyAnimationClip(clips[0], newNameSuffix);
                AnimationWrap animationWrap = childObj.AddComponent<AnimationWrap>();

                foreach (var clip in clips)
                {
                    var path = AssetDatabase.GetAssetPath(clip);
                    string relPath = Path.GetFullPath(path);
                    var desFilePath = UniversalEditorUtility.GetUnityDirectoryPath(relPath) +
                                      "/" + Path.GetFileNameWithoutExtension(relPath) +
                                      newNameSuffix + ".anim";
                    File.Copy(relPath, desFilePath, true);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    var destClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(desFilePath);
                    destClip.legacy = true;
                    if (clip.isLooping)
                    {
                        destClip.wrapMode = WrapMode.Loop;
                    }

                    EditorUtility.SetDirty(destClip);

                    animation.AddClip(destClip, destClip.name);
                }

                // Generate Animation Dic
                var states = UniversalEditorUtility.GetAllAnimatorStateInfo(animator);
                SerializableDictionary<string, List<AnimationClip>> animationDic = new SerializableDictionary<string, List<AnimationClip>>();
                foreach (AnimatorState state in states)
                {
                    AnimationClip animClip1 = new AnimationClip();
                    AnimationClip animClip2 = new AnimationClip();
                    AnimationClip animClip3 = new AnimationClip();
                    AnimationClip animClip4 = new AnimationClip();
                    List<AnimationClip> animationClips = new List<AnimationClip>();

                    animClip1 = state.motion as AnimationClip;
                    animationClips.Add(GetLegecyAnimationClip(animClip1, newNameSuffix));
                    if (state.transitions.Length > 0)
                    {
                        var nextState2 = state.transitions[0].destinationState;
                        animClip2 = nextState2.motion as AnimationClip;
                        animationClips.Add(GetLegecyAnimationClip(animClip2, newNameSuffix));
                        if (nextState2.transitions.Length > 0)
                        {
                            var nextState3 = nextState2.transitions[0].destinationState;
                            animClip3 = nextState3.motion as AnimationClip;
                            animationClips.Add(GetLegecyAnimationClip(animClip3, newNameSuffix));
                            if (nextState3.transitions.Length > 0)
                            {
                                var nextState4 = nextState3.transitions[0].destinationState;
                                animClip4 = nextState4.motion as AnimationClip;
                                animationClips.Add(GetLegecyAnimationClip(animClip4, newNameSuffix));
                            }
                        }
                    }

                    animationDic.Add(state.name, animationClips);
                    // Debug.Log($"{state.name}  {animClip1.name} {animClip2.name} {animClip3.name} {animClip4.name}");
                }

                animationWrap.animationDic = animationDic;
                UniversalEditorUtility.RemoveComponent(childObj, typeof(Animator));
                EditorUtility.SetDirty(animation);
            }
        }
        // EditorUtility.ClearProgressBar();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
