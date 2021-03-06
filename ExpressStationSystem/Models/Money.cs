﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExpressStationSystem.Models
{
    public partial class Money
    {
        private DataClasses1DataContext db = new DataClasses1DataContext(Global.connstr);
        // GET: api/Money/GetSalary?account={account}
        /// <summary>
        /// 获取员工工资详情
        /// </summary>
        /// <param name="account">员工账户</param>
        /// <param name="time">发工资日</param>
        /// <remarks>获取员工工资详情</remarks>
        /// <returns>返回</returns>
        public dynamic GetSalary(string account, DateTime time)
        {
            var member = db.Member.SingleOrDefault(a => a.mId == account);
            if (member is null)
            {
                return null;
            }
            var record = new Member().GetRecordByAccount(account, time.AddMonths(-1), time);
            var value = db.Commission.OrderBy(a => a.time).FirstOrDefault();
            if (value is null)
            {
                return null;
            }
            //提成
            dynamic commission = new { pickUp = new { PickUpCount = record.PickUpCount, value = value.pickUpValue, total = record.PickUpCount * value.pickUpValue }, delivery = new { DeliveryCount = record.DeliveryCount, value = value.deliveryValue, total = record.DeliveryCount * value.deliveryValue }, transfer = new { TransferCount = record.TransferCount, value = value.transferValue, total = record.TransferCount * value.transferValue }, total = record.PickUpCount * value.pickUpValue + record.DeliveryCount * value.deliveryValue + record.TransferCount * value.transferValue };
            //补贴
            decimal subsidy = 0;
            //罚款
            decimal fine = 0;

            List<dynamic> subsidyList = new List<dynamic>();
            List<dynamic> fineList = new List<dynamic>();
            var money = db.Money.Where(a => a.mId == account && DateTime.Compare(a.time, time.AddMonths(-1)) >= 0 && DateTime.Compare(a.time, time) <= 0);
            foreach (var x in money)
            {
                //补贴
                if (x.subsidy != 0)
                {
                    subsidy += x.subsidy;
                    subsidyList.Add(x);
                }
                //罚款
                else
                {
                    fine += x.fine;
                    fineList.Add(x);
                }
            }
            return new { baseSalary = member.baseSalary, commission = commission, subsidy = new { details = subsidyList, total = subsidy }, fine = new { details = fineList, total = fine }, total = member.baseSalary + commission.pickUp.total + commission.delivery.total + commission.transfer.total + subsidy + fine };
        }
        // GET: api/Leave/GetMoneyInfo
        /// <summary>
        /// 获取奖罚信息
        /// </summary>
        /// <param name="id">奖罚信息id</param>
        /// <remarks>获取奖罚信息</remarks>
        /// <returns>返回</returns>
        public dynamic GetMoneyInfo(int id)
        {
            var money = db.Money.Join(db.Member, a => a.mId, b => b.mId, (a, b) => new { money = a, member = b }).SingleOrDefault(a => a.money.sId == id);
            if (money is null)
            {
                return null;
            }
            else
            {

                return money;
            }
        }
        // PUT: api/Money/GetMoneyHistory
        /// <summary>
        /// 获取金钱条目历史记录
        /// </summary>
        /// <param name="start">起始时间</param>
        /// <param name="end">终止时间</param>
        /// <remarks>获取金钱条目历史记录</remarks>
        /// <returns>返回</returns>
        public List<int> GetMoneyHistory(DateTime start, DateTime end)
        {
            List<int> list = new List<int>();
            var money = db.Money.Where(a => DateTime.Compare(a.time, start) >= 0 && DateTime.Compare(a.time, end) <= 0);
            if (money is null)
            {
                return list;
            }
            else
            {
                foreach (var x in money)
                {
                    list.Add(x.sId);
                }
            }
            return list;
        }
        // POST: api/Money/FineorPrize
        /// <summary>
        /// 罚款
        /// </summary>
        /// <param name="x">罚款实体信息</param>
        /// <remarks>罚款</remarks>
        /// <returns>返回</returns>
        public bool FineorPrize(MoneyClass x)
        {
            Money money = new Money();
            money.mId = x.mId;
            money.subsidy = x.subsidy;
            money.fine = x.fine;
            money.time = DateTime.Now;
            money.reason = x.reason;
            money.person = x.person;
            try
            {
                db.Money.InsertOnSubmit(money);
                db.SubmitChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        // PUT: api/Money/UpdateMoneyItem
        /// <summary>
        /// 更新罚款补贴信息
        /// </summary>
        /// <param name="x">罚款补贴实体信息</param>
        /// <remarks>更新罚款补贴信息</remarks>
        /// <returns>返回</returns>
        public bool UpdateMoneyItem(MoneyClassPlus x)
        {;
            var money = db.Money.Where(a => a.sId == x.sId).FirstOrDefault();
            if (money is null)
            {
                return false;
            }
            money.subsidy = x.subsidy;
            money.fine = x.fine;
            money.time = DateTime.Now;
            money.reason = x.reason;
            money.person = x.person;
            try
            {
                db.SubmitChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        // DELETE: api/Money/Delete
        /// <summary>
        /// 删除罚款
        /// </summary>
        /// <param name="id">金额项目id</param>
        /// <remarks>删除罚款</remarks>
        /// <returns>返回</returns>
        public bool Delete(int id)
        {
            var money = db.Money.SingleOrDefault(a => a.sId == id);
            if (money is null)
            {
                return false;
            }
            else
            {
                db.Money.DeleteOnSubmit(money);
                db.SubmitChanges();
                return true;
            }
        }
    }
}