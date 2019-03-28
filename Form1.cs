using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ListarProjeto
{
    public partial class Form1 : Form
    {
        DataTable tableExt;
        DataTable tableTemp;
        DataTable tableConfig;
        DataTable tablePesquisa;
        DataSet ds;
        public Form1()
        {
            InitializeComponent(); 
            prepAll();
        }

        private void prepAll() 
        {
            tableExt = new DataTable("EXTENSAO");
            tableExt.Columns.Add("TIPO", typeof(string)).Caption = "Tipo";
            tableExt.Columns.Add("EXT", typeof(string)).Caption = "Extensão";
            tableExt.PrimaryKey = new DataColumn[] { tableExt.Columns["EXT"] };
            
            tableExt.Rows.Add("Classe", ".cs");
            tableExt.Rows.Add("Form", ".resx");

            lkTipo.Properties.DataSource = tableExt;
            lkTipo.Properties.ValueMember = "EXT";
            lkTipo.Properties.DisplayMember = "TIPO";

            tableTemp = new DataTable("TEMP");

            tablePesquisa = new DataTable("PESQUISA");
            tablePesquisa.Columns.Add("ID",typeof(int)).Caption = "Id";
            tablePesquisa.Columns.Add("NOM_PROJ", typeof(string)).Caption = "Nome em Projeto";
            tablePesquisa.Columns.Add("NOM_RUNT", typeof(string)).Caption = "Nome em Runtime";
            tablePesquisa.Columns.Add("APELIDO", typeof(string)).Caption = "Apelido";

            tableConfig = new DataTable("CONFIG");
            tableConfig.Columns.Add("DIR", typeof(string)).Caption = "Diretório";
            tableConfig.Columns.Add("TIPO", typeof(string)).Caption = "Tipo";

            ds = new DataSet();
            ds.Tables.Add(tablePesquisa);
            ds.Tables.Add(tableConfig);
            if (System.IO.File.Exists("formsbase.xml"))
            {
                try
                {
                    ds.ReadXml("formsbase.xml");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao Ler dados.\n" + ex.ToString());
                    throw;
                }
                
            }
            foreach (DataRow dr in ds.Tables["CONFIG"].Rows)
            {
                edDir.EditValue = (dr["DIR"]?.ToString() ?? "") != "" ? dr["DIR"].ToString() : "";
                lkTipo.EditValue = (dr["TIPO"]?.ToString() ?? "") != "" ? dr["TIPO"].ToString() : "";
            }
            gridControl1.DataSource = ds.Tables["PESQUISA"];
            
        } 
        private void btnCarregar_Click(object sender, EventArgs e)
        {
            if (lkTipo.EditValue == null || lkTipo.EditValue.ToString() == "")
            {
                return;
            }
            ds.Tables["PESQUISA"].Rows.Clear();
            if (edDir.EditValue != null && edDir.EditValue.ToString() != "")
            {
                if (System.IO.Directory.Exists(edDir.EditValue.ToString()))
                {
                    string[] pastas = Directory.GetDirectories(edDir.EditValue.ToString());

                    List<string> pastasList = new List<string>();
                    List<string> pastasTemp = new List<string>();
                    pastasTemp = getPastas(edDir.EditValue.ToString());
                    foreach (string s in pastasTemp)
                    {
                        if (!pastasList.Contains(s))
                        {
                            pastasList.Add(s);   
                        }
                    }
                    string[] arquivosB = Directory.GetFiles(edDir.EditValue.ToString());
                    if (arquivosB.Length > 0)
                    {
                        foreach (string arquivo in arquivosB)
                        {
                            if (tableExt.Rows.Find(new object[] { System.IO.Path.GetExtension(arquivo).ToString() }) != null && System.IO.Path.GetExtension(arquivo).ToString() == lkTipo.EditValue.ToString())
                            {
                                DataRow dr = ds.Tables["PESQUISA"].NewRow();
                                int id = 0;
                                if (ds.Tables["PESQUISA"].Rows.Count > 0)
                                {
                                    id = Convert.ToInt32((ds.Tables["PESQUISA"].Rows[ds.Tables["PESQUISA"].Rows.Count - 1]["ID"]).ToString()) + 1;
                                }
                                dr["ID"] = id;
                                dr["NOM_PROJ"] = System.IO.Path.GetFileNameWithoutExtension(arquivo).ToString();
                                dr["NOM_RUNT"] = GetNameFileInRunTime(arquivo);
                                ds.Tables["PESQUISA"].Rows.Add(dr);
                            }
                        }
                    }
                    foreach (string pasta in pastasList)
                    {
                        string[] arquivos = Directory.GetFiles(pasta);
                        if (arquivos.Length > 0)
                        {
                            foreach (string arquivo in arquivos)
                            {
                                if (tableExt.Rows.Find(new object[] { System.IO.Path.GetExtension(arquivo).ToString() }) != null && System.IO.Path.GetExtension(arquivo).ToString() == lkTipo.EditValue.ToString())
                                {
                                    DataRow dr = ds.Tables["PESQUISA"].NewRow();
                                    int id = 0;
                                    if (ds.Tables["PESQUISA"].Rows.Count > 0)
                                    {
                                        id = Convert.ToInt32((ds.Tables["PESQUISA"].Rows[ds.Tables["PESQUISA"].Rows.Count - 1]["ID"]).ToString()) + 1;
                                    }
                                    dr["ID"] = id;
                                    dr["NOM_PROJ"] = System.IO.Path.GetFileNameWithoutExtension(arquivo).ToString();
                                    dr["NOM_RUNT"] = GetNameFileInRunTime(arquivo);
                                    ds.Tables["PESQUISA"].Rows.Add(dr);
                                }
                            }
                        }
                    }
                }
            }
        }

        private List<string> getPastas(string dir) 
        {
            List<string> Pastas = new List<string>();
            string[] pastas = Directory.GetDirectories(dir);
            if (!Pastas.Contains(dir))
            {
                Pastas.Add(dir);
            }
            foreach (string pasta in pastas)
            {
                if (!Pastas.Contains(pasta)) 
                {
                    Pastas.Add(pasta);
                }
                string[] paths = Directory.GetDirectories(pasta);
                if (paths.Length > 0) 
                {
                    foreach (string path in paths)
                    {
                        if (!Pastas.Contains(path))
                        {
                            Pastas.Add(path);
                        }
                        Pastas.AddRange(getPastas(path));
                    }
                }
            }
            
            return Pastas;
        }

        private void edDir_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            if (folderBrowserDialogDir.ShowDialog() == DialogResult.OK)
            {
                edDir.EditValue = folderBrowserDialogDir.SelectedPath;
            }
        }

        private string GetNameFileInRunTime(string path)
        {
            string retorno = "";

            string arqForm = System.IO.Path.GetFileNameWithoutExtension(path);
            arqForm += ".Designer.cs";
            
            string pathForm = path.Replace(System.IO.Path.GetFileName(path),"");
            pathForm += arqForm;
            
            if (!System.IO.File.Exists(pathForm)) return "";

            string wordKeyIni = "this.Text = \"";
            string wordKeyFim = "\";";
            if (wordKeyIni == "" || wordKeyFim == "") return "";
            string arquivo = File.ReadAllText(pathForm);

            int countCorretos = 0;
            int countCorretosFim = 0;
            int ini = -1;
            int fim = -1;
            for (int i = 0; i < arquivo.Length; i++)
            {
                if (ini == -1)
                {
                    if (arquivo[i] == wordKeyIni[countCorretos])
                    {
                        countCorretos++;
                    }
                    else
                    {
                        countCorretos = 0;
                    }

                    if (countCorretos == wordKeyIni.Length) 
                    {
                        ini = i;
                        countCorretos = 0;
                    }
                }
                else
                {
                    if (countCorretos < wordKeyIni.Length && arquivo[i] == wordKeyIni[countCorretos])
                    {
                        countCorretosFim = 0;
                        countCorretos++;
                    }
                    else if (arquivo[i] == wordKeyFim[countCorretosFim]) 
                    {
                        countCorretosFim++;
                        countCorretos = 0;
                    }
                    else
                    {
                        countCorretos = 0;
                        countCorretosFim = 0;
                    }

                    if (countCorretosFim == wordKeyFim.Length) 
                    {
                        fim = i;
                        break;
                    }
                    else if (countCorretos == wordKeyIni.Length)
                    {
                        ini = i;
                    }
                }
            }

            if (ini != -1 && fim != -1) 
            {
                retorno = arquivo.Substring(ini, fim - ini);
            }

            return retorno.Trim('"');
        }

        private void edDir_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.ToString() == "F4")
            {
                edDir_ButtonClick(sender, null);
            }
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            try
            {
                ds.Tables["CONFIG"].Clear();
                DataRow drAdd = ds.Tables["CONFIG"].NewRow();
                drAdd["DIR"] = edDir.EditValue.ToString();
                drAdd["TIPO"] = lkTipo.EditValue.ToString();
                ds.Tables["CONFIG"].Rows.Add(drAdd);
                ds.WriteXml("formsbase.xml");
                MessageBox.Show("Dados Salvos com Sucesso.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao Salvar Dados.\n" + ex.ToString());
                throw;
            }
        }


        private void repositoryItemTextEditFormName_Click(object sender, EventArgs e)
        {
            if ((sender as TextEdit).EditValue == null || (sender as TextEdit).EditValue.ToString() == "") 
            {
                return;
            }
            else 
            {
                string temp = (sender as TextEdit).EditValue.ToString();
                (sender as TextEdit).EditValue += ".cs";
                lblCopy.Text = (sender as TextEdit).EditValue.ToString();               
                (sender as TextEdit).SelectAll();
                (sender as TextEdit).Copy();
                (sender as TextEdit).EditValue = temp;
            }
        }
    }
}
