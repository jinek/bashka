namespace Bashka.Plugins.Core.Abstract.Surrounding
{
    public class UserCancelException : UserErrorException
    {
        public UserCancelException() : base("Operation cancelled")
        {
        }
    }
}