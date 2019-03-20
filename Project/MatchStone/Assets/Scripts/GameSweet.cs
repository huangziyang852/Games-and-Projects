using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSweet : MonoBehaviour
{
    private int x;
    private int y;

    public int X
    { get => x;

        set
        {
            if(CanMove())//可以移动，才可以更改坐标
            {
                x = value;
            }
        } 
    }
    public int Y
    { get => y;
        set
        {
            if(CanMove())
            {
                y = value;
            }
        }
    }

    [HideInInspector]//隐藏
    public GameManager gameManager;

    private GameManager.SweetsType type;
    public GameManager.SweetsType Type { get => type;}
    /// <summary>
    /// 获取移动脚本
    /// </summary>
    public MoveSweet MoveComponent { get => moveComponent;  }
    private MoveSweet moveComponent;

    public ColorSweet ColorComponent { get => colorComponent; }
    private ColorSweet colorComponent;

    public ClearSweet ClearComponent { get => clearComponent;  }
    private ClearSweet clearComponent;

    private void Awake()
    {
        moveComponent = GetComponent<MoveSweet>();
        colorComponent = GetComponent<ColorSweet>();
        clearComponent = GetComponent<ClearSweet>();
    }

    /// <summary>
    /// 判断是否能移动
    /// </summary>
    /// <returns></returns>
    public bool CanMove()
    {
        return moveComponent != null;
    }
    /// <summary>
    /// 判断是否可以改变颜色
    /// </summary>
    /// <returns></returns>
    public bool CanColored()
    {
        return colorComponent != null;
    }

    public bool CanClear()
    {
        return clearComponent != null;
    }
    public void Init(int _x,int _y,GameManager _gameManager,GameManager.SweetsType _type)
    {
        x = _x;
        y = _y;
        gameManager = _gameManager;
        type = _type;
    }

    //鼠标监听需要Boxcollision
    private void OnMouseEnter()
    {
        gameManager.EnterSweet(this);
    }

    private void OnMouseDown()
    {
        gameManager.PressSweet(this);
    }

    private void OnMouseUp()
    {
        gameManager.ReleaseSweet();
    }
}
