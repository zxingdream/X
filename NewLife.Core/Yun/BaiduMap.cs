﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NewLife.Data;
using NewLife.Serialization;

namespace NewLife.Yun
{
    /// <summary>百度地图</summary>
    /// <remarks>
    /// 参考手册 http://lbsyun.baidu.com/index.php?title=webapi/guide/webservice-geocoding
    /// </remarks>
    public class BaiduMap : Map, IMap
    {
        #region 构造
        /// <summary>高德地图</summary>
        public BaiduMap()
        {
            AppKey = "C73357a276668f8b0563d3f936475007";
            KeyName = "ak";
        }
        #endregion

        #region 地址转坐标
        //class Geocoder
        //{
        //    /// <summary>返回结果状态值， 成功返回0，其他值请查看下方返回码状态表。</summary>
        //    public Int32 Status { get; set; }

        //    public result Result { get; set; } = new result();

        //    public class result
        //    {
        //        /// <summary>经纬度坐标</summary>
        //        public location Location { get; set; } = new location();

        //        /// <summary>位置的附加信息，是否精确查找。1为精确查找，即准确打点；0为不精确，即模糊打点（模糊打点无法保证准确度，不建议使用）。</summary>
        //        public Int32 precise { get; set; }

        //        /// <summary>可信度，描述打点准确度，大于80表示误差小于100m。该字段仅作参考，返回结果准确度主要参考precise参数。</summary>
        //        public Int32 confidence { get; set; }

        //        /// <summary>地址类型</summary>
        //        public String level { get; set; }
        //    }

        //    /// <summary>经纬度坐标</summary>
        //    public class location
        //    {
        //        /// <summary>纬度值</summary>
        //        public Double lng { get; set; }

        //        /// <summary>经度值</summary>
        //        public Double lat { get; set; }
        //    }
        //}

        //private String GeoCoderUrl = "http://api.map.baidu.com/geocoder/v2/?address={0}&city={1}&output=json&ak={appkey}";
        private String GeoCoderUrl = "http://api.map.baidu.com/geocoder/v2/?address={0}&city={1}&output=json";
        /// <summary>查询地址的经纬度坐标</summary>
        /// <param name="address"></param>
        /// <param name="city"></param>
        /// <returns></returns>
        public async Task<IDictionary<String, Object>> GetGeocoderAsync(String address, String city = null)
        {
            if (address.IsNullOrEmpty()) throw new ArgumentNullException(nameof(address));

            var url = GeoCoderUrl.F(address, city);

            var html = await GetStringAsync(url);
            if (html.IsNullOrEmpty()) return null;

            var dic = new JsonParser(html).Decode() as IDictionary<String, Object>;
            if (dic == null || dic.Count == 0) return null;

            var status = dic["status"].ToInt();
            if (status != 0) throw new Exception(dic["msg"] + "");

            return dic["result"] as IDictionary<String, Object>;
        }

        /// <summary>查询地址获取坐标</summary>
        /// <param name="address"></param>
        /// <param name="city"></param>
        /// <returns></returns>
        public async Task<GeoPoint> GetGeoAsync(String address, String city = null)
        {
            var rs = await GetGeocoderAsync(address, city);
            if (rs == null || rs.Count == 0) return null;

            var ds = rs["location"] as IDictionary<String, Object>;
            if (ds == null || ds.Count < 2) return null;

            var gp = new GeoPoint
            {
                Longitude = ds["lng"].ToDouble(),
                Latitude = ds["lat"].ToDouble()
            };

            return gp;
        }
        #endregion

        #region 逆地址编码
        private String url2 = "http://api.map.baidu.com/geocoder/v2/?location={0},{1}&output=json";
        /// <summary>根据坐标获取地址</summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public async Task<IDictionary<String, Object>> GetGeocoderAsync(GeoPoint point)
        {
            if (point.Longitude < 0.1 || point.Latitude < 0.1) throw new ArgumentNullException(nameof(point));

            var url = url2.F(point.Latitude, point.Longitude);

            var html = await GetStringAsync(url);
            if (html.IsNullOrEmpty()) return null;

            var dic = new JsonParser(html).Decode() as IDictionary<String, Object>;
            if (dic == null || dic.Count == 0) return null;

            var status = dic["status"].ToInt();
            if (status != 0) throw new Exception(dic["msg"] + "");

            return dic["result"] as IDictionary<String, Object>;
        }
        #endregion
    }
}