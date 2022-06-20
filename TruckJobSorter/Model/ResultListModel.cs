using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TruckJobSorter.Model
{
    class ResultListModel
    {
        public int truckId { get; set; }
        public string job { get; set; }

        public ResultListModel(int _truckId, string _job)
        {
            truckId = _truckId;
            job = _job;
        }
    }
}
