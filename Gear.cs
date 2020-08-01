using System;
using System.Windows.Forms;

namespace GearConsole
{
    public class Gear
    {
        private const double pi = Math.PI;
        //Объявление переменных
        public double Ra;                     //Окружность вершин
        public double Rf;                     //Окружность впадин
        private double Rb;                    //Основная окружность
        private double Rw;                    //Начальная окружность
        private double R;                     //Делительная окружность
        private double L;                     //Межосевое расстояние
        private double u;                     //Передаточное число
        private double angleProf;             //Угол профиля инструментальной рейки 
        private double workingPressureAngle;  //Угол зацепления
        private double angleSa;               //Угол профиля зубьев колес по окружностям вершин зубьев
        public double stAngle;                //Угол толщины делительной окружности зуба в радианах
        private double staAngle;              //Угол толщины вершины зуба в радианах
        private double angleForArc;           //Угол до вершины второго профиля
        public double realRadius;             //Радиус до последней точки профиля
        public double[,] teeths;              //Массив для координат профиля зуба
        private double h;                     //Высота зубца
        private double st;                    //Толщина зубца по делительной окружности
        private double sta;                   //Толщина зуба по окружности вершин
        private double e;                     //Ширина впадины
        private double p;                     //Шаг зубчатого зацепления
        public double pAngle;                 //Угол шага
        public int accuracy = 30;             //Точность в построении профиля зуба

        //Входящие параметры
        private double m;                     //Модуль зубчатого колеса
        public int z1;                        //Количество зубцов Шестерни
        public float d;                       //Диаметр посадочного отверстия
        public float bw;                      //Ширина зубчатого венца

        //Параметры зубчатого КОЛЕСА
        public float Dc;                      //Диаметр ступицы
        private float D0;                     //Диаметр центровой окружности
        public float Lc;                      //Длина ступицы
        public float A1;                      //Толщина обода
        private float d0;                     //Диаметр отверстий
        public float ec;                      //Толщина диска
        public float C1;                      //Фаска
        private float C2;                     //Фаска
        private float C3;                     //Фаска
        private float Rc;                     //Радиус скругления

        public Gear()
        {
        }

        public void SetValue(int z1, float m, float d, float bw)
        {
            if (z1 < 6 || z1 > 100)
                Console.WriteLine("incorrect number input/ntry writing a number from 10 to 100 for z1 or z2");
            else
            {
                this.z1 = z1;
                this.m = m;
                this.d = d;
                this.bw = bw;
                teeths = new double[accuracy, 2];
                CreateGear();
            }
        }

        //Расчет начальных параметров
        private void Calculate()
        {
            //Переводим углы в радианы
            angleProf = AngleToRadian(20);
            workingPressureAngle = angleProf;
            p = pi * m;
        }

        public virtual void CalculateRemaningParameters()
        {
            st = (pi * m) / 2;
            //Расчет радиусов окружностей
            R = (z1 * m) / 2;
            Ra = R + m;
            Rf = R - 1.25 * m;
            Rb = R * Math.Cos(angleProf);
            Rw = R;
            //Углы профилей зубьев колес по окружностям вершин зубьев и их инвалюты,Расчет толщины зуба окр. выступов
            angleSa = Math.Acos(Rb / Ra);
            sta = 2 * Ra * ((st / (2 * R)) + InvoluteValue(angleProf) - InvoluteValue(angleSa));
            staAngle = sta / Ra;
            stAngle = st / R;
            pAngle = (p / R) * (180 / pi);
        }

