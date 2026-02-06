using WebAPI_2.Core;

namespace WebAPI_2.Abstract
{
    public interface IJwtToken
    {
        string Create(UserRecord user);
    }
}
