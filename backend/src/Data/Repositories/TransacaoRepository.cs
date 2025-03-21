﻿using Business.Entities;
using Business.FiltrosBusca;
using Business.Interfaces;
using Business.ValueObjects;
using Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories
{
    public class TransacaoRepository(AppDbContext dbContext) : Repository<Transacao>(dbContext), ITransacaoRepository
    {
        public new async Task<Transacao?> ObterPorId(Guid id)
        {
            return await DbSet.Include(t => t.Categoria).AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        }
        public async Task<IEnumerable<Transacao>> ObterTodos(FiltroTransacao filtro, string usuarioId)
        {
            var query = DbSet.AsNoTracking().AsQueryable();

            if (filtro.TipoTransacao is not null)
            {
                query = query.Where(x => x.Tipo == filtro.TipoTransacao);
            }

            if (filtro.CategoriaId is not null)
            {
                query = query.Where(x => x.CategoriaId == filtro.CategoriaId);
            }

            if (filtro.Data is not null)
            {
                query = query.Where(x => x.Data == filtro.Data);
            }

            query = query.Where(x => x.Usuario.Id == usuarioId);

            query = query.Include(t => t.Categoria);

            query = query.OrderBy(t => t.Data).ThenBy(t => t.DataCriacao);

            return await query.ToListAsync();
        }

        public decimal ObterSaldoTotal(string usuarioId)
        {
           var result = DbSet.Where(x => x.UsuarioId == usuarioId).ToList().Sum(x => x.Valor);
           return result;
        }

        public decimal ObterValorTotalDeSaidasNoPeriodo(string usuarioId, DateOnly periodo, Guid? categoriaId)
        {
            var query = DbSet.Where(t => t.UsuarioId == usuarioId
                                         && t.Data.Year == periodo.Year
                                         && t.Data.Month == periodo.Month
                                         && t.Tipo == TipoTransacao.Saida);
            if (categoriaId.HasValue)
            {
                query = query.Where(t => t.CategoriaId == categoriaId);
            }
                
            var result = query.ToList().Sum(t => t.Valor);
            return result;
        }


        public async Task<ResumoFinanceiro> ObterResumoEntradasESaidas(string usuarioId)
        {   
            var query = await DbSet
                .Where(x => x.UsuarioId == usuarioId)
                .GroupBy(t => t.Tipo).ToDictionaryAsync(
                    g => g.Key,
                    g => g.Sum(t => t.Valor)
                );

            var resumo = new ResumoFinanceiro
            {
                TotalReceita = query.FirstOrDefault(x => x.Key == TipoTransacao.Entrada).Value,
                TotalDespesa = query.FirstOrDefault(x => x.Key == TipoTransacao.Saida).Value
            };

            return resumo;
           
        }
    }
}
