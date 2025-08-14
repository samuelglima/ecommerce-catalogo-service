using Catalogo.Application.Commands;
using Catalogo.Application.DTOs;
using Catalogo.Application.Interfaces;
using Catalogo.Application.Queries;
using Catalogo.Domain.Entities;
using Catalogo.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Catalogo.Infrastructure.MessageBus;



namespace Catalogo.Application.Services
{
    /// <summary>
    /// Implementação do serviço de aplicação para Produtos
    /// </summary>
    public class ProdutoService : IProdutoService
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly IEventBus _eventBus;
        private readonly ILogger<ProdutoService> _logger;

        public ProdutoService(
            IProdutoRepository produtoRepository,
            IEventBus eventBus,
            ILogger<ProdutoService> logger)
        {
            _produtoRepository = produtoRepository ?? throw new ArgumentNullException(nameof(produtoRepository));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
                // Log de debug para confirmar injeção
             _logger.LogDebug("ProdutoService inicializado com EventBus: {EventBusType}", _eventBus.GetType().Name);
        }

        // Commands
        public async Task<ProdutoDto> CriarProdutoAsync(CriarProdutoCommand command)
{
    _logger.LogInformation("Criando novo produto com SKU {Sku}", command.Sku);

    // Verificar se já existe um produto com o mesmo SKU
    if (await _produtoRepository.ExisteSkuAsync(command.Sku))
    {
        _logger.LogWarning("Tentativa de criar produto com SKU duplicado: {Sku}", command.Sku);
        throw new InvalidOperationException($"Já existe um produto com o SKU {command.Sku}");
    }

    // Criar a entidade
    var produto = new Produto(
        command.Nome,
        command.Descricao,
        command.Preco,
        command.QuantidadeEstoque,
        command.Sku,
        command.Categoria
    );

    // Adicionar imagem se fornecida
    if (!string.IsNullOrWhiteSpace(command.ImagemUrl))
    {
        produto.DefinirImagem(command.ImagemUrl);
    }

    // Log de debug - verificar eventos antes de salvar
    _logger.LogDebug("Produto criado com {EventCount} eventos", produto.DomainEvents?.Count ?? 0);

    // Salvar no repositório
    var produtoSalvo = await _produtoRepository.AdicionarAsync(produto);

    // Log de debug - verificar eventos após salvar
    _logger.LogDebug("Produto salvo. Eventos após salvar: {EventCount}", produtoSalvo.DomainEvents?.Count ?? 0);

    // Publicar eventos de domínio
    if (produtoSalvo.DomainEvents != null && produtoSalvo.DomainEvents.Any())
    {
        _logger.LogInformation("Publicando {EventCount} eventos para produto {ProdutoId}", 
            produtoSalvo.DomainEvents.Count, produtoSalvo.Id);
        
        await PublicarEventosDominio(produtoSalvo);
    }
    else
    {
        _logger.LogWarning("Nenhum evento de domínio encontrado para publicar após criar produto {ProdutoId}", produtoSalvo.Id);
    }

    _logger.LogInformation("Produto criado com sucesso. ID: {ProdutoId}, SKU: {Sku}", 
        produtoSalvo.Id, produtoSalvo.Sku);

    // Retornar DTO
    return MapearParaDto(produtoSalvo);
}

        public async Task<ProdutoDto> AtualizarProdutoAsync(AtualizarProdutoCommand command)
        {
            _logger.LogInformation("Atualizando produto {ProdutoId}", command.Id);

            var produto = await _produtoRepository.ObterPorIdAsync(command.Id);
            
            if (produto == null)
            {
                _logger.LogWarning("Produto {ProdutoId} não encontrado para atualização", command.Id);
                throw new InvalidOperationException($"Produto com ID {command.Id} não encontrado");
            }

            // Atualizar informações
            produto.AtualizarInformacoes(command.Nome, command.Descricao, command.Categoria);

            // Salvar alterações
            await _produtoRepository.AtualizarAsync(produto);

            // Publicar eventos de domínio
            await PublicarEventosDominio(produto);

            _logger.LogInformation("Produto {ProdutoId} atualizado com sucesso", command.Id);

            return MapearParaDto(produto);
        }

