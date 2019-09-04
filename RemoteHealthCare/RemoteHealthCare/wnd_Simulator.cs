using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RemoteHealthCare
{
    public partial class wnd_Simulator : Form
    {
        private byte distance = 0;

        private StationaryBike simulatingBike;
        public ref StationaryBike SimulatingBike => ref simulatingBike;

        private Timer sendMsgTimer = new Timer();
        private Timer speedModifierTimer = new Timer();

        public wnd_Simulator()
        {
            InitializeComponent();

            simulatingBike = new StationaryBike("SIM");

            Timer timer = new Timer()
            {
                Interval = 10
            };

            timer.Tick += OnTimerTick;
            timer.Start();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            distance++;
            if (distance == 255)
                distance = 0;

            simulatingBike.PushDataChange(new byte[] { 0xA4, 0x09, 0x4E, 0x05, 0x10, 0x19, 0x04, distance, 0x00, 0x00, 0xFF, 0x24, 0x30 });
        }
    }
}
