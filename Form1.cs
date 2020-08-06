using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kompas6API5;
using KompasAPI7;
using Kompas6Constants3D;
using System.Runtime.InteropServices;
using KAPITypes;

namespace GearConsole
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            KompasObject kompas = GearBuilder.GetKompas();
            ksDocument3D gearDocument = (ksDocument3D)kompas.ActiveDocument3D();

            GearBuilder gearBuilder = new GearBuilder(int.Parse(tbZ.Text), float.Parse(tbModule.Text), float.Parse(tbDiameter.Text), float.Parse(tbGearWidth.Text), 30);
            gearBuilder.CreateGearInKompas(kompas, gearDocument);
        }
    }
}
