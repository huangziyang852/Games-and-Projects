using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //甜品种类
    public enum SweetsType
    {
        EMPTY,
        NORMAL,
        BARRIER,
        ROW_CLEAR,
        COLUMN_CLEAR,
        RAINBOWCANDY,
        COUNT //标记类型
    }
    private Dictionary<SweetsType, GameObject> sweetPrefabDir; //HashTable不可检视

    [System.Serializable]//序列化
    public struct SweetPrefab
    {
        public SweetsType type;
        public GameObject prefab;
    }

    public SweetPrefab[] sweetPrefabs;

    private static GameManager _instance;   //单例模式

    public static GameManager Instance { get => _instance; set => _instance = value; }

    public int xColumn;
    public int yRow;
    //填充时间
    public float fillTime;

    public GameObject gridPrefab;

    //甜品数组
    private GameSweet[,] sweets;
    //要交换的甜品对象
    private GameSweet pressedSweet;
    private GameSweet enteredSweet;
    //关于UI显示的内容
    public Text timeText;
    private float gameTime =60;
    private bool gameOver;
    public int playerScore;
    public Text playerScoreText;
    public Button quitButton;

    private void Awake()
    {
        _instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        //字典实例化
        sweetPrefabDir = new Dictionary<SweetsType, GameObject>();
        for(int i=0; i<sweetPrefabs.Length;i++)
        {
            if(!sweetPrefabDir.ContainsKey(sweetPrefabs[i].type))  //如果不含有该Key就添加到字典中
            {
                sweetPrefabDir.Add(sweetPrefabs[i].type, sweetPrefabs[i].prefab);
            }
        }

        //创建巧克力
        CreateGrid();
        //实例化甜品数组
        sweets = new GameSweet[xColumn, yRow];
        for (int i = 0; i < xColumn; i++)
        {
            for (int j = 0; j < yRow; j++)
            {
                CreateNewSweet(i, j, SweetsType.EMPTY);

            }
        }
        //填充甜品

        Destroy(sweets[4, 4].gameObject);
        CreateNewSweet(4, 4, SweetsType.BARRIER);
        StartCoroutine(AllFill());
        
    }

    // Update is called once per frame
    void Update()
    {
        if(gameOver)
        {
            return;
        }

        gameTime -= Time.deltaTime;
        if(gameTime<=0)
        {
            gameTime = 0;
            //显示失败面板
            gameOver = true;
            return;
        }
        timeText.text = gameTime.ToString("0");  //取整
        playerScoreText.text = playerScore.ToString();
        
        
    }
    /// <summary>
    /// 生成巧克力,从上往下，从左往右
    /// </summary>
    private void CreateGrid()
    {
        for(int i=0;i<xColumn;i++)
        {
            for(int j=0;j<yRow;j++)
            {
                GameObject chocolate = Instantiate(gridPrefab,CorrectPos(i,j), Quaternion.identity);
                chocolate.transform.SetParent(_instance.transform);
            }
        }
    }
    /// <summary>
    /// 纠正位置,长和高减去网格长度的一半
    /// </summary>
    /// <returns></returns>
    public Vector3 CorrectPos(int x,int y)
    {
        return new Vector3(transform.position.x - xColumn / 2f + x, transform.position.y + yRow / 2f - y,0);
    }
    /// <summary>
    /// 生成甜品的方法
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="sweetsType"></param>
    /// <returns></returns>
    public GameSweet CreateNewSweet(int x,int y,SweetsType sweetsType)
    {
        GameObject newSweet= Instantiate(sweetPrefabDir[sweetsType], CorrectPos(x, y), Quaternion.identity);
        newSweet.transform.parent = transform;

        sweets[x, y] = newSweet.GetComponent<GameSweet>();
        sweets[x, y].Init(x,y,this,sweetsType);

        return sweets[x, y];
    }
    /// <summary>
    /// 全部填充的方法
    /// </summary>
    public IEnumerator AllFill()
    {
        bool needRefill = true;

        while(needRefill)
        {
            //等待填充
            yield return new WaitForSeconds(fillTime);

            while (Fill())  //调用一次Fill方法，根据返回值判断是否继续
            {
                yield return new WaitForSeconds(fillTime);
            }

            needRefill = ClearAllMatchedSweets();
        }

    }
    /// <summary>
    /// 分步填充
    /// </summary>
    public bool Fill()
    {
        bool filledNotFinished = false;   //判断本次填充是否完成

        for(int y=yRow-2;y>=0;y--)//遍历顺序从下往上
        {
            for(int x=0;x<xColumn;x++)
            {
                GameSweet gameSweet = sweets[x, y];   //得到当前位置的甜品对象

                if(gameSweet.CanMove()) //判断是否可以移动
                {
                    GameSweet gameSweetBelow = sweets[x, y+1];

                    if(gameSweetBelow.Type==SweetsType.EMPTY)//垂直填充
                    {
                        Destroy(gameSweetBelow.gameObject);
                        gameSweet.MoveComponent.Move(x, y + 1,fillTime);
                        sweets[x, y + 1] = gameSweet;
                        CreateNewSweet(x, y, SweetsType.EMPTY);
                        filledNotFinished = true;
                    }
                    else                //斜向填充
                    {
                        for (int down = -1; down <= 1; down++)  //-1代表左下，+1代表右下
                        {
                            if (down != 0)
                            {
                                int downX = x + down;

                                if (downX >= 0 && downX < xColumn)  //排除边界情况
                                {
                                    GameSweet downSweet = sweets[downX, y + 1];
                                    if (downSweet.Type == SweetsType.EMPTY)
                                    {
                                        bool canfill = true;  //检测两侧垂直填充是否能满足条件
                                        for (int aboveY = y; aboveY >= 0; aboveY--)
                                        {
                                            GameSweet sweeetAbove = sweets[downX, aboveY];
                                            if (sweeetAbove.CanMove())
                                            {
                                                break;
                                            }
                                            else if (!sweeetAbove.CanMove() && sweeetAbove.Type != SweetsType.EMPTY)
                                            {
                                                canfill = false;
                                            }
                                        }
                                        if (!canfill)
                                        {
                                            Destroy(downSweet.gameObject);
                                            gameSweet.MoveComponent.Move(downX, y + 1, fillTime);  //移动甜品
                                            sweets[downX, y + 1] = gameSweet;       //更新脚本信息
                                            CreateNewSweet(x, y, SweetsType.EMPTY);  //将之前的位置置空
                                            filledNotFinished = true;
                                            break;
                                        }

                                    }
                                }
                            }
                        }
                    }
                }


            }
        }
        //最上排特殊情况
        for(int i=0;i<xColumn;i++)
        {
            GameSweet sweet = sweets[i, 0];
            
            if(sweet.Type==SweetsType.EMPTY)
            {
                GameObject newSweet = Instantiate(sweetPrefabDir[SweetsType.NORMAL], CorrectPos(i,-1), Quaternion.identity);
                newSweet.transform.SetParent(transform);

                sweets[i, 0] = newSweet.GetComponent<GameSweet>();
                sweets[i, 0].Init(i,-1,this,SweetsType.NORMAL);
                sweets[i, 0].MoveComponent.Move(i, 0,fillTime);
                sweets[i, 0].ColorComponent.SetColor((ColorSweet.ColorType)Random.Range(0, sweets[i, 0].ColorComponent.NumColors));
                filledNotFinished = true;
            }
        }

        return filledNotFinished;
    }
    /// <summary>
    /// 判断是否相邻
    /// </summary>
    /// <returns></returns>
    private bool IsAdjacent(GameSweet sweet1,GameSweet sweet2)
    {
        if(sweet1.X==sweet2.X&&Mathf.Abs(sweet1.Y-sweet2.Y)==1||sweet1.Y==sweet2.Y&&Mathf.Abs(sweet1.X-sweet2.X)==1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    /// <summary>
    /// 交换两个甜品的方法
    /// </summary>
    /// <param name="sweet1"></param>
    /// <param name="sweet2"></param>
    private void ExchangeSweets(GameSweet sweet1,GameSweet sweet2)
    {
        if(sweet1.CanMove()&&sweet2.CanMove())
        {
            //更新脚本内的信息
            sweets[sweet1.X, sweet1.Y] = sweet2;
            sweets[sweet2.X, sweet2.Y] = sweet1;
            //判断匹配是否成功
            if (MatchSweets(sweet1, sweet2.X, sweet2.Y)!=null||MatchSweets(sweet2,sweet1.X,sweet1.Y)!=null)
            {
                //交换位置
                int tempX = sweet1.X;
                int tempY = sweet1.Y;
                sweet1.MoveComponent.Move(sweet2.X, sweet2.Y, fillTime);
                sweet2.MoveComponent.Move(tempX, tempY, fillTime);
                //清除
                ClearAllMatchedSweets();
                //再次填充
                StartCoroutine(AllFill());
            }
            else
            {
                sweets[sweet1.X, sweet1.Y] = sweet1;
                sweets[sweet2.X, sweet2.Y] = sweet2;
            }
        }
    }
    /// <summary>
    /// 玩家对甜品的操作，点击拖拽松开
    /// </summary>
    /// <param name="gameSweet"></param>
    #region
    public void PressSweet(GameSweet gameSweet)
    {
        if(gameOver)
        {
            return;
        }
        pressedSweet = gameSweet;
    }

    public void EnterSweet(GameSweet gameSweet)
    {
        if (gameOver)
        {
            return;
        }
        enteredSweet = gameSweet;
    }

    public void ReleaseSweet()
    {
        if (gameOver)
        {
            return;
        }
        if (IsAdjacent(pressedSweet,enteredSweet))
        {
            ExchangeSweets(pressedSweet, enteredSweet);
        }  
    }
    #endregion
    /// <summary>
    /// 匹配方法
    /// </summary>
    /// <param name="gamesweet"></param>
    /// <param name="newX"></param>
    /// <param name="newY"></param>
    /// <returns></returns>
    public List<GameSweet> MatchSweets(GameSweet gamesweet,int newX,int newY)
    {
        if(gamesweet.CanColored())
        {
            ColorSweet.ColorType colorType = gamesweet.ColorComponent.Color;
            List<GameSweet> matchRowSweets = new List<GameSweet>();   //行匹配列表
            List<GameSweet> matchLineSweets = new List<GameSweet>(); //列匹配列表
            List<GameSweet> finishedMatchingSweets = new List<GameSweet>();  //完成匹配列表

            //行匹配
            matchRowSweets.Add(gamesweet);

            for (int i=0;i<=1;i++) //i=0向左，i=1向右
            {
                for(int xDistance=1;xDistance < xColumn;xDistance++)
                {
                    int x;
                    if(i==0)
                    {
                        x = newX - xDistance;  //向左遍历
                    }
                    else
                    {
                        x = newX + xDistance;
                    }
                    if(x<0||x>=xColumn)  //边界情况
                    {
                        break;
                    }

                    if(sweets[x,newY].CanColored()&&sweets[x,newY].ColorComponent.Color==colorType)//当可着色且颜色相等时
                    {
                        matchRowSweets.Add(sweets[x, newY]);
                    }
                    else
                    {
                        break;  //匹配失败
                    }
                }
            }

            if(matchRowSweets.Count>=3)
            {
                for(int i=0;i<matchRowSweets.Count;i++)
                {
                    finishedMatchingSweets.Add(matchRowSweets[i]);
                }
            }
            //检查行匹配的元素是否大于3
            if(matchRowSweets.Count>=3)
            {
                for(int i=0;i<matchRowSweets.Count;i++)
                {
                    //行匹配列表中满足匹配条件的元素，依次进行列遍历(T形遍历)
                    //0代表上方，1代表下方
                    for(int j=0;j<=1;j++)
                    {
                        for(int yDistance=1;yDistance<yRow;yDistance++)
                        {
                            int y;
                            if(j==0)
                            {
                                y = newY - yDistance;
                            }
                            else
                            {
                                y = newY + yDistance;
                            }
                            if(y<0||y>=yRow)
                            {
                                break;
                            }

                            if(sweets[matchRowSweets[i].X,y].CanColored()&&sweets[matchRowSweets[i].X,y].ColorComponent.Color==colorType)
                            {
                                matchLineSweets.Add(sweets[matchRowSweets[i].X, y]);
                            }
                            else
                            {
                                break;
                            }

                        }
                    }

                    if(matchLineSweets.Count<2)//数量小于2清空列表
                    {
                        matchLineSweets.Clear();
                    }
                    else
                    {
                        for(int j=0;j<matchLineSweets.Count;j++)
                        {
                            finishedMatchingSweets.Add(matchLineSweets[j]);
                        }

                        break;
                    }
                }
            }
            //返回完成匹配列表
            if(finishedMatchingSweets.Count>=3)
            {
                return finishedMatchingSweets;
            }
            matchRowSweets.Clear();//没有匹配成功则清空列表
            matchLineSweets.Clear();

            //列匹配
            matchLineSweets.Add(gamesweet);

            for (int i = 0; i <= 1; i++) //i=0向左，i=1向右
            {
                for (int yDistance = 1; yDistance < yRow; yDistance++)
                {
                    int y;
                    if (i == 0)
                    {
                        y = newY - yDistance;  //向左遍历,每次一个
                    }
                    else
                    {
                        y = newY + yDistance;
                    }
                    if (y < 0 || y >=yRow)  //边界情况,此处存疑xColumn
                    {
                        break;
                    }

                    if (sweets[newX,y].CanColored() && sweets[newX, y].ColorComponent.Color == colorType)//当可着色且颜色相等时
                    {
                        matchLineSweets.Add(sweets[newX, y]);
                    }
                    else
                    {
                        break;  //匹配失败
                    }
                }
            }

            if (matchLineSweets.Count >= 3)
            {
                for (int i = 0; i < matchLineSweets.Count; i++)
                {
                    finishedMatchingSweets.Add(matchLineSweets[i]);
                }
            }
            //检查行匹配的元素是否大于3
            if (matchLineSweets.Count >= 3)
            {
                for (int i = 0; i < matchLineSweets.Count; i++)
                {
                    //行匹配列表中满足匹配条件的元素，依次进行列遍历(T形遍历)
                    //0代表上方，1代表下方
                    for (int j = 0; j <= 1; j++)
                    {
                        for (int xDistance = 1; xDistance < xColumn; xDistance++)
                        {
                            int x;
                            if (j == 0)
                            {
                                x = newX - xDistance;  
                            }
                            else
                            {
                                x = newX + xDistance;  
                            }
                            if (x < 0 || x >=xColumn )
                            {
                                break;
                            }

                            if (sweets[x,matchLineSweets[i].Y].CanColored() && sweets[x, matchLineSweets[i].Y].ColorComponent.Color == colorType)
                            {
                                matchRowSweets.Add(sweets[x, matchLineSweets[i].Y]);
                            }
                            else
                            {
                                break;
                            }

                        }
                    }

                    if (matchRowSweets.Count < 2)//数量小于2清空列表
                    {
                        matchRowSweets.Clear();
                    }
                    else
                    {
                        for (int j = 0; j < matchRowSweets.Count; j++)
                        {
                            finishedMatchingSweets.Add(matchLineSweets[j]);
                        }

                        break;
                    }
                }
            }


            //返回完成匹配列表
            if (finishedMatchingSweets.Count >= 3)
            {
                return finishedMatchingSweets;
            }

        }
        return null;
    }
    /// <summary>
    /// 清除方法
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool ClearSweet(int x,int y)
    {
        if(sweets[x,y].CanClear()&&!sweets[x,y].ClearComponent.IsClearing)
        {
            sweets[x, y].ClearComponent.Clear();
            CreateNewSweet(x, y, SweetsType.EMPTY);

            return true;
        }

        return false;
    }
    /// <summary>
    /// 清除所有匹配好的甜品
    /// </summary>
    /// <returns></returns>
    private bool ClearAllMatchedSweets()
    {

        bool needRefill = false;  //是否需要重新填充

        for(int y=0;y<yRow;y++)
        {
            for(int x=0;x<xColumn;x++)
            {
                if(sweets[x,y].CanClear())
                {
                    List<GameSweet> matchList= MatchSweets(sweets[x, y], x, y);

                    if(matchList!=null)
                    {
                        for(int i=0;i<matchList.Count;i++)
                        {
                            if(ClearSweet(matchList[i].X,matchList[i].Y))
                            {
                                needRefill = true;
                            }
                        }
                    }
                }
            }
        }

        return needRefill;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
