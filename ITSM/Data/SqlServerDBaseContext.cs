using Microsoft.EntityFrameworkCore;

namespace ITSM.Data;

public class SqlServerDBaseContext(DbContextOptions<SqlServerDBaseContext> options) : DBaseContext(options);
