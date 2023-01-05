using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAnimationStart : MonoBehaviour
{
    private void Start()
    {
        Animation animation = GetComponent<Animation>();
        float time = animation.clip.length;
        animation.Stop();
        StartCoroutine(I_DelayAnimation(animation, Random.Range(0.0F, time)));
    }

    private IEnumerator I_DelayAnimation(Animation _animation, float _seconds)
    {
        yield return new WaitForSeconds(_seconds);
        _animation.Play();
    }
}
