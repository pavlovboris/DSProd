using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Meshing;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DSProductDesigner;
using DSProductDesigner.Module;
using DSProductDesigner.Module.DSEntities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;

namespace DSProductDesigner.Module.NPBusinessObjects;

[DomainComponent]
[DefaultClassOptions]
public class NPObject : NonPersistentBaseObject, IDSEntityHolder
{
    private devDept.Eyeshot.EntityList entityList;
    [Browsable(false)]
    public devDept.Eyeshot.EntityList EntityList
    {
        get
        {
            entityList ??= GetEntityList();

            return entityList;
        }
    }

    public override void OnLoaded()
    {
        base.OnLoaded();

    }

    public override void OnCreated()
    {
        base.OnCreated();

        Entities.CollectionChanged += Entities_CollectionChanged;
    }

    private void Entities_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            foreach (var item in e.NewItems.Cast<DSEntityObject>())
            {
                var box = Mesh.CreateBox(
                item.Width,
                item.Length,
                item.Height);

                box.Translate(item.StartX, item.StartY, item.StartZ);

                item.Entity = box;

                // вместо item.Color → правим го полупрозрачен или напълно прозрачен
                // ако Eyeshot поддържа alpha: Color.FromArgb(50, item.Color)
                // ако не – можеш да оставиш лек цвят + wireframe мода по-късно
                EntityList.Add(box, color: Color.FromArgb(70, item.Color));

                // Add text label to indicate one-sided / two-sided
                string label = item.CoatBothSides ? "2S" : "1S";

                // текст малко над релсата, не точно върху детайла
                double textHeight = 80; // нагласи спрямо мащаба
                double textX = item.StartX + item.Width / 2.0;
                double textY = item.StartY + item.Length / 2.0 + 100; // 100 мм напред по дължина
                double textZ = item.StartZ + item.Height + RailHeight + 50; // над детайла и релсата

                var textEntity = new devDept.Eyeshot.Entities.Text(
                    new devDept.Geometry.Point3D(textX, textY, textZ),
                    label,
                    textHeight);

                textEntity.ColorMethod = devDept.Eyeshot.Entities.colorMethodType.byEntity;
                // по-контрастни цветове
                textEntity.Color = item.CoatBothSides ? Color.Lime : Color.Red;
                textEntity.Selectable = false;

                item.LabelEntity = textEntity;
                EntityList.Add(textEntity);

                DrawComplexShapeFor(item);

                EntityListChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
        {
            foreach (var item in e.OldItems.Cast<DSEntityObject>())
            {
                if (item.Entity != null)
                {
                    EntityList.Remove(item.Entity);
                }
                if (item.LabelEntity != null)
                {
                    EntityList.Remove(item.LabelEntity);
                }
                EntityListChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    [Aggregated]
    public ObservableCollection<DSEntityObject> Entities { get; } = new();

    public event EventHandler EntityListChanged;

    devDept.Eyeshot.EntityList GetEntityList()
    {
        var newEntityList = new devDept.Eyeshot.EntityList();

        // Oven box (outer/inner volume)
        //double ovenWidth = 1500;
        //double ovenLength = 7000;
        //double ovenHeight = 2500;

        var box = Mesh.CreateBox(
            OvenWidth,
            OvenLength,
            OvenHeight);

        box.Selectable = false;
        newEntityList.Add(box, color: Color.Transparent);

        // Add 5 rails inside the oven
        int railCount = 5;

        double railLength = OvenLength;   // rails run along length
        double railWidth = 50;            // thickness in X, adjust as needed
        //double railHeight = 50;           // thickness in Z, adjust as needed

        // rails spaced evenly across width (X)
        double spacing = OvenWidth / (railCount + 1);

        for (int i = 1; i <= railCount; i++)
        {
            double railCenterX = i * spacing;
            double railCenterY = OvenLength / 2.0;
            double railCenterZ = OvenHeight * 0.9; // near the top; use 0.5 for middle

            var rail = Mesh.CreateBox(
                railWidth,
                railLength,
                RailHeight);

            // move box center to desired position
            rail.Translate(
                railCenterX - railWidth / 2.0,
                railCenterY - railLength / 2.0,
                railCenterZ - RailHeight / 2.0);

            rail.Selectable = false; // allow selecting / hanging details
            newEntityList.Add(rail, color: Color.Silver);
        }

        return newEntityList;
    }


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

    private Color color = Color.LightGray;
    public Color Color
    {
        get => color;
        set => SetPropertyValue(ref color, value);
    }

    private int railIndex = 1;
    public int RailIndex
    {
        get => railIndex;
        set => SetPropertyValue(ref railIndex, value);
    }

    // Oven / rail layout constants (put near top of NPObject)
    private const double OvenWidth = 1500;
    private const double OvenLength = 7000;
    private const double OvenHeight = 2500;

    public const int RailCount = 5; // остава константа – броя релси

    private double railHeight = 50;
    public double RailHeight
    {
        get => railHeight;
        set => SetPropertyValue(ref railHeight, value);
    }

    private double hangGap = 20;
    public double HangGap
    {
        get => hangGap;
        set => SetPropertyValue(ref hangGap, value);
    }

    private double railDetailGap = 50;
    public double RailDetailGap
    {
        get => railDetailGap;
        set => SetPropertyValue(ref railDetailGap, value);
    }

    private double verticalDetailGap = 50;
    public double VerticalDetailGap
    {
        get => verticalDetailGap;
        set => SetPropertyValue(ref verticalDetailGap, value);
    }

    private double backToBackGap = 5;
    public double BackToBackGap
    {
        get => backToBackGap;
        set => SetPropertyValue(ref backToBackGap, value);
    }

    private double newDetailVerticalGapOverride;
    public double NewDetailVerticalGapOverride
    {
        get => newDetailVerticalGapOverride;
        set => SetPropertyValue(ref newDetailVerticalGapOverride, value);
    }

    private int newDetailType;
    public int NewDetailType
    {
        get => newDetailType;
        set => SetPropertyValue(ref newDetailType, value);
    }

    [Action]
    public void AddEntity()
    {
        double rawWidth = width;
        double rawLength = length;
        double rawHeight = height;

        if (rawLength <= 0 || rawWidth <= 0 || rawHeight <= 0)
            return;

        int startRail = Math.Max(1, Math.Min(RailCount, RailIndex));

        double chosenX = 0;
        double chosenY = 0;
        double chosenZ = 0;
        double chosenW = 0;
        double chosenL = 0;
        double chosenH = 0;
        int chosenRail = -1;

        // Use NPObject.NewDetailAllowRotation for this new detail
        foreach (var ori in GetOrientations(rawWidth, rawLength, rawHeight, NewDetailAllowRotation))
        {
            for (int offset = 0; offset < RailCount; offset++)
            {
                int idx = startRail + offset;
                if (idx > RailCount)
                    idx -= RailCount;

                if (TryFindFreeSpotOnRail(idx, ori.Width, ori.Length, ori.Height,
                                  out var x, out var y, out var z))
                {
                    chosenRail = idx;
                    chosenX = x;
                    chosenY = y;
                    chosenZ = z;
                    chosenW = ori.Width;
                    chosenL = ori.Length;
                    chosenH = ori.Height;
                    break;
                }
            }

            if (chosenRail > 0)
                break;
        }

        if (chosenRail < 0)
        {
            throw new UserFriendlyException(
                "No free space on any rail to place this detail (with or without rotation).");
        }

        // spacing / railCenterX are currently only needed for potential future logic;
        // placement uses chosenX/Y/Z from TryFindFreeSpotOnRail.
        double spacing = OvenWidth / (RailCount + 1);
        double railCenterX = chosenRail * spacing;

        var newEntity = ObjectSpace.CreateObject<DSEntityObject>();
        newEntity.Width = chosenW;
        newEntity.Length = chosenL;
        newEntity.Height = chosenH;
        newEntity.StartX = chosenX;
        newEntity.StartY = chosenY;
        newEntity.StartZ = chosenZ;
        newEntity.Color = color;
        newEntity.RailIndex = chosenRail;
        newEntity.AllowRotation = NewDetailAllowRotation;
        newEntity.CoatBothSides = NewDetailCoatBothSides;

        // новото:
        newEntity.DetailType = NewDetailType; // ако искаш, тук можеш да вържеш по някакъв избор
        newEntity.VerticalGapOverride = NewDetailVerticalGapOverride;
        
        Entities.Add(newEntity);
    }

    private static bool BoxesOverlap(
        double x1, double y1, double z1, double w1, double l1, double h1,
        double x2, double y2, double z2, double w2, double l2, double h2)
    {
        bool overlapX = x1 < x2 + w2 && x1 + w1 > x2;
        bool overlapY = y1 < y2 + l2 && y1 + l1 > y2;
        bool overlapZ = z1 < z2 + h2 && z1 + h1 > z2;

        return overlapX && overlapY && overlapZ;
    }

    private IEnumerable<DSEntityObject> GetEntitiesOnRail(int railIndex)
    {
        return Entities.Where(e => e.RailIndex == railIndex);
    }

    // връща и X, защото имаме до 2 позиции по X под релсата
    private bool TryFindFreeSpotOnRail(
        int railIndex,
        double detailWidth,
        double detailLength,
        double detailHeight,
        out double posX,
        out double posY,
        out double posZ)
    {
        posX = 0;
        posY = 0;
        posZ = 0;

        var existing = GetEntitiesOnRail(railIndex).ToList();
        if (detailLength <= 0 || detailWidth <= 0 || detailHeight <= 0)
            return false;

        // ако на релсата вече има детайли, не позволяваме смесване на типове
        if (existing.Count > 0)
        {
            // приемаме, че всички вече поставени са от един тип
            int existingType = existing[0].DetailType;

            // ако и новият детайл има зададен тип (различен от 0)
            // и типът е различен от вече наличния → тази релса се счита пълна за този тип
            if (NewDetailType != 0 && existingType != 0 && NewDetailType != existingType)
            {
                return false;
            }
        }

        bool newDetailCoatBothSides = NewDetailCoatBothSides;

        // диапазон по Y за вече поставените едностранни
        double? minOneSidedY = null;
        double? maxOneSidedY = null;

        foreach (var e in existing)
        {
            if (!e.CoatBothSides)
            {
                double s = e.StartY;
                double eY = e.StartY + e.Length;
                if (minOneSidedY == null || s < minOneSidedY.Value)
                    minOneSidedY = s;
                if (maxOneSidedY == null || eY > maxOneSidedY.Value)
                    maxOneSidedY = eY;
            }
        }

        double spacing = OvenWidth / (RailCount + 1);
        double railCenterX = railIndex * spacing;
        double railCenterZ = OvenHeight * 0.9;
        double railBottomZ = railCenterZ - RailHeight / 2.0;

        // общи Y/Z параметри
        double railMinY = 0;
        double railMaxY = OvenLength - RailDetailGap;

        double baseStepY = Math.Max(10, detailLength / 3);
        double stepY = Math.Min(RailDetailGap, baseStepY);
        double stepZ = Math.Min(VerticalDetailGap, Math.Max(10, detailHeight / 2));

        // X позиции:
        double centerX = railCenterX - detailWidth / 2.0;
        double lane0X = railCenterX - (BackToBackGap / 2.0) - detailWidth; // ляво
        double lane1X = railCenterX + (BackToBackGap / 2.0);               // дясно

        for (double y = railMinY; y + detailLength <= railMaxY; y += stepY)
        {
            for (double z = railBottomZ - HangGap - detailHeight;
                 z >= 0;
                 z -= stepZ)
            {
                if (newDetailCoatBothSides)
                {
                    bool collision = false;

                    double candStartY = y;
                    double candEndY   = y + detailLength;
                    double candStartZ = z;
                    double candEndZ   = z + detailHeight;

                    // ако има едностранни на релсата, забраняваме Y,
                    // които пресичат техния диапазон [minOneSidedY, maxOneSidedY]
                    if (minOneSidedY.HasValue && maxOneSidedY.HasValue)
                    {
                        bool intersectsOneSidedBand =
                            candStartY <  maxOneSidedY.Value &&
                            candEndY   >  minOneSidedY.Value;

                        if (intersectsOneSidedBand)
                        {
                            // този кандидатски Y би бил \"между\" едностранните → прескачаме
                            continue;
                        }

                        // допълнително: ако детайлът е след блока едностранни,
                        // да започва поне на maxOneSidedY + RailDetailGap
                        if (candStartY >= maxOneSidedY.Value &&
                            candStartY <  maxOneSidedY.Value + RailDetailGap)
                        {
                            continue;
                        }
                    }

                    foreach (var e in existing)
                    {
                        double exStartY = e.StartY - RailDetailGap;
                        double exEndY   = e.StartY + e.Length + RailDetailGap;

                        // вертикален gap за вече поставения детайл:
                        // ако VerticalGapOverride == 0 -> ползваме глобалния VerticalDetailGap
                        double placedVertGap =
                            e.VerticalGapOverride > 0 ? e.VerticalGapOverride : VerticalDetailGap;

                        double exStartZ = e.StartZ - placedVertGap;
                        double exEndZ   = e.StartZ + e.Height + placedVertGap;

                        if (BoxesOverlap(
                                centerX,
                                candStartY,
                                candStartZ,
                                detailWidth,
                                candEndY - candStartY,
                                candEndZ - candStartZ,
                                e.StartX,
                                exStartY,
                                exStartZ,
                                e.Width,
                                exEndY - exStartY,
                                exEndZ - exStartZ))
                        {
                            collision = true;
                            break;
                        }
                    }

                    if (!collision)
                    {
                        posX = centerX;
                        posY = y;
                        posZ = z;
                        return true;
                    }
                }
                else
                {
                    // НОВО ПОВЕДЕНИЕ: едностранен → 2 ленти по X, без разширения по Y/Z
                    // лява лента
                    if (!CollidesAt(existing, lane0X, y, z, detailWidth, detailLength, detailHeight))
                    {
                        posX = lane0X;
                        posY = y;
                        posZ = z;
                        return true;
                    }

                    // дясна лента
                    if (!CollidesAt(existing, lane1X, y, z, detailWidth, detailLength, detailHeight))
                    {
                        posX = lane1X;
                        posY = y;
                        posZ = z;
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private readonly struct OrientedSize
    {
        public OrientedSize(double w, double l, double h)
        {
            Width = w;
            Length = l;
            Height = h;
        }

        public double Width { get; }
        public double Length { get; }
        public double Height { get; }
    }

    /// <summary>
    /// Returns possible orientations for a detail, based on its own AllowRotation flag.
    /// Only rotation allowed: swap Length and Height.
    /// </summary>
    private IEnumerable<OrientedSize> GetOrientations(double w, double l, double h, bool allowRotation)
    {
        // original orientation
        yield return new OrientedSize(w, l, h);

        if (!allowRotation)
            yield break;

        // only allowed rotation: length <-> height
        if (!l.Equals(h))
            yield return new OrientedSize(w, h, l);
    }

    private bool newDetailAllowRotation = true;
    public bool NewDetailAllowRotation
    {
        get => newDetailAllowRotation;
        set => SetPropertyValue(ref newDetailAllowRotation, value);
    }

    private bool newDetailCoatBothSides = true;
    public bool NewDetailCoatBothSides
    {
        get => newDetailCoatBothSides;
        set => SetPropertyValue(ref newDetailCoatBothSides, value);
    }

    // проста проверка за колизия спрямо вече окачените детайли на релсата
    private bool CollidesAt(
    IReadOnlyList<DSEntityObject> existing,
    double x, double y, double z,
    double w, double l, double h)
    {
        // X: разширяваме новия детайл с BackToBackGap (за 5мм между едностранни)
        double candX = x - BackToBackGap / 2.0;
        double candWidth = w + BackToBackGap;

        // Y: самият кандидат може да е \"истинския\" размер по дължина,
        // gap-а по дължина ще дойде от разширяване на съществуващите детайли
        double candY = y;
        double candLength = l;

        // Z: разширяваме новия детайл с VerticalDetailGap нагоре и надола
        double candZ = z - VerticalDetailGap;
        double candHeight = h + 2 * VerticalDetailGap;

        foreach (var e in existing)
        {
            // разширяваме всеки вече поставен детайл по Y с RailDetailGap
            double exStartY = e.StartY - RailDetailGap;
            double exEndY = e.StartY + e.Length + RailDetailGap;

            double exY = exStartY;
            double exLength = exEndY - exStartY;

            if (BoxesOverlap(
                    candX, candY, candZ, candWidth, candLength, candHeight,
                    e.StartX, exY, e.StartZ,
                    e.Width, exLength, e.Height))
            {
                return true;
            }
        }
        return false;
    }
    [Action]
    public void RearrangeEntities()
    {
        // 1) махаме старите мешове
        foreach (var e in Entities)
        {
            if (e.Entity != null)
            {
                EntityList.Remove(e.Entity);
                e.Entity = null;
            }
        }
        EntityListChanged?.Invoke(this, EventArgs.Empty);

        // 2) снимка на всички детайли
        var details = Entities.ToList();

        // 3) чистим Entities
        Entities.Clear();

        // 4) сортираме детайлите (примерно по обем)
        details.Sort((a, b) =>
        {
            double va = a.Width * a.Length * a.Height;
            double vb = b.Width * b.Length * b.Height;
            return vb.CompareTo(va);
        });

        // 5) за всеки детайл: настройваме текущите параметри на NPObject и ползваме AddEntity
        foreach (var d in details)
        {
            // настройваме входните параметри на AddEntity
            Width = d.Width;
            Length = d.Length;
            Height = d.Height;

            // тези две флага трябва да се копират в новите екземпляри
            NewDetailAllowRotation = d.AllowRotation;
            NewDetailCoatBothSides = d.CoatBothSides;

            // можеш да пробваш да запазиш и RailIndex като „предпочитан“ стартов релс:
            RailIndex = d.RailIndex;

            try
            {
                AddEntity();
            }
            catch (UserFriendlyException)
            {
                // ако няма място, можеш да решиш дали да го игнорираш или да го логнеш
            }

            NewDetailVerticalGapOverride = d.VerticalGapOverride;
        }
    }

    private void DrawComplexShapeFor(DSEntityObject item)
    {
        if (item.DetailType == 0)
            return;

        item.ComplexShape ??= ObjectSpace.CreateObject<DSComplexShape>();
        item.ComplexShape.ShapeType = item.DetailType;
        item.ComplexShape.ComplexEntities.Clear();

        var entities = CreateProfileShapeEntities(item);
        foreach (var ent in entities)
        {
            item.ComplexShape.ComplexEntities.Add(ent);
            // не ги добавяме в EntityList тук
        }
    }

    private Entity CreateComplexEntityFor(DSEntityObject item)
    {
        switch (item.DetailType)
        {
            case 1:
                return CreateProfileShape(item);
            case 2:
                return CreatePerforatedPlate(item);
            // ...
            default:
                return null;
        }
    }

    private Entity CreatePerforatedPlate(DSEntityObject item)
    {
        throw new NotImplementedException();
    }

    private Entity CreateProfileShape(DSEntityObject item)
    {
        string dwgPath = GetDwgPathForType(item.DetailType);
        if (string.IsNullOrEmpty(dwgPath) || !File.Exists(dwgPath))
            return null;

        var readFile = new devDept.Eyeshot.Translators.ReadDWG(dwgPath);
        readFile.DoWork();

        var importedEntities = readFile.Entities;
        if (importedEntities == null || importedEntities.Count == 0)
            return null;

        // взимаме първото подходящо entity
        Entity ent = (Entity)importedEntities[0].Clone();

        // МНОГО ВАЖНО: махаме DWG layer-а, за да не гърми за несъществуващ слой
        // ако имаш дефиниран слой "0" в design1, можеш да сложиш "0"
        ent.LayerName = string.Empty;

        // позиционираме във вътрешността на нашия детайл
        ent.Translate(item.StartX, item.StartY, item.StartZ);

        ent.ColorMethod = colorMethodType.byEntity;
        ent.Color = item.Color;

        return ent;
    }

    private IList<Entity> CreateProfileShapeEntities(DSEntityObject item)
    {
        string dwgPath = GetDwgPathForType(item.DetailType);
        if (string.IsNullOrEmpty(dwgPath) || !File.Exists(dwgPath))
            return Array.Empty<Entity>();

        var readFile = new devDept.Eyeshot.Translators.ReadDWG(dwgPath);
        readFile.DoWork();

        var src = readFile.Entities;
        if (src == null || src.Count == 0)
            return Array.Empty<Entity>();

        // 1) взимаме 2D профил – затворен LinearPath
        LinearPath profile2D = src
            .OfType<LinearPath>()
            .FirstOrDefault(lp => lp.IsClosed);

    if (profile2D == null)
        return Array.Empty<Entity>();

    // 2) клонираме профила и махаме слоя
    var profileClone = (LinearPath)profile2D.Clone();
    profileClone.LayerName = string.Empty;

    // 3) екструзираме по дължина на детайла -> получаваш 3D mesh
    double length = item.Length;

    var solid = profileClone.ExtrudeAsMesh(
        new devDept.Geometry.Vector3D(0, length, 0), // екструзия по Y
        0.0,Mesh.natureType.ColorPlain);

    // 4) завъртаме, за да влезе в твоята координатна система (както правеше с плоския профил)
    solid.Rotate(Math.PI / 2, new devDept.Geometry.Vector3D(1, 0, 0));

    // 5) позиционираме спрямо детайла
    solid.Translate(item.StartX, item.StartY, item.StartZ);

    solid.ColorMethod = colorMethodType.byEntity;
    solid.Color = item.Color;

    return new Entity[] { solid };
    }

    private string GetDwgPathForType(int detailType)
    {
        // примитивен mapping; по-добре да го изнесеш в конфигурация/таблица
        switch (detailType)
        {
            case 1:
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Profiles", "Type1.dwg");
            case 2:
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Profiles", "Type2.dwg");
            default:
                return null;
        }
    }
}