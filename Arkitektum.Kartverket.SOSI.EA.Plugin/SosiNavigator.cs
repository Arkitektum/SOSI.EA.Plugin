using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Arkitektum.Kartverket.SOSI.Model;
using Arkitektum.Kartverket.SOSI.EA.Plugin.Services;


namespace Arkitektum.Kartverket.SOSI.EA.Plugin
{
    public partial class SosiNavigator : UserControl
    {
        //private BackgroundWorker bw = new BackgroundWorker();

        public SosiNavigator()
        {
            InitializeComponent();
            //bw.WorkerReportsProgress = true;
            //bw.WorkerSupportsCancellation = true;
            //bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            //bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            //bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);


        }

        //private void bw_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    BackgroundWorker worker = sender as BackgroundWorker;

        //    for (int i = 1; (i <= 10); i++)
        //    {
        //        if ((worker.CancellationPending == true))
        //        {
        //            e.Cancel = true;
        //            break;
        //        }
        //        else
        //        {
        //            // Perform a time consuming operation and report progress.
        //            System.Threading.Thread.Sleep(500);
        //            worker.ReportProgress((i * 10));
        //        }
        //    }
        //}
        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //if ((e.Cancelled == true))
            //{
            //    this.tbProgress.Text = "Canceled!";
            //}

            //else if (!(e.Error == null))
            //{
            //    this.tbProgress.Text = ("Error: " + e.Error.Message);
            //}

            //else
            //{
            //    this.tbProgress.Text = "Done!";
            //}
        }
        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //this.tbProgress.Text = (e.ProgressPercentage.ToString() + "%");
        }


        internal void setSearch(global::EA.Repository Repository, string GUID, global::EA.ObjectType ot)
        {

            //if (bw.IsBusy != true)
            //{
            //    bw.RunWorkerAsync();
            //}


            //


            if (chVisSosiObjekt.Checked)
            {
                if (ot == global::EA.ObjectType.otElement)
                {
                    global::EA.Element elm = Repository.GetElementByGuid(GUID);

                    if (elm.Stereotype.ToLower() == "featuretype" || elm.Stereotype.ToLower() == "type")
                    { 
                        Sosimodell modell = new Sosimodell();
                        var gen = new SOSIKontrollGenerator();
                        txtSosi.Text = gen.LagSosiObjekt(modell.LagObjekttype(Repository, elm, "..", true,null),true);
                    }


                        
                }
            }
            if (chSyntaks.Checked)
            {
                if (ot == global::EA.ObjectType.otElement)
                {
                    global::EA.Element elm = Repository.GetElementByGuid(GUID);

                    if (elm.Stereotype.ToLower() == "featuretype" || elm.Stereotype.ToLower() == "type")
                    {
                        Sosimodell modell = new Sosimodell();
                        Objekttype o = modell.LagObjekttype(Repository, elm, "..", false,null);
                        List<Basiselement> listBasis = new List<Basiselement>();
                        List<Gruppeelement> listGruppe = new List<Gruppeelement>();
                        var gen = new SOSIKontrollGenerator();

                        txtSosi.Text = gen.LagSosiSyntaks(o, listBasis, listGruppe);
                        txtSosi.Text = txtSosi.Text + gen.LagSosiSyntaksGrupper(listGruppe);
                        txtSosi.Text = txtSosi.Text + gen.LagSosiSyntaksBasiselementer(listBasis);

                    }



                }
            }
            
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void chSyntaks_CheckedChanged(object sender, EventArgs e)
        {
            if (chSyntaks.Checked) chVisSosiObjekt.Checked = false;
        }

        private void chVisSosiObjekt_CheckedChanged(object sender, EventArgs e)
        {
            if (chVisSosiObjekt.Checked) chSyntaks.Checked = false;
        }
    }
}
