using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Catalogo.Domain.Entities;
using Catalogo.Domain.Interfaces;

namespace Catalogo.Infrastructure.Repositories
{
    /// <summary>
    /// Implementação base de repositório em memória
    /// </summary>
    public class MemoryRepository<T> : IRepository<T> where T : Entity, IAggregateRoot
    {
        protected readonly List<T> _entities;

        public MemoryRepository()
        {
            _entities = new List<T>();
        }

        public Task<T> AdicionarAsync(T entity)
        {
            _entities.Add(entity);
            // IMPORTANTE: Retornar a mesma instância para manter os eventos
            return Task.FromResult(entity);
        }

        public Task AtualizarAsync(T entity)
        {
            var index = _entities.FindIndex(e => e.Id == entity.Id);
            if (index != -1)
            {
                _entities[index] = entity;
            }
            // IMPORTANTE: Não limpar os eventos aqui
            return Task.CompletedTask;
        }

        public Task RemoverAsync(T entity)
        {
            _entities.RemoveAll(e => e.Id == entity.Id);
            return Task.CompletedTask;
        }

        public Task<T> ObterPorIdAsync(Guid id)
        {
            var entity = _entities.FirstOrDefault(e => e.Id == id);
            return Task.FromResult(entity);
        }

        public Task<IEnumerable<T>> ObterTodosAsync()
        {
            return Task.FromResult(_entities.AsEnumerable());
        }

        public Task<IEnumerable<T>> BuscarAsync(Expression<Func<T, bool>> predicate)
        {
            var func = predicate.Compile();
            var result = _entities.Where(func);
            return Task.FromResult(result.AsEnumerable());
        }

        public Task<bool> ExisteAsync(Expression<Func<T, bool>> predicate)
        {
            var func = predicate.Compile();
            var exists = _entities.Any(func);
            return Task.FromResult(exists);
        }

        public Task<int> ContarAsync(Expression<Func<T, bool>> predicate)
        {
            var func = predicate.Compile();
            var count = _entities.Count(func);
            return Task.FromResult(count);
        }
    }
}