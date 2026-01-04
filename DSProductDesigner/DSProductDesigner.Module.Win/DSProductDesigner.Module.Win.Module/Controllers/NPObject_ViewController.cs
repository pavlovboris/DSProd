using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using devDept.Eyeshot.Entities;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.XtraTreeList.Native;
using DSProductDesigner.Module.NPBusinessObjects;
using DSProductDesigner.Module.Win.Module.Controlls;
using View = DevExpress.ExpressApp.View;

namespace DSProductDesigner.Module.Win.Module.Controllers;

// For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.ViewController.
public class NPObject_ViewController : ObjectViewController<DetailView, IDSEntityHolder> {
    // Use CodeRush to create Controllers and Actions with a few keystrokes.
    // https://docs.devexpress.com/CodeRushForRoslyn/403133/
    public NPObject_ViewController() {


        SingleChoiceAction showRail = new SingleChoiceAction(this, "ShowRail", PredefinedCategory.View);

        showRail.Items.Add(new ChoiceActionItem($"Show All Rails", 0));


        for (int i = 1;NPObject.RailCount>=i;i++)
        {
            showRail.Items.Add(new ChoiceActionItem($"Show Rail {i}", i));
        }

        showRail.Execute += ShowRail_Execute1; ;

    }

    private void ShowRail_Execute1(object sender, SingleChoiceActionExecuteEventArgs e)
    {
        if ((int)e.SelectedChoiceActionItem.Data == 0)
        {
            View.CustomizeViewItemControl<ViewItem>(this, (a) =>
            {
                if (a.Control is DSViewPort dsViewPort)
                {
                    for (int i = 1; NPObject.RailCount >= i; i++)
                    {
                        dsViewPort.SetRailVisible(i, true);
                    }
                }
            });

        }
        else
        {
            int railIndex = (int)e.SelectedChoiceActionItem.Data;
            View.CustomizeViewItemControl<ViewItem>(this, (a) =>
            {
                if (a.Control is DSViewPort dsViewPort)
                {
                    for (int i = 1; NPObject.RailCount >= i; i++)
                    {
                        dsViewPort.SetRailVisible(i, i == railIndex);
                    }
                }
            });
        }
    }


    protected override void OnActivated() {
        base.OnActivated();

        //View.CustomizeViewItemControl<ViewItem>(this, (a) =>
        //{
        //    if (a.Control is DSViewPort dsViewPort)
        //    {
        //        dsViewPort.Setup(ObjectSpace, Application);
        //    }
        //});

        // Perform various tasks depending on the target View,
        // customize view items: https://docs.devexpress.com/eXpressAppFramework/120092.
    }
    protected override void OnViewControlsCreated() {
        base.OnViewControlsCreated();
        
        View.CustomizeViewItemControl<ViewItem>(this, (a) =>
        {
            if (a.Control is DSViewPort dsViewPort)
            {
                dsViewPort.NPObject = this.ViewCurrentObject;
                dsViewPort.design1.SelectionChanged += Design1_SelectionChanged;
            }
        });

        // Access and customize the target View control.
    }

    private void Design1_SelectionChanged(object sender, devDept.Eyeshot.Control.SelectionChangedEventArgs e)
    {
        if (ViewCurrentObject is not IDSEntityHolder holder)
            return;

        var npObject = holder as NPObject;
        var allDetails = npObject?.Entities ?? holder.Entities;

        // детайлите, чиито Entity е в AddedItems
        var selectedDetails = allDetails
            .Where(d => d.Entity != null && e.AddedItems.Any(a => a.Item == d.Entity))
            .ToList();

        var listPropEditor = View.GetItems<ListPropertyEditor>()
            .FirstOrDefault(pe => pe.MemberInfo.Name == nameof(IDSEntityHolder.Entities));

        var listView = listPropEditor?.ListView;
        var listEditor = listView?.Editor as DevExpress.ExpressApp.Win.Editors.GridListEditor;

        if (listView == null || listEditor == null)
            return;

        // 1) чистим селекцията през ListEditor
        //listEditor.
        listEditor.GridView.ClearSelection();
        // 2) селектираме новите обекти
        foreach (var detail in selectedDetails)
        {
            listEditor.GridView.SelectRow(listEditor.GetIndexByObject(detail));
        }

        // 3) фокусираме първия
        //if (selectedDetails.Count > 0)
        //{
        //    listEditor.FocusedObject = selectedDetails[0];
        //}
    }

    protected override void OnDeactivated() {
        // Unsubscribe from previously subscribed events and release other references and resources.
        base.OnDeactivated();
    }
}
