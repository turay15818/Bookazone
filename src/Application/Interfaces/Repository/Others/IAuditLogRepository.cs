using Bookazone.Domain.Entities.Others;

namespace Bookazone.Application.Interfaces.Repository.Others;

public interface IAuditLogRepository
{
    List<AuditLog> All();
    AuditLog? Find(int? id);
    bool Create(AuditLog auditLog, string? username = null);
    bool Update(AuditLog auditLog, string? username = null);
    bool Delete(int? id, string? username = null);
    bool Status(int? id, bool active, string? username = null);
}