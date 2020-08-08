using System;
using System.Windows.Forms;

namespace GearConsole
{
    public class Gear
    {
        private const double pi = Math.PI;
        //Объявление переменных
        //Радиусы окружностей
        public double Ra { get; set; }        //Вершин
        public double Rf { get; set; }        //Впадин
        private double Rb;                    //Основной
        private double Rw;                    //Начальной
        private double R;                     //Делительной
        public double RealRadius { get; set; }//До последней точки профиля

        //Углы - angles
        private double angleProf;             //Профиля инструментальной рейки 
        private double workingPressureAngle;  //Зацепления
        private double angleSa;               //Профиля зубьев колес по окружностям вершин зубьев
        private double staAngle;              //Толщины вершины зуба в радианах
        private double angleForArc;           //До вершины второго профиля
        public double StAngle { get; set; }   //Толщины делительной окружности зуба в радианах
        public double pAngle { get; set; }    //Шага

        //Толщина
        private double st;                    //Зубца по делительной окружности
        private double sta;                   //Зуба по окружности вершин

        public double[,] Teeths { get; set; } //Массив для координат профиля зуба
        private double p;                     //Шаг зубчатого зацепления
        public int accuracy { get; }          //Точность в построении профиля зуба

        //Входные параметры
        readonly double m;                    //Модуль зубчатого колеса
        public int Z1 { get; }                //Количество зубцов Шестерни
        public float d { get; }               //Диаметр посадочного отверстия
        public float Bw { get; }              //Ширина зубчатого венца

        //Остальные параметры зубчатого колеса
        public float Dc { get; }              //Диаметр ступицы
        private float D0;                     //Диаметр центровой окружности
        public float Lc { get; }              //Длина ступицы
        public float A1 { get; }              //Толщина обода
        private float d0;                     //Диаметр отверстий
        public float ec { get; }              //Толщина диска
        public float C1 { get; }              //Фаска

        public Gear(int z1, float m, float d, float bw, int accuracy)
        {
            this.accuracy = accuracy;
            if (z1 < 6 || z1 > 100)
                Console.WriteLine("incorrect number input/ntry writing a number from 10 to 100 for z1 or z2");
            else
            {
                this.Z1 = z1;
                this.m = m;
                this.d = d;
                this.Bw = bw;
                Teeths = new double[accuracy, 2];
                Dc = 1.55f * d;
                Lc = 1.2f * d;
                if (Lc < Bw)
                {
                    MessageBox.Show("Data entered incorrectly, change 'd' or 'bw'");
                    return;
                }
                A1 = (float)m * 5;
                ec = 0.3f * Bw;
                D0 = 0.5f * ((float)(Ra * 2) - 2 * A1 + Dc);
                d0 = 0.25f * ((float)(Ra * 2) - 2 * A1 - Dc);
                C1 = 1;
                CreateGear();
            }
        }

        //Расчет начальных параметров
        private void CalculateParameters()
        {
            //Переводим углы в радианы
            angleProf = AngleCalculator.AngleToRadian(20);
            workingPressureAngle = angleProf;
            p = pi * m;
            st = (pi * m) / 2;
            //Расчет радиусов окружностей
            R = (Z1 * m) / 2;
            Ra = R + m;
            Rf = R - 1.25 * m;
            Rb = R * Math.Cos(angleProf);
            Rw = R;
            //Углы профилей зубьев колес по окружностям вершин зубьев и их инвалюты,Расчет толщины зуба окр. выступов
            angleSa = Math.Acos(Rb / Ra);
            sta = 2 * Ra * ((st / (2 * R)) + AngleCalculator.InvoluteValue(angleProf) - AngleCalculator.InvoluteValue(angleSa));
            staAngle = sta / Ra;
            StAngle = st / R;
            pAngle = (p / R) * (180 / pi);
        }

        //Расчет координат профиля зуба
        public void CalculateProfileTeeth()
        {
            double angleBegin = angleProf - (2 * Math.Asin((Rb * Math.Tan(angleProf)) / (2 * Rb)));
            double angleEnd = Math.Sqrt((Ra + Rb) * (Ra - Rb)) / Rb;
            double currentAngle = angleBegin;
            int numberOfPointProfile = 1;
            double stepAngle = (angleEnd - angleBegin) / (accuracy - 3);
            double angleToStop = angleEnd + ((angleEnd - angleBegin) / (accuracy - 3));
            Teeths[0, 0] = Rf * Math.Cos(0);
            Teeths[0, 1] = Rf * Math.Sin(0);
            while (currentAngle < angleToStop)
            {
                if (numberOfPointProfile == accuracy)
                    break;
                else
                {
                    currentAngle += stepAngle;
                    Teeths[numberOfPointProfile, 0] = Rb * (Math.Cos(currentAngle) + currentAngle * Math.Sin(currentAngle));
                    Teeths[numberOfPointProfile, 1] = (Rb * (Math.Sin(currentAngle) - Math.Abs(currentAngle) * Math.Cos(currentAngle))) + Math.Sin(angleBegin);
                    numberOfPointProfile++;
                }
            }
            angleForArc = (StAngle / 2) + ((StAngle / 2) - Math.Atan(Teeths[accuracy - 1, 1] / Teeths[accuracy - 1, 0]));                                            //Угол до вершины второго профиля
            RealRadius = Math.Sqrt((Teeths[accuracy - 1, 0] * Teeths[accuracy - 1, 0]) + (Teeths[accuracy - 1, 1] * Teeths[accuracy - 1, 1]));                       //Радиус до последней точки профиля
        }

        //Построить зубчатое шестерню(колесо)
        public void CreateGear()
        {
            CalculateParameters();
            CalculateProfileTeeth();
        }
    }
}