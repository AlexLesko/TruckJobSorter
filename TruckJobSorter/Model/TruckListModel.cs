using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TruckJobSorter.Model
{
    class TruckListModel
    {
        public int ID { get; set; }
        public List<string> jobKnowledge { get; set; }

        public TruckListModel(int _ID, List<string> _jobKnowledge)
        {
            ID = _ID;
            jobKnowledge = _jobKnowledge;
        }
    }
}
