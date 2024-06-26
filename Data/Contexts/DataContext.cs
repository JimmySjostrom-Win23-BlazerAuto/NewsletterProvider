﻿using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Contexts;

public class DataContext(DbContextOptions options) : DbContext(options)
{
	public DbSet<SubscribeEntity> Subscribers { get; set; }
}
