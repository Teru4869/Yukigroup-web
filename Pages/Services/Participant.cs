using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yukigroup_WEB.Pages.Services;
namespace Yukigroup_WEB.Pages.Services
{
    public class Participant
    {
            public string ID { get; set; }
            public string Name { get; set; }
            public string Country { get; set; }
            public string ParticipationCount { get; set; }
            public string StudentOrWorker { get; set; }
            public string BaseOrComoes { get; set; }
        public string Status { get; internal set; }
        public string Place { get; internal set; }
        public string JapaneseSkill { get; set; }
        public string Note { get; set; }
    }
}
