// Ignore Spelling: Creat

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ER.Items
{
    public class Item
    {
        #region 物品属性
        /// <summary>
        /// 物品名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 物品ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 文本属性（键名：文本所在的标识头）
        /// </summary>
        protected Dictionary<string,string> attributeText = new Dictionary<string,string>();
        /// <summary>
        /// 整形属性
        /// </summary>
        protected Dictionary<string,int> attributeInt = new Dictionary<string,int>();
        /// <summary>
        /// 浮点属性
        /// </summary>
        protected Dictionary<string, float> attributeFloat = new Dictionary<string, float>();
        /// <summary>
        /// 布尔属性
        /// </summary>
        protected Dictionary<string,bool> attributeBool = new Dictionary<string,bool>();
        #endregion

        #region 尝试获取属性
        public bool TryGetInt(string key,out int value)
        {
            return attributeInt.TryGetValue(key,out value);
        }
        public bool TryGetText(string key, out string value)
        {
            return attributeText.TryGetValue(key, out value);
        }
        public bool TryGetFloat(string key, out float value)
        {
            return attributeFloat.TryGetValue(key, out value);
        }
        public bool TryGetBool(string key, out bool value)
        {
            return attributeBool.TryGetValue(key, out value);
        }
        #endregion

        #region 创建属性
        public void CreatAttribute(string key,int value)
        {
            attributeInt[key] = value;
        }
        public void CreatAttribute(string key, float value)
        {
            attributeFloat[key] = value;
        }
        public void CreatAttribute(string key, string value)
        {
            attributeText[key] = value;
        }
        public void CreatAttribute(string key, bool value)
        {
            attributeBool[key] = value;
        }
        #endregion
    }
}
