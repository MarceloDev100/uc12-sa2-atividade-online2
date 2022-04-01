namespace Curso.Interfaces
{
    public interface IPessoaFisica
    {
        bool ValidarNascimento(DateTime dataNasc);
        bool ValidarCpf(string? cpf);
    }
}