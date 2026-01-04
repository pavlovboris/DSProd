using DevExpress.ExpressApp.DC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using devDept.Eyeshot;
using devDept.Eyeshot.Entities;

namespace DSProductDesigner.Module.DSEntities
{
    [DomainComponent]
    public class DSEntityObject:DSEntityBase
    {
        private double width;
        public double Width
        {
            get => width;
            set => SetPropertyValue(ref width, value);
        }

        private double length;
        public double Length
        {
            get => length;
            set => SetPropertyValue(ref length, value);
        }

        private double height;
        public double Height
        {
            get => height;
            set => SetPropertyValue(ref height, value);
        }

        private double startX;
        public double StartX
        {
            get => startX;
            set => SetPropertyValue(ref startX, value);
        }

        private double startY;
        public double StartY
        {
            get => startY;
            set => SetPropertyValue(ref startY, value);
        }

        private double startZ;
        public double StartZ
        {
            get => startZ;
            set => SetPropertyValue(ref startZ, value);
        }

        private Color color = Color.Transparent;
        public Color Color
        {
            get => color;
            set => SetPropertyValue(ref color, value);
        }

        private int railIndex = 1; // 1..5
        public int RailIndex
        {
            get => railIndex;
            set => SetPropertyValue(ref railIndex, value);
        }

        private bool allowRotation;
        public bool AllowRotation
        {
            get => allowRotation;
            set => SetPropertyValue(ref allowRotation, value);
        }

        private bool coatBothSides;
        public bool CoatBothSides
        {
            get => coatBothSides;
            set => SetPropertyValue(ref coatBothSides, value);
        }

        [Browsable(false)]
        public Entity LabelEntity { get; set; }

        public int DetailType { get; set; }

        /// <summary>
        /// Optional per-detail vertical gap (Z). If less than or equal to 0, NPObject.VerticalDetailGap is used.
        /// </summary>
        public double VerticalGapOverride { get; set; }

    }
}
