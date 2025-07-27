namespace GBES.Models
{
    public interface IGBESportsRepository
    {
        Task<Z_Member> GetMember(string schoolName);
    }
}
