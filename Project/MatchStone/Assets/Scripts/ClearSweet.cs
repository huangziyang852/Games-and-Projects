using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearSweet : MonoBehaviour
{
    public AnimationClip clearAnimation;

    public AudioClip destroyAudio;

    private bool isClearing;

    public bool IsClearing { get => isClearing; }

    protected GameSweet gameSweet;

    public virtual void Clear()
    {
        isClearing = true;
        StartCoroutine(clearCoroutine());
    }

    private IEnumerator clearCoroutine()
    {
        Animator animator = GetComponent<Animator>();
        if(animator!=null)
        {
            animator.Play(clearAnimation.name);
            //玩家得分+1，
            GameManager.Instance.playerScore += 1;
            //播放音效
            AudioSource.PlayClipAtPoint(destroyAudio, transform.position);//音源和位置
            yield return new WaitForSeconds(clearAnimation.length);
            Destroy(gameObject);
        }
    }
}
