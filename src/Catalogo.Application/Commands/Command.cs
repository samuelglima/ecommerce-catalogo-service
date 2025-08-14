using System;

namespace Catalogo.Application.Commands
{
    /// <summary>
    /// Classe base para todos os Commands
    /// </summary>
    public abstract class Command
    {
        public DateTime Timestamp { get; protected set; }
        public string UsuarioId { get; set; }

        protected Command()
        {
            Timestamp = DateTime.UtcNow;
        }
    }
}