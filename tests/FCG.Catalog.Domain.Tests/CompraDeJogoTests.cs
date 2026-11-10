using FCG.Catalog.Domain.Bibliotecas;
using FCG.Catalog.Domain.Comum;
using FCG.Catalog.Domain.Jogos;
using FCG.Catalog.Domain.Pedidos;
using FCG.Catalog.Domain.SeedWork;

namespace FCG.Catalog.Domain.Tests;

public class CompraDeJogoTests
{
    private static readonly Guid UsuarioId = Guid.NewGuid();
    private readonly ServicoDeCompraDeJogos _servico = new();

    private static Jogo NovoJogo(decimal preco = 100m) =>
        Jogo.Cadastrar("Hollow Knight", "Metroidvania", "Aventura", Dinheiro.Criar(preco), new DateTime(2017, 2, 24));

    [Fact]
    public void IniciarCompra_deve_criar_pedido_aguardando_pagamento_e_evento_PedidoRealizado()
    {
        var jogo = NovoJogo();

        var pedido = _servico.IniciarCompra(UsuarioId, "user@fiap.com", jogo, null, false, DateTime.UtcNow);

        Assert.Equal(StatusDoPedido.AguardandoPagamento, pedido.Status);
        var evento = Assert.IsType<PedidoRealizado>(Assert.Single(pedido.EventosDeDominio));
        Assert.Equal(100m, evento.Valor);
    }

    [Fact]
    public void IniciarCompra_aplica_preco_promocional_vigente()
    {
        var jogo = NovoJogo(100m);
        jogo.AdicionarPromocao("Black Friday", PercentualDeDesconto.Criar(25),
            PeriodoDeVigencia.Criar(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1)));

        var pedido = _servico.IniciarCompra(UsuarioId, "user@fiap.com", jogo, null, false, DateTime.UtcNow);

        Assert.Equal(75m, pedido.Valor.Valor);
    }

    [Fact]
    public void IniciarCompra_de_jogo_ja_na_biblioteca_deve_falhar()
    {
        var jogo = NovoJogo();
        var biblioteca = Biblioteca.CriarPara(UsuarioId);
        biblioteca.AdicionarJogo(jogo.Id, jogo.Titulo, jogo.Preco, Guid.NewGuid());

        Assert.Throws<DomainException>(() =>
            _servico.IniciarCompra(UsuarioId, "user@fiap.com", jogo, biblioteca, false, DateTime.UtcNow));
    }

    [Fact]
    public void IniciarCompra_com_pedido_em_andamento_deve_falhar() =>
        Assert.Throws<DomainException>(() =>
            _servico.IniciarCompra(UsuarioId, "user@fiap.com", NovoJogo(), null, true, DateTime.UtcNow));

    [Fact]
    public void IniciarCompra_de_jogo_inativo_deve_falhar()
    {
        var jogo = NovoJogo();
        jogo.Desativar();

        Assert.Throws<DomainException>(() =>
            _servico.IniciarCompra(UsuarioId, "user@fiap.com", jogo, null, false, DateTime.UtcNow));
    }

    [Fact]
    public void ConfirmarPagamento_eh_idempotente()
    {
        var pedido = _servico.IniciarCompra(UsuarioId, "user@fiap.com", NovoJogo(), null, false, DateTime.UtcNow);

        Assert.True(pedido.ConfirmarPagamento());
        Assert.False(pedido.ConfirmarPagamento());
        Assert.Equal(StatusDoPedido.Pago, pedido.Status);
    }

    [Fact]
    public void Pedido_recusado_nao_pode_ser_pago()
    {
        var pedido = _servico.IniciarCompra(UsuarioId, "user@fiap.com", NovoJogo(), null, false, DateTime.UtcNow);
        pedido.RecusarPagamento("Limite excedido");

        Assert.Throws<DomainException>(() => pedido.ConfirmarPagamento());
    }

    [Fact]
    public void Biblioteca_nao_duplica_jogo()
    {
        var biblioteca = Biblioteca.CriarPara(UsuarioId);
        var jogoId = Guid.NewGuid();

        Assert.True(biblioteca.AdicionarJogo(jogoId, "Celeste", Dinheiro.Criar(30), Guid.NewGuid()));
        Assert.False(biblioteca.AdicionarJogo(jogoId, "Celeste", Dinheiro.Criar(30), Guid.NewGuid()));
        Assert.Single(biblioteca.Itens);
    }

    [Fact]
    public void Promocoes_sobrepostas_devem_falhar()
    {
        var jogo = NovoJogo();
        var inicio = DateTime.UtcNow;
        jogo.AdicionarPromocao("A", PercentualDeDesconto.Criar(10), PeriodoDeVigencia.Criar(inicio, inicio.AddDays(5)));

        Assert.Throws<DomainException>(() =>
            jogo.AdicionarPromocao("B", PercentualDeDesconto.Criar(20), PeriodoDeVigencia.Criar(inicio.AddDays(2), inicio.AddDays(8))));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(95)]
    public void Percentual_de_desconto_fora_da_faixa_deve_falhar(decimal percentual) =>
        Assert.Throws<DomainException>(() => PercentualDeDesconto.Criar(percentual));
}
