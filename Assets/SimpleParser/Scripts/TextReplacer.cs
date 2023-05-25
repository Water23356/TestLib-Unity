#define Test
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;

namespace CommandParser
{
    /// <summary>
    /// 语言包加载错误
    /// </summary>
    public class LanguagePackLoadException : Exception
    {
        public LanguagePackLoadException() : base("语言包加载错误")
        {
            Console.WriteLine("语言包加载错误");
        }

        public LanguagePackLoadException(string message) : base(message)
        {
            Console.WriteLine("语言包加载错误");
        }
    }

    /// <summary>
    /// 语言包对象
    /// </summary>
    public class LanguagePack
    {

        /// <summary>
        /// 消息输出接口
        /// </summary>
        public static event Action<string> Output = message => Console.WriteLine(message);

        /// <summary>
        /// 语言包说明文件名称
        /// </summary>
        public const string IllustrationFileName = "illustration.txt";
        /// <summary>
        /// 路径转接文件名称
        /// </summary>
        public const string AdapterFileName = "adapter.txt";

        /// <summary>
        /// 文件包所在的路径
        /// </summary>
        public string LanguagePackPath { get; private set; }
        /// <summary>
        /// 语言包名称
        /// </summary>
        public string LanguagePackName = string.Empty;
        /// <summary>
        /// 语言包版本
        /// </summary>
        public string LanguagePackVersion = string.Empty;
        /// <summary>
        /// 语言包作者
        /// </summary>
        public string LanguagePackAuthor = string.Empty;
        /// <summary>
        /// 语言包图片路径
        /// </summary>
        public string ImagePath = string.Empty;
        /// <summary>
        /// 语言包描述
        /// </summary>
        public string LanguagePackDescription = string.Empty;
        /// <summary>
        /// 路径转接器，用于快速查找文本文件所在的路径
        /// </summary>
        private Dictionary<string, string> PathAdapter = new Dictionary<string, string>();
        /// <summary>
        /// 当前正在加载的文本片段文件路径（用于避免重复加载）
        /// </summary>
        private string loadedFilePath = string.Empty;
        /// <summary>
        /// 文本缓存器
        /// </summary>
        public Dictionary<string, string> TextCache = new Dictionary<string, string>();
        /// <summary>
        /// 封装一个语言包对象，当语言包存在缺失成分时，将使用默认包中的内容
        /// </summary>
        /// <param name="path">包路径</param>
        /// <param name="defaultPath">默认包路径，默认为空</param>
        /// <exception cref="LanguagePackLoadException">当语言包路径无效，或者包资源结构不完整时，抛出这个异常</exception>
        public LanguagePack(string path,string defaultPath = "")
        {
            LanguagePackPath = path;
            string illustration_path = Path.Combine(path, IllustrationFileName);

            //首先先尝试加载默认语言包
            string tp = Path.Combine(defaultPath, AdapterFileName);
            if (File.Exists(tp))
            {
                Dictionary<string, Data>  text = SimpleParser.Parse(File.ReadAllText(tp));
                foreach (var key in text.Keys)
                {
                    PathAdapter.Add(key, Path.Combine(defaultPath, text.GetIllustrationInfo(key)));
                }
            }

            //尝试加载新的语言包
            if (File.Exists(illustration_path))//检查配置文件是否存在
            {
                Dictionary<string, Data> illustration = SimpleParser.Parse(File.ReadAllText(illustration_path));
                LanguagePackName = illustration.GetIllustrationInfo("Name");
                LanguagePackVersion = illustration.GetIllustrationInfo("Version");
                LanguagePackAuthor = illustration.GetIllustrationInfo("Author");
                ImagePath = illustration.GetIllustrationInfo("Image");
                LanguagePackDescription = illustration.GetIllustrationInfo("Description");
#if Test
                //illustration.Print();
                Output($"Name:{LanguagePackName}");
                Output($"Version:{LanguagePackVersion}");
                Output($"Author:{LanguagePackAuthor}");
                Output($"Image:{ImagePath}");
                Output($"Description:{LanguagePackDescription}");
#endif

                illustration_path = Path.Combine(path, AdapterFileName);
                if (File.Exists(illustration_path))//检查路径转接文件是否存在
                {
                    illustration = SimpleParser.Parse(File.ReadAllText(illustration_path));
                    foreach (var key in illustration.Keys)
                    {
                        PathAdapter.Change(key, illustration.GetIllustrationInfo(key));
                    }
                }
                else
                {
                    throw new LanguagePackLoadException("语言包缺失转接配置文件！");
                }
            }
            else
            {
                throw new LanguagePackLoadException();
            }
        }
        /// <summary>
        /// 缓存指定文本片段
        /// </summary>
        /// <param name="name">文本片段名</param>
        /// <returns>是否加载成功</returns>
        public bool Load(string name)
        {
            string path = PathAdapter.GetIllustrationInfo(name);
            if (path == string.Empty)
            {
                return false;
            }
            else
            {
                path = Path.Combine(LanguagePackPath, path);
                if (File.Exists(path))
                {
                    if (path != loadedFilePath)//避免重复加载
                    {
                        var texts = SimpleParser.ParseFormFile(path);
                        TextCache.Clear();

                        foreach (var key in texts.Keys)
                        {
                            TextCache.Add(key, texts.GetIllustrationInfo(key));
                        }
                        loadedFilePath = path;
                        
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        /// <summary>
        /// 清空文本缓存
        /// </summary>
        public void Clear()
        {
            TextCache.Clear();
            loadedFilePath = string.Empty;
        }
        /// <summary>
        /// 读取指定键的文本内容，如果不存在相应的文本内容则返回null
        /// </summary>
        /// <param name="key">文本键名</param>
        /// <returns></returns>
        public string? this[string key]
        {
            get
            {
                if (TextCache.Count == 0) { return null; }
                if(TextCache.ContainsKey(key))
                {
                    return TextCache[key];
                }
                return null;
            }
        }
    }

    /// <summary>
    /// 文本替换器
    /// </summary>
    public class TextReplacer
    {
        /// <summary>
        /// 语言包路径
        /// </summary>
        public string LanguagePackPath = string.Empty;

        private List<FileInfo> packs = new List<FileInfo>();

        public static bool PathExist(string path)
        {
            if (File.Exists(path) || Directory.Exists(path))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsDirectory(string path)
        {
            FileAttributes attributes = File.GetAttributes(path);
            return (attributes & FileAttributes.Directory) == FileAttributes.Directory;
        }
        /// <summary>
        /// 检查并更新语言包
        /// </summary>
        public void CheckPath()
        {
            if(PathExist(LanguagePackPath))
            {
                if(IsDirectory(LanguagePackPath))//判断语言包路径是否为一个文件夹
                {
                    string[] subdirectories = Directory.GetDirectories(LanguagePackPath);
                }
            }
        }
        

        public static void CompressFolder(string sourceFolderPath, string compressedFilePath)
        {
            ZipFile.CreateFromDirectory(sourceFolderPath, compressedFilePath);
        }

        public static void DecompressFolder(string compressedFilePath, string decompressedFolderPath)
        {
            ZipFile.ExtractToDirectory(compressedFilePath, decompressedFolderPath);
        }
    }
}

