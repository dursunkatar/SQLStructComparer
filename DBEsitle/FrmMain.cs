using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DBEsitle
{
    public partial class FrmMain : Form
    {
        private List<Table> kaynakTablolar;
        private List<Table> hedefTablolar;
        private List<View> kaynakViewlar;
        private List<View> hedefViewlar;
        private List<ProcFunc> kaynakPF;
        private List<ProcFunc> hedefPF;
        private DbServer kaynakServer;
        private DbServer hedefServer;

        Point ilkkonum;
        bool durum = false;

        public FrmMain()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;

            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(this.btnTabloTumunuSec, "Tümünü Seç");
            toolTip.SetToolTip(this.btnKolonTumunuSec, "Tümünü Seç");
            toolTip.SetToolTip(this.btnPFTumunuSec, "Tümünü Seç");
            toolTip.SetToolTip(this.btnViewTumunuSec, "Tümünü Seç");
            toolTip.SetToolTip(this.btnTabloAktar, "Seçilenleri Aktar");
            toolTip.SetToolTip(this.btnKolonAktar, "Seçilenleri Aktar");
            toolTip.SetToolTip(this.btnPFAktar, "Seçilenleri Aktar");
            toolTip.SetToolTip(this.btnViewAktar, "Seçilenleri Aktar");

            kaynakServer = new DbServer();
            hedefServer = new DbServer();

            cboKaynakAuthType.SelectedIndex = 0;
            cboHedefAuthType.SelectedIndex = 0;
        }


        private void btnKaynakDbBaglan_Click(object sender, EventArgs e)
        {

            if (txtKaynakDbIpAdresi.Text.Trim() == "")
                return;

            btnKaynakDbBaglan.Enabled = false;
            cboKaynakDbDatabaseler.Text = "";

            kaynakServer.Ip = txtKaynakDbIpAdresi.Text;
            kaynakServer.Username = txtKaynakDbKullaniciAdi.Text;
            kaynakServer.Password = txtKaynakDbKullaniciSifre.Text;
            kaynakServer.LoginSecure = cboKaynakAuthType.SelectedIndex == 0;

            Task.Run(() =>
            {
                lblKaynakBaglantiDurum.Text = "Bağlanıyor...";
                lblKaynakBaglantiDurum.ForeColor = Color.Black;

                if (DBUtilities.BaglantiTest(kaynakServer, cboKaynakDbDatabaseler))
                {
                    lblKaynakBaglantiDurum.ForeColor = Color.Green;
                    lblKaynakBaglantiDurum.Text = "Bağlandı";
                }
                else
                {
                    lblKaynakBaglantiDurum.ForeColor = Color.Red;
                    lblKaynakBaglantiDurum.Text = "Bağlanamadı !";
                }
                btnKaynakDbBaglan.Enabled = true;
            });
        }

        private void btnKarsilastir_Click(object sender, EventArgs e)
        {
            if (cboKaynakDbDatabaseler.Text == "" || cboHedefDbDatabaseler.Text == "")
            {
                MessageBox.Show("Karşılaştırma yapmak için database seçmelisin", "Ben");
                return;
            }
            clear();
            karsilastir();
        }

        private void karsilastir()
        {
            panelSol.Enabled = false;
            panelSag.Enabled = false;
            Task.Run(() =>
            {
                tabloVeKolonlariGetir();
                tablolariKarsilastir();
                kolonlariKarsilastir();

                viewlariGetir();
                viewlariKarsilastir();

                pfleriGetir();
                pfleriKarsilastir();

                panelSol.Enabled = true;
                panelSag.Enabled = true;
            });
        }

        private void viewlariGetir()
        {
            if (kaynakViewlar != null)
            {
                kaynakViewlar.Clear();
                kaynakViewlar = null;
            }

            if (hedefViewlar != null)
            {
                hedefViewlar.Clear();
                hedefViewlar = null;
            }
            kaynakViewlar = DBUtilities.ViewlariGetir(kaynakServer);
            hedefViewlar = DBUtilities.ViewlariGetir(hedefServer);
        }
        private void pfleriGetir()
        {

            if (kaynakPF != null)
            {
                kaynakPF.Clear();
                kaynakPF = null;
            }

            if (hedefPF != null)
            {
                hedefPF.Clear();
                hedefPF = null;
            }
            kaynakPF = DBUtilities.ProsodurVeFonksiyonlariGetir(kaynakServer);
            hedefPF = DBUtilities.ProsodurVeFonksiyonlariGetir(hedefServer);
        }
        private void tabloVeKolonlariGetir()
        {
            if (kaynakTablolar != null)
            {
                kaynakTablolar.Clear();
                kaynakTablolar = null;
            }

            if (hedefTablolar != null)
            {
                hedefTablolar.Clear();
                kaynakTablolar = null;
            }
            kaynakTablolar = DBUtilities.TabloVeKolonlariGetir(kaynakServer);
            hedefTablolar = DBUtilities.TabloVeKolonlariGetir(hedefServer);
        }
        private void tablolariKarsilastir()
        {
            listViewTablolar.Items.Clear();
            for (int i = 0; i < kaynakTablolar.Count; i++)
            {
                if (!hedefTablolar.Any(m => m.Name == kaynakTablolar[i].Name))
                {
                    var lvi = new ListViewItem();
                    lvi.Text = kaynakTablolar[i].Name;
                    lvi.Tag = i;
                    listViewTablolar.Items.Add(lvi);
                }
            }
        }

        private void kolonlariKarsilastir()
        {
            listViewKolonlar.Items.Clear();
            hedefTablolar.ForEach(hedefTablo =>
            {
                var _kaynakTablo = kaynakTablolar.FirstOrDefault(m => m.Name == hedefTablo.Name);
                if (_kaynakTablo != null)
                    _kaynakTablo.Columns.ForEach(kolon =>
                    {
                        if (!hedefTablo.Columns.Any(m => m.Name == kolon.Name))
                        {
                            var lvi = new ListViewItem();
                            lvi.Text = _kaynakTablo.Name;
                            lvi.SubItems.Add(kolon.Name);
                            lvi.SubItems.Add(kolon.Type);
                            listViewKolonlar.Items.Add(lvi);
                        }
                    });
            });
        }

        private void viewlariKarsilastir()
        {
            listViewViewlar.Items.Clear();

            for (int i = 0; i < kaynakViewlar.Count; i++)
            {
                if (!hedefViewlar.Any(hView => hView.Name == kaynakViewlar[i].Name))
                {
                    var lvi = new ListViewItem();
                    lvi.Tag = i;
                    lvi.Text = kaynakViewlar[i].Name;
                    listViewViewlar.Items.Add(lvi);
                }
            }
        }

        private void pfleriKarsilastir()
        {
            listViewPF.Items.Clear();

            for (int i = 0; i < kaynakPF.Count; i++)
            {
                if (!hedefPF.Any(hPf => hPf.Name == kaynakPF[i].Name))
                {
                    var lvi = new ListViewItem();
                    lvi.Text = kaynakPF[i].Name;
                    lvi.Tag = i;
                    listViewPF.Items.Add(lvi);
                }
            }
        }


        private void btnHedefDbBaglan_Click(object sender, EventArgs e)
        {

            if (txtHedefDbIpAdresi.Text.Trim() == "")
                return;

            btnHedefDbBaglan.Enabled = false;
            cboHedefDbDatabaseler.Text = "";

            hedefServer.Ip = txtHedefDbIpAdresi.Text;
            hedefServer.Username = txtHedefDbKullaniciAdi.Text;
            hedefServer.Password = txtHedefDbKullaniciSifre.Text;
            hedefServer.LoginSecure = cboHedefAuthType.SelectedIndex == 0;

            Task.Run(() =>
            {
                lblHedefBaglantiDurum.Text = "Bağlanıyor...";
                lblHedefBaglantiDurum.ForeColor = Color.Black;

                if (DBUtilities.BaglantiTest(hedefServer, cboHedefDbDatabaseler))
                {
                    lblHedefBaglantiDurum.ForeColor = Color.Green;
                    lblHedefBaglantiDurum.Text = "Bağlandı";
                }
                else
                {
                    lblHedefBaglantiDurum.ForeColor = Color.Red;
                    lblHedefBaglantiDurum.Text = "Bağlanamadı !";
                }

                btnHedefDbBaglan.Enabled = true;
            });
        }

        private void clear()
        {
            if (kaynakTablolar != null)
            {
                kaynakTablolar.Clear();
                kaynakTablolar = null;
            }

            if (hedefTablolar != null)
            {
                hedefTablolar.Clear();
                kaynakTablolar = null;
            }

            if (kaynakViewlar != null)
            {
                kaynakViewlar.Clear();
                kaynakViewlar = null;
            }

            if (hedefViewlar != null)
            {
                hedefViewlar.Clear();
                hedefViewlar = null;
            }

            if (kaynakPF != null)
            {
                kaynakPF.Clear();
                kaynakPF = null;
            }

            if (hedefPF != null)
            {
                hedefPF.Clear();
                hedefPF = null;
            }
        }

        private void LblBaslik_MouseDown(object sender, MouseEventArgs e)
        {
            durum = true;
            this.Cursor = Cursors.SizeAll;
            ilkkonum = e.Location;
        }

        private void LblBaslik_MouseMove(object sender, MouseEventArgs e)
        {
            if (durum)
            {
                this.Left = e.X + this.Left - (ilkkonum.X);
                this.Top = e.Y + this.Top - (ilkkonum.Y);
            }
        }

        private void LblBaslik_MouseUp(object sender, MouseEventArgs e)
        {
            durum = false;
            this.Cursor = Cursors.Default;
        }

        private void LblFormuLapat_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void LblFormuKapat_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void BtnKolonAktar_Click(object sender, EventArgs e)
        {
            if (listViewKolonlar.Items.Count == 0
              || cboHedefDbDatabaseler.Text == ""
              || cboKaynakDbDatabaseler.Text == "")
            {
                return;
            }

            panelSol.Enabled = false;
            panelKolon.Enabled = false;
            Task.Run(() =>
            {
                var sqlCodes = new List<string>();
                foreach (ListViewItem item in listViewKolonlar.CheckedItems)
                {
                    sqlCodes.Add($"ALTER TABLE {item.Text} ADD [{item.SubItems[1].Text}] {item.SubItems[2].Text}; ");
                }
                Exec(sqlCodes);
                tabloVeKolonlariGetir();
                kolonlariKarsilastir();
                panelSol.Enabled = true;
                panelKolon.Enabled = true;
            });
        }

        private void BtnPFAktar_Click(object sender, EventArgs e)
        {
            if (listViewPF.Items.Count == 0
            || cboHedefDbDatabaseler.Text == ""
            || cboKaynakDbDatabaseler.Text == "")
            {
                return;
            }

            panelPF.Enabled = false;
            panelSol.Enabled = false;
            Task.Run(() =>
            {
                var sqlCodes = new List<string>();
                foreach (ListViewItem item in listViewPF.CheckedItems)
                {
                    sqlCodes.Add(kaynakPF[int.Parse(item.Tag.ToString())].SqlCreate);
                }

                Exec(sqlCodes);
                pfleriGetir();
                pfleriKarsilastir();
                panelPF.Enabled = true;
                panelSol.Enabled = true;
            });
        }

        private void BtnViewAktar_Click(object sender, EventArgs e)
        {

            if (listViewViewlar.Items.Count == 0
                  || cboHedefDbDatabaseler.Text == ""
                  || cboKaynakDbDatabaseler.Text == "")
            {
                return;
            }

            PanelView.Enabled = false;
            panelSol.Enabled = false;
            Task.Run(() =>
             {
                 var sqlCodes = new List<string>();
                 foreach (ListViewItem item in listViewViewlar.CheckedItems)
                 {
                     sqlCodes.Add(kaynakViewlar[int.Parse(item.Tag.ToString())].SqlCreate);
                 }
                 Exec(sqlCodes);
                 viewlariGetir();
                 viewlariKarsilastir();
                 PanelView.Enabled = true;
                 panelSol.Enabled = true;
             });
        }

        private void btnTabloAktar_Click(object sender, EventArgs e)
        {
            if (listViewTablolar.Items.Count == 0
                || cboHedefDbDatabaseler.Text == ""
                || cboKaynakDbDatabaseler.Text == "")
            {
                return;
            }

            panelTablo.Enabled = false;
            panelSol.Enabled = false;
            Task.Run(() =>
            {
                var tabloAdlari = new List<string>();
                foreach (ListViewItem item in listViewTablolar.CheckedItems)
                {
                    tabloAdlari.Add(kaynakTablolar[int.Parse(item.Tag.ToString())].Name);
                }
                var hatalar = DBUtilities.TabloOlustur(kaynakServer, hedefServer, tabloAdlari);
                if (hatalar.Count() > 0)
                {
                    MessageBox.Show(string.Join(Environment.NewLine, hatalar), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                tabloVeKolonlariGetir();
                tablolariKarsilastir();
                panelTablo.Enabled = true;
                panelSol.Enabled = true;
            });
        }

        private void Exec(IEnumerable<string> sqlCodes)
        {
            var hatalar = DBUtilities.SqlExec(hedefServer, sqlCodes);
            if (hatalar.Count() > 0)
            {
                MessageBox.Show(string.Join(Environment.NewLine, hatalar), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnTabloTumunuSec_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewTablolar.Items)
            {
                item.Checked = true;
            }
        }

        private void btnKolonTumunuSec_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewKolonlar.Items)
            {
                item.Checked = true;
            }
        }

        private void btnPFTumunuSec_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewPF.Items)
            {
                item.Checked = true;
            }
        }

        private void btnViewTumunuSec_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewViewlar.Items)
            {
                item.Checked = true;
            }
        }

        private void cboKaynakDbDatabaseler_SelectedIndexChanged(object sender, EventArgs e)
        {
            kaynakServer.Database = cboKaynakDbDatabaseler.Text;
        }

        private void cboHedefDbDatabaseler_SelectedIndexChanged(object sender, EventArgs e)
        {
            hedefServer.Database = cboHedefDbDatabaseler.Text;
        }

        private void cboKaynakAuthType_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool enable = cboKaynakAuthType.SelectedIndex != 0;
            txtKaynakDbKullaniciAdi.Enabled = enable;
            txtKaynakDbKullaniciSifre.Enabled = enable;

        }

        private void cboHedefAuthType_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool enable = cboHedefAuthType.SelectedIndex != 0;
            txtHedefDbKullaniciAdi.Enabled = enable;
            txtHedefDbKullaniciSifre.Enabled = enable;
        }

        private void btnHakkinda_Click(object sender, EventArgs e)
        {
            new FrmAbout().Show();
        }
    }
}
