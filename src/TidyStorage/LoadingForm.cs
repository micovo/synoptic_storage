using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TidyStorage
{
    public partial class LoadingForm : Form
    {
        bool allowedToClose;

        /// <summary>
        /// 
        /// </summary>
        public bool AllowedToClose
        {
            get
            {
                return allowedToClose;
            }

            set
            {
                allowedToClose = value;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public LoadingForm()
        {
            InitializeComponent();
            AllowedToClose = false;
        }

       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (AllowedToClose == false)
            {
                System.Media.SystemSounds.Beep.Play();
                e.Cancel = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        public void UpdateProgress(int v)
        {
            progressBar1.Invoke(new Action(() => { progressBar1.Value = v; }));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        public void UpdateLabel(string v)
        {
            label1.Invoke(new Action(() => { label1.Text = v; }));
        }
    }
}
