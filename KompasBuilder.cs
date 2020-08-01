using System;
using Kompas6API5;
using KompasAPI7;
using Kompas6Constants3D;
using System.Runtime.InteropServices;
using KAPITypes;
using reference = System.Int32;

namespace GearConsole
{
    public class KompasBuilder
    {
        //Получает экземпляр запущенного компаса
        public static KompasObject GetKompas()
        {
            KompasObject kompas = (KompasObject)GetApplicationObject("KOMPAS.Application.5");
            if (kompas != null) return kompas;
            throw new SystemException("Проблема подключения к Kompas!");
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
                    obj = (KompasObject)Activator.CreateInstance(Type.GetTypeFromProgID(progId)/*FBE002A6-1E06-4703-AEC5-9AD8A10FA1FA*/);
                    return obj;
                }
            }
            catch
            {
                return null;
            }
        }

        //зкапускает Компас
        public static KompasObject CreateKompas()
        {
            KompasObject kompas = (KompasObject)CreateApplicationObject("KOMPAS.Application.5");
            if (kompas != null) return kompas;
            throw new SystemException("Проблема запуска Kompas, возможно приложение не установлено!");
        }

        private static object CreateApplicationObject(string progId)
        {
            try
            {
                object obj = (KompasObject)Activator.CreateInstance(Type.GetTypeFromProgID(progId) /*Type.GetTypeFromProgID(progId)*/);
                return obj;
            }
            catch
            {
                return null;
            }
        }

        private void ClearCurrentSketch(ksDocument2D sketchEdit, KompasObject kompas)
        {
            // создадим итератор и удалим все существующие объекты в эскизе          
            ksIterator iter = (ksIterator)kompas.GetIterator();
            if (iter != null)
            {
                if (iter.ksCreateIterator(ldefin2d.ALL_OBJ, 0))
                {
                    reference rf;
                    if ((rf = iter.ksMoveIterator("F")) != 0)
                    {
                        // сместить указатель на первый элемент в списке
                        // в цикле сместить указатель на следующий элемент в списке пока не дойдем до последнего
                        do
                        {
                            if (sketchEdit.ksExistObj(rf) == 1)
                                sketchEdit.ksDeleteObj(rf); // если объект существует удалить его 
                        }
                        while ((rf = iter.ksMoveIterator("N")) != 0);
                    }
                    iter.ksDeleteIterator();    // удалим итератор
                }
            }
        }

        public void CreateGearKompas(Gear gear, KompasObject kompas, ksDocument3D doc)
        {
            if (kompas != null)
            {
                kompas.Visible = true;
                if (gear != null)
                {
                    kompas.ActivateControllerAPI();
                    if (doc == null || doc.reference == 0)
                    {
                        doc = (ksDocument3D)kompas.Document3D();
                        doc.Create(false, true);

                        //doc.author = "Torokov Vladislav Viktorovich";
                        //doc.comment = "3D Wheel";
                        //doc.UpdateDocumentParam();

                        //СОЗДАНИЕ ЦИЛИНДРА

                        /////////////////////////////////////////////

                        ksPart part = (ksPart)doc.GetPart((short)Part_Type.pTop_Part);  // новый компонент
                        if (part != null)
                        {
                            ksEntity basePlane = (ksEntity)part.GetDefaultEntity((short)Obj3dType.o3d_planeXOZ);
                            ksEntity entitySketch = (ksEntity)part.NewEntity((short)Obj3dType.o3d_sketch);
                            if (entitySketch != null)
                            {
                                ksSketchDefinition sketchDef = (ksSketchDefinition)entitySketch.GetDefinition();
                                if (sketchDef != null)
                                {
                                    sketchDef.SetPlane(basePlane); // установим плоскость
                                    entitySketch.Create();         // создадим эскиз

                                    // интерфейс редактора эскиза
                                    ksDocument2D sketchEdit = (ksDocument2D)sketchDef.BeginEdit();

                                    sketchEdit.ksLineSeg(gear.Lc/2, 0, -gear.Lc / 2, 0, 3);

                                    reference grp = sketchEdit.ksNewGroup(0);

                                    sketchEdit.ksLineSeg( 0, gear.d/2, -gear.Lc / 2, gear.d / 2, 1);
                                    sketchEdit.ksLineSeg(-gear.Lc / 2, gear.d / 2, -gear.Lc/2, gear.Dc / 2, 1);
                                    sketchEdit.ksLineSeg(-gear.Lc/2, gear.Dc / 2, -gear.ec / 2, gear.Dc / 2, 1);
                                    sketchEdit.ksLineSeg(-gear.ec / 2, gear.Dc / 2, -gear.ec / 2, gear.Ra - gear.A1, 1);
                                    //sketchEdit.ksArcByPoint(-gear.ec - gear.Rc, gear.Dc / 2 + gear.Rc, gear.Rc, -gear.ec - gear.Rc, gear.Dc / 2, -gear.ec, gear.Dc / 2 + gear.Rc, 1, 1);
                                    sketchEdit.ksLineSeg(-gear.ec / 2, gear.Ra - gear.A1, -gear.bw / 2, gear.Ra - gear.A1, 1);
                                    //sketchEdit.ksArcByPoint(-gear.ec - gear.Rc, gear.Dc / 2 + gear.Rc, gear.Rc, -gear.ec - gear.Rc, gear.Dc / 2, -gear.ec, gear.Dc / 2 + gear.Rc, 1, 1);
                                    sketchEdit.ksLineSeg(-gear.bw / 2, gear.Ra - gear.A1, -gear.bw / 2, gear.Ra - gear.C1, 1);
                                    sketchEdit.ksLineSeg(-gear.bw / 2, gear.Ra - gear.C1, -gear.bw / 2 + gear.C1, gear.Ra, 1);
                                    sketchEdit.ksLineSeg(-gear.bw / 2 + gear.C1, gear.Ra, 0, gear.Ra, 1);

                                    sketchEdit.ksEndGroup();
                                    sketchEdit.ksSymmetryObj(grp, 0, 0, 0, gear.Ra, "1");

                                    sketchDef.EndEdit();   // завершение редактирования эскиза

                                    //Приклеим Вращение
                                    ksEntity entityBossRotate = (ksEntity)part.NewEntity((short)Obj3dType.o3d_bossRotated);
                                    if (entityBossRotate != null)
                                    {
                                        ksBossRotatedDefinition bossRotateDef = (ksBossRotatedDefinition)entityBossRotate.GetDefinition();
                                        if (bossRotateDef != null)
                                        {
                                            bossRotateDef.directionType = (short)Direction_Type.dtNormal;
                                            bossRotateDef.SetSideParam(true, 360);
                                            bossRotateDef.SetSketch(entitySketch);     // эскиз операции вращения
                                            entityBossRotate.Create();                 // создать операцию
                                        }
                                    }
                                }
                            }

                            //СОЗДАНИЕ ЗУБЬЕВ

                            /////////////////////////////////////////////
                            ksEntity basePlane2 = (ksEntity)part.GetDefaultEntity((short)Obj3dType.o3d_planeYOZ);
                            ksEntity entitySketch2 = (ksEntity)part.NewEntity((short)Obj3dType.o3d_sketch);
                            if (entitySketch2 != null)
                            {
                                // интерфейс свойств эскиза
                                ksSketchDefinition sketchDef2 = (ksSketchDefinition)entitySketch2.GetDefinition();
                                if (sketchDef2 != null)
                                {
                                    sketchDef2.SetPlane(basePlane2); // установим плоскость
                                    entitySketch2.Create();         // создадим эскиз

                                    //СОЗДАНИЕ ЭСКИЗА ЗУБА

                                    /////////////////////////////////////////////

                                    // интерфейс редактора эскиза
                                    ksDocument2D sketchEdit2 = (ksDocument2D)sketchDef2.BeginEdit();

                                    //Создание профилей зуба
                                    reference grp = sketchEdit2.ksNewGroup(0);
                                    for (int i = 0; i < gear.accuracy - 1; i++)
                                        sketchEdit2.ksLineSeg((gear.teeths[i, 0]), (gear.teeths[i, 1]), (gear.teeths[i + 1, 0]), (gear.teeths[i + 1, 1]), 1);
                                    sketchEdit2.ksLineSeg(gear.teeths[gear.accuracy - 1, 0], gear.teeths[gear.accuracy - 1, 1], gear.realRadius * Math.Cos(-gear.stAngle / 2), gear.realRadius * Math.Sin(-gear.stAngle / 2), 1);
                                    sketchEdit2.ksEndGroup();
                                    sketchEdit2.ksSymmetryObj(grp, 0, 0, 100 * Math.Cos(-gear.stAngle / 2), 100 * Math.Sin(-gear.stAngle / 2), "1");

                                    //Создание дуги вершины зуба и дуги основания зуба
                                    sketchEdit2.ksArcByPoint(0, 0, gear.Rf, gear.Rf * Math.Cos(0), gear.Rf * Math.Sin(0), gear.Rf * Math.Cos(-gear.stAngle), gear.Rf * Math.Sin(-gear.stAngle), -1, 1);

                                    // завершение редактирования эскиза
                                    sketchDef2.EndEdit();


                                    //ВЫДАВЛИВАНИЕ ЗУБА

                                    /////////////////////////////////////////////

                                    //Приклеим выдавливанием

                                    // вырежим выдавливанием
                                    ksEntity entityCutExtr = (ksEntity)part.NewEntity((short)Obj3dType.o3d_cutExtrusion);
                                    if (entityCutExtr != null)
                                    {
                                        ksCutExtrusionDefinition cutExtrDef = (ksCutExtrusionDefinition)entityCutExtr.GetDefinition();
                                        if (cutExtrDef != null)
                                        {
                                            cutExtrDef.SetSketch(entitySketch2);    // установим эскиз операции
                                            cutExtrDef.directionType = (short)Direction_Type.dtBoth; //прямое направление
                                            cutExtrDef.SetSideParam(true, (short)End_Type.etBlind, gear.bw, 0, false);
                                            cutExtrDef.SetThinParam(false, 0, 0, 0);
                                        }

                                        entityCutExtr.Create(); // создадим операцию вырезание выдавливанием

                                        //СДЕЛАТЬ МАССИВ

                                        //Операция копирования по концентрической сетке

                                        /////////////////////////////////////////////

                                        //Получаем интерфейс объекта операции
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
                                                circularCopyDefinition.SetCopyParamAlongDir(gear.z1, gear.pAngle, false, false);

                                                //Получаем массив копируемых элементов
                                                ksEntityCollection EntityCollection = (ksEntityCollection)circularCopyDefinition.GetOperationArray();

                                                EntityCollection.Clear();

                                                //Заполняем массив копируемыъ элементов
                                                EntityCollection.Add(entityCutExtr);

                                                //Создаем операцию
                                                entityCircularCopy.Create();
                                            }
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
