using DataImport.Models;

namespace DataImport.Web.Areas.Instance.Modules;

public interface IInstancePostMigrationProcessingProvider
{
    void PostMigrationProcessing(DataImportDbContext dbContext,string instanceConnectionString);
}
