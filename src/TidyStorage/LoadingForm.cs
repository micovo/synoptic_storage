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
        /// Property that blocks user request to close this form.
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
        /// Constructor. Closing of this form is disable by default.
        /// </summary>
        public LoadingForm()
        {
            InitializeComponent();
            AllowedToClose = false;
        }
        
        /// <summary>
        /// Form closing event handler. 
        /// Form is closed only if it was allowed by the app beforehand.
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
        /// Progress bar update function.
        /// </summary>
        /// <param name="v"></param>
        public void UpdateProgress(int v)
        {
            if (v < progressBar1.Minimum) v = progressBar1.Minimum;
            if (v > progressBar1.Maximum) v = progressBar1.Maximum;
            progressBar1.Invoke(new Action(() => { progressBar1.Value = v; }));
        }
        
        /// <summary>
        /// Form message update function.
        /// </summary>
        /// <param name="v"></param>
        public void UpdateLabel(string v)
        {
            label1.Invoke(new Action(() => { label1.Text = v; }));
        }
        
        /// <summary>
        /// Set position of this form in the center of the requested form.
        /// </summary>
        /// <param name="form"></param>
        public void Center(Form form)
        {
            this.Left = form.Left + form.Width / 2 - this.Width / 2;
            this.Top = form.Top + form.Height / 2 - this.Height / 2;
        }

        /// <summary>
        /// Cancel button click event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            allowedToClose = true;
            this.Close();
        }
    }
}
