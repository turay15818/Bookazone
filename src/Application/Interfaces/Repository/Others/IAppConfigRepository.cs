using Bookazone.Domain.Entities.Others;

namespace Bookazone.Application.Interfaces.Repository.Others;

public interface IAppConfigRepository
{
    List<AppConfig> All();
    AppConfig? Find(Guid? id);
    AppConfig? FindBySlug(string? slug);
    AppConfig? Create(AppConfig entity);
    AppConfig? Update(AppConfig entity);
    bool Delete(Guid? id);
    bool Status(Guid? id, bool active);
}