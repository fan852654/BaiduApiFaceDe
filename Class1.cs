using Baidu.Aip.Face;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;

namespace BaiduPro
{
    public class BaiduPro
    {
        protected string Api_key = string.Empty;
        protected string AppID = string.Empty;
        protected string Secret_Key = string.Empty;
        protected Face client = null;
        /// <summary>
        /// 从appsettings.json文件中获取BAIDU的对应属性
        /// </summary>
        public BaiduPro()
        {
            FileInfo fi = new FileInfo(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json")));
            if (!fi.Exists)
            {
                throw new Exception("请在根目录下存放appsettings.json");
            }
            JObject jobj = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json")));
            if(jobj == null || jobj.Count < 2)
            {
                throw new Exception("请在appsettings.json中放入Api_Key,Secret_Key这两个内容");
            }
            if(!jobj.ContainsKey("Api_Key") || !jobj.ContainsKey("Secret_Key"))
            {
                throw new Exception("请在appsettings.json中正确存放Api_Key,Secret_Key这两个内容");
            }
            Api_key = jobj["Api_key"].ToString();
            AppID = jobj.ContainsKey("AppID") ? jobj["AppID"].ToString() : "";
            Secret_Key = jobj["Secret_Key"].ToString();
            client = new Face(Api_key, Secret_Key);
        }
        /// <summary>
        /// 主动传入两个属性，申请自百度
        /// </summary>
        /// <param name="Api_key"></param>
        /// <param name="Secret_Key"></param>
        public BaiduPro(string Api_key, string Secret_Key)
        {
            this.Api_key = Api_key;
            this.Secret_Key = Secret_Key;
            client = new Face(Api_key, Secret_Key);
        }
        /// <summary>
        /// 主动传入对应属性，申请自百度
        /// </summary>
        /// <param name="Api_key"></param>
        /// <param name="AppID"></param>
        /// <param name="Secret_Key"></param>
        public BaiduPro(string Api_key,string AppID,string Secret_Key)
        {
            this.Api_key = Api_key;
            this.AppID = AppID;
            this.Secret_Key = Secret_Key;
            client = new Face(Api_key, Secret_Key);
        }
        public JObject DetectFace(string FilePath,Dictionary<string,object> Options = null)
        {
            if(client == null)
            {
                throw new Exception("未初始化！");
            }
            FileInfo fi = new FileInfo(FilePath);
            if (!fi.Exists)
            {
                throw new Exception("文件不存在！");
            }
            var image = File.ReadAllBytes(FilePath);
            var result = client.Detect(image, Options);
            return result;
        }
        /// <summary>
        /// 把结果解析只剩下方位数据
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public Dictionary<string,float> GetLJustLocation(JObject result)
        {
            if(result == null)
            {
                return null;
            }else if(result.Count != 4)
            {
                return null;
            }
            string[] location =
                    {
                    result["result"][0]["location"]["left"].ToString(),
                    result["result"][0]["location"]["top"].ToString(),
                    result["result"][0]["location"]["width"].ToString(),
                    result["result"][0]["location"]["height"].ToString(),
                };
            Dictionary<string, float> dic = new Dictionary<string, float>();
            dic.Add("left", float.Parse(location[0]));
            dic.Add("top", float.Parse(location[1]));
            dic.Add("width", float.Parse(location[2]));
            dic.Add("height", float.Parse(location[3]));
            return dic;
        }
        public Bitmap DrawPicWithResult(JObject result,string ImagePath)
        {
            FileInfo fi = new FileInfo(ImagePath);
            if (!fi.Exists)
            {
                throw new Exception("未找到文件！");
            }
            Bitmap bitmap = new Bitmap(ImagePath);
            Dictionary<string, float> dic = GetLJustLocation(result);
            try
            {
                float[] locationk = { Convert.ToSingle(dic["left"]), Convert.ToSingle(dic["top"]), Convert.ToSingle(dic["width"]), Convert.ToSingle(dic["height"]) };
                //绘制人脸位置
                var imageinmen = Image.FromFile(ImagePath);
                //未装饰样式
                /*
                using (var g = Graphics.FromImage(bitmap))
                {
                    Pen forlocation = new Pen(Color.YellowGreen, (float)8);
                    g.DrawRectangle(forlocation, locationk[0], locationk[1], locationk[2], locationk[3]);
                }*/
                using (var g = Graphics.FromImage(bitmap))
                {
                    Pen kuangxian = new Pen(Color.YellowGreen, (float)8);
                    g.DrawLine(kuangxian, locationk[0] - 40, locationk[1], locationk[0] + locationk[2] + 40, locationk[1]);//上方
                    g.DrawLine(kuangxian, locationk[0] - 40, locationk[1] + locationk[3], locationk[0] + locationk[2] + 40, locationk[1] + locationk[3]);//下方
                    g.DrawLine(kuangxian, locationk[0], locationk[1] - 40, locationk[0], locationk[1] + locationk[3] + 40);//左方
                    g.DrawLine(kuangxian, locationk[0] + locationk[2], locationk[1] - 40, locationk[0] + locationk[2], locationk[1] + locationk[3] + 40);//右方
                }
            }catch(Exception e)
            {
                throw new Exception(e.Message);
            }
            return bitmap;
        }
    }
}
