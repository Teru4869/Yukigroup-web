using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yukigroup_WEB.Pages.Services;
namespace Yukigroup_WEB.Pages.Services
{
    public class Participant
    {
        public int ID { get; set; }

        [Display(Name = "名前")]
        public string Name { get; set; }

        [Display(Name = "国")]
        public string Country { get; set; }

        [Display(Name = "参加回数")]
        public string ParticipationCount { get; set; }

        [Display(Name = "ステータス")]
        public string Status { get; set; }

        [Display(Name = "場所")]
        public string Place { get; set; }

        [Display(Name = "語学スキル")]
        public string LanguageSkill { get; set; }

        [Display(Name = "備考")]
        public string Note { get; set; }
    }

}
