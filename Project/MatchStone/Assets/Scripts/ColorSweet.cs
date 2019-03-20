using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSweet : MonoBehaviour
{
    public enum ColorType
    {
        YELLOW,
        PURPLE,
        RED,
        BLUE,
        GREEN,
        PINK,
        ANY,
        COUNT
    }
    [System.Serializable]//用结构体实现可视化
    public struct ColorSprite
    {
        public ColorType colorType;
        public Sprite sprite;
    }

    public ColorSprite[] colorSprites;

    private Dictionary<ColorType, Sprite> colorSpriteDir;

    SpriteRenderer spriteRenderer;
    //获取颜色的长度
    public int NumColors
    {
        get
        {
            return colorSprites.Length;
        }
    }
    //获取或设置颜色
    public ColorType Color { get => color; set => SetColor(value); }
    private ColorType color;

    private void Awake()
    {
        spriteRenderer = transform.Find("Sweet").GetComponent<SpriteRenderer>();

        colorSpriteDir = new Dictionary<ColorType, Sprite>();
        for(int i=0;i<colorSprites.Length;i++)
        {
            if(!colorSpriteDir.ContainsKey(colorSprites[i].colorType))
            {
                colorSpriteDir.Add(colorSprites[i].colorType, colorSprites[i].sprite);
            }
        }
    }
    //设置颜色的方法
    public void SetColor(ColorType newColor)
    {
        color = newColor;
        if(colorSpriteDir.ContainsKey(newColor))
        {
            spriteRenderer.sprite = colorSpriteDir[newColor];
        }
    }
}
