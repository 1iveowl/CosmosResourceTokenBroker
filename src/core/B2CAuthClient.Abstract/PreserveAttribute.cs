using System;

namespace B2CAuthClient.Abstract
{
    // https://github.com/aspnet/EntityFrameworkCore/issues/10963#issuecomment-370085182
    public sealed class PreserveAttribute : Attribute
    {
        public bool AllMembers;
        public bool Conditional;
    }
}
