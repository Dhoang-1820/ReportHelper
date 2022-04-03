using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReportHelper.Models.TreeView
{
    public class TreeNode
    {
        public string id { get; set; }
        public string parent { get; set; }
        public string text { get; set; }
    }
}