        public async Task<ProdutoDto> AlterarPrecoAsync(AlterarPrecoProdutoCommand command)
        {
            _logger.LogInformation("Alterando preço do produto {ProdutoId} para {NovoPreco}", 
                command.Id, command.NovoPreco);

            var produto = await _produtoRepository.ObterPorIdAsync(command.Id);
            
            if (produto == null)
            {
                _logger.LogWarning("Produto {ProdutoId} não encontrado para alteração de preço", command.Id);
                throw new InvalidOperationException($"Produto com ID {command.Id} não encontrado");
            }

            var precoAnterior = produto.Preco.Valor;
            produto.AlterarPreco(command.NovoPreco);
            await _produtoRepository.AtualizarAsync(produto);

            // Publicar eventos de domínio
            await PublicarEventosDominio(produto);

            _logger.LogInformation("Preço do produto {ProdutoId} alterado de {PrecoAnterior} para {NovoPreco}", 
                command.Id, precoAnterior, command.NovoPreco);

            return MapearParaDto(produto);
        }

        public async Task<ProdutoDto> AtualizarEstoqueAsync(AtualizarEstoqueCommand command)
        {
            _logger.LogInformation("Atualizando estoque do produto {ProdutoId}. Operação: {Operacao}, Quantidade: {Quantidade}", 
                command.Id, command.TipoOperacao, command.Quantidade);

            var produto = await _produtoRepository.ObterPorIdAsync(command.Id);
            
            if (produto == null)
            {
                _logger.LogWarning("Produto {ProdutoId} não encontrado para atualização de estoque", command.Id);
                throw new InvalidOperationException($"Produto com ID {command.Id} não encontrado");
            }

            var estoqueAnterior = produto.QuantidadeEstoque;

            if (command.TipoOperacao == TipoOperacaoEstoque.Adicionar)
            {
                produto.AdicionarEstoque(command.Quantidade);
            }
            else
            {
                produto.RemoverEstoque(command.Quantidade);
            }

            await _produtoRepository.AtualizarAsync(produto);

            // Publicar eventos de domínio
            await PublicarEventosDominio(produto);

            _logger.LogInformation("Estoque do produto {ProdutoId} atualizado de {EstoqueAnterior} para {EstoqueAtual}", 
                command.Id, estoqueAnterior, produto.QuantidadeEstoque);

            return MapearParaDto(produto);
        }

        public async Task<bool> DeletarProdutoAsync(Guid id)
        {
            _logger.LogInformation("Deletando produto {ProdutoId}", id);

            var produto = await _produtoRepository.ObterPorIdAsync(id);
            
            if (produto == null)
            {
                _logger.LogWarning("Produto {ProdutoId} não encontrado para deleção", id);
                return false;
            }

            await _produtoRepository.RemoverAsync(produto);
            
            _logger.LogInformation("Produto {ProdutoId} deletado com sucesso", id);
            return true;
        }

        public async Task<bool> AtivarProdutoAsync(Guid id)
        {
            _logger.LogInformation("Ativando produto {ProdutoId}", id);

            var produto = await _produtoRepository.ObterPorIdAsync(id);
            
            if (produto == null)
            {
                _logger.LogWarning("Produto {ProdutoId} não encontrado para ativação", id);
                return false;
            }

            produto.Ativar();
            await _produtoRepository.AtualizarAsync(produto);

            // Publicar eventos de domínio
            await PublicarEventosDominio(produto);

            _logger.LogInformation("Produto {ProdutoId} ativado com sucesso", id);
            return true;
        }

        public async Task<bool> DesativarProdutoAsync(Guid id)
        {
            _logger.LogInformation("Desativando produto {ProdutoId}", id);

            var produto = await _produtoRepository.ObterPorIdAsync(id);
            
            if (produto == null)
            {
                _logger.LogWarning("Produto {ProdutoId} não encontrado para desativação", id);
                return false;
            }

            produto.Desativar();
            await _produtoRepository.AtualizarAsync(produto);

            // Publicar eventos de domínio
            await PublicarEventosDominio(produto);

            _logger.LogInformation("Produto {ProdutoId} desativado com sucesso", id);
            return true;
        }

