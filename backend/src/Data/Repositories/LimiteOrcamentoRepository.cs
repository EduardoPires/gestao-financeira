﻿using Business.Entities;
using Business.FiltrosBusca;
using Business.Interfaces;
using Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories
{
    public class LimiteOrcamentoRepository(AppDbContext dbContext) : Repository<LimiteOrcamento>(dbContext), ILimiteOrcamentoRepository
    {
        public async Task<IEnumerable<LimiteOrcamento>> ObterTodos(FiltroLimiteOrcamento filtro, string usuarioIdentityId)
        {
            var query = DbSet.AsNoTracking().AsQueryable();

            if (filtro.Periodo != null)
            {
                query = query.Where(x => x.Periodo.Year == filtro.Periodo.Value.Year && 
                                    x.Periodo.Month == filtro.Periodo.Value.Month);
            }

            if (filtro.CategoriaId != null)
            {
                query = query.Where(x => x.CategoriaId == filtro.CategoriaId);
            }

            query = query.Where(x => x.UsuarioId == usuarioIdentityId);

            query = query.Include(x => x.Categoria);

            query = query.OrderBy(x => x.Periodo).ThenBy(x => x.TipoLimite).ThenBy(x => x.Categoria.Nome).ThenBy(x => x.DataCriacao);

            return await query.ToListAsync();
        }
    }
}
