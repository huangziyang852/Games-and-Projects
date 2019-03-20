using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSweet : MonoBehaviour
{
    private GameSweet sweet;  //获取脚本
    private IEnumerator moveCoroutine;  //单个甜品的协程，得到其他指令可以终止携程
    private void Awake()
    {
        sweet = GetComponent<GameSweet>();
    }
    //负责开启和结束一个协程
    public void Move(int newX,int newY,float time)
    {
        sweet.X = newX;
        sweet.Y = newY;
        sweet.transform.position = sweet.gameManager.CorrectPos(newX, newY);   //更新位置

        if(moveCoroutine!=null)
        {
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine = MoveCoroutine(newX, newY,time);
        StartCoroutine(moveCoroutine);
    }

    //负责移动的协程
    private IEnumerator MoveCoroutine(int newX,int newY,float time)
    {
        sweet.X = newX;
        sweet.Y = newY;

        //每一帧移动一点点
        Vector3 startPos = transform.position;
        Vector3 endPos = sweet.gameManager.CorrectPos(newX, newY);

        for(float t=0;t<time;t+=Time.deltaTime)
        {
            sweet.transform.position = Vector3.Lerp(startPos, endPos, t / time);//平滑插值，lerp(,,0.5f)就是每次接近一半
            yield return 0;  //等待一帧
        }

        sweet.transform.position = endPos;//强制到达指定位置
    }
}
