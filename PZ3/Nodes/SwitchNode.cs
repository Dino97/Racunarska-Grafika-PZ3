using System;
using System.Windows.Media;

namespace PZ3
{
    public class SwitchNode : PowerNode
    {
        public bool Status { get; set; }

        public override string ToolTip => base.ToolTip + Environment.NewLine + "Status: " + (Status ? "CLOSED" : "OPEN");
        public override SolidColorBrush Color => Brushes.Blue;
    }
}