using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotCore.Managers
{
    public class TaskManager
    {
        [JsonProperty]
        private List<Task> taskList = new List<Task>();

        public Task[] Tasks
        {
            get
            {
                return taskList.ToArray();
            }
        }

        public int Works => taskList.Where(x => x.Status == TaskStatus.Running).Count();
        public bool IsAbuse
        {
            get
            {
                return (Works >= 20);
            }
        }
        public TaskManager()
        {

        }
        
        private void DropTrash()
        {
            if (taskList.Count > 10)
                taskList.RemoveAll(x => x.IsCompleted);
        }

        public void Add(Task task)
        {
            DropTrash();
            taskList.Add(task);
        }

        public void WaitAll()
        {
            Task.WaitAll(Tasks);
        }
    }
}