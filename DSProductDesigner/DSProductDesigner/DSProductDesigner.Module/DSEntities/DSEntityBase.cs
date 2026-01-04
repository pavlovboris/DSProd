using devDept.Eyeshot.Entities;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Data;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DSProductDesigner.Module.DSEntities;

[DomainComponent]
public class DSEntityBase : IXafEntityObject, INotifyPropertyChanged, ICustomPropertyStore, IObjectSpaceLink
{
    private Entity entity;
    public Entity Entity
    {
        get { return entity; }
        set
        {
            SetPropertyValue(ref entity, value);
        }
    }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static bool DefaultUpdateCalculatedPropertiesOnChanged = true;

    private static Dictionary<Type, object> defaultValues;

    private bool updateCalculatedPropertiesOnChanged = DefaultUpdateCalculatedPropertiesOnChanged;

    private Dictionary<IMemberInfo, object> customPropertyStore;

    private Lazy<ITypeInfo> typeInfo;
    private IObjectSpace objectSpace;

    protected IObjectSpace ObjectSpace
    {
        get
        {
            return objectSpace;
        }
        set
        {
            if (objectSpace != value)
            {
                OnObjectSpaceChanging();
                objectSpace = value;
                OnObjectSpaceChanged();
            }
        }
    }

    IObjectSpace IObjectSpaceLink.ObjectSpace
    {
        get
        {
            return ObjectSpace;
        }
        set
        {
            ObjectSpace = value;
        }
    }

    protected virtual void OnObjectSpaceChanging()
    {
    }

    protected virtual void OnObjectSpaceChanged()
    {
    }
    private Guid oid;

    [Key]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    public Guid Oid
    {
        get
        {
            return oid;
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        set
        {
            oid = value;
        }
    }

    public DSEntityBase()
    {
        typeInfo = new Lazy<ITypeInfo>(GetTypeInfo);
        oid = Guid.NewGuid();
    }

    public DSEntityBase(Guid oid)
    {
        typeInfo = new Lazy<ITypeInfo>(GetTypeInfo);
        this.oid = oid;
    }

    private Dictionary<IMemberInfo, object> CustomPropertyStore
    {
        get
        {
            if (customPropertyStore == null)
            {
                customPropertyStore = new Dictionary<IMemberInfo, object>();
            }

            return customPropertyStore;
        }
    }

    protected bool UpdateCalculatedPropertiesOnChanged
    {
        get
        {
            return updateCalculatedPropertiesOnChanged;
        }
        set
        {
            updateCalculatedPropertiesOnChanged = value;
        }
    }

    bool ICustomPropertyStore.UpdateCalculatedPropertiesOnChanged
    {
        get
        {
            return UpdateCalculatedPropertiesOnChanged;
        }
        set
        {
            UpdateCalculatedPropertiesOnChanged = value;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual ITypeInfo GetTypeInfo()
    {
        if (ObjectSpace == null)
        {
            return XafTypesInfo.Instance.FindTypeInfo(GetType());
        }

        return ObjectSpace.TypesInfo.FindTypeInfo(GetType());

        
    }

    public virtual void OnCreated()
    {
    }

    public virtual void OnSaving()
    {
    }

    public virtual void OnLoaded()
    {
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetPropertyValue<T>(ref T propertyValue, T newValue, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(propertyValue, newValue))
        {
            return false;
        }

        propertyValue = newValue;
        OnPropertyChanged(propertyName);
        return true;
    }

    private object CreateDefValue(IMemberInfo memberInfo)
    {
        object value = null;
        if (memberInfo.MemberTypeInfo.IsValueType)
        {
            if (defaultValues == null)
            {
                defaultValues = new Dictionary<Type, object>();
            }

            if (!defaultValues.TryGetValue(memberInfo.MemberType, out value))
            {
                value = Activator.CreateInstance(memberInfo.MemberType);
                defaultValues[memberInfo.MemberType] = value;
            }
        }

        return value;
    }

    private object GetCustomPropertyValue(IMemberInfo memberInfo)
    {
        object value = null;
        if (customPropertyStore != null)
        {
            customPropertyStore.TryGetValue(memberInfo, out value);
        }

        if (value == null)
        {
            value = CreateDefValue(memberInfo);
            if (value != null)
            {
                CustomPropertyStore[memberInfo] = value;
            }
        }

        return value;
    }

    object ICustomPropertyStore.GetCustomPropertyValue(IMemberInfo memberInfo)
    {
        return GetCustomPropertyValue(memberInfo);
    }

    bool ICustomPropertyStore.SetCustomPropertyValue(IMemberInfo memberInfo, object value)
    {
        object customPropertyValue = GetCustomPropertyValue(memberInfo);
        if (CanSkipAssignment(customPropertyValue, value))
        {
            return false;
        }

        if (value == null)
        {
            CustomPropertyStore.Remove(memberInfo);
        }
        else
        {
            CustomPropertyStore[memberInfo] = value;
        }

        OnPropertyChanged(memberInfo.Name);
        return true;
    }

    private bool CanSkipAssignment(object oldValue, object newValue)
    {
        if (oldValue == newValue)
        {
            return true;
        }

        if (oldValue is ValueType && newValue is ValueType && object.Equals(oldValue, newValue))
        {
            return true;
        }

        if (oldValue is string && newValue is string && object.Equals(oldValue, newValue))
        {
            return true;
        }

        return false;
    }

    public void SetMemberValue(string propertyName, object newValue)
    {
        typeInfo.Value.FindMember(propertyName).SetValue(this, newValue);
    }

    public object GetMemberValue(string propertyName)
    {
        return typeInfo.Value.FindMember(propertyName).GetValue(this);
    }

}