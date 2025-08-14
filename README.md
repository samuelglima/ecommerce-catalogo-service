# 📦 Catálogo Service

Microserviço responsável pela gestão do catálogo de produtos.

## 🏗️ Arquitetura

Este serviço implementa:
- **Domain-Driven Design (DDD)**
- **Clean Architecture**
- **CQRS Pattern**
- **Repository Pattern**

## 📁 Estrutura do Projeto

src/
├── Catalogo.Domain/        # Domínio - Regras de negócio
├── Catalogo.Application/   # Aplicação - Casos de uso
├── Catalogo.Infrastructure/# Infraestrutura - Implementações
└── Catalogo.API/          # API - Controllers e configurações

## 🚀 Como Executar

```bash
# Restaurar pacotes
dotnet restore

# Compilar
dotnet build

# Executar
dotnet run --project src/Catalogo.API

## 🧪 Testes

### Executar todos os testes
dotnet test
Executar com detalhes
bashdotnet test --logger "console;verbosity=detailed"
Executar com cobertura
bashdotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

Estrutura de Testes

Catalogo.Domain.Tests: Testes unitários do domínio
Catalogo.Application.Tests: Testes da camada de aplicação
Catalogo.API.Tests: Testes dos controllers
Catalogo.Integration.Tests: Testes de integração


📋 Funcionalidades

 Cadastro de produtos
 Listagem de produtos
 Atualização de produtos
 Exclusão de produtos
 Controle de estoque
 Eventos de domínio

🛠️ Tecnologias

.NET 8
C# 12
ASP.NET Core
MassTransit (Mensageria)

📝 Licença
MIT