using Curso.Interfaces;
using System.Text.RegularExpressions;

namespace Curso.Classes
{
    public class PessoaFisica : Pessoa, IPessoaFisica
    {
        public string? Cpf { get; set; }
        public DateTime DataNascimento { get; set; }
        public string CaminhoRelativo { get; private set; } = "Database/PessoaFisica/";

        public PessoaFisica()
        {
        }

        public PessoaFisica(string nome, Endereco endereco, float rendimento, string cpf,
        DateTime dataNascimento) : base(nome, endereco, rendimento)
        {
            Cpf = cpf;
            DataNascimento = dataNascimento;
        }

        public PessoaFisica(Pessoa pessoa, string? cpf, DateTime dataNascimento) : base(pessoa)
        {
            Cpf = cpf;
            DataNascimento = dataNascimento;
        }

        public bool ValidarNascimento(DateTime dataNasc)
        {
            DateTime dataAtual = DateTime.Today;

            int idade = dataAtual.Year - dataNasc.Year;

            if (dataAtual.Month < dataNasc.Month ||
               (dataAtual.Month == dataNasc.Month &&
               dataAtual.Day < dataNasc.Day))
            {
                idade--;
            }

            if (idade >= 18 && idade <= 100)
            {
                return true;
            }

            return false;
        }

