using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DXF.Models
{
    public class Line
    {
        public Point startPoint;
        public Point endPoint;
        public double length;
        public double angle;
        public int connectedLines;

        public Line (Point startPoint, Point endPoint, double length, double angle)
        {
            this.startPoint = startPoint;
            this.endPoint = endPoint;
            this.length = length;
            this.angle = angle;
            connectedLines = 0;
        }

    }
}
