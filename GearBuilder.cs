using System;
using Kompas6API5;
using KompasAPI7;
using Kompas6Constants3D;
using System.Runtime.InteropServices;
using KAPITypes;
using reference = System.Int32;

namespace GearConsole
{
    public class GearBuilder
    {
        private Gear gear { get; }

        public GearBuilder(int z1, float m, float d, float bw, int accuracy)
        {
            gear = new Gear(z1, m, d, bw, accuracy);
        }
        //Получает экземпляр запущенного компаса
        public static KompasObject GetKompas()
        {
            KompasObject kompas = (KompasObject)GetApplicationObject("KOMPAS.Application.5");
            if (kompas != null) return kompas;
            throw new SystemException("Kompas connection problem!");

        }

        private static object GetApplicationObject(string progId)
        {
            try
            {
                object obj = null;
                try
                {
                    obj = Marshal.GetActiveObject(progId);
                    return obj;
                }
                catch
                {
                    obj = (KompasObject)Activator.CreateInstance(Type.GetTypeFromProgID(progId));
                    return obj;
                }
            }
            catch
            {
                return null;
            }
        }

        public static KompasObject CreateKompas()
        {
            KompasObject kompas = (KompasObject)CreateApplicationObject("KOMPAS.Application.5");
            if (kompas != null) return kompas;
            throw new SystemException("Kompas launch problem, the application may not be installed!");
        }

        private static object CreateApplicationObject(string progId)
        {
            try
            {
                object obj = (KompasObject)Activator.CreateInstance(Type.GetTypeFromProgID(progId));
                return obj;
            }
            catch
            {
                return null;
            }
        }

        public void CreateGearInKompas(KompasObject kompas, ksDocument3D gearDocument)
        {
            if ((kompas != null) && (gear != null))
            {
                kompas.Visible = true;
                kompas.ActivateControllerAPI();
                if (gearDocument == null || gearDocument.reference == 0)
                {
                    gearDocument = (ksDocument3D)kompas.Document3D();
                    gearDocument.Create(false, true);

                    ksPart part = (ksPart)gearDocument.GetPart((short)Part_Type.pTop_Part);  // новый компонент
                    if (part != null)
                    {
                        //Создание цилиндра
                        ksEntity PlaneXOZ = (ksEntity)part.GetDefaultEntity((short)Obj3dType.o3d_planeXOZ);
                        ksEntity SketchForCylinder = (ksEntity)part.NewEntity((short)Obj3dType.o3d_sketch);
                        CreateSketchForRotate(SketchForCylinder, PlaneXOZ);
                        Rotate(part, SketchForCylinder);

                        //Вырезание зубьев
                        ksEntity PlaneYOZ = (ksEntity)part.GetDefaultEntity((short)Obj3dType.o3d_planeYOZ);
                        ksEntity toothSketch = (ksEntity)part.NewEntity((short)Obj3dType.o3d_sketch);
                        CreateToothSketch(toothSketch, PlaneYOZ);

                        //Вырез выдавливанием, Cut - операция выдавливания
                        var сutExtr = Cut(part, toothSketch);

                        //Операция копирования по концентрической сетке
                        CircularCopy(part, сutExtr);  //Передаем в копирование операцию выдавливания
                    }
                }
            }
        }

        private void CreateSketchForRotate(ksEntity sketch, ksEntity basePlane)
        {
            if (sketch != null)
            {
                ksSketchDefinition sketchDef = (ksSketchDefinition)sketch.GetDefinition();
                if (sketchDef != null)
                {
                    sketchDef.SetPlane(basePlane); // установим плоскость
                    sketch.Create();         // создадим эскиз

                    // интерфейс редактора эскиза
                    ksDocument2D sketchEdit = (ksDocument2D)sketchDef.BeginEdit();

                    sketchEdit.ksLineSeg(gear.Lc / 2, 0, -gear.Lc / 2, 0, 3);

                    reference grp = sketchEdit.ksNewGroup(0);

                    sketchEdit.ksLineSeg(0, gear.d / 2, -gear.Lc / 2, gear.d / 2, 1);
                    sketchEdit.ksLineSeg(-gear.Lc / 2, gear.d / 2, -gear.Lc / 2, gear.Dc / 2, 1);
                    sketchEdit.ksLineSeg(-gear.Lc / 2, gear.Dc / 2, -gear.ec / 2, gear.Dc / 2, 1);
                    sketchEdit.ksLineSeg(-gear.ec / 2, gear.Dc / 2, -gear.ec / 2, gear.Ra - gear.A1, 1);
                    sketchEdit.ksLineSeg(-gear.ec / 2, gear.Ra - gear.A1, -gear.Bw / 2, gear.Ra - gear.A1, 1);
                    sketchEdit.ksLineSeg(-gear.Bw / 2, gear.Ra - gear.A1, -gear.Bw / 2, gear.Ra - gear.C1, 1);
                    sketchEdit.ksLineSeg(-gear.Bw / 2, gear.Ra - gear.C1, -gear.Bw / 2 + gear.C1, gear.Ra, 1);
                    sketchEdit.ksLineSeg(-gear.Bw / 2 + gear.C1, gear.Ra, 0, gear.Ra, 1);

                    sketchEdit.ksEndGroup();
                    sketchEdit.ksSymmetryObj(grp, 0, 0, 0, gear.Ra, "1");

                    sketchDef.EndEdit();   // завершение редактирования эскиза
                }
            }
        }

