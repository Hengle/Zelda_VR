using System.Linq;
using UnityEngine;

public static class AnimationExtensions
{
    // Animation
    // NOTE: The following Animation methods basically assume this Animation has only one state

    public static float DurationInSeconds(this Animation anim)
    {
        float duration = 0;
        foreach (AnimationState state in anim)
        {
            duration = state.length;
            break;
        }
        return duration;
    }

    public static void JumpToStartOfAnimationAndStop(this Animation anim)
    {
        foreach (AnimationState state in anim)
        {
            state.time = 0;
            state.speed = 0;
            anim.Play();
        }
    }

    public static void JumpToStartOfAnimationAndPlay(this Animation anim)
    {
        foreach (AnimationState state in anim)
        {
            state.time = 0;
            state.speed = 1;
        }
        anim.Play();
    }

    public static void JumpToEndOfAnimationAndStop(this Animation anim)
    {
        foreach (AnimationState state in anim)
        {
            state.time = state.length - 0.01f;
            state.speed = 1;
        }
        anim.Play();
    }

    public static void JumpToEndOfAnimationAndPlayInReverse(this Animation anim)
    {
        foreach (AnimationState state in anim)
        {
            state.time = state.length - 0.01f;
            state.speed = -1;
        }
        anim.Play();
    }


    // Animator
    public static bool ContainsParam(this Animator _anim, string _paramName)
    {
        return _anim.parameters.FirstOrDefault(p => p.name == _paramName) != null;
    }
}