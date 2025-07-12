using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yukigroup_WEB.Pages.Services;
namespace Yukigroup_WEB.Pages.Services
{
    public class GoogleSheetResponse
    {
        public string range { get; set; }
        public string majorDimension { get; set; }
        public List<List<string>> values { get; set; }
    }
}
