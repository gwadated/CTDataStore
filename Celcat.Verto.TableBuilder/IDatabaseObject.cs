namespace Celcat.Verto.TableBuilder
{
    public interface IDatabaseObject
    {
        string Name { get; set; }

        string GenerateSqlToCreate();
    }
}
