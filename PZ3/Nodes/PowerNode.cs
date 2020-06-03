using System;
using System.Windows.Media;

namespace PZ3
{
    public class PowerNode
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int ConnectionCount { get; set; }

        public virtual string ToolTip => GetType().Name.ToString() + Environment.NewLine + Id + " " + Name + Environment.NewLine + "Connections: " + ConnectionCount;
        public virtual SolidColorBrush Color => Brushes.Red;
    }
}