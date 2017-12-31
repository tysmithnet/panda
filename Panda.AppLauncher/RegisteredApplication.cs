using System.Collections.Generic;

namespace Panda.AppLauncher
{
    public sealed class RegisteredApplication
    {
        public string DisplayName { get; set; }
        public string FullPath { get; set; }

        public override bool Equals(object obj)
        {
            return obj is RegisteredApplication application &&
                   DisplayName == application.DisplayName &&
                   FullPath == application.FullPath;
        }

        public override int GetHashCode()
        {
            var hashCode = 1891732417;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DisplayName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FullPath);
            return hashCode;
        }
    }
}