        public void CalculateDrivenGear()
        {
            Dc = 1.55f * d;
            Lc = 1.2f * d;
            if (Lc < bw)
            {
                MessageBox.Show("Данные введены некорректно, поменяйте значение d либо bw");
                return;
            }
            A1 = (float)m * 5;
            ec = 0.3f * bw;
            D0 = 0.5f * ((float)(Ra * 2) - 2 * A1 + Dc);
            d0 = 0.25f * ((float)(Ra * 2) - 2 * A1 - Dc);
            C1 = 1/*0.5f * (float)m*/;
            C2 = 2;
            Rc = 4;

            if (d <= 30)
                C3 = 1.0f;
            if (30 <= d || d <= 50) 
                C3 = 1.6f;
            if (50 <= d || d <= 80)
                C3 = 2.0f;
            if (80 <= d || d <= 120)
                C3 = 2.5f;
            if (120 <= d || d <= 150)
                C3 = 3.0f;
            if (150 <= d || d <= 250)
                C3 = 4.0f;
            if (250 <= d || d <= 500)
                C3 = 5.0f;
            if(d>500)
                MessageBox.Show("Данные введены некорректно, поменяйте значение d, которое будет меньше либо равно значению 500");
        }

        //Расчет координат профиля зуба
        public virtual void GetCoordinatesFirstProfile()
        {
            double startAngle = angleProf - (2 * Math.Asin((Rb * Math.Tan(angleProf)) / (2 * Rb)));
            double finnalyAngle = Math.Sqrt((Ra + Rb) * (Ra - Rb)) / Rb;
            double currentAngle = startAngle;
            int i = 1;
            teeths[0, 0] = Rf * Math.Cos(0);
            teeths[0, 1] = Rf * Math.Sin(0);
            double stepAngle = ((finnalyAngle - startAngle) / (accuracy - 3));
            while (currentAngle < (finnalyAngle + ((finnalyAngle - startAngle) / (accuracy - 3))))
            {
                if (i == accuracy)
                    break;
                else
                {
                    currentAngle += stepAngle;
                    teeths[i, 0] = Rb * (Math.Cos(currentAngle) + currentAngle * Math.Sin(currentAngle));
                    teeths[i, 1] = (Rb * (Math.Sin(currentAngle) - Math.Abs(currentAngle) * Math.Cos(currentAngle))) + Math.Sin(startAngle);
                    i++;
                }
            }
            angleForArc = (stAngle / 2) + ((stAngle / 2) - Math.Atan(teeths[accuracy - 1, 1] / teeths[accuracy - 1, 0]));                                            //Угол до вершины второго профиля
            realRadius = Math.Sqrt((teeths[accuracy - 1, 0] * teeths[accuracy - 1, 0]) + (teeths[accuracy - 1, 1] * teeths[accuracy - 1, 1]));                       //Радиус до последней точки профиля
        }



        //Рассчет функции инвалюты
        public double InvoluteAngle(double inv)
        {
            return (Math.Pow(3 * inv, 1 / 3.0) - (2 * inv) / 5.0 + (9 / 175.0) * Math.Pow(3, 2 / 3.0) * Math.Pow(inv, 5 / 3.0) - (2 / 175.0) * Math.Pow(3, 1 / 3.0) * Math.Pow(inv, 7 / 3.0) - (144 / 67375.0) * Math.Pow(inv, 3) + (3258 / 3128125.0) * Math.Pow(3, 2 / 3.0) * Math.Pow(inv, 11 / 3.0) - (49711 / 153278125.0) * Math.Pow(3, 1 / 3.0) * Math.Pow(inv, 13 / 3.0));
        }

        public double InvoluteValue(double angle)
        {
            return (Math.Tan(angle) - angle);
        }

        private double AngleToRadian(double angle)
        {
            return (angle * pi) / 180.0;
        }

        private double GetFinalAngle()
        {
            double Q = Math.Sqrt(Rb + Ra);
            return Math.Acos(((Q * Math.Sin(Q / Rb)) / Ra) + ((Rb * Math.Cos(Q / Rb)) / Ra));
        }

        //Построить зубчатое шестерню(колесо)
        public void CreateGear()
        {
            Calculate();
            CalculateRemaningParameters();
            GetCoordinatesFirstProfile();
            CalculateDrivenGear();
        }
    }
}