        public bool ValidarCpf(string? cpf)
        {
            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int somatorio;
            int resto;
            string digito;
            string cpfAux;

            try
            {
                //Verifica se o CPF está nulo ou vazio.
                if (String.IsNullOrEmpty(cpf))
                    return false;

                //Remove os caracteres em branco do início e do final da string.
                cpf = cpf.Trim();

                //Verifica o padrão de caracteres para um CPF.
                if (Regex.IsMatch(cpf, @"((\d{3}\.\d{3}\.\d{3}-\d{2})|(\d{11}))"))
                {
                    //Caso a string tenha caracteres ("." e "-"), os mesmos serão removidos restando apenas números.
                    if (cpf.Length == 14)
                        cpf = cpf.Replace(".", "").Replace("-", "");

                    //Verifica se o CPF possui todos os dígitos iguais.
                    if (cpf.All(c => c.Equals(cpf.First())))
                        return false;

                    //Define a variável acumuladora como zero.
                    somatorio = 0;

                    //Obtém os 9 primeiros caracteres do CPF ( sem os dígitos de verificação ).
                    cpfAux = cpf.Substring(0, 9);

                    //Percorre cada número dos 9 primeiros e faz a multiplicação pelos valores estabelecidos
                    for (int i = 0; i < cpfAux.Length; i++)
                        somatorio += int.Parse(cpfAux[i].ToString()) * multiplicador1[i];

                    //Após acumular o somatório, obtém o resto da divisão por 11.
                    resto = somatorio % 11;

                    if (resto < 2)
                        resto = 0;
                    else
                        resto = 11 - resto;

                    //Obtém 9 dígitos mais o primeiro dígito verificador.
                    digito = resto.ToString();
                    cpfAux = cpfAux + digito;

                    //Redefine a variável acumuladora como zero.
                    somatorio = 0;

                    //Percorre cada número dos 10 primeiros e faz a multiplicação pelos valores estabelecidos.
                    for (int i = 0; i < cpfAux.Length; i++)
                        somatorio += int.Parse(cpf[i].ToString()) * multiplicador2[i];

                    //Após acumular o somatório, obtém o resto da divisão por 11.
                    resto = somatorio % 11;

                    if (resto < 2)
                        resto = 0;
                    else
                        resto = 11 - resto;

                    //Obtém o valor último dígito verificador e concatena ao primeiro.
                    digito = digito + resto.ToString();
                }
                else
                    return false;

                //Compara os dígitos verificadores com os dígitos calculados
                return cpf.EndsWith(digito);

            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool ExisteCpf(string? cpf)
        {
            cpf = RemoveMascaraCpf(cpf);

            VerificarPasta(CaminhoRelativo);

            DirectoryInfo directoryInfo = new DirectoryInfo(CaminhoRelativo);

            FileInfo[] arquivos = directoryInfo.GetFiles("*.txt");

            string cpfNomeArquivo;
            foreach (FileInfo cadaArquivo in arquivos)
            {
                cpfNomeArquivo = Path.GetFileNameWithoutExtension(cadaArquivo.Name.Split("-")[1]);

                if (cpfNomeArquivo.Equals(cpf))
                {
                    return true;
                }
            }
            return false;
        }

        public override float PagarImposto(float rendimento)
        {
            float desconto;

            if (rendimento <= 1500)
            {
                desconto = 0;
            }
            else if (rendimento <= 3500)
            {
                desconto = (rendimento / 100) * 2f;
            }
            else if (rendimento <= 6000)
            {
                desconto = (rendimento / 100) * 3.5f;
            }
            else
            {
                desconto = (rendimento / 100) * 5.5f;
            }

            return desconto;
        }

        public void Inserir(PessoaFisica pf)
        {
            string caminho = Path.Combine(CaminhoRelativo, $"{ pf.Nome }-{ pf.Cpf }.txt");

            VerificarPastaArquivo(caminho);

            using (StreamWriter sw = new StreamWriter(caminho))
            {
                string pfString = $"{ pf.Nome };{ pf.Endereco?.Logradouro };{ pf.Endereco?.Numero };{ pf.Endereco?.Complemento };{ pf.Endereco?.EndComercial };{ pf.Rendimento };{ pf.Cpf };{ pf.DataNascimento }";
                sw.Write(pfString);
            }
        }

        public List<PessoaFisica> Ler()
        {
            List<PessoaFisica> listaPf = new List<PessoaFisica>();

            VerificarPasta(CaminhoRelativo);

            DirectoryInfo directoryInfo = new DirectoryInfo(CaminhoRelativo);

            FileInfo[] arquivos = directoryInfo.GetFiles("*.txt");

            string[] atributos;
            foreach (FileInfo cadaArquivo in arquivos)
            {
                using (StreamReader sr = cadaArquivo.OpenText())
                {
                    string? linha ;
                    while (!sr.EndOfStream)
                    {
                        linha = sr.ReadLine();
                        atributos = linha.Split(";");
                        PessoaFisica pf = new PessoaFisica();
                        Endereco ender = new Endereco();

                        pf.Nome = atributos[0];
                        ender.Logradouro = atributos[1];
                        ender.Numero = atributos[2];
                        ender.Complemento = atributos[3];
                        ender.EndComercial = Boolean.Parse(atributos[4]);
                        pf.Endereco = ender;
                        pf.Rendimento = float.Parse(atributos[5]);
                        pf.Cpf = InsereMascaraCpf(atributos[6]);
                        pf.DataNascimento = DateTime.Parse(atributos[7]);

                        listaPf.Add(pf);
                    }
                }
            }
            return listaPf;
        }

        public PessoaFisica? BuscarPessoaFisica(string? cpf)
        {
            VerificarPasta(CaminhoRelativo);

            DirectoryInfo directoryInfo = new DirectoryInfo(CaminhoRelativo);

            FileInfo[] arquivos = directoryInfo.GetFiles("*.txt");

            string[] atributos;
            string cpfNomeArquivo;
            foreach (FileInfo cadaArquivo in arquivos)
            {
                cpfNomeArquivo = Path.GetFileNameWithoutExtension(cadaArquivo.Name.Split("-")[1]);

                if (cpfNomeArquivo.Equals(cpf))
                {
                    using (StreamReader sr = cadaArquivo.OpenText())
                    {
                        string? linha;
                        while ((linha = sr.ReadLine()) != null)
                        {
                            atributos = linha.Split(";");
                            PessoaFisica pf = new PessoaFisica();
                            Endereco ender = new Endereco();

                            pf.Nome = atributos[0];
                            ender.Logradouro = atributos[1];
                            ender.Numero = atributos[2];
                            ender.Complemento = atributos[3];
                            ender.EndComercial = Boolean.Parse(atributos[4]);
                            pf.Endereco = ender;
                            pf.Rendimento = float.Parse(atributos[5]);
                            pf.Cpf = InsereMascaraCpf(atributos[6]);
                            pf.DataNascimento = DateTime.Parse(atributos[7]);

                            return pf;
                        }
                    }
                }
            }
            return null;
        }

        public bool ExcluirPessoaFisica(string? cpf)
        {
            VerificarPasta(CaminhoRelativo);

            DirectoryInfo directoryInfo = new DirectoryInfo(CaminhoRelativo);

            FileInfo[] arquivos = directoryInfo.GetFiles("*.txt");

            string cpfNomeArquivo;
            foreach (FileInfo cadaArquivo in arquivos)
            {
                cpfNomeArquivo = Path.GetFileNameWithoutExtension(cadaArquivo.Name.Split("-")[1]);

                if (cpfNomeArquivo.Equals(cpf))
                {
                    cadaArquivo.Delete();
                    return true;
                }
            }
            return false;
        }

        public void ExcluirTodasPessoasFisicas()
        {
            VerificarPasta(CaminhoRelativo);

            DirectoryInfo directoryInfo = new DirectoryInfo(CaminhoRelativo);

            FileInfo[] arquivos = directoryInfo.GetFiles("*.txt");

            foreach (FileInfo cadaArquivo in arquivos)
            {
                cadaArquivo.Delete();
            }
        }

        public void EditarPessoaFisica(PessoaFisica pf)
        {
            ExcluirPessoaFisica(pf.Cpf);
            Inserir(pf);
        }

        public int TotalPessoasFisicas()
        {
            VerificarPasta(CaminhoRelativo);

            DirectoryInfo directoryInfo = new DirectoryInfo(CaminhoRelativo);

            FileInfo[] arquivos = directoryInfo.GetFiles("*.txt");

            return arquivos.Length;
        }

        public string? RemoveMascaraCpf(string? cpf)
        {
            if (!String.IsNullOrEmpty(cpf))
            {
                cpf = cpf.Replace(".", "").Replace("-", "");
                return cpf;
            }
            return null;
        }

        public string? InsereMascaraCpf(string? cpf)
        {
            string? cpfFormatado = "";

            if (!String.IsNullOrEmpty(cpf))
            {
                cpf = cpf.Trim();

                if (cpf.Length == 11)
                    cpfFormatado = Convert.ToUInt64(cpf).ToString(@"000\.000\.000-00");
            }

            return cpfFormatado;
        }

        public override string ToString()
        {
            return base.ToString()
            + "\tCPF: " + Cpf
            + "\n\tData de nascimento: " + DataNascimento
            + "\n\tTaxa de imposto a ser pago: " + PagarImposto(Rendimento).ToString("C");
        }
    }
}