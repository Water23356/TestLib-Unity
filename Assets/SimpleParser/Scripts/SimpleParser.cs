// Ignore Spelling: dstr datas
#define Tests
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace CommandParser
{
    /// <summary>
    /// 数据类型
    /// </summary>
    public enum DataType
    {
        Unknown,
        Integer,
        Double,
        Boolean,
        Text,
        Function,
        Error
    }

    /// <summary>
    /// 解析数据，包含一个数据本体(object)，以及它的真实数据类型
    /// </summary>
    public struct Data
    {
        public object? Value { get; private set; }
        public DataType Type { get; private set; }

        public static event Action<string> Output = message => Console.WriteLine(message);

        public Data(object value, DataType type)
        {
            Value = value;
            Type = type;
        }

        public static Data Empty => new Data(null, DataType.Unknown);
        public static Data Error => new Data(null, DataType.Error);

        public bool isEmpty() { if (Value is null) { return true; }return false; }
        public bool isError() { if (Type == DataType.Error) { return true; } return false; }

        public void Parse(string dataString, DataType type = DataType.Unknown)
        {
            switch (type)
            {
                case DataType.Unknown:
                    if (Regex.IsMatch(dataString, @"^[+-]?\d+$"))
                    {
                        Type = DataType.Integer;
                        Value = Convert.ToInt32(dataString);
                    }
                    else if (Regex.IsMatch(dataString, @"^-?([1-9]\d*\.\d*|0\.\d*[1-9]\d*)$"))
                    {
                        Type = DataType.Double;
                        Value = Convert.ToDouble(dataString);
                    }
                    else if (dataString.ToUpper() == "TRUE")
                    {
                        Type = DataType.Boolean;
                        Value = true;
                    }
                    else if (dataString.ToUpper() == "FALSE")
                    {
                        Type = DataType.Boolean;
                        Value = false;
                    }
                    else
                    {
                        Type = DataType.Text;
                        Value = dataString;
                    }
                    break;

                case DataType.Integer:
                    Type = DataType.Integer;
                    try
                    {
                        Value = Convert.ToInt32(dataString);
                    }
                    catch (FormatException)
                    {
                        Output("源数据格式出错，无法转化至Int类型");
                    }
                    catch (OverflowException)
                    {
                        Output("源数据超出Int类型的数值范围！");
                    }
                    break;

                case DataType.Double:
                    Type = DataType.Double;
                    try
                    {
                        Value = Convert.ToDouble(dataString);
                    }
                    catch (FormatException)
                    {
                        Output("源数据格式出错，无法转化至Double类型");
                    }
                    catch (OverflowException)
                    {
                        Output("源数据超出Double类型的数值范围！");
                    }
                    break;

                case DataType.Boolean:
                    if (dataString.ToUpper() == "TRUE")
                    {
                        Type = DataType.Boolean;
                        Value = true;
                    }
                    else if (dataString.ToUpper() == "FALSE")
                    {
                        Type = DataType.Boolean;
                        Value = false;
                    }
                    else
                    {
                        Output("源数据无法转化为Boolean类型");
                    }
                    break;

                case DataType.Text:
                    Type = DataType.Text;
                    Value = dataString;
                    break;

                case DataType.Function:
                    Type = DataType.Function;
                    Value = dataString;
                    break;

                default:
                    Output("类型枚举出错！转化失败！");
                    break;
            }
        }

        public void Print()
        {
            if (Value != null)
            {
                Output($"[{Type}]: {Value}");
            }
            else
            {
                Output($"[{Type}]:");
            }
        }

        public static Data ParseTo(string dataString, DataType type = DataType.Unknown)
        {
            Data data = new Data();
            data.Parse(dataString, type);
            return data;
        }
    }

    /// <summary>
    /// 简单的文本解析器
    /// </summary>
    public class SimpleParser
    {
        private enum TokenType { None, Key, Value, String }
        public static event Action<string> Output = delegate (string input) { Console.WriteLine(input); };
        public static Dictionary<string, Data> ParseFormFile(string fileName)
        {
            string texts = File.ReadAllText(fileName);
            return Parse(texts);
        }
        public static Dictionary<string, Data> Parse(string text)
        {
            Dictionary<string, Data> dataDictionary = new Dictionary<string, Data>();
            TokenType tokenType = TokenType.None;
            int index = 0;
            string token;
            string key = string.Empty;
            bool isKey = false;

            while (index < text.Length && index > -1)
            {
                token = GetNextToken(ref text, index, out index, out tokenType);
#if Test
                Console.WriteLine($"Parse: token->{token} index->{index} type->{tokenType}");
#endif

                switch (tokenType)
                {
                    case TokenType.Key:
                        dataDictionary.Add(token, Data.Empty);
                        isKey = true;
                        key = token;
                        break;

                    case TokenType.Value:
                        if (isKey)
                        {
                            isKey = false;
                            dataDictionary[key] = Data.ParseTo(token);
                        }
                        break;

                    case TokenType.String:
                        break;

                    default:
                        Output("解析异常，数据类型枚举出错！");
                        break;
                }
            }

            return dataDictionary;
        }
        private static string GetNextToken(ref string text, int startIndex, out int nextIndex, out TokenType tokenType)
        {
            int i = startIndex;
            bool hasNext = true;
            StringBuilder tokenBuilder = new StringBuilder();
            nextIndex = -1;
            tokenType = TokenType.String;

            int quote = 0;//引用模式
            bool trans = false;//转义状态
            bool note = false;//注释模式

            while (i < text.Length && hasNext)
            {
                char character = text[i++];
                if (trans)
                {
                    switch (character)
                    {
                        case '\\':
                            tokenBuilder.Append(character);
                            break;
                        case 'r':
                            tokenBuilder.Append('\r');
                            break;
                        case 'n':
                            tokenBuilder.Append('\n');
                            break;
                        case 't':
                            tokenBuilder.Append('\r');
                            break;
                        case '<':
                            tokenBuilder.Append('<');
                            break;
                        case '>':
                            tokenBuilder.Append('>');
                            break;
                    }
                    trans = false;
                }
                else if (note)
                {
                    if (character == '\r')
                    {
                        hasNext = false;
                        nextIndex = i;
                    }
                }
                else
                {
                    switch (character)
                    {
                        case ' ':
                        case '\n':
                        case '\t':
                        case '\r':
                            if (quote > 0)
                            {
                                tokenBuilder.Append(character);
                            }
                            break;

                        case '\\':
                            trans = true;
                            break;
                        case '=':
                            if (quote > 0)
                            {
                                tokenBuilder.Append(character);
                            }
                            else
                            {
                                hasNext = false;
                                nextIndex = i;
                                tokenType = TokenType.Key;
                            }
                            break;
                        case ';':
                            if (quote > 0)
                            {
                                tokenBuilder.Append(character);
                            }
                            else
                            {
                                note = true;
                                tokenType = TokenType.Value;
                            }
                            break;

                        case '<':
                            if (quote > 0)
                            {
                                tokenBuilder.Append(character);
                            }
                            quote++;
                            break;

                        case '>':
                            quote--;
                            if (quote > 0)
                            {
                                tokenBuilder.Append(character);
                            }
                            break;
                        default:
                            tokenBuilder.Append(character);
                            break;
                    }
                }
            }

            return tokenBuilder.ToString();
        }
    }

    public static class SimpleParserTool
    {
        /// <summary>
        /// 一种安全的获取字典值的方法，若键值对不存在则返回空字符串
        /// </summary>
        /// <param name="illustration"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetIllustrationInfo(this Dictionary<string, Data> illustration, string key)
        {
            if (illustration.ContainsKey(key))
            {
                string? back = illustration[key].Value?.ToString();
                if (back == null) return string.Empty;
                return back;
            }
            return string.Empty;
        }
        /// <summary>
        /// 一种安全的获取字典值的方法，若键值对不存在则返回空字符串
        /// </summary>
        /// <param name="illustration"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetIllustrationInfo(this Dictionary<string, string> illustration, string key)
        {
            if (illustration.ContainsKey(key))
            {
                return illustration[key];
            }
            return string.Empty;
        }

        public static void Print(this Dictionary<string, Data> datas)
        {
            foreach (var key in datas.Keys)
            {
                Console.WriteLine($"[key]:{key}");
                datas[key].Print();
            }
        }
        /// <summary>
        /// 安全修改字典的值，如果键不存在则直接创建新的键值对，如果输入的键为空则不会创建新的键值对
        /// </summary>
        /// <param name="illustration">字典</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns>是否修改成功</returns>
        public static bool Change(this Dictionary<string, string> illustration, string key, string value)
        {
            if (key is null)
            {
                return false;
            }
            illustration[key] = value;
            return true;
        }
        /// <summary>
        /// 安全的获取数组的值
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Data Get(this Data[] datas,int index)
        {
            if(index<0||index>=datas.Length)
            {
                return Data.Error;
            }
            return datas[index];
        }
    }

}
