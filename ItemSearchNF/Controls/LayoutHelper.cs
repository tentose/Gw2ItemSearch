using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemSearch.Controls
{
    public enum DimensionSizeMode
    {
        Static,
        Inherit,
    }

    public class LayoutHelper
    {
        public static void SetWidthSizeMode(Control control, DimensionSizeMode mode, int margin = 0)
        {
            if (mode == DimensionSizeMode.Inherit)
            {
                control.Width = control.Parent.Width - margin;
            }
        }

        public static void SetHeightSizeMode(Control control, DimensionSizeMode mode, int margin = 0)
        {
            if (mode == DimensionSizeMode.Inherit)
            {
                control.Height = control.Parent.Height - margin;
            }
        }

        public static void SetHorizontalAlignment(Control control, HorizontalAlignment alignment, int margin)
        {
            int newX = 0;
            if (alignment == HorizontalAlignment.Left)
            {
                newX = margin;
            }
            else if (alignment == HorizontalAlignment.Center)
            {
                newX = control.Parent.Width / 2 - control.Width / 2;
            }
            else if (alignment == HorizontalAlignment.Right)
            {
                newX = control.Parent.Width - control.Width - margin;
            }

            control.Location = new Point(newX, control.Location.Y);
        }

        public static void SetVerticalAlignment(Control control, VerticalAlignment alignment, int margin)
        {
            int newY = 0;
            if (alignment == VerticalAlignment.Top)
            {
                newY = margin;
            }
            else if (alignment == VerticalAlignment.Middle)
            {
                newY = control.Parent.Height / 2 - control.Height / 2;
            }
            else if (alignment == VerticalAlignment.Bottom)
            {
                newY = control.Parent.Height - control.Height - margin;
            }

            control.Location = new Point(control.Location.X, newY);
        }
    }
}