        // Queries (mantém igual, sem alterações)
        public async Task<ProdutoDto> ObterPorIdAsync(ObterProdutoPorIdQuery query)
        {
            var produto = await _produtoRepository.ObterPorIdAsync(query.Id);
            
            if (produto == null)
            {
                return null;
            }

            return MapearParaDto(produto);
        }

        public async Task<IEnumerable<ProdutoResumoDto>> ListarProdutosAsync(ListarProdutosQuery query)
        {
            IEnumerable<Produto> produtos;

            if (query.ApenasAtivos.HasValue && query.ApenasAtivos.Value)
            {
                produtos = await _produtoRepository.ObterProdutosAtivosAsync();
            }
            else
            {
                produtos = await _produtoRepository.ObterTodosAsync();
            }

            // Filtrar por categoria se especificado
            if (!string.IsNullOrWhiteSpace(query.Categoria))
            {
                produtos = produtos.Where(p => p.Categoria.Equals(query.Categoria, StringComparison.OrdinalIgnoreCase));
            }

            // Filtrar por disponibilidade se especificado
            if (query.ApenasDisponiveis.HasValue && query.ApenasDisponiveis.Value)
            {
                produtos = produtos.Where(p => p.EstaDisponivel());
            }

            // Aplicar paginação
            var produtosList = produtos.ToList();
            var skip = ((query.Pagina ?? 1) - 1) * (query.ItensPorPagina ?? 10);
            var take = query.ItensPorPagina ?? 10;

            var produtosPaginados = produtosList.Skip(skip).Take(take);

            return produtosPaginados.Select(MapearParaResumoDto);
        }

        public async Task<IEnumerable<ProdutoResumoDto>> ObterPorCategoriaAsync(string categoria)
        {
            var produtos = await _produtoRepository.ObterPorCategoriaAsync(categoria);
            return produtos.Select(MapearParaResumoDto);
        }

        public async Task<ProdutoDto> ObterPorSkuAsync(string sku)
        {
            var produto = await _produtoRepository.ObterPorSkuAsync(sku);
            
            if (produto == null)
            {
                return null;
            }

            return MapearParaDto(produto);
        }

        // Método privado para publicar eventos
       private async Task PublicarEventosDominio(Produto produto)
{
    if (produto.DomainEvents != null && produto.DomainEvents.Any())
    {
        _logger.LogInformation("Iniciando publicação de {EventCount} eventos", produto.DomainEvents.Count);
        
        foreach (var domainEvent in produto.DomainEvents)
        {
            _logger.LogDebug("Publicando evento {EventType} para produto {ProdutoId}", 
                domainEvent.EventType, produto.Id);
            
            try
            {
                await _eventBus.PublishAsync(domainEvent);
                _logger.LogInformation("Evento {EventType} publicado com sucesso", domainEvent.EventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao publicar evento {EventType}", domainEvent.EventType);
                throw;
            }
        }

        // Limpar eventos após publicação
        produto.ClearDomainEvents();
        _logger.LogDebug("Eventos limpos após publicação");
    }
    else
    {
        _logger.LogWarning("Nenhum evento para publicar");
    }
}

        // Métodos auxiliares de mapeamento (mantém igual)
        private ProdutoDto MapearParaDto(Produto produto)
        {
            if (produto == null) return null;

            return new ProdutoDto
            {
                Id = produto.Id,
                Nome = produto.Nome,
                Descricao = produto.Descricao,
                Preco = produto.Preco.Valor,
                Moeda = produto.Preco.Moeda,
                QuantidadeEstoque = produto.QuantidadeEstoque,
                Ativo = produto.Ativo,
                Sku = produto.Sku,
                Categoria = produto.Categoria,
                ImagemUrl = produto.ImagemUrl,
                DataCriacao = produto.DataCriacao,
                DataAtualizacao = produto.DataAtualizacao,
                Disponivel = produto.EstaDisponivel()
            };
        }

        private ProdutoResumoDto MapearParaResumoDto(Produto produto)
        {
            if (produto == null) return null;

            return new ProdutoResumoDto
            {
                Id = produto.Id,
                Nome = produto.Nome,
                Preco = produto.Preco.Valor,
                Categoria = produto.Categoria,
                ImagemUrl = produto.ImagemUrl,
                Disponivel = produto.EstaDisponivel(),
                Sku = produto.Sku
            };
        }
    }
}