using System;
using System.Collections.Generic;
using System.Text;
using TianCheng.DAL.MongoDB;
using TianCheng.Model;

namespace SamplesMongoDB.Model
{
    /// <summary>
    /// 话务员联系客户的结果
    /// </summary>
    [CollectionMapping("RT_ContactClientInfo")]
    public class ContactClientInfo : BusinessMongoModel
    {

        /// <summary>
        /// 话务员Id
        /// </summary>
        public string SalerId { get; set; }
        /// <summary>
        /// 话务员名称
        /// </summary>
        public string SalerName { get; set; }
        /// <summary>
        /// 业务员所在部门ID
        /// </summary>
        public string DepartmentId { get; set; }

        /// <summary>
        /// 再联系时间
        /// </summary>
        public DateTime? AgainDate { get; set; }
        /// <summary>
        /// 拒绝原因
        /// </summary>
        public string DisagreedText { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Intro { get; set; }
        /// <summary>
        /// 广告词
        /// </summary>
        public string OrderMessage { get; set; }
        /// <summary>
        /// 保留给话务员的最后时间，过期会清空
        /// </summary>
        public DateTime? EndData { get; set; }
        /// <summary>
        /// 剩余天数        每日凌晨自动更新剩余天数
        /// </summary>
        public int Remaining { get; set; }
        /// <summary>
        /// 是否完成，新的联系记录会将上一个设置成完成状态
        /// </summary>
        public bool IsClose { get; set; }

        /// <summary>
        /// 是否仅为内部查看
        /// </summary>
        public bool IsInside { get; set; }
    }
}
