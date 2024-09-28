using System;
using System.Windows.Forms;

namespace JasoFixer
{
    public class Search
    {
        private readonly List list;

        public string RootPath { get; set; } = string.Empty;
        public bool IsIncludeSubDirectory { get; set; } = false;
        public bool IsIncludeDirectory { get; set; } = false;

        public List GetList()
        {
            return this.list;
        }

        public Search(List list)
        {
            this.list = list ?? throw new ArgumentNullException(nameof(list));
        }

        public void Add(string beforeFile, string afterFile, string type)
        {
            string[] row = { beforeFile, afterFile, type };
            InvokeIfNeeded(() => { this.list.Items.Add(new ListItem(row)); });
        }

        public void Clear()
        {
            InvokeIfNeeded(() => { this.list.Items.Clear(); });
        }

        private void InvokeIfNeeded(Action action)
        {
            if (this.list.InvokeRequired)
            {
                this.list.Invoke(action);
            }
            else
            {
                action();
            }
        }
    }
}