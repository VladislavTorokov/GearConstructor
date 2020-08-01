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
            KompasObject kompas = KompasBuilder.GetKompas();
            IApplication kompasApi7 = (IApplication)kompas.ksGetApplication7();
            ksDocument3D doc = (ksDocument3D)kompas.ActiveDocument3D();

            Gear gear = new Gear();
            gear.SetValue(int.Parse(textBoxZ.Text), float.Parse(textBoxM.Text), float.Parse(textBoxd.Text), float.Parse(textBoxbw.Text));

            KompasBuilder kompasBuilder = new KompasBuilder();
            kompasBuilder.CreateGearKompas(gear, kompas, doc);
        }
    }
}
