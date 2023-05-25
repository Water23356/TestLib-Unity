﻿using CommandParser;
using Mod_Console;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using TMPro;
using UnityEngine;

public class ConsolePanel : MonoBehaviour
{
    #region 单例封装
    public static ConsolePanel Instance { get; private set; } = null;

    public void Awake()
    {
        if (Instance is null) { Instance = this; }
        else { Destroy(gameObject); }
        DontDestroyOnLoad(this);
    }
    #endregion

    #region 组件 | 属性
    public TMP_InputField monitor;//显示器
    public TMP_InputField input;//输入框
    public DefaultInterpreter interpreter = new DefaultInterpreter();//指令器
    public int maxLines = 50;//消息最大数量
    public int maxHistory = 20;//历史输入最大记录数量
    public List<string> history= new List<string>(20);//历史输入

    private bool inputing = false;//是否正处于输入状态
    #endregion

    #region 功能函数
    public void ActiveInput()
    {
        inputing = true;
    }
    public void InactiveInput()
    {
        inputing = false;
    }
    public void SendMessage()
    {
        if(input.text.EndsWith('\n'))//换行提交
        {
            if(!input.text.StartsWith('\n'))
            {
                if (input.text.StartsWith('/'))//输入的是指令
                {
                    string command = input.text.Substring(1,input.text.Length-2);
                    if (!Command(command))//指令错误
                    {
                        PrintError($"This is not a valid instruction statement : {command}");
                    }
                }
                else
                {
                    Print(input.text.Substring(0, input.text.Length - 1));
                }
                RecordInput(input.text);
            }
            input.text = string.Empty;
        }
    }
    /// <summary>
    /// 向控制台打印消息
    /// </summary>
    /// <param name="txt">消息</param>
    public void Print(string txt,bool newLine = true)
    {
        if (newLine) { monitor.text += '\n'; }
        monitor.text += txt;
        LimitLines();
    }
    public void PrintError(string txt, bool newLine = true)
    {
        if (newLine) { monitor.text += '\n'; }
        monitor.text += new StringBuilder("<color=red>").Append(txt).Append("</color>");
        LimitLines();
    }
    public void Clear()
    {
        monitor.text = string.Empty;
    }
    /// <summary>
    /// 解释指定指令
    /// </summary>
    /// <param name="commandText">指令内容</param>
    /// <returns>是否是一个有效指令</returns>
    public bool Command(string commandText)
    {
        if(SimpleCommandParser.Parse(commandText, interpreter).isError())
        {
            return false;
        }
        return true;
    }
    #endregion
    /// <summary>
    /// 限制消息行数
    /// </summary>
    private void LimitLines()
    {
        string[] ts = monitor.text.Split('\n');
        if (ts.Length > maxLines)
        {
            StringBuilder newText = new StringBuilder();
            for (int i = ts.Length - maxLines; i < ts.Length; i++)
            {
                newText.Append(ts[i]).Append("\n");
            }
            monitor.text = newText.ToString();
        }
    }
    /// <summary>
    /// 记录输入历史
    /// </summary>
    private void RecordInput(string input)
    {
        history.Add(input);
        if(history.Count > maxHistory)
        {
            history.RemoveAt(0);
        }
    }

    private void Update()
    {
        if(inputing)
        {
            if(Input.anyKeyDown)
            {
                if(Input.GetKeyDown( KeyCode.UpArrow))//上一条历史输入
                {

                }
                if (Input.GetKeyDown(KeyCode.DownArrow))//下一条历史输入
                {

                }
            }
        }
    }
}
