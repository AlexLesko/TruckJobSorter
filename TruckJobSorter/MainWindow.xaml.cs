using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TruckJobSorter.Model;

namespace TruckJobSorter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void StartProcessClickEvent(object sender, RoutedEventArgs e)
        {
            List<TruckListModel> truckList = new List<TruckListModel>();
            List<TruckListModel> originalTruckList = new List<TruckListModel>();
            List<string> jobList = new List<string>();
            List<JobListModel> differentJobs = new List<JobListModel>();
            List<ResultListModel> resultList = new List<ResultListModel>();

            var fileContent = string.Empty;
            var filePath = string.Empty;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            string basePath = Environment.CurrentDirectory;

            openFileDialog.InitialDirectory = basePath;
            openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                //Get the path of specified file
                filePath = openFileDialog.FileName;

                //Read the contents of the file into a stream
                var fileStream = openFileDialog.OpenFile();

                string[] readOutFile = File.ReadAllLines(filePath);
                int availableTruckCount = Convert.ToInt32(readOutFile[0]);
                int availableJobCount = Convert.ToInt32(readOutFile[availableTruckCount+1]);
                int countOfHighestJobKnowledge = 0;

                // Collect job list, and count the available jobs
                for (int index = availableTruckCount + 2; index < availableTruckCount + availableJobCount + 2; index++)
                {
                    string[] tmpResult = readOutFile[index].Split(" ");
                    jobList.Add(tmpResult[1]);

                    var tmp = differentJobs.Find(x => x.jobName == tmpResult[1]);
                    if (tmp == null)
                    {
                        differentJobs.Add(new JobListModel(tmpResult[1], 1, 0));
                    }
                    else
                    {
                        differentJobs.Find(x => x.jobName == tmpResult[1]).jobCount++;
                    }
                }

                for (int index = 1; index < availableTruckCount + 1; index++)
                {
                    string[] tmpResult = readOutFile[index].Split(" ");
                    truckList.Add(new TruckListModel(Convert.ToInt32(tmpResult.First()), tmpResult.Skip(1).ToList()));

                    var tmpList = truckList.LastOrDefault();
                    foreach(string job in tmpList.jobKnowledge)
                    {
                        differentJobs.Find(x => x.jobName == job).availableTruckForJob++;
                    }
                    if(countOfHighestJobKnowledge < tmpList.jobKnowledge.Count)
                    {
                        countOfHighestJobKnowledge = tmpList.jobKnowledge.Count;
                    }
                }

                originalTruckList = truckList.ToList();
                
                for (int index = 1; index < countOfHighestJobKnowledge + 1; index++)
                {
                    List<TruckListModel> tmpTruckList = (List<TruckListModel>)truckList.Where(x => x.jobKnowledge.Count == index).ToList();
                    List<string> tempJobList = new List<string>(jobList);

                    foreach (string job in tempJobList)
                    {
                        foreach (TruckListModel truck in tmpTruckList.Where(x => x.jobKnowledge.Contains(job)))
                        {
                            if (CheckKnowledgeAvailability(job, truck, differentJobs))
                            {
                                resultList.Add(new ResultListModel(truck.ID, job));
                                tmpTruckList.Remove(truck);
                                truckList.Remove(truck);
                                foreach (string jobKnowledge in truck.jobKnowledge)
                                {
                                    differentJobs.FirstOrDefault(x => x.jobName == jobKnowledge).availableTruckForJob--;
                                }
                                differentJobs.FirstOrDefault(x => x.jobName == job).jobCount--;

                                int jobIndex = jobList.FindIndex(x => x == job);
                                if (jobIndex != -1)
                                {
                                    jobList.RemoveAt(jobIndex);
                                }
                                break;
                            }
                        }
                    }
                }

                foreach (string job in jobList)
                {
                    if(truckList.Count != 0)
                    {
                        TruckListModel truck = truckList.FirstOrDefault(x => x.jobKnowledge.Contains(job));
                        if (truck != null)
                        {
                            resultList.Add(new ResultListModel(truck.ID, job));
                        }
                        else 
                        {
                            bool replacedJob = false;
                            truck = truckList.First();
                            List<ResultListModel> tmpResultTruck = resultList.Where(x => x.job == job).ToList();
                            foreach(ResultListModel tmpResult in tmpResultTruck)
                            {
                                TruckListModel tmpTruck = originalTruckList.FirstOrDefault(x => x.ID == tmpResult.truckId);
                                foreach(string truckJobKnowledge in truck.jobKnowledge)
                                {
                                    if(tmpTruck.jobKnowledge.Contains(truckJobKnowledge))
                                    {
                                        resultList.Remove(tmpResult);
                                        resultList.Add(new ResultListModel(truck.ID, tmpResult.job));
                                        resultList.Add(new ResultListModel(tmpResult.truckId, job));
                                        truckList.Remove(truck);
                                        replacedJob = true;
                                        break;
                                    }
                                }
                                if(replacedJob)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                resultList = resultList.OrderBy(x => x.truckId).ToList();

                await WriteToFile(resultList);
            }
        }

        private async Task WriteToFile(List<ResultListModel> resultLists)
        {
            List<string> tmpResultList = new List<string>();

            foreach (ResultListModel result in resultLists)
            {
                tmpResultList.Add(result.truckId + " " + result.job);
            }

            await File.WriteAllLinesAsync(Environment.CurrentDirectory + @"\result.txt", tmpResultList);
        }

        private static bool CheckKnowledgeAvailability(string job, TruckListModel truckList, List<JobListModel> jobLists)
        {
            JobListModel tmpResult = jobLists.FirstOrDefault(x => x.jobName == job);
            if(tmpResult.jobCount >= tmpResult.availableTruckForJob && tmpResult.jobCount != 0)
            {
                return true;
            }
            else if(tmpResult.jobCount == 0)
            {
                return false;
            }
            else
            {
                foreach(string jobKnowledge in truckList.jobKnowledge)
                {
                    tmpResult = jobLists.FirstOrDefault(x => x.jobName == jobKnowledge);
                    if (tmpResult.jobCount >= tmpResult.availableTruckForJob)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
