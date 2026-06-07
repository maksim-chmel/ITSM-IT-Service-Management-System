using Microsoft.EntityFrameworkCore;

namespace ITSM.Data;

public class PostgresDBaseContext(DbContextOptions<PostgresDBaseContext> options) : DBaseContext(options);
