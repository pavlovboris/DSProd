using Azure.Core.GeoJson;
using devDept.Eyeshot.Entities;
using DevExpress.ExpressApp;
using DevExpress.XtraEditors;
using DSProductDesigner.Module.DSEntities;
using DSProductDesigner.Module.NPBusinessObjects;
using System.ComponentModel;

namespace DSProductDesigner.Module.Win.Module.Controlls;

public partial class DSViewPort : XtraUserControl,DevExpress.ExpressApp.Editors.IComplexControl{
    public DSViewPort() 
    {
        InitializeComponent();

        
    }

    IObjectSpace ObjectSpace;

    public void Setup(IObjectSpace objectSpace, XafApplication application)
    {
        ObjectSpace = objectSpace;
    }

    IDSEntityHolder _npObject;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IDSEntityHolder NPObject
    {
        get => _npObject;
        set
        {
            if (_npObject != value) 
            {
                var old = _npObject;
                _npObject = value;
                NPObjectChanged(old, _npObject);
            }
        }
    }


    public void NPObjectChanged(IDSEntityHolder oldObject, IDSEntityHolder newObject)
    {
        design1.Clear();
        
        if (newObject != null && newObject.EntityList != null)
        {
            newObject.EntityListChanged += NewObject_EntityListChanged;
            design1.Entities.AddRange(newObject.EntityList);
        }

        design1.CreateControl();

        design1.Entities.Regen();

        design1.Invalidate();

        design1.Refresh();

        design1.ZoomFit();
    }

    private void NewObject_EntityListChanged(object sender, EventArgs e)
    {
        design1.Entities.Clear();
        design1.Entities.AddRange(NPObject.EntityList);

        // нормализираме и добавяме DWG профилите за всеки детайл
        foreach (var d in NPObject.Entities)
        {
            if (d.ComplexShape?.ComplexEntities == null || d.ComplexShape.ComplexEntities.Count == 0)
                continue;

            // нормализиране в правилната равнина и размер
            NormalizeDetailProfile(d);

            // добавяме ентитата към сцената
            foreach (var ce in d.ComplexShape.ComplexEntities)
            {
                if (!design1.Entities.Contains(ce))
                {
                    design1.Entities.Add(ce);
                }
            }
        }

        design1.Entities.Regen();
        design1.Invalidate();
        design1.Refresh();
        design1.ZoomFit();
    }
    public void SetRailVisible(int railIndex, bool visible)
    {
        if (NPObject == null)
            return;

        foreach (var d in NPObject.Entities.Where(e => e.RailIndex == railIndex))
        {
            if (d.Entity is Entity ent)
            {
                if (visible && !design1.Entities.Contains(ent))
                    design1.Entities.Add(ent, color: d.Color);
                else if (!visible && design1.Entities.Contains(ent))
                    design1.Entities.Remove(ent);
            }

            if (d.LabelEntity is Entity labelEnt)
            {
                if (visible && !design1.Entities.Contains(labelEnt))
                    design1.Entities.Add(labelEnt);
                else if (!visible && design1.Entities.Contains(labelEnt))
                    design1.Entities.Remove(labelEnt);
            }

            // DWG профили за този детайл
            if (d.ComplexShape?.ComplexEntities != null)
            {
                foreach (var ce in d.ComplexShape.ComplexEntities)
                {
                    if (visible && !design1.Entities.Contains(ce))
                        design1.Entities.Add(ce);
                    else if (!visible && design1.Entities.Contains(ce))
                        design1.Entities.Remove(ce);
                }
            }
        }

        design1.Invalidate();
    }

    private void NormalizeDetailProfile(DSEntityObject detail)
    {
        // засега не правим нищо – позиция и ротация са в NPObject
    }
}