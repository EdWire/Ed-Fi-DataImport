using DataImport.Models;

namespace DataImport.Web.Areas.Instance.Modules;
public class DefaultInstancePostMigrationProcessingProvider : IInstancePostMigrationProcessingProvider
{
    public void PostMigrationProcessing(DataImportDbContext dbContext, string instanceConnectionString)
    {
        return;//NOTE: Placeholder only
    }
}
