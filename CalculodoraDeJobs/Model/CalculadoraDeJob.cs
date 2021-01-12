using System;
using System.Diagnostics;
using System.Collections.Generic;
using CalculodoraDeJobs.Model.Enums;
using System.Text;

namespace CalculodoraDeJobs.Model
{
    public class CalculadoraDeJob
    {
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public int DiasV1 { get; set; }
        public int SugestaoDias { get; set; }
        public List<string> Produtos { get; set; }
        public decimal? Orcamento { get; set; }
        public TipoDinheiro Tipo { get; set; }
        public ArquivoAberto ArquivoAberto { get; set; }
        public ClienteBom ClienteBom { get; set; }

        public CalculadoraDeJob(DateTime dataInicio, DateTime? dataFim, int diasV1, int sugestaoDias, List<string> produtos, decimal? orcamento,
            TipoDinheiro tipo, ArquivoAberto arquivoAberto, ClienteBom clienteBom)
        {
            DataInicio = dataInicio;
            DataFim = dataFim;
            DiasV1 = diasV1;
            SugestaoDias = sugestaoDias;
            Produtos = produtos;
            Orcamento = orcamento;
            Tipo = tipo;
            ArquivoAberto = arquivoAberto;
            ClienteBom = clienteBom;


        }

        public decimal RefinarValores(decimal valBruto)
        {
            decimal K;
            K = ArquivoAberto == ArquivoAberto.Sim ? (decimal)1.3 : 1;
            switch (ClienteBom)
            {
                case ClienteBom.Nao:
                    K *= (decimal)1.3;
                    break;
                case ClienteBom.Sim:
                    K *= (decimal)0.8;
                    break;
            }
            return valBruto * K;
        }

        public decimal RetornarParaValorBruto(decimal valRefinado)
        {
            decimal K;
            K = ArquivoAberto == ArquivoAberto.Sim ? (decimal)1.3 : 1;
            switch (ClienteBom)
            {
                case ClienteBom.Nao:
                    K *= (decimal)1.3;
                    break;
                case ClienteBom.Sim:
                    K *= (decimal)0.8;
                    break;
            }
            return valRefinado / K;
        }

        private DateTime ObterDataFim()
        {
            if (DataFim != null)
            {
                return (DateTime)DataFim;
            }

            DateTime dataFim;
            if (DiasV1 != -1)
            {
                dataFim = DataInicio.AddDays(DiasV1 + SugestaoDias - 1);
            }
            else
            {
                decimal totalBruto = RetornarParaValorBruto((decimal)Orcamento);
                int diasTrabalhados = Tipo == TipoDinheiro.Real ? (int)Math.Ceiling((totalBruto - 1000) / 110) : (int)Math.Ceiling(totalBruto / 270);
                dataFim = DataInicio.AddDays(diasTrabalhados-1);

                Debug.WriteLine("Data Fim: " + dataFim);
                Debug.WriteLine("Dias trabalhados: " + diasTrabalhados);
            }

            return dataFim;
        }

        public decimal Total()
        {
            decimal total;
            if (Orcamento != null)
            {
                total = (decimal)Orcamento;
            }
            else
            {
                int dias = DiasTrabalhados();

                decimal valBruto;
                if (Tipo == TipoDinheiro.Real)
                {
                    valBruto = (dias * 110) + 1000;
                }
                else
                {
                    valBruto = dias * 270;
                }

                total = RefinarValores(valBruto);
            }


            return total;
        }

        public string ListarProdutos()
        {
            string m = "";
            for (int i = 0; i < Produtos.Count; i++)
            {
                m += Produtos[i] + (Produtos.Count == i + 2 ? " e " : "");
                m += i != Produtos.Count - 1 && Produtos.Count != i + 2 ? ", " : "";
            }
            return m;
        }

        public int DiasTrabalhados()
        {
            if (DiasV1 == -1)
            {
                return (ObterDataFim() - DataInicio).Days + 1;
            }
            else
            {
                return DiasV1 + SugestaoDias;
            }
        }

        public int ObterDiasV1()
        {
            if (DiasV1 != -1)
            {
                return DiasV1;
            }
            return (int)Math.Ceiling((decimal)DiasTrabalhados() * 2 / 3);
        }

        public int ObterDiasAjuste()
        {
            if(SugestaoDias != -1)
            {
                return SugestaoDias;
            }

            return ObterDataFim().Subtract(DataV1()).Days;
        }

        public DateTime DataV1()
        {
            return DataInicio.AddDays(ObterDiasV1() - 1);
        }

        public DateTime PrazoCancelamento()
        {
            return DataInicio.AddDays((DiasTrabalhados() * 4) / 9);
        }

        public string ToString(int index = -1)
        {
            int padRight = 25;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("DADOS OBTIDOS DA CALCULADORA DE JOBS:\n");
            sb.AppendLine($"----------- Orçamento " + (index == -1 ? "" : index.ToString()) + "------------\n");
            sb.AppendLine("PRODUTOS:".PadRight(padRight) + ListarProdutos());
            sb.AppendLine("Data de início:".PadRight(padRight) + DataInicio.ToString("dd/MM/yyyy"));
            sb.AppendLine("Data de entrega:".PadRight(padRight) + ObterDataFim().ToString("dd/MM/yyyy"));
            sb.AppendLine("Data da V1:".PadRight(padRight) + DataV1().ToString("dd/MM/yyyy"));
            sb.AppendLine("Prazo de cancelamento:".PadRight(padRight) + PrazoCancelamento().ToString("dd/MM/yyyy"));
            sb.AppendLine("Dias para V1:".PadRight(padRight) + ObterDiasV1().ToString());
            sb.AppendLine("Dias de ajuste:".PadRight(padRight) + ObterDiasAjuste().ToString());
            sb.AppendLine();
            sb.AppendLine("Arquivo aberto?".PadRight(padRight) + ArquivoAberto);
            sb.AppendLine("Cliente bom?".PadRight(padRight) + ClienteBom);
            sb.AppendLine("Dinheiro em:".PadRight(padRight) + Tipo);
            sb.AppendLine("\nValor diária:".PadRight(padRight, '.') + $" { (Tipo == TipoDinheiro.Dolar ? "U$" : "R$") } " + (Total()/DiasTrabalhados()).ToString("N2"));
            sb.AppendLine("Dias trabalhados:".PadRight(padRight) + DiasTrabalhados().ToString() + " dias");
            sb.AppendLine("\nTOTAL:".PadRight(padRight, '.') + $" { (Tipo == TipoDinheiro.Dolar ? "U$" : "R$") } " + Total().ToString("N2"));
            return sb.ToString();
        }

        public static int CalcularDiasAjuste(int diasProd)
        {
            return diasProd / 2;
        }
    }
}
