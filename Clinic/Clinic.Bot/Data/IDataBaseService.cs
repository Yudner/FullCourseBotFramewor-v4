using Clinic.Bot.Common.Models.MedicalAppointment;
using Clinic.Bot.Common.Models.Qualification;
using Clinic.Bot.Common.Models.User;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Clinic.Bot.Data
{
    public interface IDataBaseService
    {
        DbSet<UserModel> User { get; set; }
        DbSet<QualificationModel> Qualification { get; set; }
        DbSet<MedicalAppointmentModel> MedicalAppointment { get; set; }
        Task<bool> SaveAsync();
    }
}
