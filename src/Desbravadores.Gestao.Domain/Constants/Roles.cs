using System.ComponentModel.DataAnnotations;

namespace Desbravadores.Gestao.Domain.Constants;

public enum Roles
{
  [Display(Name = "Diretor(a)")]
  DIRETORA = 0,

  [Display(Name = "Secretario(a)")]
  SECRETARIA = 1,

  [Display(Name = "Tesoureiro(a)")]
  TESOURARIA = 2,

  [Display(Name = "Diretor(a)")]
  DIRETORIA = 3,

  [Display(Name = "Desbravador(a)")]
  DESBRAVADOR = 4
}