        private void Rotate(ksPart part, ksEntity SktechForCylinder)
        {
            ksEntity entityBossRotate = (ksEntity)part.NewEntity((short)Obj3dType.o3d_bossRotated);
            if (entityBossRotate != null)
            {
                ksBossRotatedDefinition bossRotateDef = (ksBossRotatedDefinition)entityBossRotate.GetDefinition();
                if (bossRotateDef != null)
                {
                    bossRotateDef.directionType = (short)Direction_Type.dtNormal;
                    bossRotateDef.SetSideParam(true, 360);
                    bossRotateDef.SetSketch(SktechForCylinder);     // эскиз операции вращения
                    entityBossRotate.Create();                      // создать операцию
                }
            }
        }

        private void CreateToothSketch(ksEntity sketch, ksEntity basePlane)
        {
            if (sketch != null)
            {
                // интерфейс свойств эскиза
                ksSketchDefinition sketchDef2 = (ksSketchDefinition)sketch.GetDefinition();
                if (sketchDef2 != null)
                {
                    sketchDef2.SetPlane(basePlane); // установим плоскость
                    sketch.Create();         // создадим эскиз

                    //СОЗДАНИЕ ЭСКИЗА ЗУБА
                    // интерфейс редактора эскиза
                    ksDocument2D sketchEdit2 = (ksDocument2D)sketchDef2.BeginEdit();

                    //Создание профилей зуба
                    reference grp = sketchEdit2.ksNewGroup(0);
                    for (int i = 0; i < gear.accuracy - 1; i++)
                        sketchEdit2.ksLineSeg((gear.Teeths[i, 0]), (gear.Teeths[i, 1]), (gear.Teeths[i + 1, 0]), (gear.Teeths[i + 1, 1]), 1);
                    sketchEdit2.ksLineSeg(gear.Teeths[gear.accuracy - 1, 0], gear.Teeths[gear.accuracy - 1, 1], gear.RealRadius * Math.Cos(-gear.StAngle / 2), gear.RealRadius * Math.Sin(-gear.StAngle / 2), 1);
                    sketchEdit2.ksEndGroup();
                    sketchEdit2.ksSymmetryObj(grp, 0, 0, 100 * Math.Cos(-gear.StAngle / 2), 100 * Math.Sin(-gear.StAngle / 2), "1");

                    //Создание дуги вершины зуба и дуги основания зуба
                    sketchEdit2.ksArcByPoint(0, 0, gear.Rf, gear.Rf * Math.Cos(0), gear.Rf * Math.Sin(0), gear.Rf * Math.Cos(-gear.StAngle), gear.Rf * Math.Sin(-gear.StAngle), -1, 1);

                    // завершение редактирования эскиза
                    sketchDef2.EndEdit();
                }
            }
        }

        private ksEntity Cut(ksPart part, ksEntity sketch)
        {
            ksEntity entityCutExtr = (ksEntity)part.NewEntity((short)Obj3dType.o3d_cutExtrusion);
            if (entityCutExtr != null)
            {
                ksCutExtrusionDefinition cutExtrDef = (ksCutExtrusionDefinition)entityCutExtr.GetDefinition();
                if (cutExtrDef != null)
                {
                    cutExtrDef.SetSketch(sketch);    // установим эскиз операции
                    cutExtrDef.directionType = (short)Direction_Type.dtBoth; //прямое направление
                    cutExtrDef.SetSideParam(true, (short)End_Type.etBlind, gear.Bw, 0, false);
                    cutExtrDef.SetThinParam(false, 0, 0, 0);
                }

                entityCutExtr.Create();
                return entityCutExtr;
            }
            return null;
        }

        private void CircularCopy(ksPart part, ksEntity operation)
        {
            ksEntity entityCircularCopy = (ksEntity)part.NewEntity((short)Obj3dType.o3d_circularCopy);
            if (entityCircularCopy != null)
            {

                //Получаем интерфейс параметрова операции
                ksCircularCopyDefinition circularCopyDefinition = (ksCircularCopyDefinition)entityCircularCopy.GetDefinition();
                if (circularCopyDefinition != null)
                {

                    //Количество копий в радиальном направлении
                    circularCopyDefinition.count1 = 1;

                    //Устанавливаем ось операции
                    circularCopyDefinition.SetAxis(part.GetDefaultEntity((short)Obj3dType.o3d_axisOX));

                    //Устанавливаем параметры копирования
                    circularCopyDefinition.SetCopyParamAlongDir(gear.Z1, gear.pAngle, false, false);

                    //Получаем массив копируемых элементов
                    ksEntityCollection EntityCollection = (ksEntityCollection)circularCopyDefinition.GetOperationArray();

                    EntityCollection.Clear();

                    //Заполняем массив копируемых элементов
                    EntityCollection.Add(operation);

                    //Создаем операцию
                    entityCircularCopy.Create();
                }
            }
        }
    }
}
