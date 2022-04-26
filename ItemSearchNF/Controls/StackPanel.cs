using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemSearch.Controls
{
    public enum StackPanelOrientation
    {
        Horizontal,
        Vertical,
    };

    public class StackPanel : Container
    {
        
        public int Gap { get; set; }
        public StackPanelOrientation Orientation { get; set; }

        public override void RecalculateLayout()
        {
            base.RecalculateLayout();

            Point nextPosition = new Point(0, 0);
            foreach (var child in Children)
            {
                child.Location = nextPosition;

                if (Orientation == StackPanelOrientation.Horizontal)
                {
                    nextPosition = new Point(child.Right + Gap, 0);
                }
                else
                {
                    nextPosition = new Point(0, child.Bottom + Gap);
                }
            }
        }
    }
}
