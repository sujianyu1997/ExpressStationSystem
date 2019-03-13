﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ExpressStationSystem.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ExpressStationSystem.Controllers.ViewController
{
    public class PackageController : Controller
    {
        public ActionResult Package(string status, string searchWith, string searchWithContent, string date1, string date2, string page)
        {
            int pageNum = 7;
            string defaultSearchWith = "按姓名";
            string defaultSearchWithContent = "";
            string defaultStatus = "选择分类";

            if (status == "undefined" || status == null || status == "")
            {
                status = defaultStatus;
                ViewBag.status = status;
            }
            else
            {
                ViewBag.status = status;
            }
            if (searchWith == null)
            {
                ViewBag.searchWith = defaultSearchWith;
            }
            else
            {
                ViewBag.searchWith = searchWith;
            }
            if (searchWithContent == null)
            {
                ViewBag.searchWithContent = defaultSearchWithContent;
            }
            else
            {
                ViewBag.searchWithContent = searchWithContent;
            }
            //时间
            if (date1 == "" || date1 == null)
            {
                date1 = DateTime.Now.AddYears(-20).ToString("yyyy-MM-dd");
            }
            if (date2 == "" || date2 == null)
            {
                date2 = DateTime.Now.ToString("yyyy-MM-dd");
            }
            if(date1.CompareTo(date2) > 0)
            {
                string temp = date1;
                date1 = date2;
                date2 = date1;
            }
            //包裹信息
            List<int> PIDList = new ManagerController().GetAllPackage(Convert.ToDateTime(date1), Convert.ToDateTime(date2));
            //中间数组
            List<dynamic> step = new List<dynamic>();
            //也是中间数组
            List<dynamic> showPackage = new List<dynamic>();
            //决定显示的前端数组
            List<dynamic> showPackage2 = new List<dynamic>();
            //第一次筛选，分类
            foreach (var PID in PIDList)
            {
                var PackageInfo = new QueryController().GetAllInfo(PID);
                var josnStr = JsonConvert.SerializeObject(PackageInfo);
                JObject jo = (JObject)JsonConvert.DeserializeObject(josnStr);

                if (status == PackageInfo.package.status)
                {
                    step.Add(jo);
                    continue;
                }
                else if (PackageInfo.error != null && status == PackageInfo.error.status)
                {
                    step.Add(jo);
                    continue;
                }
                else if (status == defaultStatus)
                {
                    step.Add(jo);
                    continue;
                }
            }
            //第二次筛选，按搜索内容分类
            foreach (var PInfo in step)
            {
                if (searchWith != null)
                {
                    if (searchWith == "按单号")
                    {
                        if (PInfo.package.id.ToString().StartsWith(searchWithContent))
                        {
                            showPackage.Add(PInfo);
                        }
                    }
                    else if (searchWith == "按姓名")
                    {
                        if (PInfo.dest.name.ToString().StartsWith(searchWithContent))
                        {
                            showPackage.Add(PInfo);
                        }
                    }
                    else if (searchWith == "按手机")
                    {
                        if (PInfo.dest.phone.ToString().StartsWith(searchWithContent))
                        {
                            showPackage.Add(PInfo);
                        }
                    }
                }
                else
                {
                    showPackage.Add(PInfo);
                }
            }
            //默认在第一页
            if (page == null || page == "")
            {
                page = "1";
            }
            //临时数据
            int pageTemp = Convert.ToInt32(page);
            //第三次筛选，是把该页的信息的插进临时数组
            for (int i = (pageTemp - 1) * pageNum; i < pageTemp * pageNum; i++)
            {
                if (i >= showPackage.Count)
                {
                    break;
                }
                else
                {
                    showPackage2.Add(showPackage[i]);
                }

            }
            ViewBag.showPackage = showPackage2;
            //前端要用到的数据
            if (showPackage.Count % pageNum == 0)
            {
                ViewBag.PageSum = showPackage.Count / pageNum;
            }
            else
            {
                ViewBag.PageSum = showPackage.Count / pageNum + 1;
            }
            ViewBag.currentPage = pageTemp;
            ViewBag.date1 = date1;
            ViewBag.date2 = date2;
            return View();
        }


        public ActionResult checkPackage(string id)
        {
            if (id == "" || id == null)
            {
                return View();
            }
            var pack = new QueryController().GetAllInfo(Convert.ToInt32(id));

            var josnStr = JsonConvert.SerializeObject(pack);
            JObject jo = (JObject)JsonConvert.DeserializeObject(josnStr);

            ViewBag.package = jo;

            return View();
        }

    }


}