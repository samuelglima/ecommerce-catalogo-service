using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Catalogo.Domain.Entities;

namespace Catalogo.Domain.Interfaces
{
    /// <summary>
    /// Interface base para repositórios
    /// </summary>
    public interface IRepository<T> where T : Entity, IAggregateRoot
    {
        /// <summary>
        /// Adiciona uma nova entidade
        /// </summary>
        Task<T> AdicionarAsync(T entity);

        /// <summary>
        /// Atualiza uma entidade existente
        /// </summary>
        Task AtualizarAsync(T entity);

        /// <summary>
        /// Remove uma entidade
        /// </summary>
        Task RemoverAsync(T entity);

        /// <summary>
        /// Busca uma entidade por ID
        /// </summary>
        Task<T> ObterPorIdAsync(Guid id);

        /// <summary>
        /// Obtém todas as entidades
        /// </summary>
        Task<IEnumerable<T>> ObterTodosAsync();

        /// <summary>
        /// Busca entidades que atendam a uma condição
        /// </summary>
        Task<IEnumerable<T>> BuscarAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Verifica se existe alguma entidade que atenda a condição
        /// </summary>
        Task<bool> ExisteAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Conta quantas entidades atendem a condição
        /// </summary>
        Task<int> ContarAsync(Expression<Func<T, bool>> predicate);
    }
}