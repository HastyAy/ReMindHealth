namespace ReMindHealth.Services.Interfaces
{
    public interface IDiseaseSearchService
    {
        Task<DiseaseSearchResult> SearchDiseaseAsync(string diseaseName, CancellationToken cancellationToken = default);
    }
}
