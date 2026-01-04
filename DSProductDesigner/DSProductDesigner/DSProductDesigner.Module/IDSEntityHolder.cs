using DSProductDesigner.Module.DSEntities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace DSProductDesigner.Module
{
    public interface IDSEntityHolder
    {
        public devDept.Eyeshot.EntityList EntityList { get; }
        public event EventHandler EntityListChanged;
        public ObservableCollection<DSEntityObject> Entities { get; }
    }
}
