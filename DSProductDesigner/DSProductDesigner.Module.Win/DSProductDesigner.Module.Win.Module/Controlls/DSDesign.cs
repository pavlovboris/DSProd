using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Linq;
using System.Windows.Forms;

namespace DSProductDesigner.Module.Win.Module.Controlls
{
    public class DSDesign : Design
    {
        private bool _editing;

        public DSDesign()
        {
            Units = linearUnitsType.Millimeters;
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            OriginSymbol.Visible = false;
            ActiveViewport.SetView(viewType.Front);

            // allow picking entities
            ActionMode = actionType.SelectVisibleByPick;
        }

        private void SetEditing(bool value)
        {
            _editing = value;
        }

        private Entity GetSelectedEntity(out int countSelected)
        {
            countSelected = 0;
            Entity selectedEnt = null;

            foreach (Entity ent in Entities)
            {
                if (ent.Selected)
                {
                    countSelected++;
                    selectedEnt = ent;
                }
            }

            return selectedEnt;
        }

        // right-click to start/cancel moving the selected entity
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button == MouseButtons.Right)
            {
                if (_editing)
                {
                    // Cancels the ObjectManipulator editing
                    ObjectManipulator.Cancel();
                    SetEditing(false);
                }
                else
                {
                    // Starts to edit the selected entity with the ObjectManipulator
                    int countSelected;
                    Entity selectedEnt = GetSelectedEntity(out countSelected);

                    if (countSelected == 1)
                    {
                        SetEditing(true);

                        devDept.Geometry.Transformation initialTransformation = null;
                        bool center = true;

                        // If there is only one selected entity, position and orient the manipulator
                        // using the rotation point saved in its EntityData property
                        Point3D rotationPoint = null;

                        if (selectedEnt.EntityData is Point3D)
                        {
                            center = false;
                            rotationPoint = (Point3D)selectedEnt.EntityData;
                        }

                        if (rotationPoint != null)
                        {
                            initialTransformation =
                                devDept.Geometry.Transformation.CreateTranslation(
                                    rotationPoint.X,
                                    rotationPoint.Y,
                                    rotationPoint.Z);
                        }
                        else
                        {
                            initialTransformation =
                                devDept.Geometry.Transformation.CreateIdentity();
                        }

                        // Enables the ObjectManipulator to start editing the selected objects
                        ObjectManipulator.Enable(initialTransformation, center);
                    }
                }

                Invalidate();
            }
        }

        // double-click to apply the transformation
        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);

            if (!_editing)
                return;

            ObjectManipulator.Apply();
            Entities.Regen();
            SetEditing(false);
            Invalidate();
        }
    }
}
