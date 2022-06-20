using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TruckJobSorter.Model
{
    class JobListModel
    {
        public string jobName { get; set; }
        public int jobCount { get; set; }

        public int availableTruckForJob { get; set; }

        public JobListModel(string _jobName, int _jobCount, int _availableTruckForJob)
        {
            jobName = _jobName;
            jobCount = _jobCount;
            availableTruckForJob = _availableTruckForJob;
        }
    }
}
