using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(CharacterCustomization))]
public class CharacterCustomizationEditor : Editor
{
    int selectIndex;
    Canvas canvas;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();

        var characterCustomization = (CharacterCustomization)target;

        if(characterCustomization.target == null)
        {
            EditorGUILayout.LabelField("请给target赋值！");
            return;
        }

        //是否换了新的skm
        if (!characterCustomization.DoesTargetMatchSkm())   
        {
            characterCustomization.ClearDatabase();
        }

        if(characterCustomization.GetNumber() <= 0)
        {
            characterCustomization.Initialize();
        }
        
        string[] blendshapeNames = characterCustomization.GetBlendshapeNames();

        if(blendshapeNames.Length <= 0)
        {
            EditorGUILayout.LabelField("taget没有blendshape");
            characterCustomization.ClearDatabase(); 
            return;
        }

        EditorGUILayout.LabelField("请创建一个滑动条~", EditorStyles.boldLabel);

        selectIndex = EditorGUILayout.Popup("blendShapeName", selectIndex, blendshapeNames);

        if (GUILayout.Button("创建滑动条"))
        {
            if(canvas == null)
            {
                canvas = GameObject.FindObjectOfType<Canvas>();
            }

            if(canvas == null)
            {
                throw new System.Exception("场景中没有canvas ，请创建！");
                
            }

            GameObject sliderGo = Instantiate(Resources.Load("slider", typeof(GameObject))) as GameObject;

            var BShapeSlider = sliderGo.GetComponent<BlendShapeSlider>();
            //改名字
            //改父物体
            //大小
            BShapeSlider.BlendShapeName = blendshapeNames[selectIndex];
            BShapeSlider.name = blendshapeNames[selectIndex];
            BShapeSlider.transform.parent = canvas.transform;
            BShapeSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(140f, 25f);
            BShapeSlider.GetComponentInChildren<Text>().text = blendshapeNames[selectIndex];

            //获取BlendShape
            BlendShape blendShape = characterCustomization.GetBlendShape(blendshapeNames[selectIndex]);

            //获取slider
            Slider slider = sliderGo.GetComponent<Slider>();

            if (blendShape.negativeIndex == -1)
                slider.minValue = 0;

            if (blendShape.postiveIndex == -1)
                slider.maxValue = 0;

            slider.value = 0;

            Debug.Log(blendshapeNames[selectIndex] + "slider 创建完成！");
        }

    }

}
