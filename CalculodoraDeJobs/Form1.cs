using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CalculodoraDeJobs.Model;
using CalculodoraDeJobs.Model.Enums;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace CalculodoraDeJobs
{
    public partial class Form1 : Form
    {
        private List<TextBox> Produtos { get; set; }
        public string Diretorio { get; set; }

        public Form1()
        {
            InitializeComponent();
            Produtos = new List<TextBox>();
            Diretorio = Properties.Settings.Default.Diretório == "" ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : Properties.Settings.Default.Diretório;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.CheckState == CheckState.Checked)
            {
                dateTimePicker2.Enabled = false;
                flowLayoutPanel2.Height = 142;
            }
            else
            {
                dateTimePicker2.Enabled = true;
                flowLayoutPanel2.Height = 50;
                checkBox2.CheckState = CheckState.Checked;
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.Text = "R$ ";
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            textBox1.Text = "U$ ";
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.CheckState == CheckState.Checked)
            {
                textBox1.Enabled = false;
                if(textBox1.Text.Length > 3)
                {
                    textBox1.Text = textBox1.Text.Remove(3);
                }
                flowLayoutPanel2.Enabled = true;
            }
            else
            {
                textBox1.Enabled = true;
                flowLayoutPanel2.Enabled = false;
            } 
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.CheckState == CheckState.Checked)
            {
                numericUpDown2.Enabled = true;
            }
            else
            {
                numericUpDown2.Enabled = false;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (checkBox3.CheckState == CheckState.Unchecked)
            {
                numericUpDown2.Value = CalculadoraDeJob.CalcularDiasAjuste(decimal.ToInt32(numericUpDown1.Value));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime inicio = DateTime.Parse(dateTimePicker3.Value.ToString());
                DateTime? termino = null;
                if (checkBox1.CheckState == CheckState.Unchecked)
                {
                    termino = DateTime.Parse(dateTimePicker2.Value.ToString());
                }
                int diasv1 = decimal.ToInt32(numericUpDown1.Value);
                int sugestaodias = decimal.ToInt32(numericUpDown2.Value);
                List<string> prod = Produtos.Select(p => p.Text).ToList();
                TipoDinheiro tipo = radioButton3.Checked ? TipoDinheiro.Real : TipoDinheiro.Dolar;
                decimal? orcamento;

                orcamento = checkBox2.CheckState == CheckState.Unchecked ? (decimal?)decimal.Parse(textBox1.Text.Substring(2)) : null;



                ArquivoAberto arquivoAberto = radioButton5.Checked ? ArquivoAberto.Sim : ArquivoAberto.Nao;
                ClienteBom clienteBom;
                if (radioButton1.Checked)
                {
                    clienteBom = ClienteBom.Sim;
                }
                else if (radioButton2.Checked)
                {
                    clienteBom = ClienteBom.Nao;
                }
                else
                {
                    clienteBom = ClienteBom.SemOpiniao;
                }

                CalculadoraDeJob calculadora = new CalculadoraDeJob(inicio, flowLayoutPanel2.Enabled == false ? null : termino, orcamento == null && checkBox1.CheckState == CheckState.Checked ? diasv1 : -1, orcamento == null && checkBox1.CheckState == CheckState.Checked ? sugestaodias : -1, prod, orcamento, tipo, arquivoAberto, clienteBom);

                DialogResult result = MessageBox.Show(
                    calculadora.ToString() + "\n\nConfirmar informações?", "Calculadora de Jobs", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if(result == DialogResult.Yes)
                {
                    int index;
                    for (index = 1; File.Exists(Diretorio + $@"\Orcamento_{index}.txt"); index++) ;
                    string nomeArquivo = @"\Orcamento_" + index.ToString() + ".txt";

                    using(StreamWriter sw = File.AppendText(Diretorio + nomeArquivo))
                    {
                        sw.Write(calculadora.ToString(index));
                    }
                    MessageBox.Show("Arquivo final salvo em: " + Diretorio, "Concluído", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Debug.WriteLine(Diretorio);
                }
            }
            catch (FormatException ex)
            {
                MessageBox.Show("Erro! " + ex.Message,"Ops!");
            }
        }

        public string ValoresDeEntrada(CalculadoraDeJob c)
        {
            string m = "Data de início: " + c.DataInicio.ToString("dd/MM/yyyy")
                + "\nData de entrega: " + (c.DataFim == null ? "----" : c.DataFim.Value.ToString("dd/MM/yyyy"))
                + "\nDias para v1: " + (c.DiasV1 != -1 ? c.DiasV1.ToString() : "----")
                + "\nSugestão de dias para ajuste: " + (c.SugestaoDias != -1 ? c.SugestaoDias.ToString() : "----")
                + "\nProdutos: " + c.ListarProdutos()
                + "\nDinheiro em: " + c.Tipo
                + "\nOrçamento: " + (c.Orcamento != null ? ( (c.Tipo == TipoDinheiro.Real ? "R$ " : "U$ ") + c.Orcamento.Value.ToString("N2") ) : "----")
                + "\nArquivo aberto? " + c.ArquivoAberto
                + "\nCliente bom? " + c.ClienteBom;

            return m;
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            if (dateTimePicker2.Value < dateTimePicker3.Value)
            {
                dateTimePicker3.Value = dateTimePicker2.Value;
            }
        }

        private void dateTimePicker3_ValueChanged(object sender, EventArgs e)
        {
            if (dateTimePicker3.Value > dateTimePicker2.Value)
            {
                dateTimePicker2.Value = dateTimePicker3.Value;
            }
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (Placeholder(textBox))
            {
                textBox.Clear();
                textBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
                textBox.ForeColor = System.Drawing.SystemColors.WindowText;
            }
        }

        private bool Placeholder(TextBox t)
        {
            return t.ForeColor == System.Drawing.SystemColors.WindowFrame && t.Font.Italic;
        }

        private void FormatarTextoParaPlaceholder(TextBox t)
        {
            t.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Italic);
            t.ForeColor = System.Drawing.SystemColors.WindowFrame;
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Text == "")
            {
                FormatarTextoParaPlaceholder(textBox);
                textBox.Text = "Produto " + (Produtos.Count + 1) + " ...";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TextBox T = flowLayoutPanel9.Controls.Find("Produto", true).Last() as TextBox;

            if (!Placeholder(T) && T.Text != "")
            {
                ConfirmaProduto(T);
                NewTextBox_Produtos(Produtos.Count + 1);
            }
        }

        private void ConfirmaProduto(TextBox t)
        {
            Produtos.Add(t);
            t.ReadOnly = true;
            t.BackColor = System.Drawing.Color.FromArgb(226, 252, 210);
            NewButtonRemove(t);
        }

        private void NewButtonRemove(TextBox t)
        {
            Button b = new Button();
            b.Name = "RemoveProduto";
            b.Text = "✖";
            b.FlatStyle = FlatStyle.Flat;
            b.Size = new System.Drawing.Size(24, 24);
            b.BackColor = System.Drawing.Color.Red;
            b.ForeColor = System.Drawing.Color.Black;
            b.Margin = new Padding(0, 0, 0, 13);
            b.Cursor = Cursors.Hand;
            b.Click += btnRemove_Click;
            flowLayoutPanel9.Controls.Add(b);
        }

        private void NewTextBox_Produtos(int nProd)
        {
            TextBox T = new TextBox();
            TextBox ant = Produtos[Produtos.Count - 1];
            T.Anchor = AnchorStyles.Left;
            T.RightToLeft = RightToLeft.No;
            T.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Italic);
            T.ForeColor = System.Drawing.SystemColors.WindowFrame;

            T.Margin = new Padding(0, 0, 0, 13);
            T.Name = "Produto";
            T.Size = new System.Drawing.Size(213, 24);
            T.TabIndex = 12;
            T.Text = "Produto " + nProd + " ...";
            T.Enter += new EventHandler(textBox2_Enter);
            T.Leave += new EventHandler(textBox2_Leave);
            flowLayoutPanel9.Controls.Add(T);
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            Button B = sender as Button;
            int index = flowLayoutPanel9.Controls.IndexOf(B);
            TextBox T = flowLayoutPanel9.Controls[index - 1] as TextBox;
            Produtos.Remove(T);

            flowLayoutPanel9.Controls.Remove(B);
            flowLayoutPanel9.Controls.Remove(T);

            TextBox ultimo = flowLayoutPanel9.Controls.Find("Produto", true).Last() as TextBox;
            FormatarTextoParaPlaceholder(ultimo);
            ultimo.Text = "Produto " + (Produtos.Count + 1).ToString() + " ...";
            Debug.WriteLine(Diretorio);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.Description = "\nSelecione uma pasta para salvar o arquivo txt:";
            folderBrowser.ShowDialog();
            if(folderBrowser.SelectedPath != "")
            {
                Properties.Settings.Default.Diretório = folderBrowser.SelectedPath;
                Properties.Settings.Default.Save();
                Diretorio = folderBrowser.SelectedPath;
            }
        }
    }
}
